using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RadioLoggerApp.SliceMaster
{
    /// <summary>
    /// Finestra principale di SliceMaster per SmartSDR 4.0
    /// </summary>
    public partial class SliceMasterWindow : Window
    {
        private FlexRadioConnection _connection;
        private SliceController _sliceController;
        private ObservableCollection<SliceProfile> _profiles;

        public SliceMasterWindow()
        {
            InitializeComponent();

            _connection = new FlexRadioConnection();
            _sliceController = new SliceController(_connection);
            _profiles = new ObservableCollection<SliceProfile>();

            // Sottoscrivi agli eventi
            _connection.ConnectionStatusChanged += OnConnectionStatusChanged;
            _connection.ErrorOccurred += OnErrorOccurred;
            _sliceController.StatusChanged += OnStatusChanged;

            // Inizializza UI
            InitializeUI();
        }

        /// <summary>
        /// Inizializza l'interfaccia utente
        /// </summary>
        private void InitializeUI()
        {
            // Imposta la lista delle slice
            slicesList.ItemsSource = _sliceController.Slices;

            // Imposta le bande
            bandsList.ItemsSource = Band.HamBands;

            // Imposta i profili
            profilesList.ItemsSource = _profiles;

            // Popola i modi disponibili (fatto tramite codice per ogni ComboBox creato dinamicamente)
            // Questo verrà gestito nel template
        }

        /// <summary>
        /// Gestisce il click sul pulsante Connetti
        /// </summary>
        private async void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (_connection.IsConnected)
            {
                await _connection.DisconnectAsync();
                btnConnect.Content = "Connetti";
            }
            else
            {
                string ip = txtRadioIp.Text.Trim();
                if (string.IsNullOrEmpty(ip))
                {
                    MessageBox.Show("Inserisci l'indirizzo IP della radio", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                btnConnect.IsEnabled = false;
                txtStatus.Text = "Connessione in corso...";

                bool connected = await _connection.ConnectAsync(ip);

                if (connected)
                {
                    btnConnect.Content = "Disconnetti";
                    await _sliceController.RefreshSlicesAsync();
                }

                btnConnect.IsEnabled = true;
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante Scopri Radio
        /// </summary>
        private async void BtnDiscover_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Ricerca radio in corso...";
            btnDiscover.IsEnabled = false;

            var radios = await FlexRadioConnection.DiscoverRadiosAsync();

            btnDiscover.IsEnabled = true;

            if (radios.Count > 0)
            {
                // Mostra dialogo con le radio trovate
                var radio = radios.First();
                txtRadioIp.Text = radio.IpAddress;
                txtStatus.Text = $"Trovata radio: {radio.Model} ({radio.IpAddress})";
            }
            else
            {
                txtStatus.Text = "Nessuna radio trovata";
                MessageBox.Show("Nessuna radio FlexRadio trovata sulla rete", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante Nuova Slice
        /// </summary>
        private async void BtnNewSlice_Click(object sender, RoutedEventArgs e)
        {
            if (!_connection.IsConnected)
            {
                MessageBox.Show("Connetti prima alla radio", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Dialogo per inserire frequenza iniziale
            var dialog = new NewSliceDialog();
            if (dialog.ShowDialog() == true)
            {
                await _sliceController.CreateSliceAsync(dialog.Frequency, dialog.Mode);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante Rimuovi Slice
        /// </summary>
        private async void BtnRemoveSlice_Click(object sender, RoutedEventArgs e)
        {
            // Trova la slice selezionata (questo è semplificato, in una vera app useresti la selezione)
            if (_sliceController.Slices.Count > 0)
            {
                var lastSlice = _sliceController.Slices.Last();
                var result = MessageBox.Show($"Rimuovere la slice {lastSlice.SliceId}?", "Conferma",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _sliceController.RemoveSliceAsync(lastSlice);
                }
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante Aggiorna
        /// </summary>
        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (_connection.IsConnected)
            {
                await _sliceController.RefreshSlicesAsync();
                txtStatus.Text = "Slice aggiornate";
            }
        }

        /// <summary>
        /// Gestisce il cambio di frequenza
        /// </summary>
        private async void TxtFrequency_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is Slice slice)
            {
                if (double.TryParse(textBox.Text, out double frequency))
                {
                    await _sliceController.SetFrequencyAsync(slice, frequency);
                }
            }
        }

        /// <summary>
        /// Gestisce il cambio di modalità
        /// </summary>
        private async void CmbMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.Tag is Slice slice && comboBox.SelectedItem != null)
            {
                string mode = comboBox.SelectedItem.ToString() ?? "USB";
                await _sliceController.SetModeAsync(slice, mode);
            }
        }

        /// <summary>
        /// Gestisce il cambio di filtro
        /// </summary>
        private async void TxtFilter_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is Slice slice)
            {
                await _sliceController.SetFilterAsync(slice, slice.FilterLow, slice.FilterHigh);
            }
        }

        /// <summary>
        /// Gestisce il cambio di RF Gain
        /// </summary>
        private async void SliderRfGain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider && slider.Tag is Slice slice)
            {
                await _sliceController.SetRfGainAsync(slice, e.NewValue);
            }
        }

        /// <summary>
        /// Gestisce il cambio di AF Gain
        /// </summary>
        private async void SliderAfGain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider && slider.Tag is Slice slice)
            {
                await _sliceController.SetAfGainAsync(slice, e.NewValue);
            }
        }

        /// <summary>
        /// Gestisce il click su Noise Blanker
        /// </summary>
        private async void ChkNb_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is Slice slice)
            {
                await _sliceController.SetNoiseBlankerAsync(slice, checkBox.IsChecked ?? false);
            }
        }

        /// <summary>
        /// Gestisce il click su Noise Reduction
        /// </summary>
        private async void ChkNr_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is Slice slice)
            {
                await _sliceController.SetNoiseReductionAsync(slice, checkBox.IsChecked ?? false);
            }
        }

        /// <summary>
        /// Gestisce il click su Auto Notch Filter
        /// </summary>
        private async void ChkAnf_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is Slice slice)
            {
                await _sliceController.SetAutoNotchAsync(slice, checkBox.IsChecked ?? false);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante Set TX
        /// </summary>
        private async void BtnSetTx_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Slice slice)
            {
                await _sliceController.SetTxSliceAsync(slice);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante Lock
        /// </summary>
        private void BtnLockSlice_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Slice slice)
            {
                _sliceController.ToggleLock(slice);
            }
        }

        /// <summary>
        /// Gestisce il click su una banda
        /// </summary>
        private async void BtnBand_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Band band)
            {
                // Applica la banda alla prima slice attiva o crea una nuova slice
                if (_sliceController.Slices.Count > 0)
                {
                    var slice = _sliceController.Slices.First();
                    double centerFreq = (band.StartFrequency + band.EndFrequency) / 2;
                    await _sliceController.SetFrequencyAsync(slice, centerFreq);
                    await _sliceController.SetModeAsync(slice, band.DefaultMode);
                }
                else if (_connection.IsConnected)
                {
                    double centerFreq = (band.StartFrequency + band.EndFrequency) / 2;
                    await _sliceController.CreateSliceAsync(centerFreq, band.DefaultMode);
                }
            }
        }

        /// <summary>
        /// Gestisce il click su Carica Profilo
        /// </summary>
        private async void BtnLoadProfile_Click(object sender, RoutedEventArgs e)
        {
            if (profilesList.SelectedItem is SliceProfile profile && _sliceController.Slices.Count > 0)
            {
                var slice = _sliceController.Slices.First();
                profile.ApplyToSlice(slice);
                await _sliceController.SetFrequencyAsync(slice, slice.Frequency);
                await _sliceController.SetModeAsync(slice, slice.Mode);
                txtStatus.Text = $"Profilo '{profile.Name}' caricato";
            }
        }

        /// <summary>
        /// Gestisce il click su Salva Profilo
        /// </summary>
        private void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_sliceController.Slices.Count > 0)
            {
                var slice = _sliceController.Slices.First();
                var dialog = new SaveProfileDialog();
                if (dialog.ShowDialog() == true)
                {
                    var profile = new SliceProfile
                    {
                        Name = dialog.ProfileName,
                        Frequency = slice.Frequency,
                        Mode = slice.Mode,
                        FilterLow = slice.FilterLow,
                        FilterHigh = slice.FilterHigh,
                        Description = dialog.ProfileDescription
                    };
                    _profiles.Add(profile);
                    txtStatus.Text = $"Profilo '{profile.Name}' salvato";
                }
            }
        }

        /// <summary>
        /// Gestisce il click su Elimina Profilo
        /// </summary>
        private void BtnDeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            if (profilesList.SelectedItem is SliceProfile profile)
            {
                var result = MessageBox.Show($"Eliminare il profilo '{profile.Name}'?", "Conferma",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _profiles.Remove(profile);
                    txtStatus.Text = "Profilo eliminato";
                }
            }
        }

        /// <summary>
        /// Gestisce il cambio di stato della connessione
        /// </summary>
        private void OnConnectionStatusChanged(object? sender, bool isConnected)
        {
            Dispatcher.Invoke(() =>
            {
                if (isConnected)
                {
                    lblConnectionStatus.Content = $"Connesso a {_connection.RadioIp}";
                    lblConnectionStatus.Foreground = new SolidColorBrush(Colors.LightGreen);
                }
                else
                {
                    lblConnectionStatus.Content = "Non connesso";
                    lblConnectionStatus.Foreground = new SolidColorBrush(Colors.Orange);
                }
            });
        }

        /// <summary>
        /// Gestisce gli errori
        /// </summary>
        private void OnErrorOccurred(object? sender, string error)
        {
            Dispatcher.Invoke(() =>
            {
                txtStatus.Text = error;
            });
        }

        /// <summary>
        /// Gestisce i messaggi di stato
        /// </summary>
        private void OnStatusChanged(object? sender, string status)
        {
            Dispatcher.Invoke(() =>
            {
                txtStatus.Text = status;
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _connection.DisconnectAsync().Wait();
        }
    }

    /// <summary>
    /// Dialogo per creare una nuova slice
    /// </summary>
    public class NewSliceDialog : Window
    {
        private TextBox txtFrequency;
        private ComboBox cmbMode;

        public double Frequency { get; private set; }
        public string Mode { get; private set; } = "USB";

        public NewSliceDialog()
        {
            Title = "Nuova Slice";
            Width = 300;
            Height = 180;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));

            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var lblFreq = new Label { Content = "Frequenza (MHz):", Foreground = Brushes.White };
            Grid.SetRow(lblFreq, 0);
            grid.Children.Add(lblFreq);

            txtFrequency = new TextBox { Text = "14.200", Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)), Foreground = Brushes.White };
            Grid.SetRow(txtFrequency, 1);
            grid.Children.Add(txtFrequency);

            var lblMode = new Label { Content = "Modalità:", Foreground = Brushes.White, Margin = new Thickness(0, 10, 0, 0) };
            Grid.SetRow(lblMode, 2);
            grid.Children.Add(lblMode);

            cmbMode = new ComboBox { Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)), Foreground = Brushes.White };
            foreach (var mode in SliceModes.AvailableModes)
            {
                cmbMode.Items.Add(mode);
            }
            cmbMode.SelectedIndex = 1; // USB
            Grid.SetRow(cmbMode, 2);
            cmbMode.Margin = new Thickness(0, 30, 0, 0);
            grid.Children.Add(cmbMode);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
            Grid.SetRow(btnPanel, 3);

            var btnOk = new Button { Content = "OK", Width = 75, Margin = new Thickness(5, 0, 0, 0) };
            btnOk.Click += (s, e) => { Frequency = double.Parse(txtFrequency.Text); Mode = cmbMode.SelectedItem?.ToString() ?? "USB"; DialogResult = true; Close(); };
            btnPanel.Children.Add(btnOk);

            var btnCancel = new Button { Content = "Annulla", Width = 75, Margin = new Thickness(5, 0, 0, 0) };
            btnCancel.Click += (s, e) => { DialogResult = false; Close(); };
            btnPanel.Children.Add(btnCancel);

            grid.Children.Add(btnPanel);

            Content = grid;
        }
    }

    /// <summary>
    /// Dialogo per salvare un profilo
    /// </summary>
    public class SaveProfileDialog : Window
    {
        private TextBox txtName;
        private TextBox txtDescription;

        public string ProfileName { get; private set; } = "";
        public string ProfileDescription { get; private set; } = "";

        public SaveProfileDialog()
        {
            Title = "Salva Profilo";
            Width = 350;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));

            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var lblName = new Label { Content = "Nome profilo:", Foreground = Brushes.White };
            Grid.SetRow(lblName, 0);
            grid.Children.Add(lblName);

            txtName = new TextBox { Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)), Foreground = Brushes.White };
            Grid.SetRow(txtName, 1);
            grid.Children.Add(txtName);

            var lblDesc = new Label { Content = "Descrizione:", Foreground = Brushes.White, Margin = new Thickness(0, 10, 0, 0) };
            Grid.SetRow(lblDesc, 2);
            grid.Children.Add(lblDesc);

            txtDescription = new TextBox { Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)), Foreground = Brushes.White };
            Grid.SetRow(txtDescription, 3);
            grid.Children.Add(txtDescription);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
            Grid.SetRow(btnPanel, 5);

            var btnOk = new Button { Content = "OK", Width = 75, Margin = new Thickness(5, 0, 0, 0) };
            btnOk.Click += (s, e) => { ProfileName = txtName.Text; ProfileDescription = txtDescription.Text; DialogResult = true; Close(); };
            btnPanel.Children.Add(btnOk);

            var btnCancel = new Button { Content = "Annulla", Width = 75, Margin = new Thickness(5, 0, 0, 0) };
            btnCancel.Click += (s, e) => { DialogResult = false; Close(); };
            btnPanel.Children.Add(btnCancel);

            grid.Children.Add(btnPanel);

            Content = grid;
        }
    }
}
