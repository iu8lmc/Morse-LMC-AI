using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.IO;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace RadioLoggerApp.MorseDecoder
{
    /// <summary>
    /// Logica di interazione per MorseDecoderWindow.xaml
    /// </summary>
    public partial class MorseDecoderWindow : Window
    {
        // Componenti del sistema
        private MorseAudioCapture? audioCapture;
        private MorseSignalProcessor? signalProcessor;
        private MorseDecoder? morseDecoder;
        private MorseAIEnhancer? aiEnhancer;

        // Timer per aggiornamenti UI
        private DispatcherTimer? updateTimer;
        private DispatcherTimer? processingTimer;

        // Buffer per i grafici
        private readonly Queue<double> waveformBuffer;
        private readonly Queue<double> spectrumBuffer;
        private const int GraphBufferSize = 1000;

        // Timestamp per il processing
        private DateTime startTime;
        private long currentTimestamp;

        // Stato
        private bool isDecoding;

        public MorseDecoderWindow()
        {
            InitializeComponent();

            waveformBuffer = new Queue<double>(GraphBufferSize);
            spectrumBuffer = new Queue<double>(GraphBufferSize);
            isDecoding = false;

            InitializeComponents();
            InitializeGraphs();
            LoadAudioDevices();

            Loaded += MorseDecoderWindow_Loaded;
            Closing += MorseDecoderWindow_Closing;
        }

        /// <summary>
        /// Inizializza i componenti del sistema
        /// </summary>
        private void InitializeComponents()
        {
            try
            {
                audioCapture = new MorseAudioCapture(sampleRate: 44100, channels: 1);
                signalProcessor = new MorseSignalProcessor(sampleRate: 44100);
                morseDecoder = new MorseDecoder(initialWPM: 20);
                aiEnhancer = new MorseAIEnhancer();

                // Sottoscrivi agli eventi
                if (audioCapture != null)
                {
                    audioCapture.AudioDataAvailable += AudioCapture_AudioDataAvailable;
                    audioCapture.AudioLevelChanged += AudioCapture_AudioLevelChanged;
                    audioCapture.ErrorOccurred += AudioCapture_ErrorOccurred;
                }

                if (morseDecoder != null)
                {
                    morseDecoder.CharacterDecoded += MorseDecoder_CharacterDecoded;
                    morseDecoder.WordDecoded += MorseDecoder_WordDecoded;
                    morseDecoder.TimingCalibrated += MorseDecoder_TimingCalibrated;
                }

                // Timer per aggiornamenti periodici
                updateTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(100)
                };
                updateTimer.Tick += UpdateTimer_Tick;

                processingTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(10)
                };
                processingTimer.Tick += ProcessingTimer_Tick;

                LogEvent("Sistema inizializzato correttamente");
            }
            catch (Exception ex)
            {
                LogEvent($"Errore nell'inizializzazione: {ex.Message}");
                MessageBox.Show($"Errore nell'inizializzazione del sistema: {ex.Message}",
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Inizializza i grafici OxyPlot
        /// </summary>
        private void InitializeGraphs()
        {
            // Grafico forma d'onda
            var waveformModel = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderColor = OxyColor.FromRgb(62, 62, 66),
                TextColor = OxyColors.White
            };

            waveformModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = -1,
                Maximum = 1,
                AxislineColor = OxyColor.FromRgb(62, 62, 66),
                TicklineColor = OxyColor.FromRgb(62, 62, 66),
                TextColor = OxyColors.White
            });

            waveformModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                AxislineColor = OxyColor.FromRgb(62, 62, 66),
                TicklineColor = OxyColor.FromRgb(62, 62, 66),
                TextColor = OxyColors.White
            });

            var waveformSeries = new LineSeries
            {
                Color = OxyColor.FromRgb(78, 201, 176),
                StrokeThickness = 1
            };
            waveformModel.Series.Add(waveformSeries);
            waveformPlot.Model = waveformModel;

            // Grafico spettro
            var spectrumModel = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderColor = OxyColor.FromRgb(62, 62, 66),
                TextColor = OxyColors.White
            };

            spectrumModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                AxislineColor = OxyColor.FromRgb(62, 62, 66),
                TicklineColor = OxyColor.FromRgb(62, 62, 66),
                TextColor = OxyColors.White
            });

            spectrumModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Frequenza (Hz)",
                AxislineColor = OxyColor.FromRgb(62, 62, 66),
                TicklineColor = OxyColor.FromRgb(62, 62, 66),
                TextColor = OxyColors.White
            });

            var spectrumSeries = new LineSeries
            {
                Color = OxyColor.FromRgb(0, 122, 204),
                StrokeThickness = 2
            };
            spectrumModel.Series.Add(spectrumSeries);
            spectrumPlot.Model = spectrumModel;
        }

        /// <summary>
        /// Carica i dispositivi audio disponibili
        /// </summary>
        private void LoadAudioDevices()
        {
            try
            {
                var devices = MorseAudioCapture.GetAvailableDevices();
                cmbAudioDevices.ItemsSource = devices;

                if (devices.Count > 0)
                {
                    cmbAudioDevices.SelectedIndex = 0;
                }

                LogEvent($"Trovati {devices.Count} dispositivi audio");
            }
            catch (Exception ex)
            {
                LogEvent($"Errore nel caricamento dei dispositivi: {ex.Message}");
            }
        }

        /// <summary>
        /// Evento: Avvia decodifica
        /// </summary>
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (audioCapture == null || signalProcessor == null || morseDecoder == null)
                {
                    MessageBox.Show("Sistema non inizializzato correttamente", "Errore",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int deviceId = cmbAudioDevices.SelectedIndex;
                audioCapture.StartCapture(deviceId);

                startTime = DateTime.Now;
                currentTimestamp = 0;
                isDecoding = true;

                updateTimer?.Start();
                processingTimer?.Start();

                btnStart.IsEnabled = false;
                btnStop.IsEnabled = true;
                cmbAudioDevices.IsEnabled = false;

                txtStatus.Text = "Decodifica in corso...";
                statusIndicator.Fill = new SolidColorBrush(Color.FromRgb(209, 76, 76)); // Rosso (attivo)

                LogEvent("Decodifica avviata");
            }
            catch (Exception ex)
            {
                LogEvent($"Errore nell'avvio: {ex.Message}");
                MessageBox.Show($"Errore nell'avvio della decodifica: {ex.Message}",
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Evento: Ferma decodifica
        /// </summary>
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            StopDecoding();
        }

        /// <summary>
        /// Ferma la decodifica
        /// </summary>
        private void StopDecoding()
        {
            try
            {
                audioCapture?.StopCapture();
                updateTimer?.Stop();
                processingTimer?.Stop();

                isDecoding = false;

                btnStart.IsEnabled = true;
                btnStop.IsEnabled = false;
                cmbAudioDevices.IsEnabled = true;

                txtStatus.Text = "Pronto";
                statusIndicator.Fill = new SolidColorBrush(Color.FromRgb(78, 201, 176)); // Verde

                LogEvent("Decodifica fermata");
            }
            catch (Exception ex)
            {
                LogEvent($"Errore nell'arresto: {ex.Message}");
            }
        }

        /// <summary>
        /// Evento: Pulisci testo
        /// </summary>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            txtDecoded.Clear();
            txtLog.Clear();
            morseDecoder?.ClearDecodedText();
            waveformBuffer.Clear();
            spectrumBuffer.Clear();

            txtCharCount.Text = "0";
            txtErrorCount.Text = "0";
            txtAccuracy.Text = "100%";

            LogEvent("Interfaccia pulita");
        }

        /// <summary>
        /// Evento: Esporta risultati
        /// </summary>
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "File di testo (*.txt)|*.txt|Tutti i file (*.*)|*.*",
                    DefaultExt = ".txt",
                    FileName = $"morse_decoded_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                };

                if (dialog.ShowDialog() == true)
                {
                    string content = $"Decodifica Morse - {DateTime.Now}\n";
                    content += $"========================================\n\n";
                    content += $"Testo decodificato:\n{txtDecoded.Text}\n\n";
                    content += $"Statistiche:\n";
                    content += $"- Caratteri: {txtCharCount.Text}\n";
                    content += $"- Errori: {txtErrorCount.Text}\n";
                    content += $"- Accuratezza: {txtAccuracy.Text}\n";
                    content += $"- Velocità: {txtWPM.Text}\n";
                    content += $"- Frequenza: {txtFrequency.Text}\n";
                    content += $"- SNR: {txtSNR.Text}\n";

                    File.WriteAllText(dialog.FileName, content);

                    LogEvent($"Risultati esportati in: {dialog.FileName}");
                    MessageBox.Show("Risultati esportati con successo!", "Esportazione",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                LogEvent($"Errore nell'esportazione: {ex.Message}");
                MessageBox.Show($"Errore nell'esportazione: {ex.Message}",
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Evento: Impostazioni
        /// </summary>
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Finestra impostazioni in sviluppo.\n\n" +
                          "Funzionalità disponibili:\n" +
                          "- Auto-calibrazione WPM\n" +
                          "- Rilevamento automatico frequenza\n" +
                          "- Machine Learning integrato\n" +
                          "- Filtri adattivi AGC",
                          "Impostazioni", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Evento: Addestra AI
        /// </summary>
        private void BtnTrainAI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                aiEnhancer?.TrainModel();
                UpdateAIStatus();
                LogEvent("Modello AI addestrato");
                MessageBox.Show("Modello AI addestrato con successo!", "Training AI",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LogEvent($"Errore nel training AI: {ex.Message}");
                MessageBox.Show($"Errore nel training: {ex.Message}",
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Gestisce i dati audio in arrivo
        /// </summary>
        private void AudioCapture_AudioDataAvailable(object? sender, AudioDataEventArgs e)
        {
            if (!isDecoding || signalProcessor == null) return;

            try
            {
                // Processa il segnale
                var processedData = signalProcessor.ProcessAudioBuffer(e.Samples);

                // Aggiorna i buffer per i grafici
                Dispatcher.Invoke(() =>
                {
                    foreach (var sample in e.Samples.Take(100))
                    {
                        waveformBuffer.Enqueue(sample);
                        if (waveformBuffer.Count > GraphBufferSize)
                        {
                            waveformBuffer.Dequeue();
                        }
                    }

                    if (processedData.Envelope.Length > 0)
                    {
                        foreach (var env in processedData.Envelope.Take(100))
                        {
                            spectrumBuffer.Enqueue(env);
                            if (spectrumBuffer.Count > GraphBufferSize)
                            {
                                spectrumBuffer.Dequeue();
                            }
                        }
                    }
                });

                // Passa l'envelope al decoder Morse
                if (morseDecoder != null && processedData.Envelope.Length > 0)
                {
                    foreach (var envelopeValue in processedData.Envelope)
                    {
                        morseDecoder.ProcessEnvelopeSample(envelopeValue, currentTimestamp++);
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => LogEvent($"Errore nel processing: {ex.Message}"));
            }
        }

        /// <summary>
        /// Gestisce i cambiamenti del livello audio
        /// </summary>
        private void AudioCapture_AudioLevelChanged(object? sender, float level)
        {
            Dispatcher.Invoke(() =>
            {
                audioLevelBar.Value = level * 100;
            });
        }

        /// <summary>
        /// Gestisce gli errori audio
        /// </summary>
        private void AudioCapture_ErrorOccurred(object? sender, string error)
        {
            Dispatcher.Invoke(() => LogEvent($"Errore audio: {error}"));
        }

        /// <summary>
        /// Gestisce i caratteri decodificati
        /// </summary>
        private void MorseDecoder_CharacterDecoded(object? sender, CharacterDecodedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                txtDecoded.AppendText(e.Character.ToString());
                txtDecoded.ScrollToEnd();
            });
        }

        /// <summary>
        /// Gestisce le parole decodificate
        /// </summary>
        private void MorseDecoder_WordDecoded(object? sender, WordDecodedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                LogEvent($"Parola: {e.Word}");
            });
        }

        /// <summary>
        /// Gestisce la calibrazione del timing
        /// </summary>
        private void MorseDecoder_TimingCalibrated(object? sender, TimingCalibratedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                LogEvent($"Timing calibrato: {e.WPM:F1} WPM (confidenza: {e.Confidence:P0})");
            });
        }

        /// <summary>
        /// Timer per aggiornamenti UI
        /// </summary>
        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            UpdateStatistics();
            UpdateGraphs();
            UpdateAIStatus();
        }

        /// <summary>
        /// Timer per elaborazione continua
        /// </summary>
        private void ProcessingTimer_Tick(object? sender, EventArgs e)
        {
            // Processing continuo se necessario
        }

        /// <summary>
        /// Aggiorna le statistiche
        /// </summary>
        private void UpdateStatistics()
        {
            if (signalProcessor != null)
            {
                txtFrequency.Text = $"{signalProcessor.CurrentFrequency:F1} Hz";
                txtSNR.Text = $"{signalProcessor.SNR:F1} dB";
            }

            if (morseDecoder != null)
            {
                txtWPM.Text = $"{morseDecoder.CurrentWPM:F1} WPM";
                txtConfidence.Text = $"{morseDecoder.Confidence:P0}";
                txtCharCount.Text = morseDecoder.CharactersDecoded.ToString();
                txtErrorCount.Text = morseDecoder.ErrorsDetected.ToString();

                if (morseDecoder.CharactersDecoded > 0)
                {
                    double accuracy = 1.0 - (double)morseDecoder.ErrorsDetected / morseDecoder.CharactersDecoded;
                    txtAccuracy.Text = $"{accuracy:P0}";
                }
            }
        }

        /// <summary>
        /// Aggiorna i grafici
        /// </summary>
        private void UpdateGraphs()
        {
            // Aggiorna forma d'onda
            if (waveformPlot.Model?.Series.Count > 0 && waveformPlot.Model.Series[0] is LineSeries waveformSeries)
            {
                waveformSeries.Points.Clear();
                int i = 0;
                foreach (var sample in waveformBuffer)
                {
                    waveformSeries.Points.Add(new DataPoint(i++, sample));
                }
                waveformPlot.InvalidatePlot(true);
            }

            // Aggiorna spettro/envelope
            if (spectrumPlot.Model?.Series.Count > 0 && spectrumPlot.Model.Series[0] is LineSeries spectrumSeries)
            {
                spectrumSeries.Points.Clear();
                int i = 0;
                foreach (var sample in spectrumBuffer)
                {
                    spectrumSeries.Points.Add(new DataPoint(i++, sample));
                }
                spectrumPlot.InvalidatePlot(true);
            }
        }

        /// <summary>
        /// Aggiorna lo stato dell'AI
        /// </summary>
        private void UpdateAIStatus()
        {
            if (aiEnhancer != null)
            {
                txtMLStatus.Text = aiEnhancer.IsModelTrained ? "Addestrato" : "Non addestrato";
                txtMLStatus.Foreground = aiEnhancer.IsModelTrained ?
                    new SolidColorBrush(Color.FromRgb(78, 201, 176)) :
                    new SolidColorBrush(Color.FromRgb(209, 76, 76));

                txtMLAccuracy.Text = $"{aiEnhancer.ModelAccuracy:P1}";
                txtTrainingData.Text = aiEnhancer.TrainingDataCount.ToString();
            }
        }

        /// <summary>
        /// Logga un evento
        /// </summary>
        private void LogEvent(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{timestamp}] {message}\n");
            txtLog.ScrollToEnd();
        }

        /// <summary>
        /// Evento: Finestra caricata
        /// </summary>
        private void MorseDecoderWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LogEvent("Decodificatore Morse AI pronto");
            txtStatusBar.Text = "Sistema pronto - Seleziona un dispositivo audio e premi Avvia";
        }

        /// <summary>
        /// Evento: Finestra in chiusura
        /// </summary>
        private void MorseDecoderWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            StopDecoding();
            audioCapture?.Dispose();
        }
    }
}
