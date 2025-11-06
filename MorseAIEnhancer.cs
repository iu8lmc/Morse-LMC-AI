using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace RadioLoggerApp.MorseDecoder
{
    /// <summary>
    /// Enhancer basato su Machine Learning per migliorare l'accuratezza della decodifica Morse
    /// Utilizza ML.NET per pattern recognition e correzione degli errori
    /// </summary>
    public class MorseAIEnhancer
    {
        private readonly MLContext mlContext;
        private ITransformer? model;
        private PredictionEngine<SignalFeatures, SignalPrediction>? predictionEngine;

        // Dataset di training
        private readonly List<SignalFeatures> trainingData;
        private const int MinTrainingDataSize = 100;

        // Statistiche
        public int TrainingDataCount => trainingData.Count;
        public bool IsModelTrained { get; private set; }
        public double ModelAccuracy { get; private set; }

        /// <summary>
        /// Inizializza l'AI enhancer
        /// </summary>
        public MorseAIEnhancer()
        {
            mlContext = new MLContext(seed: 42);
            trainingData = new List<SignalFeatures>();
            IsModelTrained = false;
            ModelAccuracy = 0;

            // Inizializza con dati sintetici per bootstrap
            InitializeWithSyntheticData();
        }

        /// <summary>
        /// Inizializza il modello con dati sintetici per il bootstrap
        /// </summary>
        private void InitializeWithSyntheticData()
        {
            // Genera esempi sintetici per dit e dah con varie condizioni
            Random random = new Random(42);

            for (int i = 0; i < 50; i++)
            {
                // Dit ideale
                trainingData.Add(new SignalFeatures
                {
                    Duration = 100 + random.Next(-10, 10),
                    Amplitude = 0.8f + (float)random.NextDouble() * 0.15f,
                    SNR = 15 + random.Next(-3, 5),
                    FrequencyStability = 0.9f + (float)random.NextDouble() * 0.1f,
                    RiseTime = 5 + random.Next(-2, 2),
                    FallTime = 5 + random.Next(-2, 2),
                    Label = "Dit"
                });

                // Dah ideale
                trainingData.Add(new SignalFeatures
                {
                    Duration = 300 + random.Next(-30, 30),
                    Amplitude = 0.8f + (float)random.NextDouble() * 0.15f,
                    SNR = 15 + random.Next(-3, 5),
                    FrequencyStability = 0.9f + (float)random.NextDouble() * 0.1f,
                    RiseTime = 5 + random.Next(-2, 2),
                    FallTime = 5 + random.Next(-2, 2),
                    Label = "Dah"
                });
            }
        }

        /// <summary>
        /// Addestra il modello ML
        /// </summary>
        public void TrainModel()
        {
            if (trainingData.Count < MinTrainingDataSize)
            {
                Console.WriteLine($"Dati di training insufficienti: {trainingData.Count}/{MinTrainingDataSize}");
                return;
            }

            try
            {
                // Carica i dati di training
                IDataView dataView = mlContext.Data.LoadFromEnumerable(trainingData);

                // Dividi in training e test set (80/20)
                var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

                // Pipeline di training
                var pipeline = mlContext.Transforms.Conversion
                    .MapValueToKey("Label")
                    .Append(mlContext.Transforms.Concatenate("Features",
                        nameof(SignalFeatures.Duration),
                        nameof(SignalFeatures.Amplitude),
                        nameof(SignalFeatures.SNR),
                        nameof(SignalFeatures.FrequencyStability),
                        nameof(SignalFeatures.RiseTime),
                        nameof(SignalFeatures.FallTime)))
                    .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                    .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                    .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

                // Addestra il modello
                Console.WriteLine("Training del modello ML in corso...");
                model = pipeline.Fit(split.TrainSet);

                // Valuta l'accuratezza
                var predictions = model.Transform(split.TestSet);
                var metrics = mlContext.MulticlassClassification.Evaluate(predictions);

                ModelAccuracy = metrics.MicroAccuracy;
                IsModelTrained = true;

                Console.WriteLine($"Modello addestrato con accuratezza: {ModelAccuracy:P2}");
                Console.WriteLine($"Log-loss: {metrics.LogLoss:F4}");

                // Crea il prediction engine
                predictionEngine = mlContext.Model.CreatePredictionEngine<SignalFeatures, SignalPrediction>(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel training del modello: {ex.Message}");
                IsModelTrained = false;
            }
        }

        /// <summary>
        /// Predice se un segnale è un Dit o un Dah usando ML
        /// </summary>
        public SignalClassification ClassifySignal(double duration, double amplitude, double snr,
                                                   double frequencyStability)
        {
            if (!IsModelTrained || predictionEngine == null)
            {
                // Fallback: classificazione basata su regole
                return FallbackClassification(duration);
            }

            try
            {
                var features = new SignalFeatures
                {
                    Duration = (float)duration,
                    Amplitude = (float)amplitude,
                    SNR = (float)snr,
                    FrequencyStability = (float)frequencyStability,
                    RiseTime = 5,
                    FallTime = 5
                };

                var prediction = predictionEngine.Predict(features);

                return new SignalClassification
                {
                    Type = prediction.PredictedLabel == "Dit" ? SignalType.Dit : SignalType.Dah,
                    Confidence = prediction.Score.Max(),
                    Probability = prediction.Score,
                    IsMLPrediction = true
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nella predizione: {ex.Message}");
                return FallbackClassification(duration);
            }
        }

        /// <summary>
        /// Classificazione fallback basata su regole
        /// </summary>
        private SignalClassification FallbackClassification(double duration)
        {
            // Usa una soglia semplice (assumendo ~150ms come divisione)
            bool isDit = duration < 150;

            return new SignalClassification
            {
                Type = isDit ? SignalType.Dit : SignalType.Dah,
                Confidence = 0.7,
                Probability = isDit ? new float[] { 0.7f, 0.3f } : new float[] { 0.3f, 0.7f },
                IsMLPrediction = false
            };
        }

        /// <summary>
        /// Aggiunge un esempio di training
        /// </summary>
        public void AddTrainingExample(double duration, double amplitude, double snr,
                                      double frequencyStability, SignalType actualType)
        {
            trainingData.Add(new SignalFeatures
            {
                Duration = (float)duration,
                Amplitude = (float)amplitude,
                SNR = (float)snr,
                FrequencyStability = (float)frequencyStability,
                RiseTime = 5,
                FallTime = 5,
                Label = actualType == SignalType.Dit ? "Dit" : "Dah"
            });

            // Riaddestra periodicamente
            if (trainingData.Count % 50 == 0 && trainingData.Count >= MinTrainingDataSize)
            {
                TrainModel();
            }
        }

        /// <summary>
        /// Corregge errori comuni nella decodifica
        /// </summary>
        public string CorrectDecodedText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            // Dizionario di correzioni comuni
            var corrections = new Dictionary<string, string>
            {
                { "YELLO", "HELLO" },
                { "TLST", "TEST" },
                { "CQ CQ", "CQ" },
                { "BREQK", "BREAK" },
                // Aggiungi altre correzioni comuni
            };

            string correctedText = text;

            foreach (var correction in corrections)
            {
                correctedText = correctedText.Replace(correction.Key, correction.Value);
            }

            return correctedText;
        }

        /// <summary>
        /// Analizza pattern comuni nel testo decodificato
        /// </summary>
        public TextAnalysis AnalyzeDecodedText(string text)
        {
            var analysis = new TextAnalysis
            {
                Text = text,
                TotalCharacters = text.Length,
                UnknownCharacters = text.Count(c => c == '?'),
                HasCommonPhrases = DetectCommonPhrases(text)
            };

            // Calcola la confidence complessiva
            if (analysis.TotalCharacters > 0)
            {
                double errorRate = (double)analysis.UnknownCharacters / analysis.TotalCharacters;
                analysis.OverallConfidence = Math.Max(0, 1.0 - errorRate * 2);
            }

            return analysis;
        }

        /// <summary>
        /// Rileva frasi comuni nel codice Morse
        /// </summary>
        private bool DetectCommonPhrases(string text)
        {
            var commonPhrases = new[] { "CQ", "TEST", "QSO", "73", "SK", "DE", "K" };
            return commonPhrases.Any(phrase => text.Contains(phrase, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Esporta il modello addestrato
        /// </summary>
        public void SaveModel(string filePath)
        {
            if (model == null || !IsModelTrained)
            {
                throw new InvalidOperationException("Nessun modello da salvare");
            }

            mlContext.Model.Save(model, null, filePath);
            Console.WriteLine($"Modello salvato in: {filePath}");
        }

        /// <summary>
        /// Importa un modello addestrato
        /// </summary>
        public void LoadModel(string filePath)
        {
            model = mlContext.Model.Load(filePath, out var modelSchema);
            predictionEngine = mlContext.Model.CreatePredictionEngine<SignalFeatures, SignalPrediction>(model);
            IsModelTrained = true;
            Console.WriteLine($"Modello caricato da: {filePath}");
        }
    }

    /// <summary>
    /// Features del segnale per il ML
    /// </summary>
    public class SignalFeatures
    {
        public float Duration { get; set; }
        public float Amplitude { get; set; }
        public float SNR { get; set; }
        public float FrequencyStability { get; set; }
        public float RiseTime { get; set; }
        public float FallTime { get; set; }

        [ColumnName("Label")]
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// Predizione del modello ML
    /// </summary>
    public class SignalPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedLabel { get; set; } = string.Empty;

        [ColumnName("Score")]
        public float[] Score { get; set; } = Array.Empty<float>();
    }

    /// <summary>
    /// Tipo di segnale Morse
    /// </summary>
    public enum SignalType
    {
        Dit,
        Dah
    }

    /// <summary>
    /// Risultato della classificazione
    /// </summary>
    public class SignalClassification
    {
        public SignalType Type { get; set; }
        public double Confidence { get; set; }
        public float[] Probability { get; set; } = Array.Empty<float>();
        public bool IsMLPrediction { get; set; }
    }

    /// <summary>
    /// Analisi del testo decodificato
    /// </summary>
    public class TextAnalysis
    {
        public string Text { get; set; } = string.Empty;
        public int TotalCharacters { get; set; }
        public int UnknownCharacters { get; set; }
        public bool HasCommonPhrases { get; set; }
        public double OverallConfidence { get; set; }
    }
}
