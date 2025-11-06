using System;
using System.IO;
using System.Linq;
using RadioLoggerApp.MorseDecoder.Testing;

namespace RadioLoggerApp.MorseDecoder.Examples
{
    /// <summary>
    /// Esempi pratici di utilizzo del sistema di decodifica Morse
    /// Questi esempi mostrano come usare i vari componenti
    /// </summary>
    public class MorseDecoderExamples
    {
        /// <summary>
        /// Esempio 1: Generare e analizzare un segnale Morse semplice
        /// </summary>
        public static void Example01_GenerateSimpleSignal()
        {
            Console.WriteLine("═══ ESEMPIO 1: Generare un Segnale Morse Semplice ═══\n");

            // Crea un generatore con 20 WPM, 800 Hz
            var generator = new MorseSignalGenerator(wpm: 20, frequency: 800);

            // Genera il segnale per "SOS"
            Console.WriteLine("Generazione segnale per 'SOS'...");
            var signal = generator.GenerateSignal("SOS");

            // Analizza il segnale
            var stats = generator.AnalyzeSignal(signal);
            Console.WriteLine($"\nStatistiche segnale:");
            Console.WriteLine($"  - Durata: {stats.Duration:F2} secondi");
            Console.WriteLine($"  - Campioni: {stats.SampleCount}");
            Console.WriteLine($"  - Peak amplitude: {stats.PeakAmplitude:F3}");
            Console.WriteLine($"  - RMS level: {stats.RMSLevel:F3}");

            // Mostra rappresentazione Morse
            string morseString = MorseSignalGenerator.TextToMorseString("SOS");
            Console.WriteLine($"\nRappresentazione Morse: {morseString}");
            Console.WriteLine("(... = S, --- = O, ... = S)\n");
        }

        /// <summary>
        /// Esempio 2: Testare diversi scenari di segnale
        /// </summary>
        public static void Example02_TestDifferentScenarios()
        {
            Console.WriteLine("═══ ESEMPIO 2: Test Scenari Diversi ═══\n");

            var generator = new MorseSignalGenerator(wpm: 20, frequency: 800);

            var scenarios = new[]
            {
                (TestScenario.CleanSignal, "Segnale pulito"),
                (TestScenario.NoisySignal, "Segnale rumoroso"),
                (TestScenario.WeakSignal, "Segnale debole"),
                (TestScenario.SlowSpeed, "Velocità lenta (10 WPM)"),
                (TestScenario.FastSpeed, "Velocità veloce (40 WPM)")
            };

            foreach (var (scenario, description) in scenarios)
            {
                Console.WriteLine($"Test: {description}");
                var signal = generator.GenerateTestSignal(scenario);
                var stats = generator.AnalyzeSignal(signal);
                Console.WriteLine($"  Durata: {stats.Duration:F2}s, Peak: {stats.PeakAmplitude:F3}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Esempio 3: Signal Processing con DSP
        /// </summary>
        public static void Example03_SignalProcessing()
        {
            Console.WriteLine("═══ ESEMPIO 3: Elaborazione del Segnale (DSP) ═══\n");

            // Setup
            var generator = new MorseSignalGenerator(wpm: 20, frequency: 800);
            var processor = new MorseSignalProcessor(sampleRate: 44100, targetFrequency: 800);

            // Genera segnale
            Console.WriteLine("Generazione e processing di 'TEST'...\n");
            var signal = generator.GenerateSignal("TEST", noiseLevel: 0.1, signalStrength: 0.8);

            // Processa il segnale
            var processed = processor.ProcessAudioBuffer(signal);

            // Mostra risultati
            Console.WriteLine("Risultati del processing:");
            Console.WriteLine($"  - Frequenza rilevata: {processed.Frequency:F1} Hz (target: 800 Hz)");
            Console.WriteLine($"  - Signal strength: {processed.SignalStrength:F3}");
            Console.WriteLine($"  - Noise floor: {processed.NoiseFloor:F4}");
            Console.WriteLine($"  - SNR: {processed.SNR:F1} dB");
            Console.WriteLine($"  - Envelope samples: {processed.Envelope.Length}");
            Console.WriteLine($"  - Filtered samples: {processed.FilteredSamples.Length}\n");

            // Test con frequenze diverse
            Console.WriteLine("Test auto-tuning frequenza:");
            foreach (int freq in new[] { 400, 600, 800, 1000, 1200 })
            {
                var freqGen = new MorseSignalGenerator(wpm: 20, frequency: freq);
                var freqSignal = freqGen.GenerateSignal("A");

                processor.TargetFrequency = freq;
                var freqProcessed = processor.ProcessAudioBuffer(freqSignal);

                Console.WriteLine($"  {freq} Hz → Rilevata: {freqProcessed.Frequency:F0} Hz (diff: {Math.Abs(freqProcessed.Frequency - freq):F0} Hz)");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Esempio 4: Decodifica completa di un messaggio
        /// </summary>
        public static void Example04_DecodeMorseMessage()
        {
            Console.WriteLine("═══ ESEMPIO 4: Decodifica Messaggio Completo ═══\n");

            // Setup componenti
            var generator = new MorseSignalGenerator(wpm: 20, frequency: 800);
            var processor = new MorseSignalProcessor(sampleRate: 44100, targetFrequency: 800);
            var decoder = new MorseDecoder(initialWPM: 20, threshold: 0.3);

            // Messaggi di test
            string[] messages = { "SOS", "CQ CQ", "HELLO WORLD", "TEST 123" };

            foreach (string message in messages)
            {
                Console.WriteLine($"Decodifica: '{message}'");

                // 1. Genera segnale
                var signal = generator.GenerateSignal(message, noiseLevel: 0.05);

                // 2. Processa segnale
                var processed = processor.ProcessAudioBuffer(signal);

                // 3. Decodifica
                decoder.Reset();
                string decodedText = "";

                decoder.CharacterDecoded += (s, e) =>
                {
                    Console.Write($"  [{e.Character}]");
                };

                decoder.TextDecoded += (s, text) =>
                {
                    decodedText += text;
                };

                // Processa envelope
                long timestamp = 0;
                foreach (var sample in processed.Envelope)
                {
                    decoder.ProcessEnvelopeSample(sample, timestamp++);
                }

                // Forza terminazione
                for (int i = 0; i < 2000; i++)
                {
                    decoder.ProcessEnvelopeSample(0, timestamp++);
                }

                Console.WriteLine();
                Console.WriteLine($"  Originale:    '{message}'");
                Console.WriteLine($"  Decodificato: '{decodedText.Trim()}'");
                Console.WriteLine($"  WPM: {decoder.CurrentWPM:F1}, Caratteri: {decoder.CharactersDecoded}, Errori: {decoder.ErrorsDetected}");
                Console.WriteLine($"  Confidenza: {decoder.Confidence:P0}\n");
            }
        }

        /// <summary>
        /// Esempio 5: Usare l'AI per migliorare la decodifica
        /// </summary>
        public static void Example05_UseAIEnhancement()
        {
            Console.WriteLine("═══ ESEMPIO 5: AI Enhancement ═══\n");

            var enhancer = new MorseAIEnhancer();

            // Training iniziale
            Console.WriteLine("Training del modello ML...");
            enhancer.TrainModel();
            Console.WriteLine($"  Modello addestrato: {enhancer.IsModelTrained}");
            Console.WriteLine($"  Accuratezza: {enhancer.ModelAccuracy:P2}");
            Console.WriteLine($"  Dataset size: {enhancer.TrainingDataCount} esempi\n");

            // Test classificazione Dit/Dah
            Console.WriteLine("Classificazione Dit/Dah:");
            var tests = new[]
            {
                (duration: 60.0, label: "Dit corto"),
                (duration: 100.0, label: "Dit lungo"),
                (duration: 200.0, label: "Dah corto"),
                (duration: 300.0, label: "Dah lungo")
            };

            foreach (var (duration, label) in tests)
            {
                var result = enhancer.ClassifySignal(duration, 0.8, 15, 0.9);
                Console.WriteLine($"  {label} ({duration}ms):");
                Console.WriteLine($"    → {result.Type}, Confidenza: {result.Confidence:P0}");
                Console.WriteLine($"    → Probabilità: Dit={result.Probability[0]:P0}, Dah={result.Probability[1]:P0}");
            }

            Console.WriteLine();

            // Test correzione testo
            Console.WriteLine("Correzione errori:");
            string[] testTexts = { "YELLO WORLD", "TLST", "CQ CQ CQ" };

            foreach (string text in testTexts)
            {
                string corrected = enhancer.CorrectDecodedText(text);
                Console.WriteLine($"  '{text}' → '{corrected}'");
            }

            Console.WriteLine();

            // Analisi testo
            Console.WriteLine("Analisi testo decodificato:");
            string sampleText = "CQ CQ DE IU8LMC TEST 73";
            var analysis = enhancer.AnalyzeDecodedText(sampleText);

            Console.WriteLine($"  Testo: '{analysis.Text}'");
            Console.WriteLine($"  Caratteri totali: {analysis.TotalCharacters}");
            Console.WriteLine($"  Caratteri sconosciuti: {analysis.UnknownCharacters}");
            Console.WriteLine($"  Frasi comuni: {analysis.HasCommonPhrases}");
            Console.WriteLine($"  Confidenza complessiva: {analysis.OverallConfidence:P0}\n");
        }

        /// <summary>
        /// Esempio 6: Pipeline completa End-to-End
        /// </summary>
        public static void Example06_EndToEndPipeline()
        {
            Console.WriteLine("═══ ESEMPIO 6: Pipeline Completa End-to-End ═══\n");

            // Inizializzazione completa
            Console.WriteLine("Inizializzazione componenti...");
            var generator = new MorseSignalGenerator(wpm: 15, frequency: 800);
            var processor = new MorseSignalProcessor(sampleRate: 44100, targetFrequency: 800);
            var decoder = new MorseDecoder(initialWPM: 15, threshold: 0.3);
            var enhancer = new MorseAIEnhancer();

            enhancer.TrainModel();
            Console.WriteLine("✓ Componenti inizializzati\n");

            // Messaggio da processare
            string message = "HELLO WORLD";
            Console.WriteLine($"Messaggio originale: '{message}'\n");

            // Step 1: Generazione segnale
            Console.WriteLine("[1/5] Generazione segnale audio...");
            var signal = generator.GenerateSignal(message, noiseLevel: 0.1, signalStrength: 0.8);
            var signalStats = generator.AnalyzeSignal(signal);
            Console.WriteLine($"      {signalStats}");

            // Step 2: Signal Processing
            Console.WriteLine("\n[2/5] Elaborazione segnale (FFT, filtri, AGC)...");
            var processed = processor.ProcessAudioBuffer(signal);
            Console.WriteLine($"      Freq: {processed.Frequency:F1} Hz");
            Console.WriteLine($"      SNR: {processed.SNR:F1} dB");
            Console.WriteLine($"      Envelope: {processed.Envelope.Length} samples");

            // Step 3: Decodifica Morse
            Console.WriteLine("\n[3/5] Decodifica codice Morse...");
            decoder.Reset();
            string decodedText = "";
            int charCount = 0;

            decoder.CharacterDecoded += (s, e) =>
            {
                charCount++;
                if (charCount % 5 == 1)
                    Console.Write("      ");
                Console.Write($"[{e.Character}:{e.Confidence:P0}] ");
                if (charCount % 5 == 0)
                    Console.WriteLine();
            };

            decoder.TextDecoded += (s, text) => decodedText += text;

            long timestamp = 0;
            foreach (var sample in processed.Envelope)
            {
                decoder.ProcessEnvelopeSample(sample, timestamp++);
            }

            // Termina decodifica
            for (int i = 0; i < 2000; i++)
            {
                decoder.ProcessEnvelopeSample(0, timestamp++);
            }

            Console.WriteLine();
            Console.WriteLine($"      Decodificato: '{decodedText.Trim()}'");
            Console.WriteLine($"      WPM: {decoder.CurrentWPM:F1}");

            // Step 4: AI Enhancement
            Console.WriteLine("\n[4/5] Miglioramento AI...");
            var corrected = enhancer.CorrectDecodedText(decodedText);
            var analysis = enhancer.AnalyzeDecodedText(corrected);
            Console.WriteLine($"      Testo corretto: '{corrected.Trim()}'");
            Console.WriteLine($"      Confidenza AI: {analysis.OverallConfidence:P0}");

            // Step 5: Risultati finali
            Console.WriteLine("\n[5/5] Risultati finali:");
            Console.WriteLine($"      Originale:     '{message}'");
            Console.WriteLine($"      Decodificato:  '{decodedText.Trim()}'");
            Console.WriteLine($"      Corretto:      '{corrected.Trim()}'");
            Console.WriteLine($"      Accuratezza:   {(1.0 - (double)decoder.ErrorsDetected / Math.Max(1, decoder.CharactersDecoded)):P0}");
            Console.WriteLine($"      Calibrato:     {decoder.IsCalibrated}");
            Console.WriteLine();
        }

        /// <summary>
        /// Esempio 7: Salvare segnali per test offline
        /// </summary>
        public static void Example07_SaveSignalsToFile()
        {
            Console.WriteLine("═══ ESEMPIO 7: Salvare Segnali per Test ═══\n");

            var generator = new MorseSignalGenerator(wpm: 20, frequency: 800);

            // Directory per i test
            string testDir = "MorseTestSignals";
            if (!Directory.Exists(testDir))
            {
                Directory.CreateDirectory(testDir);
            }

            Console.WriteLine($"Creazione segnali di test in '{testDir}'...\n");

            // Genera vari segnali di test
            var testCases = new[]
            {
                ("SOS", TestScenario.CleanSignal, "sos_clean.dat"),
                ("TEST", TestScenario.NoisySignal, "test_noisy.dat"),
                ("HELLO", TestScenario.SlowSpeed, "hello_slow.dat"),
                ("CQ CQ", TestScenario.FastSpeed, "cq_fast.dat"),
                ("73", TestScenario.WeakSignal, "73_weak.dat")
            };

            foreach (var (text, scenario, filename) in testCases)
            {
                // Genera segnale appropriato
                float[] signal;
                if (scenario == TestScenario.CleanSignal)
                {
                    signal = generator.GenerateSignal(text, noiseLevel: 0.0, signalStrength: 0.8);
                }
                else
                {
                    signal = generator.GenerateTestSignal(scenario);
                }

                // Salva su file (formato binario semplice)
                string filePath = Path.Combine(testDir, filename);
                using (var writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
                {
                    foreach (float sample in signal)
                    {
                        writer.Write(sample);
                    }
                }

                var stats = generator.AnalyzeSignal(signal);
                Console.WriteLine($"✓ {filename}");
                Console.WriteLine($"    Text: '{text}', Scenario: {scenario}");
                Console.WriteLine($"    {stats}");
            }

            Console.WriteLine($"\n✓ Segnali salvati in '{Path.GetFullPath(testDir)}'\n");
        }

        /// <summary>
        /// Entry point per eseguire tutti gli esempi
        /// </summary>
        public static void RunAllExamples()
        {
            Console.Clear();
            Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
            Console.WriteLine("║  MORSE DECODER AI - ESEMPI PRATICI                    ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
            Console.WriteLine();

            try
            {
                Example01_GenerateSimpleSignal();
                PauseForUser();

                Example02_TestDifferentScenarios();
                PauseForUser();

                Example03_SignalProcessing();
                PauseForUser();

                Example04_DecodeMorseMessage();
                PauseForUser();

                Example05_UseAIEnhancement();
                PauseForUser();

                Example06_EndToEndPipeline();
                PauseForUser();

                Example07_SaveSignalsToFile();

                Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
                Console.WriteLine("║  ESEMPI COMPLETATI                                     ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nErrore durante l'esecuzione: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
            }
        }

        private static void PauseForUser()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Premi un tasto per continuare...");
            Console.ResetColor();
            Console.ReadKey(true);
            Console.WriteLine();
        }
    }
}
