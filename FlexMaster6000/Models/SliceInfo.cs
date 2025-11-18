using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlexMaster6000.Models
{
    /// <summary>
    /// Represents information about a radio slice
    /// Compatible with SmartSDR 4.0 API
    /// </summary>
    public class SliceInfo : INotifyPropertyChanged
    {
        private string _sliceId;
        private char _sliceLetter;
        private double _frequency;
        private string _mode;
        private bool _isActive;
        private bool _isTxSlice;
        private bool _isDaxEnabled;
        private int _audioGain;
        private int _agcThreshold;
        private string _agcMode;
        private bool _isMuted;
        private bool _isSolo;
        private int _daxChannel;
        private int _daxIqChannel;

        public string SliceId
        {
            get => _sliceId;
            set { _sliceId = value; OnPropertyChanged(); }
        }

        public char SliceLetter
        {
            get => _sliceLetter;
            set { _sliceLetter = value; OnPropertyChanged(); }
        }

        public double Frequency
        {
            get => _frequency;
            set { _frequency = value; OnPropertyChanged(); OnPropertyChanged(nameof(FrequencyMHz)); }
        }

        public string FrequencyMHz => $"{Frequency / 1_000_000:F3} MHz";

        public string Mode
        {
            get => _mode;
            set { _mode = value; OnPropertyChanged(); }
        }

        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        public bool IsTxSlice
        {
            get => _isTxSlice;
            set { _isTxSlice = value; OnPropertyChanged(); }
        }

        public bool IsDaxEnabled
        {
            get => _isDaxEnabled;
            set { _isDaxEnabled = value; OnPropertyChanged(); }
        }

        public int AudioGain
        {
            get => _audioGain;
            set { _audioGain = value; OnPropertyChanged(); }
        }

        public int AgcThreshold
        {
            get => _agcThreshold;
            set { _agcThreshold = value; OnPropertyChanged(); }
        }

        public string AgcMode
        {
            get => _agcMode;
            set { _agcMode = value; OnPropertyChanged(); }
        }

        public bool IsMuted
        {
            get => _isMuted;
            set { _isMuted = value; OnPropertyChanged(); }
        }

        public bool IsSolo
        {
            get => _isSolo;
            set { _isSolo = value; OnPropertyChanged(); }
        }

        public int DaxChannel
        {
            get => _daxChannel;
            set { _daxChannel = value; OnPropertyChanged(); }
        }

        public int DaxIqChannel
        {
            get => _daxIqChannel;
            set { _daxIqChannel = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
