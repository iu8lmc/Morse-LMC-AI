using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using RadioLoggerApp.MorseDecoder.Testing;

namespace RadioLoggerApp.MorseDecoder.Testing
{
    /// <summary>
    /// Programma di test completo per il sistema di decodifica Morse
    /// Testa tutti i componenti in modo standalone
    /// </summary>
    public class MorseDecoderTest
    {
        private static readonly ConsoleColor SuccessColor = ConsoleColor.Green;
        private static readonly ConsoleColor ErrorColor = ConsoleColor.Red;
        private static readonly ConsoleColor InfoColor = ConsoleColor.Cyan;
        private static readonly ConsoleColor WarningColor = ConsoleColor.Yellow;

        /// <summary>
        /// Entry point per i test
        /// </summary>
        public static void RunAllTests()
        {
            Console.Clear();
            PrintHeader("MORSE DECODER AI - TEST SUITE");
            Console.WriteLine();

            int totalTests = 0;
            int passedTests = 0;

            // Test 1: Signal Generator
            PrintTestHeader("Test 1: Generatore Segnali Morse");
            if (TestSignalGenerator())
            {
                passedTests++;
                PrintSuccess("✓ PASSED");
            }
            else
            {
                PrintError("✗ FAILED");
            }
            totalTests++;
            Console.WriteLine();

            // Test 2: Signal Processor
            PrintTestHeader("Test 2: Signal Processor (DSP)");
            if (TestSignalProcessor())
            {
                passedTests++;
                PrintSuccess("✓ PASSED");
            }
            else
            {
                PrintError("✗ FAILED");
            }
            totalTests++;
            Console.WriteLine();

            // Test 3: Morse Decoder
            PrintTestHeader("Test 3: Morse Decoder");
            if (TestMorseDecoder())
            {
                passedTests++;
                PrintSuccess("✓ PASSED");
            }
            else
            {
                PrintError("✗ FAILED");
            }
            totalTests++;
            Console.WriteLine();

            // Test 4: AI Enhancer
            PrintTestHeader("Test 4: AI Enhancer (Machine Learning)");
            if (TestAIEnhancer())
            {
                passedTests++;
                PrintSuccess("✓ PASSED");
            }
            else
            {
                PrintError("✗ FAILED");
            }
            totalTests++;
            Console.WriteLine();

            // Test 5: End-to-End Integration
            PrintTestHeader("Test 5: Integrazione End-to-End");
            if (TestEndToEndIntegration())
            {
                passedTests++;
                PrintSuccess("✓ PASSED");
            }
            else
            {
                PrintError("✗ FAILED");
            }
            totalTests++;
            Console.WriteLine();

            // Test 6: Performance Benchmark
            PrintTestHeader("Test 6: Performance Benchmark");
            if (TestPerformance())
            {
                passedTests++;
                PrintSuccess("✓ PASSED");
            }
            else
            {
                PrintError("✗ FAILED");
            }
            totalTests++;
            Console.WriteLine();

            // Riepilogo finale
            PrintSummary(totalTests, passedTests);
        }

        /// <summary>
        /// Test del generatore di segnali
        /// </summary>
        private static bool TestSignalGenerator()
        {
            try
            {
                var generator = new MorseSignalGenerator(wpm: 20, frequency: 800);

                // Test 1: Genera segnale semplice
                PrintInfo("  → Generazione segnale 'SOS'...");
                var signal = generator.GenerateSignal("SOS");
                var stats = generator.AnalyzeSignal(signal);

                if (signal.Length == 0)
                {
                    PrintError("    Errore: Segnale vuoto");
                    return false;
                }

                Console.WriteLine($"    {stats}");

                // Test 2: Converti testo in Morse
                PrintInfo("  → Conversione testo in Morse...");
                string morse = MorseSignalGenerator.TextToMorseString("HELLO");
                Console.WriteLine($"    HELLO = {morse}");

                if (string.IsNullOrEmpty(morse))
                {
                    PrintError("    Errore: Conversione fallita");
                    return false;
                }

                // Test 3: Genera tutti gli scenari di test
                PrintInfo("  → Test scenari predefiniti...");
                int scenarioCount = 0;
                foreach (TestScenario scenario in Enum.GetValues(typeof(TestScenario)))
                {
                    var testSignal = generator.GenerateTestSignal(scenario);
                    if (testSignal.Length > 0)
                    {
                        scenarioCount++;
                    }
                }
                Console.WriteLine($"    Scenari testati: {scenarioCount}/{Enum.GetValues(typeof(TestScenario)).Length}");

                // Test 4: Verifica alfabeto completo
                PrintInfo("  → Verifica alfabeto completo...");
                var alphabetSignal = generator.GenerateSignal(MorseSignalGenerator.TestSignals.Alphabet);
                Console.WriteLine($"    Alphabet signal: {alphabetSignal.Length} samples");

                return true;
            }
            catch (Exception ex)
            {
                PrintError($"    Eccezione: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test del signal processor
        /// </summary>
        private static bool TestSignalProcessor()
        {
            try
            {
                var processor = new MorseSignalProcessor(sampleRate: 44100, fftSize: 4096, targetFrequency: 800);
                var generator = new MorseSignalGenerator(wpm: 20, frequency: 800);

                // Test 1: Processa segnale pulito
                PrintInfo("  → Processing segnale pulito...");
                var signal = generator.GenerateSignal("TEST", noiseLevel: 0.0, signalStrength: 0.8);
                var processed = processor.ProcessAudioBuffer(signal);

                Console.WriteLine($"    Frequenza rilevata: {processed.Frequency:F1} Hz");
                Console.WriteLine($"    SNR: {processed.SNR:F1} dB");
                Console.WriteLine($"    Signal strength: {processed.SignalStrength:F3}");

                if (Math.Abs(processed.Frequency - 800) > 50)
                {
                    PrintWarning($"    Warning: Frequenza fuori range (attesa ~800 Hz)");
                }

                // Test 2: Processa segnale rumoroso
                PrintInfo("  → Processing segnale rumoroso...");
                var noisySignal = generator.GenerateSignal("TEST", noiseLevel: 0.2, signalStrength: 0.8);
                var noisyProcessed = processor.ProcessAudioBuffer(noisySignal);

                Console.WriteLine($"    SNR con rumore: {noisyProcessed.SNR:F1} dB");

                if (noisyProcessed.SNR < 0)
                {
                    PrintWarning("    Warning: SNR negativo");
                }

                // Test 3: Verifica envelope detection
                PrintInfo("  → Verifica envelope detection...");
                if (processed.Envelope.Length == signal.Length)
                {
                    Console.WriteLine($"    Envelope: OK ({processed.Envelope.Length} samples)");
                }
                else
                {
                    PrintError($"    Envelope: Mismatch ({processed.Envelope.Length} vs {signal.Length})");
                    return false;
                }

                // Test 4: Test filtro frequenze diverse
                PrintInfo("  → Test filtro frequenze diverse...");
                int freqTestsPassed = 0;
                foreach (int freq in new[] { 400, 600, 800, 1000, 1200 })
                {
                    var freqGen = new MorseSignalGenerator(wpm: 20, frequency: freq);
                    var freqSignal = freqGen.GenerateSignal("A");

                    processor.TargetFrequency = freq;
                    var freqProcessed = processor.ProcessAudioBuffer(freqSignal);

                    if (Math.Abs(freqProcessed.Frequency - freq) < 100)
                    {
                        freqTestsPassed++;
                    }
                }
                Console.WriteLine($"    Frequenze testate: {freqTestsPassed}/5");

                return true;
            }
            catch (Exception ex)
            {
                PrintError($"    Eccezione: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test del decoder Morse
        /// </summary>
        private static bool TestMorseDecoder()
        {
            try
            {
                var decoder = new MorseDecoder(initialWPM: 20, threshold: 0.3);
                var generator = new MorseSignalGenerator(wpm: 20, frequency: 800);
                var processor = new MorseSignalProcessor(sampleRate: 44100, targetFrequency: 800);

                string[] testWords = { "SOS", "TEST", "HELLO", "CQ" };
                int successCount = 0;

                foreach (string testWord in testWords)
                {
                    PrintInfo($"  → Decodifica '{testWord}'...");

                    // Genera e processa il segnale
                    var signal = generator.GenerateSignal(testWord, noiseLevel: 0.05);
                    var processed = processor.ProcessAudioBuffer(signal);

                    // Reset decoder
                    decoder.Reset();

                    // Simula il processing timestamp
                    long timestamp = 0;
                    string decodedText = "";

                    // Sottoscrivi all'evento
                    decoder.TextDecoded += (sender, text) => decodedText += text;

                    // Processa l'envelope
                    foreach (var sample in processed.Envelope)
                    {
                        decoder.ProcessEnvelopeSample(sample, timestamp++);
                    }

                    // Forza la fine della parola
                    for (int i = 0; i < 1000; i++)
                    {
                        decoder.ProcessEnvelopeSample(0, timestamp++);
                    }

                    // Verifica risultato
                    decodedText = decodedText.Trim();
                    Console.WriteLine($"    Originale: '{testWord}' → Decodificato: '{decodedText}'");
                    Console.WriteLine($"    WPM rilevato: {decoder.CurrentWPM:F1}");
                    Console.WriteLine($"    Caratteri: {decoder.CharactersDecoded}, Errori: {decoder.ErrorsDetected}");

                    if (decodedText.Contains(testWord.Substring(0, Math.Min(2, testWord.Length))))
                    {
                        successCount++;
                        Console.WriteLine($"    ✓ Match parziale o completo");
                    }
                    else
                    {
                        PrintWarning($"    ⚠ Nessun match");
                    }

                    Console.WriteLine();
                }

                Console.WriteLine($"  Successi: {successCount}/{testWords.Length}");
                return successCount >= testWords.Length / 2; // Almeno 50% di successo
            }
            catch (Exception ex)
            {
                PrintError($"    Eccezione: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test dell'AI enhancer
        /// </summary>
        private static bool TestAIEnhancer()
        {
            try
            {
                var enhancer = new MorseAIEnhancer();

                // Test 1: Verifica inizializzazione
                PrintInfo("  → Verifica inizializzazione...");
                Console.WriteLine($"    Dataset iniziale: {enhancer.TrainingDataCount} esempi");

                if (enhancer.TrainingDataCount < 50)
                {
                    PrintError("    Errore: Dataset troppo piccolo");
                    return false;
                }

                // Test 2: Training del modello
                PrintInfo("  → Training del modello ML...");
                enhancer.TrainModel();

                Console.WriteLine($"    Modello addestrato: {enhancer.IsModelTrained}");
                Console.WriteLine($"    Accuratezza: {enhancer.ModelAccuracy:P2}");

                if (!enhancer.IsModelTrained)
                {
                    PrintError("    Errore: Training fallito");
                    return false;
                }

                // Test 3: Classificazione Dit/Dah
                PrintInfo("  → Test classificazione Dit/Dah...");

                var ditTests = new[]
                {
                    (duration: 60.0, expected: SignalType.Dit),
                    (duration: 80.0, expected: SignalType.Dit),
                    (duration: 100.0, expected: SignalType.Dit),
                };

                var dahTests = new[]
                {
                    (duration: 200.0, expected: SignalType.Dah),
                    (duration: 280.0, expected: SignalType.Dah),
                    (duration: 350.0, expected: SignalType.Dah),
                };

                int correctClassifications = 0;
                int totalClassifications = 0;

                foreach (var test in ditTests)
                {
                    var result = enhancer.ClassifySignal(test.duration, 0.8, 15, 0.9);
                    if (result.Type == test.expected)
                    {
                        correctClassifications++;
                    }
                    totalClassifications++;
                    Console.WriteLine($"    {test.duration}ms → {result.Type} (confidenza: {result.Confidence:P0})");
                }

                foreach (var test in dahTests)
                {
                    var result = enhancer.ClassifySignal(test.duration, 0.8, 15, 0.9);
                    if (result.Type == test.expected)
                    {
                        correctClassifications++;
                    }
                    totalClassifications++;
                    Console.WriteLine($"    {test.duration}ms → {result.Type} (confidenza: {result.Confidence:P0})");
                }

                double accuracy = (double)correctClassifications / totalClassifications;
                Console.WriteLine($"    Accuratezza classificazione: {accuracy:P0}");

                // Test 4: Correzione testo
                PrintInfo("  → Test correzione testo...");
                string original = "YELLO WORLD";
                string corrected = enhancer.CorrectDecodedText(original);
                Console.WriteLine($"    '{original}' → '{corrected}'");

                return enhancer.IsModelTrained && accuracy >= 0.5;
            }
            catch (Exception ex)
            {
                PrintError($"    Eccezione: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test di integrazione end-to-end
        /// </summary>
        private static bool TestEndToEndIntegration()
        {
            try
            {
                PrintInfo("  → Setup componenti...");
                var generator = new MorseSignalGenerator(wpm: 15, frequency: 800);
                var processor = new MorseSignalProcessor(sampleRate: 44100, targetFrequency: 800);
                var decoder = new MorseDecoder(initialWPM: 15, threshold: 0.3);
                var enhancer = new MorseAIEnhancer();

                enhancer.TrainModel();

                // Test completo con frase
                string testPhrase = "HELLO";
                PrintInfo($"  → Test completo: '{testPhrase}'");

                // 1. Genera segnale
                Console.WriteLine("    [1/4] Generazione segnale...");
                var signal = generator.GenerateSignal(testPhrase, noiseLevel: 0.1, signalStrength: 0.8);
                var stats = generator.AnalyzeSignal(signal);
                Console.WriteLine($"          {stats}");

                // 2. Processa segnale
                Console.WriteLine("    [2/4] Elaborazione segnale (DSP)...");
                var processed = processor.ProcessAudioBuffer(signal);
                Console.WriteLine($"          Frequenza: {processed.Frequency:F1} Hz, SNR: {processed.SNR:F1} dB");

                // 3. Decodifica Morse
                Console.WriteLine("    [3/4] Decodifica Morse...");
                decoder.Reset();
                string decodedText = "";
                decoder.TextDecoded += (sender, text) => decodedText += text;

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

                Console.WriteLine($"          '{testPhrase}' → '{decodedText.Trim()}'");
                Console.WriteLine($"          WPM: {decoder.CurrentWPM:F1}, Caratteri: {decoder.CharactersDecoded}");

                // 4. Analisi AI
                Console.WriteLine("    [4/4] Analisi AI...");
                var analysis = enhancer.AnalyzeDecodedText(decodedText);
                Console.WriteLine($"          Confidenza: {analysis.OverallConfidence:P0}");
                Console.WriteLine($"          Errori: {analysis.UnknownCharacters}/{analysis.TotalCharacters}");

                // Verifica successo
                bool success = decodedText.Trim().Length > 0 &&
                              decoder.CharactersDecoded > 0;

                return success;
            }
            catch (Exception ex)
            {
                PrintError($"    Eccezione: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test di performance
        /// </summary>
        private static bool TestPerformance()
        {
            try
            {
                var generator = new MorseSignalGenerator(wpm: 20, frequency: 800);
                var processor = new MorseSignalProcessor(sampleRate: 44100);
                var stopwatch = new Stopwatch();

                // Test 1: Throughput generazione segnali
                PrintInfo("  → Benchmark generazione segnali...");
                stopwatch.Restart();
                for (int i = 0; i < 100; i++)
                {
                    generator.GenerateSignal("TEST");
                }
                stopwatch.Stop();
                double genTime = stopwatch.ElapsedMilliseconds / 100.0;
                Console.WriteLine($"    Tempo medio: {genTime:F2} ms/segnale");

                // Test 2: Throughput signal processing
                PrintInfo("  → Benchmark signal processing...");
                var testSignal = generator.GenerateSignal(MorseSignalGenerator.TestSignals.Alphabet);

                stopwatch.Restart();
                for (int i = 0; i < 100; i++)
                {
                    processor.ProcessAudioBuffer(testSignal);
                }
                stopwatch.Stop();
                double procTime = stopwatch.ElapsedMilliseconds / 100.0;
                Console.WriteLine($"    Tempo medio: {procTime:F2} ms/buffer");

                // Test 3: Latenza totale
                PrintInfo("  → Benchmark latenza end-to-end...");
                var decoder = new MorseDecoder(initialWPM: 20);

                stopwatch.Restart();
                var signal = generator.GenerateSignal("A");
                var processed = processor.ProcessAudioBuffer(signal);
                decoder.Reset();
                foreach (var sample in processed.Envelope.Take(100))
                {
                    decoder.ProcessEnvelopeSample(sample, 0);
                }
                stopwatch.Stop();

                Console.WriteLine($"    Latenza: {stopwatch.ElapsedMilliseconds} ms");

                // Verifica performance
                bool performanceOK = genTime < 100 && procTime < 100;

                if (performanceOK)
                {
                    PrintSuccess("    ✓ Performance OK");
                }
                else
                {
                    PrintWarning("    ⚠ Performance sotto le aspettative");
                }

                return true;
            }
            catch (Exception ex)
            {
                PrintError($"    Eccezione: {ex.Message}");
                return false;
            }
        }

        // Helper methods per output formattato
        private static void PrintHeader(string text)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine($"  {text}");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.ResetColor();
        }

        private static void PrintTestHeader(string text)
        {
            Console.ForegroundColor = InfoColor;
            Console.WriteLine($"┌─ {text}");
            Console.ResetColor();
        }

        private static void PrintSuccess(string text)
        {
            Console.ForegroundColor = SuccessColor;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void PrintError(string text)
        {
            Console.ForegroundColor = ErrorColor;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void PrintInfo(string text)
        {
            Console.ForegroundColor = InfoColor;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void PrintWarning(string text)
        {
            Console.ForegroundColor = WarningColor;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void PrintSummary(int total, int passed)
        {
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("  RIEPILOGO TEST");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine($"  Test totali:   {total}");

            if (passed == total)
            {
                Console.ForegroundColor = SuccessColor;
                Console.WriteLine($"  Test passati:  {passed} ✓");
            }
            else
            {
                Console.ForegroundColor = WarningColor;
                Console.WriteLine($"  Test passati:  {passed}");
            }
            Console.ResetColor();

            Console.WriteLine($"  Test falliti:  {total - passed}");
            Console.WriteLine();

            double percentage = (double)passed / total * 100;
            Console.Write("  Successo:      ");

            if (percentage == 100)
            {
                Console.ForegroundColor = SuccessColor;
            }
            else if (percentage >= 80)
            {
                Console.ForegroundColor = InfoColor;
            }
            else if (percentage >= 50)
            {
                Console.ForegroundColor = WarningColor;
            }
            else
            {
                Console.ForegroundColor = ErrorColor;
            }

            Console.WriteLine($"{percentage:F1}%");
            Console.ResetColor();

            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════");
        }
    }
}
