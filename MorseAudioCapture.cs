using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace RadioLoggerApp.MorseDecoder
{
    /// <summary>
    /// Gestisce l'acquisizione audio in tempo reale per la decodifica del codice Morse
    /// Supporta microfono, line-in e dispositivi audio virtuali
    /// </summary>
    public class MorseAudioCapture : IDisposable
    {
        private WaveInEvent? waveIn;
        private readonly int sampleRate;
        private readonly int channels;
        private bool isCapturing;

        // Buffer circolare per l'analisi audio
        private readonly Queue<float> audioBuffer;
        private readonly int bufferSize;

        // Eventi per notificare nuovi dati audio
        public event EventHandler<AudioDataEventArgs>? AudioDataAvailable;
        public event EventHandler<string>? ErrorOccurred;
        public event EventHandler<float>? AudioLevelChanged;

        // Statistiche audio
        private float peakLevel;
        private float averageLevel;
        private readonly Queue<float> levelHistory;
        private const int LevelHistorySize = 100;

        public bool IsCapturing => isCapturing;
        public int SampleRate => sampleRate;
        public float PeakLevel => peakLevel;
        public float AverageLevel => averageLevel;

        /// <summary>
        /// Inizializza il sistema di acquisizione audio
        /// </summary>
        /// <param name="sampleRate">Frequenza di campionamento (default: 44100 Hz)</param>
        /// <param name="channels">Numero di canali (default: 1 - mono)</param>
        /// <param name="bufferSizeMs">Dimensione del buffer in millisecondi</param>
        public MorseAudioCapture(int sampleRate = 44100, int channels = 1, int bufferSizeMs = 50)
        {
            this.sampleRate = sampleRate;
            this.channels = channels;
            this.bufferSize = (sampleRate * bufferSizeMs) / 1000;
            this.audioBuffer = new Queue<float>(bufferSize * 2);
            this.levelHistory = new Queue<float>(LevelHistorySize);
            this.isCapturing = false;
        }

        /// <summary>
        /// Ottiene la lista dei dispositivi audio disponibili
        /// </summary>
        public static List<AudioDeviceInfo> GetAvailableDevices()
        {
            var devices = new List<AudioDeviceInfo>();

            try
            {
                // Dispositivi WaveIn (microfoni, line-in, etc.)
                for (int i = 0; i < WaveInEvent.DeviceCount; i++)
                {
                    var capabilities = WaveInEvent.GetCapabilities(i);
                    devices.Add(new AudioDeviceInfo
                    {
                        DeviceId = i,
                        Name = capabilities.ProductName,
                        Channels = capabilities.Channels,
                        IsDefault = i == 0
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nell'enumerazione dei dispositivi: {ex.Message}");
            }

            return devices;
        }

        /// <summary>
        /// Avvia l'acquisizione audio dal dispositivo specificato
        /// </summary>
        /// <param name="deviceId">ID del dispositivo (-1 per default)</param>
        public void StartCapture(int deviceId = -1)
        {
            if (isCapturing)
            {
                throw new InvalidOperationException("L'acquisizione è già in corso");
            }

            try
            {
                // Configura il dispositivo di acquisizione
                waveIn = new WaveInEvent
                {
                    DeviceNumber = deviceId >= 0 ? deviceId : 0,
                    WaveFormat = new WaveFormat(sampleRate, 16, channels),
                    BufferMilliseconds = 20
                };

                // Handler per i dati audio in arrivo
                waveIn.DataAvailable += OnDataAvailable;
                waveIn.RecordingStopped += OnRecordingStopped;

                // Avvia la registrazione
                waveIn.StartRecording();
                isCapturing = true;

                Console.WriteLine($"Acquisizione audio avviata: {sampleRate}Hz, {channels} canale/i");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Errore nell'avvio dell'acquisizione: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Ferma l'acquisizione audio
        /// </summary>
        public void StopCapture()
        {
            if (!isCapturing || waveIn == null)
            {
                return;
            }

            try
            {
                waveIn.StopRecording();
                isCapturing = false;
                Console.WriteLine("Acquisizione audio fermata");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Errore nel fermare l'acquisizione: {ex.Message}");
            }
        }

        /// <summary>
        /// Gestisce i nuovi dati audio
        /// </summary>
        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            try
            {
                // Converte i byte in float normalizzati (-1.0 a 1.0)
                int samplesRecorded = e.BytesRecorded / 2; // 16-bit = 2 bytes per sample
                float[] samples = new float[samplesRecorded];

                for (int i = 0; i < samplesRecorded; i++)
                {
                    short sample = BitConverter.ToInt16(e.Buffer, i * 2);
                    samples[i] = sample / 32768f; // Normalizza a [-1, 1]
                }

                // Calcola le statistiche audio
                CalculateAudioLevels(samples);

                // Aggiorna il buffer circolare
                foreach (var sample in samples)
                {
                    audioBuffer.Enqueue(sample);
                    if (audioBuffer.Count > bufferSize * 2)
                    {
                        audioBuffer.Dequeue();
                    }
                }

                // Notifica i subscriber
                AudioDataAvailable?.Invoke(this, new AudioDataEventArgs
                {
                    Samples = samples,
                    SampleRate = sampleRate,
                    Timestamp = DateTime.Now,
                    PeakLevel = peakLevel,
                    AverageLevel = averageLevel
                });

                AudioLevelChanged?.Invoke(this, averageLevel);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Errore nell'elaborazione dei dati audio: {ex.Message}");
            }
        }

        /// <summary>
        /// Calcola i livelli audio per il monitoraggio
        /// </summary>
        private void CalculateAudioLevels(float[] samples)
        {
            if (samples.Length == 0) return;

            // Peak level
            peakLevel = samples.Max(s => Math.Abs(s));

            // Average level (RMS)
            float sumSquares = samples.Sum(s => s * s);
            float rms = (float)Math.Sqrt(sumSquares / samples.Length);

            // Aggiorna lo storico
            levelHistory.Enqueue(rms);
            if (levelHistory.Count > LevelHistorySize)
            {
                levelHistory.Dequeue();
            }

            averageLevel = levelHistory.Average();
        }

        /// <summary>
        /// Gestisce l'arresto della registrazione
        /// </summary>
        private void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                ErrorOccurred?.Invoke(this, $"Registrazione fermata con errore: {e.Exception.Message}");
            }

            isCapturing = false;
        }

        /// <summary>
        /// Ottiene un campione del buffer corrente
        /// </summary>
        public float[] GetBufferSnapshot()
        {
            lock (audioBuffer)
            {
                return audioBuffer.ToArray();
            }
        }

        /// <summary>
        /// Pulisce le risorse
        /// </summary>
        public void Dispose()
        {
            StopCapture();

            if (waveIn != null)
            {
                waveIn.DataAvailable -= OnDataAvailable;
                waveIn.RecordingStopped -= OnRecordingStopped;
                waveIn.Dispose();
                waveIn = null;
            }

            audioBuffer.Clear();
            levelHistory.Clear();
        }
    }

    /// <summary>
    /// Informazioni su un dispositivo audio
    /// </summary>
    public class AudioDeviceInfo
    {
        public int DeviceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Channels { get; set; }
        public bool IsDefault { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Channels} ch){(IsDefault ? " [Default]" : "")}";
        }
    }

    /// <summary>
    /// Argomenti dell'evento dati audio
    /// </summary>
    public class AudioDataEventArgs : EventArgs
    {
        public float[] Samples { get; set; } = Array.Empty<float>();
        public int SampleRate { get; set; }
        public DateTime Timestamp { get; set; }
        public float PeakLevel { get; set; }
        public float AverageLevel { get; set; }
    }
}
