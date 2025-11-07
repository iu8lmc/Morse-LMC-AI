// Prima posiziona tutte le dichiarazioni 'using' all'inizio del file
using System.Windows;
using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
// Altri 'using' necessari...

namespace RadioLoggerApp
{
    public partial class MainWindow : Window
{
    private SQLiteDBHandler dbHandler;
    public ObservableCollection<LogEntry> LogEntries { get; set; }

    public MainWindow()
    {
        try
        {
            InitializeComponent();

            try
            {
                dbHandler = new SQLiteDBHandler("radiologger.db");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Attenzione: Impossibile inizializzare il database.\n\n" +
                    $"L'applicazione funzionerà in modalità limitata.\n\n" +
                    $"Errore: {ex.Message}",
                    "Avviso Database",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                // Continua comunque con un handler null (gestito dopo)
                dbHandler = null!;
            }

            LogEntries = new ObservableCollection<LogEntry>();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore critico durante l'inizializzazione di MainWindow:\n\n" +
                $"{ex.Message}\n\n" +
                $"Tipo: {ex.GetType().Name}\n\n" +
                $"Stack Trace:\n{ex.StackTrace}",
                "Errore Critico",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            throw;
        }
    }
     
    private void NewLog_Click(object sender, RoutedEventArgs e)
    {
        // Reset or clear the log entries or input fields
        // Example: Clearing a text field or resetting a data collection
        // textBoxLogEntry.Text = "";
        // logEntries.Clear();

        // Add any other initialization or reset logic here
    }
   

   private void AddLogEntry_Click(object sender, RoutedEventArgs e)
{
    // Crea un'istanza di LogEntry con i dati da aggiungere
    LogEntry newLogEntry = new LogEntry
    {
        // Supponiamo che LogEntry abbia queste proprietà e che tu le prenda da alcuni controlli dell'interfaccia utente
        CallSign = txtCallSign.Text,
        Band = txtBand.Text,
        Mode = txtMode.Text,
        QsoDate = DateTime.Now, // O la data selezionata dall'utente
        ReportSent = txtReportSent.Text,
        ReportReceived = txtReportReceived.Text
    };

    try
    {
        using (SQLiteConnection connection = new SQLiteConnection(dbHandler.ConnectionString))
        {
            connection.Open();

            string sql = "INSERT INTO LogEntries (CallSign, Band, Mode, QsoDate, ReportSent, ReportReceived) VALUES (@CallSign, @Band, @Mode, @QsoDate, @ReportSent, @ReportReceived)";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@CallSign", newLogEntry.CallSign);
                command.Parameters.AddWithValue("@Band", newLogEntry.Band);
                command.Parameters.AddWithValue("@Mode", newLogEntry.Mode);
                command.Parameters.AddWithValue("@QsoDate", newLogEntry.QsoDate);
                command.Parameters.AddWithValue("@ReportSent", newLogEntry.ReportSent);
                command.Parameters.AddWithValue("@ReportReceived", newLogEntry.ReportReceived);

                command.ExecuteNonQuery();
            }
        }

        MessageBox.Show("Log entry added successfully!");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error while adding log entry: {ex.Message}");
    }
}

    private void SaveLog_Click(object sender, RoutedEventArgs e)
    {
        



    // Implementa la logica per "New Log"

    // Implementa la logica per "Save Log"
}

private void LoadLog_Click(object sender, RoutedEventArgs e)
{
    // Implementa la logica per "Load Log"
}

private void LoadLogEntries()
{
    using (SQLiteConnection connection = new SQLiteConnection(dbHandler.ConnectionString))
    {
        connection.Open();
        string selectQuery = "SELECT * FROM LogEntries";
        using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
        {
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // Crea un nuovo LogEntry e aggiungilo alla lista
                    LogEntry entry = new LogEntry
                    {
                        CallSign = reader["CallSign"] != DBNull.Value ? reader["CallSign"]?.ToString() ?? string.Empty : string.Empty,
                        Band = reader["Band"] != DBNull.Value ? reader["Band"]?.ToString() ?? string.Empty : string.Empty,

                        // Aggiungi qui la stessa logica per gli altri campi
                       Mode = reader["Mode"] != DBNull.Value ? reader["Mode"]?.ToString() ?? string.Empty : string.Empty,
                        // Assumi che QsoDate sia un campo di tipo data
                        QsoDate = reader["QsoDate"] != DBNull.Value ? Convert.ToDateTime(reader["QsoDate"]) : DateTime.MinValue,
                        ReportSent = reader["ReportSent"] != DBNull.Value ? reader["ReportSent"]?.ToString() ?? string.Empty : string.Empty,
                        ReportReceived = reader["ReportReceived"] != DBNull.Value ? reader["ReportReceived"]?.ToString() ?? string.Empty : string.Empty,

                        
                    };
                    LogEntries.Add(entry);
                }
            }
        }
    }
}

       
       
       

      

        private void AddLog_Click(object sender, RoutedEventArgs e)
        {
            // Creazione di un nuovo LogEntry da aggiungere al database.
            LogEntry newLogEntry = new LogEntry
            {
                CallSign = txtCallSign.Text,
                Band = txtBand.Text,
                Mode = txtMode.Text,
                QsoDate = dpQsoDate.SelectedDate ?? DateTime.Now, // Usa la data attuale se nessuna data è selezionata.
                ReportSent = txtReportSent.Text,
                ReportReceived = txtReportReceived.Text,
                
            };

            dbHandler.SaveLogEntry(newLogEntry); // Salvataggio dell'entry nel database.
        }

        private void dgLogEntries_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void txtCallSign_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void BtnMorseDecoder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var morseWindow = new MorseDecoder.MorseDecoderWindow();
                morseWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nell'apertura del Morse Decoder: {ex.Message}",
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}




   
        
    

 
    
                   


    // Gli altri metodi...
