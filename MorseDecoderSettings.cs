using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RadioLoggerApp.MorseDecoder
{
    /// <summary>
    /// Classe per le impostazioni del Morse Decoder AI
    /// Supporta serializzazione JSON per la persistenza
    /// </summary>
    public class MorseDecoderSettings
    {
        // ===== Parametri Codice Morse =====

        /// <summary>Auto-calibrazione automatica della velocità WPM</summary>
        public bool AutoCalibrationEnabled { get; set; } = true;

        /// <summary>Velocità iniziale in Words Per Minute (5-60)</summary>
        public int InitialWPM { get; set; } = 20;

        /// <summary>Rilevamento automatico della frequenza portante</summary>
        public bool AutoFrequencyDetection { get; set; } = true;

        /// <summary>Frequenza target in Hz (200-2000)</summary>
        public int TargetFrequency { get; set; } = 800;

        /// <summary>Soglia di rilevamento del segnale (0.1-0.9)</summary>
        public double DetectionThreshold { get; set; } = 0.3;

        // ===== Parametri DSP =====

        /// <summary>Larghezza di banda del filtro passa-banda in Hz</summary>
        public int Bandwidth { get; set; } = 100;

        /// <summary>Abilita Automatic Gain Control</summary>
        public bool AGCEnabled { get; set; } = true;

        /// <summary>Abilita Noise Gate per riduzione rumore</summary>
        public bool NoiseGateEnabled { get; set; } = true;

        /// <summary>Dimensione della FFT (1024, 2048, 4096, 8192)</summary>
        public int FFTSize { get; set; } = 4096;

        // ===== Parametri AI/ML =====

        /// <summary>Abilita classificazione AI per Dit/Dah</summary>
        public bool AIEnabled { get; set; } = true;

        /// <summary>Intervallo di re-training in numero di campioni</summary>
        public int RetrainingInterval { get; set; } = 50;

        /// <summary>Dimensione minima del dataset per training</summary>
        public int MinDatasetSize { get; set; } = 100;

        /// <summary>Abilita correzione errori basata su AI</summary>
        public bool ErrorCorrectionEnabled { get; set; } = true;

        /// <summary>Percorso del modello ML salvato</summary>
        public string ModelPath { get; set; } = string.Empty;

        /// <summary>Flag per resettare il modello</summary>
        [JsonIgnore]
        public bool ResetModel { get; set; } = false;

        // ===== Parametri Audio =====

        /// <summary>Dimensione del buffer audio in millisecondi</summary>
        public int BufferSizeMs { get; set; } = 50;

        /// <summary>Frequenza di campionamento audio (22050, 44100, 48000)</summary>
        public int SampleRate { get; set; } = 44100;

        // ===== Metadati =====

        /// <summary>Versione delle impostazioni</summary>
        public string Version { get; set; } = "1.0";

        /// <summary>Timestamp dell'ultima modifica</summary>
        public DateTime LastModified { get; set; } = DateTime.Now;

        /// <summary>
        /// Crea una copia profonda delle impostazioni
        /// </summary>
        public MorseDecoderSettings Clone()
        {
            var json = JsonSerializer.Serialize(this);
            return JsonSerializer.Deserialize<MorseDecoderSettings>(json) ?? new MorseDecoderSettings();
        }

        /// <summary>
        /// Copia i valori da un'altra istanza
        /// </summary>
        public void CopyFrom(MorseDecoderSettings other)
        {
            // Morse
            AutoCalibrationEnabled = other.AutoCalibrationEnabled;
            InitialWPM = other.InitialWPM;
            AutoFrequencyDetection = other.AutoFrequencyDetection;
            TargetFrequency = other.TargetFrequency;
            DetectionThreshold = other.DetectionThreshold;

            // DSP
            Bandwidth = other.Bandwidth;
            AGCEnabled = other.AGCEnabled;
            NoiseGateEnabled = other.NoiseGateEnabled;
            FFTSize = other.FFTSize;

            // AI/ML
            AIEnabled = other.AIEnabled;
            RetrainingInterval = other.RetrainingInterval;
            MinDatasetSize = other.MinDatasetSize;
            ErrorCorrectionEnabled = other.ErrorCorrectionEnabled;
            ModelPath = other.ModelPath;

            // Audio
            BufferSizeMs = other.BufferSizeMs;
            SampleRate = other.SampleRate;

            LastModified = DateTime.Now;
        }

        /// <summary>
        /// Valida le impostazioni
        /// </summary>
        public bool Validate(out string errorMessage)
        {
            errorMessage = string.Empty;

            // Valida WPM
            if (InitialWPM < 5 || InitialWPM > 60)
            {
                errorMessage = "WPM deve essere tra 5 e 60";
                return false;
            }

            // Valida frequenza
            if (TargetFrequency < 200 || TargetFrequency > 2000)
            {
                errorMessage = "Frequenza deve essere tra 200 e 2000 Hz";
                return false;
            }

            // Valida soglia
            if (DetectionThreshold < 0.1 || DetectionThreshold > 0.9)
            {
                errorMessage = "Soglia deve essere tra 0.1 e 0.9";
                return false;
            }

            // Valida bandwidth
            if (Bandwidth < 50 || Bandwidth > 500)
            {
                errorMessage = "Bandwidth deve essere tra 50 e 500 Hz";
                return false;
            }

            // Valida FFT size
            if (FFTSize != 1024 && FFTSize != 2048 && FFTSize != 4096 && FFTSize != 8192)
            {
                errorMessage = "FFT Size deve essere 1024, 2048, 4096 o 8192";
                return false;
            }

            // Valida sample rate
            if (SampleRate != 22050 && SampleRate != 44100 && SampleRate != 48000)
            {
                errorMessage = "Sample Rate deve essere 22050, 44100 o 48000 Hz";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Restituisce una rappresentazione testuale delle impostazioni
        /// </summary>
        public override string ToString()
        {
            return $"MorseDecoderSettings: WPM={InitialWPM}, Freq={TargetFrequency}Hz, " +
                   $"Threshold={DetectionThreshold:F2}, AI={AIEnabled}, SampleRate={SampleRate}Hz";
        }
    }

    /// <summary>
    /// Gestisce il salvataggio e caricamento delle impostazioni su file
    /// </summary>
    public static class SettingsManager
    {
        private static readonly string SettingsDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "RadioLoggerApp", "MorseDecoder");

        private static readonly string SettingsFilePath =
            Path.Combine(SettingsDirectory, "settings.json");

        /// <summary>
        /// Salva le impostazioni su file
        /// </summary>
        public static void SaveSettings(MorseDecoderSettings settings)
        {
            try
            {
                // Crea la directory se non esiste
                Directory.CreateDirectory(SettingsDirectory);

                // Valida le impostazioni
                if (!settings.Validate(out string errorMessage))
                {
                    throw new InvalidOperationException($"Impostazioni non valide: {errorMessage}");
                }

                // Aggiorna timestamp
                settings.LastModified = DateTime.Now;

                // Serializza in JSON
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string json = JsonSerializer.Serialize(settings, options);

                // Salva su file
                File.WriteAllText(SettingsFilePath, json);

                Console.WriteLine($"Impostazioni salvate in: {SettingsFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel salvataggio delle impostazioni: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Carica le impostazioni da file
        /// </summary>
        public static MorseDecoderSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    Console.WriteLine("File impostazioni non trovato, uso valori predefiniti");
                    return new MorseDecoderSettings();
                }

                // Leggi il file JSON
                string json = File.ReadAllText(SettingsFilePath);

                // Deserializza
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var settings = JsonSerializer.Deserialize<MorseDecoderSettings>(json, options);

                if (settings == null)
                {
                    Console.WriteLine("Deserializzazione fallita, uso valori predefiniti");
                    return new MorseDecoderSettings();
                }

                // Valida
                if (!settings.Validate(out string errorMessage))
                {
                    Console.WriteLine($"Impostazioni non valide ({errorMessage}), uso valori predefiniti");
                    return new MorseDecoderSettings();
                }

                Console.WriteLine($"Impostazioni caricate da: {SettingsFilePath}");
                return settings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel caricamento delle impostazioni: {ex.Message}");
                return new MorseDecoderSettings();
            }
        }

        /// <summary>
        /// Elimina il file delle impostazioni
        /// </summary>
        public static void DeleteSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    File.Delete(SettingsFilePath);
                    Console.WriteLine("File impostazioni eliminato");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nell'eliminazione delle impostazioni: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Verifica se esiste un file di impostazioni salvato
        /// </summary>
        public static bool SettingsFileExists()
        {
            return File.Exists(SettingsFilePath);
        }

        /// <summary>
        /// Ottiene il percorso del file di impostazioni
        /// </summary>
        public static string GetSettingsFilePath()
        {
            return SettingsFilePath;
        }

        /// <summary>
        /// Crea un backup delle impostazioni correnti
        /// </summary>
        public static void BackupSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string backupPath = SettingsFilePath.Replace(".json", $"_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                    File.Copy(SettingsFilePath, backupPath, true);
                    Console.WriteLine($"Backup creato: {backupPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel backup delle impostazioni: {ex.Message}");
            }
        }
    }
}
