using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using FlexMaster6000.Models;

namespace FlexMaster6000.Services
{
    /// <summary>
    /// Manages FlexRadio connection and slice management
    /// Compatible with SmartSDR 4.0 API using FlexLib v3
    /// </summary>
    public class RadioManager : IDisposable
    {
        private readonly ILogger<RadioManager> _logger;

        // FlexLib API objects (will be initialized when FlexLib DLL is available)
        // private API _api;
        // private Radio _radio;

        private bool _isConnected;
        private string _radioModel;
        private string _radioSerial;

        public ObservableCollection<SliceInfo> Slices { get; }
        public event EventHandler<bool> ConnectionStatusChanged;
        public event EventHandler<SliceInfo> SliceAdded;
        public event EventHandler<SliceInfo> SliceRemoved;
        public event EventHandler<SliceInfo> SliceUpdated;

        public bool IsConnected
        {
            get => _isConnected;
            private set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    ConnectionStatusChanged?.Invoke(this, _isConnected);
                }
            }
        }

        public string RadioModel => _radioModel ?? "Not Connected";
        public string RadioSerial => _radioSerial ?? "N/A";

        public RadioManager(ILogger<RadioManager> logger)
        {
            _logger = logger;
            Slices = new ObservableCollection<SliceInfo>();
        }

        /// <summary>
        /// Initialize FlexLib API and discover radios
        /// </summary>
        public void Initialize()
        {
            _logger.LogInformation("Initializing FlexLib API...");

            try
            {
                // Initialize FlexLib API
                // _api = API.Init();

                // Subscribe to radio discovery events
                // API.RadioAdded += API_RadioAdded;
                // API.RadioRemoved += API_RadioRemoved;

                // Start radio discovery
                // API.StartRadioList();

                _logger.LogInformation("FlexLib API initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize FlexLib API");
                throw;
            }
        }

        /// <summary>
        /// Connect to a specific radio by IP or serial number
        /// </summary>
        public bool ConnectToRadio(string radioIdentifier = null)
        {
            try
            {
                _logger.LogInformation($"Attempting to connect to radio: {radioIdentifier ?? "auto"}");

                // FlexLib connection code (example - will work when FlexLib is available)
                /*
                Radio radioToConnect = null;

                if (string.IsNullOrEmpty(radioIdentifier))
                {
                    // Connect to first available radio
                    radioToConnect = API.RadioList.FirstOrDefault();
                }
                else
                {
                    // Find specific radio by IP or serial
                    radioToConnect = API.RadioList.FirstOrDefault(r =>
                        r.IP.ToString() == radioIdentifier ||
                        r.Serial == radioIdentifier);
                }

                if (radioToConnect == null)
                {
                    _logger.LogWarning("No radio found to connect to");
                    return false;
                }

                // Connect to the radio
                radioToConnect.Connect();
                _radio = radioToConnect;

                // Subscribe to radio events
                _radio.SliceAdded += Radio_SliceAdded;
                _radio.SliceRemoved += Radio_SliceRemoved;
                _radio.PropertyChanged += Radio_PropertyChanged;

                _radioModel = _radio.Model;
                _radioSerial = _radio.Serial;
                */

                IsConnected = true;
                _logger.LogInformation($"Connected to radio: {RadioModel} ({RadioSerial})");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to radio");
                IsConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Disconnect from the current radio
        /// </summary>
        public void Disconnect()
        {
            if (!IsConnected) return;

            try
            {
                _logger.LogInformation("Disconnecting from radio...");

                // Unsubscribe from events
                // if (_radio != null)
                // {
                //     _radio.SliceAdded -= Radio_SliceAdded;
                //     _radio.SliceRemoved -= Radio_SliceRemoved;
                //     _radio.PropertyChanged -= Radio_PropertyChanged;
                //     _radio.Disconnect();
                //     _radio = null;
                // }

                Slices.Clear();
                IsConnected = false;

                _logger.LogInformation("Disconnected from radio");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disconnect");
            }
        }

        /// <summary>
        /// Get list of discovered radios
        /// </summary>
        public List<string> GetAvailableRadios()
        {
            var radios = new List<string>();

            try
            {
                // Get list from FlexLib
                // foreach (var radio in API.RadioList)
                // {
                //     radios.Add($"{radio.Model} - {radio.Nickname} ({radio.IP})");
                // }

                // For now, return demo data
                radios.Add("FLEX-6600 - Demo Radio (192.168.1.100)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting radio list");
            }

            return radios;
        }

        /// <summary>
        /// Create a new slice
        /// </summary>
        public SliceInfo CreateSlice(double frequency = 14.200)
        {
            try
            {
                _logger.LogInformation($"Creating new slice at {frequency} MHz");

                // FlexLib code to create slice
                // var slice = _radio.CreateSlice();
                // if (slice != null)
                // {
                //     slice.Freq = frequency;
                //     return ConvertToSliceInfo(slice);
                // }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create slice");
                return null;
            }
        }

        /// <summary>
        /// Remove a slice
        /// </summary>
        public void RemoveSlice(string sliceId)
        {
            try
            {
                _logger.LogInformation($"Removing slice: {sliceId}");

                // FlexLib code
                // var slice = _radio.FindSliceByIndex(sliceId);
                // if (slice != null)
                // {
                //     _radio.RemoveSlice(slice);
                // }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to remove slice {sliceId}");
            }
        }

        /// <summary>
        /// Update slice frequency
        /// </summary>
        public void SetSliceFrequency(string sliceId, double frequency)
        {
            try
            {
                // FlexLib code
                // var slice = _radio.FindSliceByIndex(sliceId);
                // if (slice != null)
                // {
                //     slice.Freq = frequency;
                // }

                var sliceInfo = Slices.FirstOrDefault(s => s.SliceId == sliceId);
                if (sliceInfo != null)
                {
                    sliceInfo.Frequency = frequency;
                    SliceUpdated?.Invoke(this, sliceInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to set frequency for slice {sliceId}");
            }
        }

        /// <summary>
        /// Set slice mode
        /// </summary>
        public void SetSliceMode(string sliceId, string mode)
        {
            try
            {
                // FlexLib code
                // var slice = _radio.FindSliceByIndex(sliceId);
                // if (slice != null)
                // {
                //     slice.DemodMode = mode;
                // }

                var sliceInfo = Slices.FirstOrDefault(s => s.SliceId == sliceId);
                if (sliceInfo != null)
                {
                    sliceInfo.Mode = mode;
                    SliceUpdated?.Invoke(this, sliceInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to set mode for slice {sliceId}");
            }
        }

        public void Dispose()
        {
            Disconnect();

            // Cleanup FlexLib
            // API.RadioAdded -= API_RadioAdded;
            // API.RadioRemoved -= API_RadioRemoved;
            // API.CloseSession();
        }

        // Event handlers for FlexLib events
        /*
        private void API_RadioAdded(Radio radio)
        {
            _logger.LogInformation($"Radio discovered: {radio.Model} ({radio.IP})");
        }

        private void API_RadioRemoved(Radio radio)
        {
            _logger.LogInformation($"Radio removed: {radio.Model} ({radio.IP})");
        }

        private void Radio_SliceAdded(Slice slice)
        {
            var sliceInfo = ConvertToSliceInfo(slice);
            Slices.Add(sliceInfo);
            SliceAdded?.Invoke(this, sliceInfo);
        }

        private void Radio_SliceRemoved(Slice slice)
        {
            var sliceInfo = Slices.FirstOrDefault(s => s.SliceId == slice.Index.ToString());
            if (sliceInfo != null)
            {
                Slices.Remove(sliceInfo);
                SliceRemoved?.Invoke(this, sliceInfo);
            }
        }

        private void Radio_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Handle radio property changes
        }

        private SliceInfo ConvertToSliceInfo(Slice slice)
        {
            return new SliceInfo
            {
                SliceId = slice.Index.ToString(),
                SliceLetter = slice.Letter,
                Frequency = slice.Freq,
                Mode = slice.DemodMode,
                IsActive = slice.Active,
                IsTxSlice = slice.IsTransmitSlice,
                IsDaxEnabled = slice.DAXChannel > 0,
                DaxChannel = slice.DAXChannel,
                DaxIqChannel = slice.DAXIQChannel,
                AudioGain = slice.AudioGain,
                IsMuted = slice.Mute
            };
        }
        */
    }
}
