using System;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace FlexMaster6000
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// FlexMaster6000 - SmartSDR 4.0 Compatible Application
    /// </summary>
    public partial class App : Application
    {
        public static ILoggerFactory LoggerFactory { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Setup logging
            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            // Global exception handling
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var logger = LoggerFactory.CreateLogger<App>();
            logger.LogCritical(e.ExceptionObject as Exception, "Unhandled exception");

            MessageBox.Show(
                $"A critical error occurred: {(e.ExceptionObject as Exception)?.Message}\n\nThe application will now close.",
                "FlexMaster6000 Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var logger = LoggerFactory.CreateLogger<App>();
            logger.LogError(e.Exception, "Unhandled dispatcher exception");

            MessageBox.Show(
                $"An error occurred: {e.Exception.Message}",
                "FlexMaster6000 Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );

            e.Handled = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            LoggerFactory?.Dispose();
            base.OnExit(e);
        }
    }
}
