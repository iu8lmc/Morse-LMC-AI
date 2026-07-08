using System;
using System.Collections.Generic;

namespace RadioLoggerApp.MorseDecoder.SmartCW
{
    /// <summary>
    /// Trasforma la sequenza di potenze per blocco in eventi mark/space (tono on/off).
    /// A differenza di una soglia fissa, insegue continuamente sia il rumore di fondo
    /// sia il picco del segnale, e usa una doppia soglia con isteresi: questo rende il
    /// gate robusto a QSB (fading), QRM e variazioni di guadagno del ricevitore.
    /// Il debounce fonde i glitch più corti di due blocchi con le run adiacenti,
    /// al costo di una run di latenza nell'emissione degli eventi.
    /// </summary>
    public class AdaptiveSignalGate
    {
        // Inseguitori di livello (in scala logaritmica per stabilità sulla dinamica)
        private double noiseFloorDb = -60;
        private double signalPeakDb = -30;
        private bool levelsInitialized;

        // Run corrente del gate
        private bool isOn;
        private double stateDurationMs;
        private bool hasState;

        // Ultima run completata, trattenuta per poter assorbire eventuali glitch
        private KeyingEvent? pendingEvent;

        // Debounce: run più corte di questa durata vengono considerate glitch
        private readonly double minEventMs;

        // Silenzio massimo prima del flush della decodifica
        private readonly double idleFlushMs;
        private double silenceMs;

        /// <summary>Soglia di accensione come frazione della dinamica rumore→picco.</summary>
        public double OnThresholdFraction { get; set; } = 0.55;

        /// <summary>Soglia di spegnimento (più bassa: isteresi).</summary>
        public double OffThresholdFraction { get; set; } = 0.35;

        /// <summary>SNR stimato in dB (dinamica picco - rumore).</summary>
        public double SnrDb => Math.Max(0, signalPeakDb - noiseFloorDb);

        // Filtro mediano a 3 blocchi sul livello: sopprime gli spike di rumore
        // da singolo blocco senza ammorbidire i fronti come farebbe una media
        private readonly double[] medianWindow = new double[3];
        private int medianCount;

        /// <summary>
        /// Squelch: sotto questa dinamica rumore→picco (dB) il gate resta chiuso.
        /// Evita che il rumore generi eventi finché non c'è un segnale reale.
        /// </summary>
        public double SquelchRangeDb { get; set; } = 8.0;

        public AdaptiveSignalGate(double minEventMs = 12, double idleFlushMs = 2500)
        {
            this.minEventMs = minEventMs;
            this.idleFlushMs = idleFlushMs;
        }

        /// <summary>
        /// Elabora un blocco e restituisce gli eventi keying completati
        /// (un evento = durata di un mark o di uno space concluso).
        /// </summary>
        public List<KeyingEvent> ProcessBlock(ToneBlock block)
        {
            var events = new List<KeyingEvent>();

            double rawDb = 10 * Math.Log10(block.TonePower + 1e-12);

            // Filtro mediano a 3 blocchi
            medianWindow[medianCount % 3] = rawDb;
            medianCount++;
            double levelDb;
            if (medianCount >= 3)
            {
                double a = medianWindow[0], b = medianWindow[1], c = medianWindow[2];
                levelDb = Math.Max(Math.Min(a, b), Math.Min(Math.Max(a, b), c));
            }
            else
            {
                levelDb = rawDb;
            }

            if (!levelsInitialized)
            {
                noiseFloorDb = levelDb;
                signalPeakDb = levelDb + 6;
                levelsInitialized = true;
            }

            double range = signalPeakDb - noiseFloorDb;

            // Margine minimo assoluto sopra il rumore: quando il picco decade
            // (inizio/fine trasmissione) la soglia frazionaria si avvicinerebbe
            // troppo al rumore rendendo il gate ipersensibile ai suoi picchi
            double onThreshold = noiseFloorDb + Math.Max(9.0, range * OnThresholdFraction);
            double offThreshold = noiseFloorDb + Math.Max(6.0, range * OffThresholdFraction);

            // Squelch: senza una dinamica credibile non c'è segnale da decodificare
            bool nowOn = range >= SquelchRangeDb
                && (isOn ? (levelDb > offThreshold) : (levelDb > onThreshold));

            UpdateLevelTrackers(levelDb, nowOn);

            if (!hasState)
            {
                hasState = true;
                isOn = nowOn;
                stateDurationMs = block.DurationMs;
                return events;
            }

            if (nowOn == isOn)
            {
                stateDurationMs += block.DurationMs;
            }
            else
            {
                // La run corrente si è appena conclusa
                if (stateDurationMs < minEventMs && pendingEvent != null && pendingEvent.IsMark == nowOn)
                {
                    // Glitch: fondi run precedente + glitch + nuova run in un'unica run
                    isOn = nowOn;
                    stateDurationMs += pendingEvent.DurationMs + block.DurationMs;
                    pendingEvent = null;
                }
                else
                {
                    // Run valida: emetti quella trattenuta e trattieni questa
                    if (pendingEvent != null)
                    {
                        events.Add(pendingEvent);
                    }
                    pendingEvent = new KeyingEvent { IsMark = isOn, DurationMs = stateDurationMs };
                    isOn = nowOn;
                    stateDurationMs = block.DurationMs;
                }
            }

            // Flush su silenzio prolungato: svuota tutto e segnala a valle
            if (!isOn)
            {
                silenceMs += block.DurationMs;
                if (silenceMs >= idleFlushMs)
                {
                    silenceMs = 0;
                    if (pendingEvent != null)
                    {
                        events.Add(pendingEvent);
                        pendingEvent = null;
                    }
                    events.Add(new KeyingEvent { IsFlush = true, DurationMs = stateDurationMs });
                    stateDurationMs = 0;
                }
            }
            else
            {
                silenceMs = 0;
            }

            return events;
        }

        /// <summary>
        /// Aggiorna gli inseguitori di rumore e picco.
        /// Rumore: EMA simmetrica ma SOLO sui blocchi classificati come spazio,
        /// così il floor stima il rumore medio (non il suo inviluppo inferiore)
        /// e resta congelato durante i mark. Picco: attacco rapido,
        /// rilascio lento (così sopravvive ai fading del QSB).
        /// </summary>
        private void UpdateLevelTrackers(double levelDb, bool nowOn)
        {
            if (!nowOn)
            {
                // Recupero rapido se il floor è rimasto molto sopra il rumore reale
                double rate = levelDb < noiseFloorDb - 6 ? 0.20 : 0.05;
                noiseFloorDb += (levelDb - noiseFloorDb) * rate;
            }

            if (levelDb > signalPeakDb)
            {
                // Attacco rapido: aggancia il primo elemento della trasmissione
                signalPeakDb += (levelDb - signalPeakDb) * 0.50;
            }
            else
            {
                // Rilascio lento durante il QSB, ma rapido dopo un lungo silenzio:
                // così lo squelch si richiude quando la trasmissione è finita
                double release = silenceMs > 2000 ? 0.02 : 0.002;
                signalPeakDb += (levelDb - signalPeakDb) * release;
            }

            if (signalPeakDb < noiseFloorDb + 3) signalPeakDb = noiseFloorDb + 3;
        }

        public void Reset()
        {
            levelsInitialized = false;
            hasState = false;
            isOn = false;
            stateDurationMs = 0;
            silenceMs = 0;
            pendingEvent = null;
        }
    }

    /// <summary>Evento di keying: durata di un tono (mark) o di una pausa (space).</summary>
    public class KeyingEvent
    {
        public bool IsMark { get; set; }
        public double DurationMs { get; set; }

        /// <summary>Vero se è una richiesta di flush per silenzio prolungato.</summary>
        public bool IsFlush { get; set; }
    }
}
