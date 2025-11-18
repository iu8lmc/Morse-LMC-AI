using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.Logging;
using FlexMaster6000.Services;
using FlexMaster6000.Models;

namespace FlexMaster6000.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILogger<MainWindow> _logger;
        private readonly RadioManager _radioManager;
        private readonly AudioMixerService _audioMixer;
        private HrdTcpServer _hrdServer;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize services
            _logger = App.LoggerFactory.CreateLogger<MainWindow>();
            _radioManager = new RadioManager(App.LoggerFactory.CreateLogger<RadioManager>());
            _audioMixer = new AudioMixerService(App.LoggerFactory.CreateLogger<AudioMixerService>(), _radioManager);

            // Subscribe to events
            _radioManager.ConnectionStatusChanged += RadioManager_ConnectionStatusChanged;
            _radioManager.SliceAdded += RadioManager_SliceAdded;
            _radioManager.SliceRemoved += RadioManager_SliceRemoved;

            // Bind slices to UI
            SlicesDataGrid.ItemsSource = _radioManager.Slices;
            MixerItemsControl.ItemsSource = _radioManager.Slices;

            _logger.LogInformation("FlexMaster 6000 initialized");

            // Initialize FlexLib
            try
            {
                _radioManager.Initialize();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize radio manager");
                MessageBox.Show(
                    "Failed to initialize FlexLib API. Make sure FlexLib DLL is in the Libs folder.\n\n" +
                    "Download FlexLib from: https://www.flexradio.com/software/flexlib_api_v3/",
                    "Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        #region Menu Event Handlers

        private void MenuConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusMessage.Text = "Connecting to radio...";

                var availableRadios = _radioManager.GetAvailableRadios();

                if (availableRadios.Count == 0)
                {
                    MessageBox.Show(
                        "No FlexRadio 6000 series radios discovered.\n\n" +
                        "Please ensure:\n" +
                        "1. SmartSDR is running\n" +
                        "2. Radio is powered on and connected to network\n" +
                        "3. Firewall allows FlexMaster to communicate",
                        "No Radios Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    StatusMessage.Text = "No radios found";
                    return;
                }

                // For now, connect to first available radio
                // In production, show selection dialog
                bool success = _radioManager.ConnectToRadio();

                if (success)
                {
                    StatusMessage.Text = "Connected successfully";
                    _logger.LogInformation("Connected to radio");
                }
                else
                {
                    StatusMessage.Text = "Connection failed";
                    MessageBox.Show("Failed to connect to radio", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to radio");
                StatusMessage.Text = "Connection error";
                MessageBox.Show($"Error: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuDisconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _radioManager.Disconnect();
                StatusMessage.Text = "Disconnected from radio";
                _logger.LogInformation("Disconnected from radio");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from radio");
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuAddSlice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_radioManager.IsConnected)
                {
                    MessageBox.Show("Please connect to a radio first", "Not Connected", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var slice = _radioManager.CreateSlice();
                if (slice != null)
                {
                    StatusMessage.Text = $"Created slice {slice.SliceLetter}";
                    _logger.LogInformation($"Created new slice: {slice.SliceLetter}");
                }
                else
                {
                    MessageBox.Show("Failed to create slice", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating slice");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuRemoveSlice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedSlice = SlicesDataGrid.SelectedItem as SliceInfo;
                if (selectedSlice == null)
                {
                    MessageBox.Show("Please select a slice to remove", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Remove slice {selectedSlice.SliceLetter}?",
                    "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    _radioManager.RemoveSlice(selectedSlice.SliceId);
                    StatusMessage.Text = $"Removed slice {selectedSlice.SliceLetter}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing slice");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuStartHrdTcp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_hrdServer != null && _hrdServer.Port > 0)
                {
                    MessageBox.Show("HRD TCP server is already running", "Already Running", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                int port = 7809;
                if (int.TryParse(HrdPortTextBox.Text, out int customPort))
                {
                    port = customPort;
                }

                _hrdServer = new HrdTcpServer(
                    App.LoggerFactory.CreateLogger<HrdTcpServer>(),
                    _radioManager,
                    port
                );

                _hrdServer.StartAsync().Wait();

                HrdServerStatus.Text = $"Running on port {port}";
                HrdServerStatus.Foreground = new SolidColorBrush(Colors.LimeGreen);
                StatusMessage.Text = $"HRD TCP server started on port {port}";

                _logger.LogInformation($"HRD TCP server started on port {port}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting HRD TCP server");
                MessageBox.Show($"Error: {ex.Message}", "Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuStopHrdTcp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_hrdServer == null)
                {
                    MessageBox.Show("HRD TCP server is not running", "Not Running", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                _hrdServer.Stop();
                _hrdServer.Dispose();
                _hrdServer = null;

                HrdServerStatus.Text = "Stopped";
                HrdServerStatus.Foreground = new SolidColorBrush(Colors.Orange);
                StatusMessage.Text = "HRD TCP server stopped";

                _logger.LogInformation("HRD TCP server stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping HRD TCP server");
                MessageBox.Show($"Error: {ex.Message}", "Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "FlexMaster 6000 v1.0.0\n\n" +
                "Enhanced slice control for Flex 6000 series radios\n" +
                "Compatible with SmartSDR 4.0\n\n" +
                "Built with FlexLib API v3\n" +
                "Open Source Software\n\n" +
                "Features:\n" +
                "• Audio mixer with solo/mute\n" +
                "• HRD TCP listener\n" +
                "• CAT over TCP\n" +
                "• Slice synchronization\n" +
                "• Modern WPF interface",
                "About FlexMaster 6000",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void MenuDocs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/K1DBO/slice-master-6000",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening documentation");
            }
        }

        private void RefreshSlices_Click(object sender, RoutedEventArgs e)
        {
            StatusMessage.Text = "Refreshing slices...";
            // Refresh is automatic through data binding
            StatusMessage.Text = $"Showing {_radioManager.Slices.Count} slices";
        }

        #endregion

        #region Event Handlers

        private void RadioManager_ConnectionStatusChanged(object sender, bool isConnected)
        {
            Dispatcher.Invoke(() =>
            {
                if (isConnected)
                {
                    ConnectionIndicator.Fill = new SolidColorBrush(Colors.LimeGreen);
                    ConnectionStatus.Text = "Connected";
                    StatusRadio.Text = $"Radio: {_radioManager.RadioModel}";
                    StatusModel.Text = $"Serial: {_radioManager.RadioSerial}";
                    OverviewModel.Text = _radioManager.RadioModel;
                    OverviewSerial.Text = _radioManager.RadioSerial;
                }
                else
                {
                    ConnectionIndicator.Fill = new SolidColorBrush(Colors.Red);
                    ConnectionStatus.Text = "Disconnected";
                    StatusRadio.Text = "Radio: Not Connected";
                    StatusModel.Text = "Model: N/A";
                    OverviewModel.Text = "Not Connected";
                    OverviewSerial.Text = "N/A";
                    OverviewSliceCount.Text = "0";
                }
            });
        }

        private void RadioManager_SliceAdded(object sender, SliceInfo slice)
        {
            Dispatcher.Invoke(() =>
            {
                OverviewSliceCount.Text = _radioManager.Slices.Count.ToString();
                StatusMessage.Text = $"Slice {slice.SliceLetter} added";
            });
        }

        private void RadioManager_SliceRemoved(object sender, SliceInfo slice)
        {
            Dispatcher.Invoke(() =>
            {
                OverviewSliceCount.Text = _radioManager.Slices.Count.ToString();
                StatusMessage.Text = $"Slice {slice.SliceLetter} removed";
            });
        }

        #endregion

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // Cleanup
                _hrdServer?.Stop();
                _hrdServer?.Dispose();
                _radioManager?.Disconnect();
                _radioManager?.Dispose();

                _logger.LogInformation("FlexMaster 6000 shutting down");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during shutdown");
            }

            base.OnClosing(e);
        }
    }
}
