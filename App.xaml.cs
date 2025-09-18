using System;
using System.Windows;

namespace HMI_ScrewingMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Handle global exceptions
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Đã xảy ra lỗi: {e.Exception.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
