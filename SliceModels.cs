using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RadioLoggerApp.SliceMaster
{
    /// <summary>
    /// Rappresenta una Slice (ricevitore virtuale) della radio FlexRadio
    /// </summary>
    public class Slice : INotifyPropertyChanged
    {
        private int _sliceId;
        private double _frequency;
        private string _mode = "USB";
        private int _filterLow;
        private int _filterHigh;
        private bool _isActive;
        private bool _isTx;
        private int _rxAntenna;
        private int _txAntenna;
        private double _rfGain;
        private double _afGain;
        private bool _nbEnabled;
        private bool _nrEnabled;
        private bool _anfEnabled;
        private int _agcMode;
        private double _agcThreshold;
        private bool _isLocked;

        public int SliceId
        {
            get => _sliceId;
            set { _sliceId = value; OnPropertyChanged(); }
        }

        public double Frequency
        {
            get => _frequency;
            set { _frequency = value; OnPropertyChanged(); OnPropertyChanged(nameof(FrequencyMHz)); }
        }

        public string FrequencyMHz => $"{_frequency:F6} MHz";

        public string Mode
        {
            get => _mode;
            set { _mode = value; OnPropertyChanged(); }
        }

        public int FilterLow
        {
            get => _filterLow;
            set { _filterLow = value; OnPropertyChanged(); }
        }

        public int FilterHigh
        {
            get => _filterHigh;
            set { _filterHigh = value; OnPropertyChanged(); }
        }

        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        public bool IsTx
        {
            get => _isTx;
            set { _isTx = value; OnPropertyChanged(); }
        }

        public int RxAntenna
        {
            get => _rxAntenna;
            set { _rxAntenna = value; OnPropertyChanged(); }
        }

        public int TxAntenna
        {
            get => _txAntenna;
            set { _txAntenna = value; OnPropertyChanged(); }
        }

        public double RfGain
        {
            get => _rfGain;
            set { _rfGain = value; OnPropertyChanged(); }
        }

        public double AfGain
        {
            get => _afGain;
            set { _afGain = value; OnPropertyChanged(); }
        }

        public bool NbEnabled
        {
            get => _nbEnabled;
            set { _nbEnabled = value; OnPropertyChanged(); }
        }

        public bool NrEnabled
        {
            get => _nrEnabled;
            set { _nrEnabled = value; OnPropertyChanged(); }
        }

        public bool AnfEnabled
        {
            get => _anfEnabled;
            set { _anfEnabled = value; OnPropertyChanged(); }
        }

        public int AgcMode
        {
            get => _agcMode;
            set { _agcMode = value; OnPropertyChanged(); }
        }

        public double AgcThreshold
        {
            get => _agcThreshold;
            set { _agcThreshold = value; OnPropertyChanged(); }
        }

        public bool IsLocked
        {
            get => _isLocked;
            set { _isLocked = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Rappresenta un profilo di configurazione per una slice
    /// </summary>
    public class SliceProfile : INotifyPropertyChanged
    {
        private string _name = "";
        private double _frequency;
        private string _mode = "USB";
        private int _filterLow;
        private int _filterHigh;
        private string _description = "";

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public double Frequency
        {
            get => _frequency;
            set { _frequency = value; OnPropertyChanged(); }
        }

        public string Mode
        {
            get => _mode;
            set { _mode = value; OnPropertyChanged(); }
        }

        public int FilterLow
        {
            get => _filterLow;
            set { _filterLow = value; OnPropertyChanged(); }
        }

        public int FilterHigh
        {
            get => _filterHigh;
            set { _filterHigh = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Applica il profilo a una slice
        /// </summary>
        public void ApplyToSlice(Slice slice)
        {
            slice.Frequency = Frequency;
            slice.Mode = Mode;
            slice.FilterLow = FilterLow;
            slice.FilterHigh = FilterHigh;
        }
    }

    /// <summary>
    /// Gestisce le modalità disponibili per le slice
    /// </summary>
    public static class SliceModes
    {
        public static readonly string[] AvailableModes = new[]
        {
            "LSB", "USB", "AM", "CW", "DIGL", "DIGU", "SAM", "FM", "NFM", "DFM", "RTTY"
        };

        public static int GetDefaultFilterLow(string mode)
        {
            return mode switch
            {
                "LSB" => -2400,
                "USB" => 100,
                "CW" => -250,
                "AM" => -4000,
                "SAM" => -4000,
                "FM" => -8000,
                "NFM" => -4000,
                "DFM" => -4000,
                "DIGL" => -2400,
                "DIGU" => 100,
                "RTTY" => 100,
                _ => -2400
            };
        }

        public static int GetDefaultFilterHigh(string mode)
        {
            return mode switch
            {
                "LSB" => -100,
                "USB" => 2400,
                "CW" => 250,
                "AM" => 4000,
                "SAM" => 4000,
                "FM" => 8000,
                "NFM" => 4000,
                "DFM" => 4000,
                "DIGL" => -100,
                "DIGU" => 2400,
                "RTTY" => 2400,
                _ => 2400
            };
        }
    }

    /// <summary>
    /// Gestisce le bande radio disponibili
    /// </summary>
    public class Band
    {
        public string Name { get; set; } = "";
        public double StartFrequency { get; set; }
        public double EndFrequency { get; set; }
        public string DefaultMode { get; set; } = "USB";

        public static readonly ObservableCollection<Band> HamBands = new()
        {
            new Band { Name = "160m", StartFrequency = 1.800, EndFrequency = 2.000, DefaultMode = "LSB" },
            new Band { Name = "80m", StartFrequency = 3.500, EndFrequency = 4.000, DefaultMode = "LSB" },
            new Band { Name = "60m", StartFrequency = 5.250, EndFrequency = 5.450, DefaultMode = "USB" },
            new Band { Name = "40m", StartFrequency = 7.000, EndFrequency = 7.300, DefaultMode = "LSB" },
            new Band { Name = "30m", StartFrequency = 10.100, EndFrequency = 10.150, DefaultMode = "CW" },
            new Band { Name = "20m", StartFrequency = 14.000, EndFrequency = 14.350, DefaultMode = "USB" },
            new Band { Name = "17m", StartFrequency = 18.068, EndFrequency = 18.168, DefaultMode = "USB" },
            new Band { Name = "15m", StartFrequency = 21.000, EndFrequency = 21.450, DefaultMode = "USB" },
            new Band { Name = "12m", StartFrequency = 24.890, EndFrequency = 24.990, DefaultMode = "USB" },
            new Band { Name = "10m", StartFrequency = 28.000, EndFrequency = 29.700, DefaultMode = "USB" },
            new Band { Name = "6m", StartFrequency = 50.000, EndFrequency = 54.000, DefaultMode = "USB" },
            new Band { Name = "2m", StartFrequency = 144.000, EndFrequency = 148.000, DefaultMode = "FM" },
        };
    }
}
