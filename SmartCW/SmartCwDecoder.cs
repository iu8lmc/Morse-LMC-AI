using System;

namespace RadioLoggerApp.MorseDecoder.SmartCW
{
    /// <summary>
    /// Facciata del motore SmartCW: riceve audio grezzo e produce testo decodificato.
    /// Catena di elaborazione:
    ///   audio → GoertzelToneDetector (potenza del tono, AFC)
    ///         → AdaptiveSignalGate  (eventi mark/space con soglie adattive)
    ///         → BayesianTimingDecoder (beam search + modello linguistico ham)
    /// Espone eventi compatibili con l'infrastruttura esistente dell'applicazione.
    /// </summary>
    public class SmartCwDecoder
    {
        private readonly GoertzelToneDetector toneDetector;
        private readonly AdaptiveSignalGate signalGate;
        private readonly BayesianTimingDecoder timingDecoder;

        private int charactersDecoded;

        /// <summary>Frequenza del tono CW agganciata (Hz).</summary>
        public double Frequency => toneDetector.TargetFrequency;

        /// <summary>SNR stimato del segnale (dB).</summary>
        public double SnrDb => signalGate.SnrDb;

        /// <summary>Velocità stimata (words per minute).</summary>
        public double CurrentWpm => timingDecoder.EstimatedWpm;

        /// <summary>Confidenza della decodifica corrente (0–1).</summary>
        public double Confidence => timingDecoder.Confidence;

        /// <summary>Testo confermato dal consenso del beam.</summary>
        public string DecodedText => timingDecoder.CommittedText;

        /// <summary>Anteprima provvisoria (migliore ipotesi non ancora confermata).</summary>
        public string TentativeText => timingDecoder.TentativeTail;

        /// <summary>Numero di caratteri confermati.</summary>
        public int CharactersDecoded => charactersDecoded;

        /// <summary>Nuovo testo confermato (delta).</summary>
        public event EventHandler<string>? TextDecoded;

        /// <summary>Carattere confermato.</summary>
        public event EventHandler<CharacterDecodedEventArgs>? CharacterDecoded;

        /// <summary>Parola confermata.</summary>
        public event EventHandler<WordDecodedEventArgs>? WordDecoded;

        public SmartCwDecoder(int sampleRate = 44100, double initialFrequency = 700)
        {
            toneDetector = new GoertzelToneDetector(sampleRate, initialFrequency);
            signalGate = new AdaptiveSignalGate();
            timingDecoder = new BayesianTimingDecoder();

            timingDecoder.TextCommitted += (_, text) =>
            {
                TextDecoded?.Invoke(this, text);
                foreach (char c in text)
                {
                    if (c == ' ') continue;
                    charactersDecoded++;
                    CharacterDecoded?.Invoke(this, new CharacterDecodedEventArgs
                    {
                        Character = c,
                        MorseCode = string.Empty,
                        Confidence = timingDecoder.Confidence,
                        Timestamp = DateTime.Now
                    });
                }
            };

            timingDecoder.WordCommitted += (_, word) =>
            {
                WordDecoded?.Invoke(this, new WordDecodedEventArgs
                {
                    Word = word,
                    Timestamp = DateTime.Now
                });
            };
        }

        /// <summary>
        /// Elabora un buffer di campioni audio normalizzati [-1, 1].
        /// Può essere chiamato con buffer di qualunque dimensione.
        /// </summary>
        public void ProcessAudio(float[] samples)
        {
            var blocks = toneDetector.ProcessSamples(samples);
            foreach (var block in blocks)
            {
                var events = signalGate.ProcessBlock(block);
                foreach (var evt in events)
                {
                    timingDecoder.ProcessEvent(evt);
                }
            }
        }

        /// <summary>Forza l'emissione del testo pendente (es. a fine ricezione).</summary>
        public void Flush()
        {
            timingDecoder.Flush();
        }

        /// <summary>Riporta il motore allo stato iniziale.</summary>
        public void Reset()
        {
            signalGate.Reset();
            timingDecoder.Reset();
            charactersDecoded = 0;
        }
    }
}
