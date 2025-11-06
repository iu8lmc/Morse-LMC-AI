using System;
using System.Collections.Generic;
using System.Linq;

namespace RadioLoggerApp.MorseDecoder.Testing
{
    /// <summary>
    /// Generatore di segnali Morse sintetici per testing e validazione
    /// Crea segnali audio simulati con caratteristiche configurabili
    /// </summary>
    public class MorseSignalGenerator
    {
        private readonly int sampleRate;
        private readonly double frequency;
        private readonly double wpm;
        private readonly double ditDuration; // in millisecondi

        // Dizionario inverso: da carattere a codice Morse
        private static readonly Dictionary<char, string> CharToMorse = new()
        {
            // Lettere
            { 'A', ".-" }, { 'B', "-..." }, { 'C', "-.-." }, { 'D', "-.." },
            { 'E', "." }, { 'F', "..-." }, { 'G', "--." }, { 'H', "...." },
            { 'I', ".." }, { 'J', ".---" }, { 'K', "-.-" }, { 'L', ".-.." },
            { 'M', "--" }, { 'N', "-." }, { 'O', "---" }, { 'P', ".--." },
            { 'Q', "--.-" }, { 'R', ".-." }, { 'S', "..." }, { 'T', "-" },
            { 'U', "..-" }, { 'V', "...-" }, { 'W', ".--" }, { 'X', "-..-" },
            { 'Y', "-.--" }, { 'Z', "--.." },

            // Numeri
            { '0', "-----" }, { '1', ".----" }, { '2', "..---" }, { '3', "...--" },
            { '4', "....-" }, { '5', "....." }, { '6', "-...." }, { '7', "--..." },
            { '8', "---.." }, { '9', "----." },

            // Punteggiatura
            { '.', ".-.-.-" }, { ',', "--..--" }, { '?', "..--.." }, { '\'', ".----." },
            { '!', "-.-.--" }, { '/', "-..-." }, { '(', "-.--." }, { ')', "-.--.-" },
            { '&', ".-..." }, { ':', "---..." }, { ';', "-.-.-." }, { '=', "-...-" },
            { '+', ".-.-." }, { '-', "-....-" }, { '_', "..--.-" }, { '"', ".-..-." },
            { '$', "...-..-" }, { '@', ".--.-." }
        };

        /// <summary>
        /// Inizializza il generatore di segnali Morse
        /// </summary>
        /// <param name="wpm">Velocità in Words Per Minute (default: 20)</param>
        /// <param name="frequency">Frequenza del tono in Hz (default: 800)</param>
        /// <param name="sampleRate">Frequenza di campionamento (default: 44100)</param>
        public MorseSignalGenerator(double wpm = 20, double frequency = 800, int sampleRate = 44100)
        {
            this.wpm = wpm;
            this.frequency = frequency;
            this.sampleRate = sampleRate;

            // Calcola la durata di un dit in base ai WPM
            // Formula standard: PARIS = 50 unità, 1 WPM = 50 unità/minuto
            this.ditDuration = 1200.0 / wpm; // millisecondi
        }

        /// <summary>
        /// Genera il segnale audio per un testo in codice Morse
        /// </summary>
        /// <param name="text">Testo da convertire in Morse</param>
        /// <param name="noiseLevel">Livello di rumore (0.0 = nessuno, 0.1 = 10%, etc.)</param>
        /// <param name="signalStrength">Forza del segnale (0.0-1.0, default: 0.8)</param>
        /// <returns>Array di campioni audio normalizzati (-1.0 a 1.0)</returns>
        public float[] GenerateSignal(string text, double noiseLevel = 0.0, double signalStrength = 0.8)
        {
            var samples = new List<float>();
            var random = new Random();

            text = text.ToUpper();

            foreach (char c in text)
            {
                if (c == ' ')
                {
                    // Spazio tra parole (7 unità)
                    AddSilence(samples, ditDuration * 7, noiseLevel, random);
                }
                else if (CharToMorse.TryGetValue(c, out string? morseCode))
                {
                    // Genera i simboli Morse per il carattere
                    foreach (char symbol in morseCode)
                    {
                        if (symbol == '.')
                        {
                            // Dit (1 unità)
                            AddTone(samples, ditDuration, signalStrength, random);
                        }
                        else if (symbol == '-')
                        {
                            // Dah (3 unità)
                            AddTone(samples, ditDuration * 3, signalStrength, random);
                        }

                        // Gap tra simboli (1 unità)
                        AddSilence(samples, ditDuration, noiseLevel, random);
                    }

                    // Gap tra lettere (3 unità totali, -1 già aggiunto)
                    AddSilence(samples, ditDuration * 2, noiseLevel, random);
                }
            }

            return samples.ToArray();
        }

        /// <summary>
        /// Genera segnale Morse direttamente da codice Morse
        /// </summary>
        /// <param name="morseCode">Codice Morse (es: "... --- ...")</param>
        /// <param name="noiseLevel">Livello di rumore</param>
        /// <param name="signalStrength">Forza del segnale</param>
        public float[] GenerateFromMorseCode(string morseCode, double noiseLevel = 0.0, double signalStrength = 0.8)
        {
            var samples = new List<float>();
            var random = new Random();

            foreach (char symbol in morseCode)
            {
                if (symbol == '.')
                {
                    AddTone(samples, ditDuration, signalStrength, random);
                    AddSilence(samples, ditDuration, noiseLevel, random);
                }
                else if (symbol == '-')
                {
                    AddTone(samples, ditDuration * 3, signalStrength, random);
                    AddSilence(samples, ditDuration, noiseLevel, random);
                }
                else if (symbol == ' ')
                {
                    AddSilence(samples, ditDuration * 2, noiseLevel, random);
                }
            }

            return samples.ToArray();
        }

        /// <summary>
        /// Aggiunge un tono sinusoidale al buffer
        /// </summary>
        private void AddTone(List<float> samples, double durationMs, double amplitude, Random random)
        {
            int numSamples = (int)(sampleRate * durationMs / 1000.0);

            // Attack e release per rendere il segnale più realistico
            int attackSamples = (int)(sampleRate * 0.002); // 2ms attack
            int releaseSamples = (int)(sampleRate * 0.002); // 2ms release

            for (int i = 0; i < numSamples; i++)
            {
                // Genera onda sinusoidale
                double t = i / (double)sampleRate;
                float sample = (float)(amplitude * Math.Sin(2.0 * Math.PI * frequency * t));

                // Applica envelope (attack/release)
                if (i < attackSamples)
                {
                    sample *= (float)i / attackSamples;
                }
                else if (i > numSamples - releaseSamples)
                {
                    sample *= (float)(numSamples - i) / releaseSamples;
                }

                samples.Add(sample);
            }
        }

        /// <summary>
        /// Aggiunge silenzio con opzionale rumore di fondo
        /// </summary>
        private void AddSilence(List<float> samples, double durationMs, double noiseLevel, Random random)
        {
            int numSamples = (int)(sampleRate * durationMs / 1000.0);

            for (int i = 0; i < numSamples; i++)
            {
                // Rumore bianco gaussiano
                float noise = 0;
                if (noiseLevel > 0)
                {
                    double u1 = random.NextDouble();
                    double u2 = random.NextDouble();
                    noise = (float)(noiseLevel * Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2));
                }

                samples.Add(noise);
            }
        }

        /// <summary>
        /// Genera segnali di test predefiniti
        /// </summary>
        public static class TestSignals
        {
            public static string SOS = "SOS";
            public static string CQ = "CQ CQ DE";
            public static string TEST = "TEST";
            public static string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            public static string Numbers = "0123456789";
            public static string QuickBrownFox = "THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG";
            public static string HelloWorld = "HELLO WORLD";
            public static string SeventyThree = "73"; // Ham radio "best regards"
        }

        /// <summary>
        /// Crea un segnale con caratteristiche specifiche per il testing
        /// </summary>
        public float[] GenerateTestSignal(TestScenario scenario)
        {
            return scenario switch
            {
                TestScenario.CleanSignal => GenerateSignal(TestSignals.TEST, noiseLevel: 0.0, signalStrength: 0.8),
                TestScenario.NoisySignal => GenerateSignal(TestSignals.TEST, noiseLevel: 0.2, signalStrength: 0.8),
                TestScenario.WeakSignal => GenerateSignal(TestSignals.TEST, noiseLevel: 0.1, signalStrength: 0.3),
                TestScenario.StrongSignal => GenerateSignal(TestSignals.TEST, noiseLevel: 0.05, signalStrength: 1.0),
                TestScenario.SlowSpeed => new MorseSignalGenerator(wpm: 10, frequency: frequency).GenerateSignal(TestSignals.TEST),
                TestScenario.FastSpeed => new MorseSignalGenerator(wpm: 40, frequency: frequency).GenerateSignal(TestSignals.TEST),
                TestScenario.LowFrequency => new MorseSignalGenerator(wpm: wpm, frequency: 400).GenerateSignal(TestSignals.TEST),
                TestScenario.HighFrequency => new MorseSignalGenerator(wpm: wpm, frequency: 1200).GenerateSignal(TestSignals.TEST),
                _ => GenerateSignal(TestSignals.TEST)
            };
        }

        /// <summary>
        /// Analizza il segnale generato e restituisce statistiche
        /// </summary>
        public SignalStatistics AnalyzeSignal(float[] signal)
        {
            if (signal.Length == 0)
            {
                return new SignalStatistics();
            }

            float peak = signal.Max(Math.Abs);
            float rms = (float)Math.Sqrt(signal.Sum(s => s * s) / signal.Length);
            double duration = signal.Length / (double)sampleRate;

            return new SignalStatistics
            {
                Duration = duration,
                PeakAmplitude = peak,
                RMSLevel = rms,
                SampleCount = signal.Length,
                SampleRate = sampleRate
            };
        }

        /// <summary>
        /// Converte testo in rappresentazione visiva Morse
        /// </summary>
        public static string TextToMorseString(string text)
        {
            var morse = new List<string>();

            foreach (char c in text.ToUpper())
            {
                if (c == ' ')
                {
                    morse.Add(" ");
                }
                else if (CharToMorse.TryGetValue(c, out string? code))
                {
                    morse.Add(code);
                }
            }

            return string.Join(" ", morse);
        }
    }

    /// <summary>
    /// Scenari di test predefiniti
    /// </summary>
    public enum TestScenario
    {
        CleanSignal,
        NoisySignal,
        WeakSignal,
        StrongSignal,
        SlowSpeed,
        FastSpeed,
        LowFrequency,
        HighFrequency
    }

    /// <summary>
    /// Statistiche del segnale generato
    /// </summary>
    public class SignalStatistics
    {
        public double Duration { get; set; }
        public float PeakAmplitude { get; set; }
        public float RMSLevel { get; set; }
        public int SampleCount { get; set; }
        public int SampleRate { get; set; }

        public override string ToString()
        {
            return $"Duration: {Duration:F2}s, Peak: {PeakAmplitude:F3}, RMS: {RMSLevel:F3}, Samples: {SampleCount}";
        }
    }
}
