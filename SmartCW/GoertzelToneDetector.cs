using System;
using System.Collections.Generic;

namespace RadioLoggerApp.MorseDecoder.SmartCW
{
    /// <summary>
    /// Front-end di rilevamento tono basato sull'algoritmo di Goertzel.
    /// Molto più efficiente di una FFT completa quando interessa una sola frequenza:
    /// calcola la potenza del tono CW in blocchi brevi (~4 ms) e la confronta con
    /// due bin laterali usati come riferimento di rumore.
    /// Include AFC (Automatic Frequency Control): una scansione periodica a banco di
    /// filtri Goertzel riaggancia automaticamente la frequenza del tono ricevuto.
    /// </summary>
    public class GoertzelToneDetector
    {
        private readonly int sampleRate;
        private readonly int blockSize;

        // Buffer di campioni in attesa di formare un blocco completo
        private readonly float[] pending;
        private int pendingCount;

        // Ring buffer per la scansione AFC (finestra più lunga = risoluzione migliore)
        private readonly float[] afcRing;
        private int afcRingPos;
        private int afcRingFilled;
        private int samplesSinceAfc;
        private readonly int afcIntervalSamples;

        private double targetFrequency;

        /// <summary>Frequenza del tono attualmente agganciata (Hz).</summary>
        public double TargetFrequency => targetFrequency;

        /// <summary>Durata di un blocco di analisi in millisecondi.</summary>
        public double BlockDurationMs => 1000.0 * blockSize / sampleRate;

        /// <summary>Limiti di ricerca AFC (Hz), tipici per il CW.</summary>
        public double AfcMinHz { get; set; } = 300;
        public double AfcMaxHz { get; set; } = 1200;

        /// <summary>Abilita/disabilita l'aggancio automatico di frequenza.</summary>
        public bool AfcEnabled { get; set; } = true;

        public GoertzelToneDetector(int sampleRate = 44100, double initialFrequency = 700,
                                    double blockMs = 4.0)
        {
            this.sampleRate = sampleRate;
            this.targetFrequency = initialFrequency;
            this.blockSize = Math.Max(16, (int)Math.Round(sampleRate * blockMs / 1000.0));
            this.pending = new float[blockSize];

            // ~46 ms di storia per l'AFC → risoluzione ~22 Hz a 44.1 kHz
            int afcWindow = 2048 * sampleRate / 44100;
            this.afcRing = new float[Math.Max(1024, afcWindow)];
            this.afcIntervalSamples = sampleRate / 4; // scansione ogni 250 ms
        }

        /// <summary>
        /// Elabora un buffer audio e restituisce una misura per ogni blocco completato:
        /// potenza del tono normalizzata rispetto al rumore dei bin adiacenti.
        /// </summary>
        public List<ToneBlock> ProcessSamples(float[] samples)
        {
            var blocks = new List<ToneBlock>();
            if (samples == null || samples.Length == 0) return blocks;

            foreach (var s in samples)
            {
                pending[pendingCount++] = s;

                afcRing[afcRingPos] = s;
                afcRingPos = (afcRingPos + 1) % afcRing.Length;
                if (afcRingFilled < afcRing.Length) afcRingFilled++;
                samplesSinceAfc++;

                if (pendingCount == blockSize)
                {
                    blocks.Add(AnalyzeBlock());
                    pendingCount = 0;
                }
            }

            if (AfcEnabled && samplesSinceAfc >= afcIntervalSamples && afcRingFilled == afcRing.Length)
            {
                samplesSinceAfc = 0;
                RunAfcScan();
            }

            return blocks;
        }

        /// <summary>
        /// Analizza il blocco corrente: Goertzel sul tono target e su due bin laterali
        /// (±2 larghezze di banda) come stima locale del rumore.
        /// </summary>
        private ToneBlock AnalyzeBlock()
        {
            double binWidth = (double)sampleRate / blockSize;
            double tone = GoertzelPower(pending, blockSize, targetFrequency);
            double noiseLo = GoertzelPower(pending, blockSize, Math.Max(100, targetFrequency - 2 * binWidth));
            double noiseHi = GoertzelPower(pending, blockSize, targetFrequency + 2 * binWidth);
            double noise = (noiseLo + noiseHi) / 2 + 1e-12;

            return new ToneBlock
            {
                TonePower = tone,
                NoisePower = noise,
                DurationMs = BlockDurationMs
            };
        }

        /// <summary>
        /// Scansione AFC: banco di Goertzel a passi di ~10 Hz sull'intera finestra;
        /// se emerge un picco netto sopra la mediana, la frequenza target viene
        /// riagganciata dolcemente verso il picco.
        /// </summary>
        private void RunAfcScan()
        {
            // Ricostruisce la finestra in ordine temporale
            var window = new float[afcRing.Length];
            for (int i = 0; i < afcRing.Length; i++)
            {
                window[i] = afcRing[(afcRingPos + i) % afcRing.Length];
            }

            double bestFreq = targetFrequency;
            double bestPower = 0;
            var powers = new List<double>();

            for (double f = AfcMinHz; f <= AfcMaxHz; f += 10)
            {
                double p = GoertzelPower(window, window.Length, f);
                powers.Add(p);
                if (p > bestPower)
                {
                    bestPower = p;
                    bestFreq = f;
                }
            }

            powers.Sort();
            double median = powers[powers.Count / 2] + 1e-12;

            // Riaggancia solo se il picco è nettamente sopra il rumore di fondo
            if (bestPower > median * 8)
            {
                targetFrequency += (bestFreq - targetFrequency) * 0.5;
            }
        }

        /// <summary>
        /// Potenza spettrale alla frequenza data (algoritmo di Goertzel, O(n) per bin).
        /// Applica una finestra di Hann per ridurre lo spectral leakage.
        /// </summary>
        private double GoertzelPower(float[] samples, int count, double frequency)
        {
            double omega = 2.0 * Math.PI * frequency / sampleRate;
            double coeff = 2.0 * Math.Cos(omega);
            double s0, s1 = 0, s2 = 0;

            for (int i = 0; i < count; i++)
            {
                double hann = 0.5 - 0.5 * Math.Cos(2.0 * Math.PI * i / (count - 1));
                s0 = samples[i] * hann + coeff * s1 - s2;
                s2 = s1;
                s1 = s0;
            }

            double power = s1 * s1 + s2 * s2 - coeff * s1 * s2;
            return power / count; // normalizza rispetto alla lunghezza della finestra
        }
    }

    /// <summary>Misura di un singolo blocco di analisi.</summary>
    public class ToneBlock
    {
        public double TonePower { get; set; }
        public double NoisePower { get; set; }
        public double DurationMs { get; set; }

        /// <summary>Rapporto tono/rumore lineare del blocco.</summary>
        public double Ratio => TonePower / (NoisePower + 1e-12);
    }
}
