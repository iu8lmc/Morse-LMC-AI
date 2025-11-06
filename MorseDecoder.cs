using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadioLoggerApp.MorseDecoder
{
    /// <summary>
    /// Decoder intelligente per il codice Morse con rilevamento automatico della velocità
    /// e correzione degli errori
    /// </summary>
    public class MorseDecoder
    {
        // Dizionario del codice Morse internazionale
        private static readonly Dictionary<string, char> MorseToChar = new()
        {
            // Lettere
            { ".-", 'A' }, { "-...", 'B' }, { "-.-.", 'C' }, { "-..", 'D' },
            { ".", 'E' }, { "..-.", 'F' }, { "--.", 'G' }, { "....", 'H' },
            { "..", 'I' }, { ".---", 'J' }, { "-.-", 'K' }, { ".-..", 'L' },
            { "--", 'M' }, { "-.", 'N' }, { "---", 'O' }, { ".--.", 'P' },
            { "--.-", 'Q' }, { ".-.", 'R' }, { "...", 'S' }, { "-", 'T' },
            { "..-", 'U' }, { "...-", 'V' }, { ".--", 'W' }, { "-..-", 'X' },
            { "-.--", 'Y' }, { "--..", 'Z' },

            // Numeri
            { "-----", '0' }, { ".----", '1' }, { "..---", '2' }, { "...--", '3' },
            { "....-", '4' }, { ".....", '5' }, { "-....", '6' }, { "--...", '7' },
            { "---..", '8' }, { "----.", '9' },

            // Punteggiatura
            { ".-.-.-", '.' }, { "--..--", ',' }, { "..--..", '?' }, { ".----.", '\'' },
            { "-.-.--", '!' }, { "-..-.", '/' }, { "-.--.", '(' }, { "-.--.-", ')' },
            { ".-...", '&' }, { "---...", ':' }, { "-.-.-.", ';' }, { "-...-", '=' },
            { ".-.-.", '+' }, { "-....-", '-' }, { "..--.-", '_' }, { ".-..-.", '"' },
            { "...-..-", '$' }, { ".--.-.", '@' },

            // Segnali prosigns
            { ".-.-.", '<AR>' },  // End of message
            { "-...-", '<BT>' },  // Break
            { "...-.-", '<SK>' }, // End of contact
            { "...-..", '<SOS>' } // SOS
        };

        // Stato del decoder
        private enum State
        {
            Idle,
            Signal,
            Gap
        }

        private State currentState;
        private long signalStartTime;
        private long gapStartTime;
        private readonly StringBuilder currentSymbol;
        private readonly StringBuilder currentWord;
        private readonly StringBuilder decodedText;

        // Parametri di timing (millisecondi)
        private double ditLength;
        private double dahLength;
        private double symbolGap;
        private double letterGap;
        private double wordGap;

        // Auto-calibrazione
        private readonly Queue<long> signalDurations;
        private readonly Queue<long> gapDurations;
        private const int CalibrationHistorySize = 50;
        private bool isCalibrated;

        // Soglie
        private double threshold;
        private readonly double defaultThreshold;

        // Statistiche
        public int CharactersDecoded { get; private set; }
        public int ErrorsDetected { get; private set; }
        public double CurrentWPM { get; private set; }
        public bool IsCalibrated => isCalibrated;
        public double Confidence { get; private set; }

        // Eventi
        public event EventHandler<CharacterDecodedEventArgs>? CharacterDecoded;
        public event EventHandler<WordDecodedEventArgs>? WordDecoded;
        public event EventHandler<string>? TextDecoded;
        public event EventHandler<TimingCalibratedEventArgs>? TimingCalibrated;

        /// <summary>
        /// Inizializza il decoder Morse
        /// </summary>
        public MorseDecoder(double initialWPM = 20, double threshold = 0.3)
        {
            this.currentState = State.Idle;
            this.currentSymbol = new StringBuilder();
            this.currentWord = new StringBuilder();
            this.decodedText = new StringBuilder();

            this.signalDurations = new Queue<long>(CalibrationHistorySize);
            this.gapDurations = new Queue<long>(CalibrationHistorySize);

            this.defaultThreshold = threshold;
            this.threshold = threshold;

            SetWPM(initialWPM);
            Reset();
        }

        /// <summary>
        /// Imposta la velocità in Words Per Minute (WPM)
        /// </summary>
        public void SetWPM(double wpm)
        {
            CurrentWPM = wpm;

            // Formula standard: PARIS come parola di riferimento (50 unità)
            // 1 WPM = 50 unità al minuto
            ditLength = 1200.0 / wpm; // millisecondi

            dahLength = ditLength * 3;
            symbolGap = ditLength;
            letterGap = ditLength * 3;
            wordGap = ditLength * 7;
        }

        /// <summary>
        /// Processa un campione del segnale envelope
        /// </summary>
        public void ProcessEnvelopeSample(double envelopeValue, long timestampMs)
        {
            bool signalPresent = envelopeValue > threshold;

            switch (currentState)
            {
                case State.Idle:
                    if (signalPresent)
                    {
                        // Inizio di un segnale
                        currentState = State.Signal;
                        signalStartTime = timestampMs;
                    }
                    break;

                case State.Signal:
                    if (!signalPresent)
                    {
                        // Fine del segnale
                        long signalDuration = timestampMs - signalStartTime;
                        ProcessSignalEnd(signalDuration);

                        currentState = State.Gap;
                        gapStartTime = timestampMs;
                    }
                    break;

                case State.Gap:
                    if (signalPresent)
                    {
                        // Fine del gap, inizio nuovo segnale
                        long gapDuration = timestampMs - gapStartTime;
                        ProcessGapEnd(gapDuration);

                        currentState = State.Signal;
                        signalStartTime = timestampMs;
                    }
                    else
                    {
                        // Controlla se il gap è abbastanza lungo per terminare una lettera o parola
                        long gapDuration = timestampMs - gapStartTime;
                        CheckForLetterOrWordEnd(gapDuration);
                    }
                    break;
            }
        }

        /// <summary>
        /// Processa la fine di un segnale (dit o dah)
        /// </summary>
        private void ProcessSignalEnd(long duration)
        {
            // Aggiungi alla calibrazione
            signalDurations.Enqueue(duration);
            if (signalDurations.Count > CalibrationHistorySize)
            {
                signalDurations.Dequeue();
            }

            // Auto-calibrazione
            if (signalDurations.Count >= 10)
            {
                CalibrateTimings();
            }

            // Determina se è un dit o un dah
            string symbol = ClassifySignal(duration);
            currentSymbol.Append(symbol);
        }

        /// <summary>
        /// Classifica un segnale come dit (.) o dah (-)
        /// </summary>
        private string ClassifySignal(long duration)
        {
            // Usa la media tra dit e dah come soglia
            double thresholdDuration = (ditLength + dahLength) / 2;

            if (duration < thresholdDuration)
            {
                return ".";
            }
            else
            {
                return "-";
            }
        }

        /// <summary>
        /// Processa la fine di un gap
        /// </summary>
        private void ProcessGapEnd(long duration)
        {
            // Aggiungi alla calibrazione
            gapDurations.Enqueue(duration);
            if (gapDurations.Count > CalibrationHistorySize)
            {
                gapDurations.Dequeue();
            }
        }

        /// <summary>
        /// Controlla se il gap è abbastanza lungo per terminare una lettera o parola
        /// </summary>
        private void CheckForLetterOrWordEnd(long gapDuration)
        {
            // Gap tra lettere
            if (gapDuration > letterGap * 0.7 && currentSymbol.Length > 0)
            {
                DecodeLetter();
            }

            // Gap tra parole
            if (gapDuration > wordGap * 0.7 && currentWord.Length > 0)
            {
                DecodeWord();
            }
        }

        /// <summary>
        /// Decodifica il simbolo corrente in una lettera
        /// </summary>
        private void DecodeLetter()
        {
            string morseSymbol = currentSymbol.ToString();

            if (MorseToChar.TryGetValue(morseSymbol, out char character))
            {
                currentWord.Append(character);
                CharactersDecoded++;

                // Calcola la confidenza
                Confidence = CalculateConfidence(morseSymbol);

                CharacterDecoded?.Invoke(this, new CharacterDecodedEventArgs
                {
                    Character = character,
                    MorseCode = morseSymbol,
                    Confidence = Confidence,
                    Timestamp = DateTime.Now
                });

                TextDecoded?.Invoke(this, character.ToString());
            }
            else
            {
                // Simbolo non riconosciuto
                currentWord.Append('?');
                ErrorsDetected++;

                CharacterDecoded?.Invoke(this, new CharacterDecodedEventArgs
                {
                    Character = '?',
                    MorseCode = morseSymbol,
                    Confidence = 0,
                    Timestamp = DateTime.Now
                });
            }

            currentSymbol.Clear();
        }

        /// <summary>
        /// Decodifica la parola corrente
        /// </summary>
        private void DecodeWord()
        {
            string word = currentWord.ToString();

            if (!string.IsNullOrEmpty(word))
            {
                decodedText.Append(word).Append(' ');

                WordDecoded?.Invoke(this, new WordDecodedEventArgs
                {
                    Word = word,
                    Timestamp = DateTime.Now
                });

                TextDecoded?.Invoke(this, " ");
            }

            currentWord.Clear();
        }

        /// <summary>
        /// Auto-calibra i timing basandosi sui segnali ricevuti
        /// </summary>
        private void CalibrateTimings()
        {
            if (signalDurations.Count < 10)
            {
                return;
            }

            // Ordina le durate
            var sortedSignals = signalDurations.OrderBy(x => x).ToList();

            // Usa il clustering per separare dit e dah
            // Metodo semplificato: usa i quartili
            int q1Index = sortedSignals.Count / 4;
            int q3Index = (sortedSignals.Count * 3) / 4;

            double estimatedDit = sortedSignals.Take(q1Index + 1).Average();
            double estimatedDah = sortedSignals.Skip(q3Index).Average();

            // Verifica che il rapporto sia ragionevole (dah ~ 3 * dit)
            double ratio = estimatedDah / estimatedDit;

            if (ratio >= 2.0 && ratio <= 4.0)
            {
                ditLength = estimatedDit;
                dahLength = estimatedDah;
                symbolGap = ditLength;
                letterGap = ditLength * 3;
                wordGap = ditLength * 7;

                // Calcola WPM
                CurrentWPM = 1200.0 / ditLength;

                isCalibrated = true;

                TimingCalibrated?.Invoke(this, new TimingCalibratedEventArgs
                {
                    DitLength = ditLength,
                    DahLength = dahLength,
                    WPM = CurrentWPM,
                    Confidence = CalculateCalibrationConfidence()
                });
            }
        }

        /// <summary>
        /// Calcola la confidenza della decodifica
        /// </summary>
        private double CalculateConfidence(string morseSymbol)
        {
            // Fattori che influenzano la confidenza:
            // 1. Calibrazione
            // 2. SNR
            // 3. Consistenza dei timing

            double baseConfidence = isCalibrated ? 0.8 : 0.5;

            // Bonus per simboli comuni
            if (morseSymbol.Length <= 4)
            {
                baseConfidence += 0.1;
            }

            return Math.Clamp(baseConfidence, 0, 1);
        }

        /// <summary>
        /// Calcola la confidenza della calibrazione
        /// </summary>
        private double CalculateCalibrationConfidence()
        {
            if (signalDurations.Count < CalibrationHistorySize / 2)
            {
                return 0.5;
            }

            // Calcola la deviazione standard
            double mean = signalDurations.Average();
            double variance = signalDurations.Sum(d => Math.Pow(d - mean, 2)) / signalDurations.Count;
            double stdDev = Math.Sqrt(variance);

            // Coefficiente di variazione
            double cv = stdDev / mean;

            // Confidenza inversamente proporzionale alla variazione
            return Math.Clamp(1.0 - cv, 0.3, 1.0);
        }

        /// <summary>
        /// Ottiene il testo decodificato
        /// </summary>
        public string GetDecodedText()
        {
            return decodedText.ToString();
        }

        /// <summary>
        /// Resetta il decoder
        /// </summary>
        public void Reset()
        {
            currentState = State.Idle;
            currentSymbol.Clear();
            currentWord.Clear();
            decodedText.Clear();
            signalDurations.Clear();
            gapDurations.Clear();
            isCalibrated = false;
            CharactersDecoded = 0;
            ErrorsDetected = 0;
            Confidence = 0;
        }

        /// <summary>
        /// Resetta solo il testo decodificato
        /// </summary>
        public void ClearDecodedText()
        {
            decodedText.Clear();
            CharactersDecoded = 0;
            ErrorsDetected = 0;
        }
    }

    /// <summary>
    /// Argomenti evento carattere decodificato
    /// </summary>
    public class CharacterDecodedEventArgs : EventArgs
    {
        public char Character { get; set; }
        public string MorseCode { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Argomenti evento parola decodificata
    /// </summary>
    public class WordDecodedEventArgs : EventArgs
    {
        public string Word { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Argomenti evento timing calibrato
    /// </summary>
    public class TimingCalibratedEventArgs : EventArgs
    {
        public double DitLength { get; set; }
        public double DahLength { get; set; }
        public double WPM { get; set; }
        public double Confidence { get; set; }
    }
}
