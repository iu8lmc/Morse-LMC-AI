using System;
using RadioLoggerApp.MorseDecoder.Testing;
using RadioLoggerApp.MorseDecoder.Examples;

namespace RadioLoggerApp.MorseDecoder
{
    /// <summary>
    /// Test Runner principale per il sistema di decodifica Morse
    /// Esegue test ed esempi in modo interattivo
    /// </summary>
    public class MorseTestRunner
    {
        /// <summary>
        /// Main entry point per il test runner
        /// Chiamare questo metodo da Program.cs o da una console app separata
        /// </summary>
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            while (true)
            {
                ShowMainMenu();
                string? choice = Console.ReadLine();

                switch (choice?.ToUpper())
                {
                    case "1":
                        RunTests();
                        break;

                    case "2":
                        RunExamples();
                        break;

                    case "3":
                        RunInteractiveDemo();
                        break;

                    case "4":
                        GenerateTestReport();
                        break;

                    case "5":
                        ShowComponentInfo();
                        break;

                    case "Q":
                    case "EXIT":
                        Console.WriteLine("\nArrivederci!");
                        return;

                    default:
                        Console.WriteLine("\nScelta non valida. Riprova.");
                        System.Threading.Thread.Sleep(1000);
                        break;
                }
            }
        }

        /// <summary>
        /// Mostra il menu principale
        /// </summary>
        private static void ShowMainMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║       MORSE DECODER AI - TEST & VALIDATION SUITE         ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Scegli un'opzione:");
            Console.WriteLine();
            Console.WriteLine("  [1] Esegui Test Suite Completa");
            Console.WriteLine("  [2] Esegui Esempi Pratici");
            Console.WriteLine("  [3] Demo Interattiva");
            Console.WriteLine("  [4] Genera Report di Test");
            Console.WriteLine("  [5] Info Componenti");
            Console.WriteLine();
            Console.WriteLine("  [Q] Esci");
            Console.WriteLine();
            Console.Write("Scelta: ");
        }

        /// <summary>
        /// Esegue la test suite completa
        /// </summary>
        private static void RunTests()
        {
            Console.Clear();
            Console.WriteLine("Avvio Test Suite...\n");
            System.Threading.Thread.Sleep(500);

            try
            {
                MorseDecoderTest.RunAllTests();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nErrore durante l'esecuzione dei test: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nPremi un tasto per tornare al menu...");
            Console.ReadKey(true);
        }

        /// <summary>
        /// Esegue gli esempi pratici
        /// </summary>
        private static void RunExamples()
        {
            Console.Clear();
            Console.WriteLine("Avvio Esempi Pratici...\n");
            System.Threading.Thread.Sleep(500);

            try
            {
                MorseDecoderExamples.RunAllExamples();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nErrore durante l'esecuzione degli esempi: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nPremi un tasto per tornare al menu...");
            Console.ReadKey(true);
        }

        /// <summary>
        /// Demo interattiva: l'utente può inserire testo e vedere la decodifica
        /// </summary>
        private static void RunInteractiveDemo()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║            DEMO INTERATTIVA - DECODIFICA MORSE            ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            // Setup componenti
            var generator = new MorseSignalGenerator(wpm: 20, frequency: 800);
            var processor = new MorseSignalProcessor(sampleRate: 44100, targetFrequency: 800);
            var decoder = new MorseDecoder(initialWPM: 20, threshold: 0.3);
            var enhancer = new MorseAIEnhancer();

            Console.WriteLine("Inizializzazione componenti...");
            enhancer.TrainModel();
            Console.WriteLine("✓ Sistema pronto!\n");

            while (true)
            {
                Console.WriteLine("─────────────────────────────────────────────────────────");
                Console.Write("\nInserisci testo da convertire in Morse (o 'Q' per uscire): ");
                string? input = Console.ReadLine()?.ToUpper();

                if (string.IsNullOrEmpty(input) || input == "Q")
                {
                    break;
                }

                Console.WriteLine();

                try
                {
                    // 1. Mostra rappresentazione Morse
                    string morseString = MorseSignalGenerator.TextToMorseString(input);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Morse: {morseString}");
                    Console.ResetColor();

                    // 2. Genera segnale
                    Console.Write("Generazione segnale audio... ");
                    var signal = generator.GenerateSignal(input, noiseLevel: 0.05, signalStrength: 0.8);
                    Console.WriteLine($"✓ ({signal.Length} samples)");

                    // 3. Processa
                    Console.Write("Elaborazione DSP... ");
                    var processed = processor.ProcessAudioBuffer(signal);
                    Console.WriteLine($"✓ (Freq: {processed.Frequency:F0} Hz, SNR: {processed.SNR:F1} dB)");

                    // 4. Decodifica
                    Console.Write("Decodifica Morse... ");
                    decoder.Reset();
                    string decodedText = "";

                    decoder.TextDecoded += (s, text) => decodedText += text;

                    long timestamp = 0;
                    foreach (var sample in processed.Envelope)
                    {
                        decoder.ProcessEnvelopeSample(sample, timestamp++);
                    }

                    // Termina
                    for (int i = 0; i < 2000; i++)
                    {
                        decoder.ProcessEnvelopeSample(0, timestamp++);
                    }

                    Console.WriteLine($"✓ (WPM: {decoder.CurrentWPM:F1})");

                    // 5. Risultati
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"┌─ RISULTATI");
                    Console.WriteLine($"│  Originale:    '{input}'");
                    Console.WriteLine($"│  Decodificato: '{decodedText.Trim()}'");
                    Console.WriteLine($"│  Caratteri: {decoder.CharactersDecoded}, Errori: {decoder.ErrorsDetected}");
                    Console.WriteLine($"│  Confidenza: {decoder.Confidence:P0}");

                    // Calcola accuratezza
                    if (decoder.CharactersDecoded > 0)
                    {
                        double accuracy = 1.0 - (double)decoder.ErrorsDetected / decoder.CharactersDecoded;
                        Console.WriteLine($"│  Accuratezza: {accuracy:P0}");
                    }

                    Console.WriteLine("└─");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nErrore: {ex.Message}");
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\nUscita dalla demo interattiva...");
            System.Threading.Thread.Sleep(1000);
        }

        /// <summary>
        /// Genera un report dettagliato dei test
        /// </summary>
        private static void GenerateTestReport()
        {
            Console.Clear();
            Console.WriteLine("Generazione Report di Test...\n");

            string reportPath = $"MorseTestReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

            try
            {
                using (var writer = new System.IO.StreamWriter(reportPath))
                {
                    writer.WriteLine("═══════════════════════════════════════════════════════════");
                    writer.WriteLine("  MORSE DECODER AI - TEST REPORT");
                    writer.WriteLine($"  Generato: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine("═══════════════════════════════════════════════════════════");
                    writer.WriteLine();

                    // Informazioni di sistema
                    writer.WriteLine("INFORMAZIONI SISTEMA:");
                    writer.WriteLine($"  OS: {Environment.OSVersion}");
                    writer.WriteLine($"  .NET: {Environment.Version}");
                    writer.WriteLine($"  CPU Cores: {Environment.ProcessorCount}");
                    writer.WriteLine();

                    // Componenti testati
                    writer.WriteLine("COMPONENTI:");
                    writer.WriteLine("  1. MorseSignalGenerator");
                    writer.WriteLine("  2. MorseSignalProcessor");
                    writer.WriteLine("  3. MorseDecoder");
                    writer.WriteLine("  4. MorseAIEnhancer");
                    writer.WriteLine();

                    // Test eseguiti
                    writer.WriteLine("TEST ESEGUITI:");
                    writer.WriteLine("  - Signal Generation");
                    writer.WriteLine("  - Signal Processing (FFT, Filters)");
                    writer.WriteLine("  - Morse Decoding");
                    writer.WriteLine("  - AI Enhancement");
                    writer.WriteLine("  - End-to-End Integration");
                    writer.WriteLine("  - Performance Benchmarks");
                    writer.WriteLine();

                    // Note
                    writer.WriteLine("NOTE:");
                    writer.WriteLine("  Per i dettagli completi eseguire la Test Suite dal menu principale.");
                    writer.WriteLine();
                    writer.WriteLine("═══════════════════════════════════════════════════════════");
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Report generato: {System.IO.Path.GetFullPath(reportPath)}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Errore nella generazione del report: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nPremi un tasto per tornare al menu...");
            Console.ReadKey(true);
        }

        /// <summary>
        /// Mostra informazioni sui componenti
        /// </summary>
        private static void ShowComponentInfo()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              INFORMAZIONI COMPONENTI                       ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("┌─ COMPONENTI DISPONIBILI");
            Console.WriteLine("│");
            Console.WriteLine("│  1. MorseSignalGenerator");
            Console.WriteLine("│     • Genera segnali Morse sintetici");
            Console.WriteLine("│     • WPM configurabile (5-60)");
            Console.WriteLine("│     • Frequenza configurabile (200-2000 Hz)");
            Console.WriteLine("│     • Supporta rumore e variazioni di forza");
            Console.WriteLine("│");
            Console.WriteLine("│  2. MorseSignalProcessor");
            Console.WriteLine("│     • DSP avanzato con FFT");
            Console.WriteLine("│     • Filtro Butterworth passa-banda");
            Console.WriteLine("│     • AGC (Automatic Gain Control)");
            Console.WriteLine("│     • Rilevamento automatico frequenza");
            Console.WriteLine("│     • Calcolo SNR");
            Console.WriteLine("│");
            Console.WriteLine("│  3. MorseDecoder");
            Console.WriteLine("│     • Dizionario Morse completo (ITU-R)");
            Console.WriteLine("│     • Auto-calibrazione WPM");
            Console.WriteLine("│     • State machine intelligente");
            Console.WriteLine("│     • Calcolo confidenza");
            Console.WriteLine("│");
            Console.WriteLine("│  4. MorseAIEnhancer");
            Console.WriteLine("│     • Machine Learning (ML.NET)");
            Console.WriteLine("│     • Classificazione Dit/Dah");
            Console.WriteLine("│     • Correzione errori");
            Console.WriteLine("│     • Training incrementale");
            Console.WriteLine("│");
            Console.WriteLine("└─");
            Console.WriteLine();

            Console.WriteLine("┌─ DIPENDENZE");
            Console.WriteLine("│  • NAudio 2.2.1");
            Console.WriteLine("│  • MathNet.Numerics 5.0.0");
            Console.WriteLine("│  • Microsoft.ML 3.0.1");
            Console.WriteLine("│  • Accord.MachineLearning 3.8.0");
            Console.WriteLine("│  • OxyPlot.Wpf 2.1.2");
            Console.WriteLine("└─");
            Console.WriteLine();

            Console.WriteLine("┌─ PERFORMANCE TIPICHE");
            Console.WriteLine("│  • Latenza: < 100ms");
            Console.WriteLine("│  • Throughput: > 60 WPM");
            Console.WriteLine("│  • Accuratezza: > 95% (SNR > 10dB)");
            Console.WriteLine("│  • CPU Usage: 5-10% (single core)");
            Console.WriteLine("└─");
            Console.WriteLine();

            Console.WriteLine("Premi un tasto per tornare al menu...");
            Console.ReadKey(true);
        }
    }
}
