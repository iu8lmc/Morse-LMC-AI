using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RadioLoggerApp.SliceMaster
{
    /// <summary>
    /// Controller per la gestione delle slice della radio FlexRadio
    /// </summary>
    public class SliceController
    {
        private readonly FlexRadioConnection _connection;
        private readonly ObservableCollection<Slice> _slices;

        public ObservableCollection<Slice> Slices => _slices;
        public event EventHandler<string>? StatusChanged;

        public SliceController(FlexRadioConnection connection)
        {
            _connection = connection;
            _slices = new ObservableCollection<Slice>();

            // Sottoscrivi agli eventi della connessione
            _connection.MessageReceived += OnMessageReceived;
        }

        /// <summary>
        /// Crea una nuova slice
        /// </summary>
        public async Task<Slice?> CreateSliceAsync(double frequency, string mode = "USB")
        {
            try
            {
                // Trova il prossimo ID disponibile
                int nextId = _slices.Count > 0 ? _slices.Max(s => s.SliceId) + 1 : 0;

                // Invia comando per creare la slice
                await _connection.SendCommandAsync($"slice create {frequency:F6} {mode}");

                // Crea l'oggetto slice localmente
                var slice = new Slice
                {
                    SliceId = nextId,
                    Frequency = frequency,
                    Mode = mode,
                    FilterLow = SliceModes.GetDefaultFilterLow(mode),
                    FilterHigh = SliceModes.GetDefaultFilterHigh(mode),
                    IsActive = true,
                    RfGain = 50,
                    AfGain = 50
                };

                _slices.Add(slice);
                StatusChanged?.Invoke(this, $"Slice {nextId} creata: {frequency:F6} MHz {mode}");

                return slice;
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Errore creazione slice: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Rimuove una slice
        /// </summary>
        public async Task RemoveSliceAsync(Slice slice)
        {
            try
            {
                await _connection.SendCommandAsync($"slice remove {slice.SliceId}");
                _slices.Remove(slice);
                StatusChanged?.Invoke(this, $"Slice {slice.SliceId} rimossa");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Errore rimozione slice: {ex.Message}");
            }
        }

        /// <summary>
        /// Imposta la frequenza di una slice
        /// </summary>
        public async Task SetFrequencyAsync(Slice slice, double frequency)
        {
            try
            {
                await _connection.SendCommandAsync($"slice set {slice.SliceId} freq={frequency:F6}");
                slice.Frequency = frequency;
                StatusChanged?.Invoke(this, $"Slice {slice.SliceId} freq: {frequency:F6} MHz");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Errore impostazione frequenza: {ex.Message}");
            }
        }

        /// <summary>
        /// Imposta la modalità di una slice
        /// </summary>
        public async Task SetModeAsync(Slice slice, string mode)
        {
            try
            {
                await _connection.SendCommandAsync($"slice set {slice.SliceId} mode={mode}");
                slice.Mode = mode;

                // Aggiorna anche i filtri predefiniti per la nuova modalità
                slice.FilterLow = SliceModes.GetDefaultFilterLow(mode);
                slice.FilterHigh = SliceModes.GetDefaultFilterHigh(mode);

                await SetFilterAsync(slice, slice.FilterLow, slice.FilterHigh);

                StatusChanged?.Invoke(this, $"Slice {slice.SliceId} mode: {mode}");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Errore impostazione modalità: {ex.Message}");
            }
        }

        /// <summary>
        /// Imposta i filtri di una slice
        /// </summary>
        public async Task SetFilterAsync(Slice slice, int filterLow, int filterHigh)
        {
            try
            {
                await _connection.SendCommandAsync($"slice set {slice.SliceId} filter_lo={filterLow} filter_hi={filterHigh}");
                slice.FilterLow = filterLow;
                slice.FilterHigh = filterHigh;
                StatusChanged?.Invoke(this, $"Slice {slice.SliceId} filtro: {filterLow} - {filterHigh} Hz");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Errore impostazione filtro: {ex.Message}");
            }
        }

        /// <summary>
        /// Imposta il guadagno RF di una slice
        /// </summary>
        public async Task SetRfGainAsync(Slice slice, double gain)
        {
            try
            {
                await _connection.SendCommandAsync($"slice set {slice.SliceId} rfgain={gain}");
                slice.RfGain = gain;
                StatusChanged?.Invoke(this, $"Slice {slice.SliceId} RF gain: {gain:F1} dB");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Errore impostazione RF gain: {ex.Message}");
            }
        }

        /// <summary>
        /// Imposta il guadagno AF di una slice
        /// </summary>
        public async Task SetAfGainAsync(Slice slice, double gain)
        {
            try
            {
                await _connection.SendCommandAsync($"slice set {slice.SliceId} afgain={gain}");
                slice.AfGain = gain;
                StatusChanged?.Invoke(this, $"Slice {slice.SliceId} AF gain: {gain:F1}");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Errore impostazione AF gain: {ex.Message}");
            }
        }

        /// <summary>
        /// Abilita/disabilita il Noise Blanker
        /// </summary>
        public async Task SetNoiseBlankerAsync(Slice slice, bool enabled)
        {
            try
            {
                await _connection.SendCommandAsync($"slice set {slice.SliceId} nb={enabled}");
                slice.NbEnabled = enabled;
                StatusChanged?.Invoke(this, $"Slice {slice.SliceId} NB: {(enabled ? "ON" : "OFF")}");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Errore impostazione NB: {ex.Message}");
            }
        }

        /// <summary>
        /// Abilita/disabilita il Noise Reduction
        /// </summary>
        public async Task SetNoiseReductionAsync(Slice slice, bool enabled)
        {
            try
            {
                await _connection.SendCommandAsync($"slice set {slice.SliceId} nr={enabled}");
                slice.NrEnabled = enabled;
                StatusChanged?.Invoke(this, $"Slice {slice.SliceId} NR: {(enabled ? "ON" : "OFF")}");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Errore impostazione NR: {ex.Message}");
            }
        }

        /// <summary>
        /// Abilita/disabilita l'Auto Notch Filter
        /// </summary>
        public async Task SetAutoNotchAsync(Slice slice, bool enabled)
        {
            try
            {
                await _connection.SendCommandAsync($"slice set {slice.SliceId} anf={enabled}");
                slice.AnfEnabled = enabled;
                StatusChanged?.Invoke(this, $"Slice {slice.SliceId} ANF: {(enabled ? "ON" : "OFF")}");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Errore impostazione ANF: {ex.Message}");
            }
        }

        /// <summary>
        /// Imposta la slice come TX
        /// </summary>
        public async Task SetTxSliceAsync(Slice slice)
        {
            try
            {
                // Disabilita TX su tutte le altre slice
                foreach (var s in _slices)
                {
                    s.IsTx = false;
                }

                await _connection.SendCommandAsync($"xmit slice {slice.SliceId}");
                slice.IsTx = true;
                StatusChanged?.Invoke(this, $"Slice {slice.SliceId} impostata come TX");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Errore impostazione TX: {ex.Message}");
            }
        }

        /// <summary>
        /// Blocca/sblocca una slice
        /// </summary>
        public void ToggleLock(Slice slice)
        {
            slice.IsLocked = !slice.IsLocked;
            StatusChanged?.Invoke(this, $"Slice {slice.SliceId} {(slice.IsLocked ? "bloccata" : "sbloccata")}");
        }

        /// <summary>
        /// Gestisce i messaggi ricevuti dalla radio
        /// </summary>
        private void OnMessageReceived(object? sender, string message)
        {
            try
            {
                // Parser dei messaggi dalla radio
                // Formato tipico: S<sliceId>|<parametro>=<valore>
                if (message.StartsWith("S") && message.Contains("|"))
                {
                    var parts = message.Split('|');
                    if (parts.Length >= 2)
                    {
                        // Estrai l'ID della slice
                        var sliceIdStr = parts[0].Substring(1);
                        if (int.TryParse(sliceIdStr, out int sliceId))
                        {
                            var slice = _slices.FirstOrDefault(s => s.SliceId == sliceId);
                            if (slice != null)
                            {
                                // Aggiorna i parametri della slice
                                foreach (var param in parts.Skip(1))
                                {
                                    var keyValue = param.Split('=');
                                    if (keyValue.Length == 2)
                                    {
                                        UpdateSliceParameter(slice, keyValue[0], keyValue[1]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignora errori di parsing
            }
        }

        /// <summary>
        /// Aggiorna un parametro di una slice
        /// </summary>
        private void UpdateSliceParameter(Slice slice, string parameter, string value)
        {
            switch (parameter.ToLower())
            {
                case "freq":
                    if (double.TryParse(value, out double freq))
                        slice.Frequency = freq;
                    break;
                case "mode":
                    slice.Mode = value;
                    break;
                case "filter_lo":
                    if (int.TryParse(value, out int filterLo))
                        slice.FilterLow = filterLo;
                    break;
                case "filter_hi":
                    if (int.TryParse(value, out int filterHi))
                        slice.FilterHigh = filterHi;
                    break;
                case "rfgain":
                    if (double.TryParse(value, out double rfGain))
                        slice.RfGain = rfGain;
                    break;
                case "afgain":
                    if (double.TryParse(value, out double afGain))
                        slice.AfGain = afGain;
                    break;
                case "nb":
                    slice.NbEnabled = value.ToLower() == "1" || value.ToLower() == "true";
                    break;
                case "nr":
                    slice.NrEnabled = value.ToLower() == "1" || value.ToLower() == "true";
                    break;
                case "anf":
                    slice.AnfEnabled = value.ToLower() == "1" || value.ToLower() == "true";
                    break;
            }
        }

        /// <summary>
        /// Richiede lo stato corrente di tutte le slice
        /// </summary>
        public async Task RefreshSlicesAsync()
        {
            try
            {
                await _connection.SendCommandAsync("slice list");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Errore refresh slice: {ex.Message}");
            }
        }
    }
}
