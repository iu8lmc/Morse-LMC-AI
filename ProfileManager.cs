using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace RadioLoggerApp.SliceMaster
{
    /// <summary>
    /// Gestisce il salvataggio e caricamento dei profili slice
    /// </summary>
    public class ProfileManager
    {
        private const string PROFILES_FILE = "slicemaster_profiles.json";
        private readonly string _profilesPath;

        public ProfileManager()
        {
            // Salva i profili nella cartella dell'applicazione
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "SliceMaster");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            _profilesPath = Path.Combine(appFolder, PROFILES_FILE);
        }

        /// <summary>
        /// Carica i profili dal file
        /// </summary>
        public async Task<ObservableCollection<SliceProfile>> LoadProfilesAsync()
        {
            try
            {
                if (!File.Exists(_profilesPath))
                {
                    return new ObservableCollection<SliceProfile>(GetDefaultProfiles());
                }

                string json = await File.ReadAllTextAsync(_profilesPath);
                var profiles = JsonSerializer.Deserialize<SliceProfile[]>(json);

                if (profiles != null && profiles.Length > 0)
                {
                    return new ObservableCollection<SliceProfile>(profiles);
                }

                return new ObservableCollection<SliceProfile>(GetDefaultProfiles());
            }
            catch (Exception)
            {
                return new ObservableCollection<SliceProfile>(GetDefaultProfiles());
            }
        }

        /// <summary>
        /// Salva i profili nel file
        /// </summary>
        public async Task SaveProfilesAsync(ObservableCollection<SliceProfile> profiles)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(profiles, options);
                await File.WriteAllTextAsync(_profilesPath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Errore salvataggio profili: {ex.Message}");
            }
        }

        /// <summary>
        /// Ritorna i profili predefiniti
        /// </summary>
        private SliceProfile[] GetDefaultProfiles()
        {
            return new[]
            {
                new SliceProfile
                {
                    Name = "20m CW",
                    Frequency = 14.050,
                    Mode = "CW",
                    FilterLow = -250,
                    FilterHigh = 250,
                    Description = "Banda 20m per CW"
                },
                new SliceProfile
                {
                    Name = "40m SSB",
                    Frequency = 7.100,
                    Mode = "LSB",
                    FilterLow = -2400,
                    FilterHigh = -100,
                    Description = "Banda 40m per SSB"
                },
                new SliceProfile
                {
                    Name = "20m SSB",
                    Frequency = 14.200,
                    Mode = "USB",
                    FilterLow = 100,
                    FilterHigh = 2400,
                    Description = "Banda 20m per SSB"
                },
                new SliceProfile
                {
                    Name = "10m FM",
                    Frequency = 29.600,
                    Mode = "FM",
                    FilterLow = -8000,
                    FilterHigh = 8000,
                    Description = "Banda 10m per FM"
                },
                new SliceProfile
                {
                    Name = "FT8 20m",
                    Frequency = 14.074,
                    Mode = "DIGU",
                    FilterLow = 100,
                    FilterHigh = 2400,
                    Description = "FT8 sulla banda 20m"
                },
                new SliceProfile
                {
                    Name = "FT8 40m",
                    Frequency = 7.074,
                    Mode = "DIGU",
                    FilterLow = 100,
                    FilterHigh = 2400,
                    Description = "FT8 sulla banda 40m"
                },
                new SliceProfile
                {
                    Name = "RTTY 20m",
                    Frequency = 14.080,
                    Mode = "RTTY",
                    FilterLow = 100,
                    FilterHigh = 2400,
                    Description = "RTTY sulla banda 20m"
                },
                new SliceProfile
                {
                    Name = "2m FM",
                    Frequency = 145.500,
                    Mode = "FM",
                    FilterLow = -8000,
                    FilterHigh = 8000,
                    Description = "Banda 2m per FM"
                }
            };
        }

        /// <summary>
        /// Esporta un profilo in un file separato
        /// </summary>
        public async Task ExportProfileAsync(SliceProfile profile, string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(profile, options);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Errore esportazione profilo: {ex.Message}");
            }
        }

        /// <summary>
        /// Importa un profilo da un file
        /// </summary>
        public async Task<SliceProfile?> ImportProfileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }

                string json = await File.ReadAllTextAsync(filePath);
                var profile = JsonSerializer.Deserialize<SliceProfile>(json);
                return profile;
            }
            catch (Exception ex)
            {
                throw new Exception($"Errore importazione profilo: {ex.Message}");
            }
        }
    }
}
