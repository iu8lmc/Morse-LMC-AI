using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.IO;

namespace RadioLoggerApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Gestione errori non gestiti
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Log di avvio
                LogStartup("Applicazione avviata correttamente");
            }
            catch (Exception ex)
            {
                ShowError("Errore durante l'avvio dell'applicazione", ex);
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowError("Errore non gestito nell'applicazione", e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                ShowError("Errore critico nell'applicazione", ex);
            }
        }

        private void ShowError(string title, Exception ex)
        {
            string errorMessage = $"{title}\n\n" +
                                $"Messaggio: {ex.Message}\n\n" +
                                $"Tipo: {ex.GetType().Name}\n\n" +
                                $"Stack Trace:\n{ex.StackTrace}";

            // Salva in un file di log
            try
            {
                string logFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"MorseDecoderAI_Error_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                File.WriteAllText(logFile, errorMessage);

                MessageBox.Show(
                    $"{errorMessage}\n\n" +
                    $"L'errore è stato salvato in:\n{logFile}",
                    "Errore - Morse Decoder AI",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch
            {
                MessageBox.Show(errorMessage, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogStartup(string message)
        {
            try
            {
                string logFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "RadioLoggerApp",
                    "startup.log");

                Directory.CreateDirectory(Path.GetDirectoryName(logFile)!);

                File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n");
            }
            catch
            {
                // Ignora errori di logging
            }
        }
    }
}
