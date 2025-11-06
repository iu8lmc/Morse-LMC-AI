using System;
using System.Windows;
using Microsoft.Win32;

namespace RadioLoggerApp.MorseDecoder
{
    /// <summary>
    /// Finestra di configurazione avanzata per il Morse Decoder AI
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly MorseDecoderSettings settings;
        private bool settingsChanged;

        public MorseDecoderSettings Settings => settings;
        public bool SettingsChanged => settingsChanged;

        /// <summary>
        /// Costruttore con impostazioni iniziali
        /// </summary>
        public SettingsWindow(MorseDecoderSettings currentSettings)
        {
            InitializeComponent();
            settings = currentSettings.Clone();
            settingsChanged = false;

            LoadSettings();
            SetupEventHandlers();
        }

        /// <summary>
        /// Carica le impostazioni nell'interfaccia
        /// </summary>
        private void LoadSettings()
        {
            // Parametri Morse
            chkAutoCalibration.IsChecked = settings.AutoCalibrationEnabled;
            sliderWPM.Value = settings.InitialWPM;
            chkAutoFrequency.IsChecked = settings.AutoFrequencyDetection;
            sliderFrequency.Value = settings.TargetFrequency;
            sliderThreshold.Value = settings.DetectionThreshold;

            // DSP
            sliderBandwidth.Value = settings.Bandwidth;
            chkAGC.IsChecked = settings.AGCEnabled;
            chkNoiseGate.IsChecked = settings.NoiseGateEnabled;
            SetComboBoxValue(cmbFFTSize, settings.FFTSize.ToString());

            // AI/ML
            chkEnableAI.IsChecked = settings.AIEnabled;
            sliderRetraining.Value = settings.RetrainingInterval;
            sliderMinDataset.Value = settings.MinDatasetSize;
            chkErrorCorrection.IsChecked = settings.ErrorCorrectionEnabled;

            // Audio
            sliderBufferSize.Value = settings.BufferSizeMs;
            SetComboBoxValue(cmbSampleRate, settings.SampleRate.ToString());

            UpdateUIState();
        }

        /// <summary>
        /// Imposta il valore di una ComboBox
        /// </summary>
        private void SetComboBoxValue(System.Windows.Controls.ComboBox comboBox, string value)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                var item = comboBox.Items[i] as System.Windows.Controls.ComboBoxItem;
                if (item?.Content?.ToString() == value)
                {
                    comboBox.SelectedIndex = i;
                    return;
                }
            }
        }

        /// <summary>
        /// Configura gli event handler
        /// </summary>
        private void SetupEventHandlers()
        {
            // Abilita/disabilita controlli in base alle checkbox
            chkAutoCalibration.Checked += (s, e) => UpdateUIState();
            chkAutoCalibration.Unchecked += (s, e) => UpdateUIState();
            chkAutoFrequency.Checked += (s, e) => UpdateUIState();
            chkAutoFrequency.Unchecked += (s, e) => UpdateUIState();
            chkEnableAI.Checked += (s, e) => UpdateUIState();
            chkEnableAI.Unchecked += (s, e) => UpdateUIState();
        }

        /// <summary>
        /// Aggiorna lo stato dell'interfaccia
        /// </summary>
        private void UpdateUIState()
        {
            // Disabilita WPM se auto-calibrazione è attiva
            sliderWPM.IsEnabled = chkAutoCalibration.IsChecked == false;
            txtWPM.IsEnabled = chkAutoCalibration.IsChecked == false;

            // Disabilita frequenza se auto-detection è attiva
            sliderFrequency.IsEnabled = chkAutoFrequency.IsChecked == false;
            txtFrequency.IsEnabled = chkAutoFrequency.IsChecked == false;

            // Disabilita controlli AI se AI è disabilitata
            sliderRetraining.IsEnabled = chkEnableAI.IsChecked == true;
            txtRetraining.IsEnabled = chkEnableAI.IsChecked == true;
            sliderMinDataset.IsEnabled = chkEnableAI.IsChecked == true;
            txtMinDataset.IsEnabled = chkEnableAI.IsChecked == true;
            chkErrorCorrection.IsEnabled = chkEnableAI.IsChecked == true;
            btnSaveModel.IsEnabled = chkEnableAI.IsChecked == true;
            btnLoadModel.IsEnabled = chkEnableAI.IsChecked == true;
            btnResetModel.IsEnabled = chkEnableAI.IsChecked == true;
        }

        /// <summary>
        /// Salva le impostazioni
        /// </summary>
        private void SaveSettings()
        {
            // Parametri Morse
            settings.AutoCalibrationEnabled = chkAutoCalibration.IsChecked ?? true;
            settings.InitialWPM = (int)sliderWPM.Value;
            settings.AutoFrequencyDetection = chkAutoFrequency.IsChecked ?? true;
            settings.TargetFrequency = (int)sliderFrequency.Value;
            settings.DetectionThreshold = sliderThreshold.Value;

            // DSP
            settings.Bandwidth = (int)sliderBandwidth.Value;
            settings.AGCEnabled = chkAGC.IsChecked ?? true;
            settings.NoiseGateEnabled = chkNoiseGate.IsChecked ?? true;
            settings.FFTSize = int.Parse((cmbFFTSize.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "4096");

            // AI/ML
            settings.AIEnabled = chkEnableAI.IsChecked ?? true;
            settings.RetrainingInterval = (int)sliderRetraining.Value;
            settings.MinDatasetSize = (int)sliderMinDataset.Value;
            settings.ErrorCorrectionEnabled = chkErrorCorrection.IsChecked ?? true;

            // Audio
            settings.BufferSizeMs = (int)sliderBufferSize.Value;
            settings.SampleRate = int.Parse((cmbSampleRate.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "44100");

            settingsChanged = true;
        }

        // Event Handlers per gli slider
        private void SliderWPM_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtWPM != null)
                txtWPM.Text = ((int)sliderWPM.Value).ToString();
        }

        private void SliderFrequency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtFrequency != null)
                txtFrequency.Text = ((int)sliderFrequency.Value).ToString();
        }

        private void SliderThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtThreshold != null)
                txtThreshold.Text = sliderThreshold.Value.ToString("F2");
        }

        private void SliderBandwidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtBandwidth != null)
                txtBandwidth.Text = ((int)sliderBandwidth.Value).ToString();
        }

        private void SliderRetraining_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtRetraining != null)
                txtRetraining.Text = ((int)sliderRetraining.Value).ToString();
        }

        private void SliderMinDataset_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtMinDataset != null)
                txtMinDataset.Text = ((int)sliderMinDataset.Value).ToString();
        }

        private void SliderBufferSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtBufferSize != null)
                txtBufferSize.Text = ((int)sliderBufferSize.Value).ToString();
        }

        // Event Handlers per i pulsanti
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveSettings();
                SettingsManager.SaveSettings(settings);
                MessageBox.Show("Impostazioni salvate con successo!\n\nRiavviare la decodifica per applicare le modifiche.",
                              "Impostazioni Salvate", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel salvataggio delle impostazioni:\n{ex.Message}",
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnResetDefaults_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Ripristinare le impostazioni predefinite?\n\nQuesta operazione non può essere annullata.",
                                        "Conferma", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var defaultSettings = new MorseDecoderSettings();
                settings.CopyFrom(defaultSettings);
                LoadSettings();
                MessageBox.Show("Impostazioni predefinite ripristinate.", "Ripristino Completato",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnSaveModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "ML Model (*.mdl)|*.mdl|All Files (*.*)|*.*",
                    DefaultExt = ".mdl",
                    FileName = $"morse_model_{DateTime.Now:yyyyMMdd_HHmmss}.mdl"
                };

                if (dialog.ShowDialog() == true)
                {
                    settings.ModelPath = dialog.FileName;
                    MessageBox.Show($"Percorso modello impostato:\n{dialog.FileName}\n\nIl modello verrà salvato automaticamente durante il training.",
                                  "Percorso Modello", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLoadModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "ML Model (*.mdl)|*.mdl|All Files (*.*)|*.*",
                    DefaultExt = ".mdl"
                };

                if (dialog.ShowDialog() == true)
                {
                    settings.ModelPath = dialog.FileName;
                    MessageBox.Show($"Percorso modello impostato:\n{dialog.FileName}\n\nIl modello verrà caricato all'avvio della decodifica.",
                                  "Modello Selezionato", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnResetModel_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Resettare il modello AI?\n\nIl modello verrà riaddestrato da zero con dati sintetici.",
                                        "Conferma Reset", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                settings.ModelPath = string.Empty;
                settings.ResetModel = true;
                MessageBox.Show("Il modello verrà resettato al prossimo avvio della decodifica.",
                              "Reset Programmato", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
