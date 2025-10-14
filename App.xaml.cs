using System;
using System.Windows;
using HMI_ScrewingMonitor.Services;
using HMI_ScrewingMonitor.Views;

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

            // Kiểm tra license
            CheckLicense();
        }

        private void CheckLicense()
        {
            try
            {
                // Initialize License Manager
                var licenseManager = new LicenseManager();

                // Kiểm tra license hợp lệ
                if (licenseManager.IsLicensed)
                {
                    // License hợp lệ → Vào app bình thường
                    Console.WriteLine("[LICENSE] Valid license detected. Starting application...");
                }
                else
                {
                    // Trial mode - 10 phút mỗi lần chạy
                    Console.WriteLine("[LICENSE] Trial mode - 10 minutes per session.");
                    // MainViewModel sẽ tự động khởi động trial timer
                }

                // MainWindow sẽ được khởi động tự động từ App.xaml
            }
            catch (InvalidOperationException ex)
            {
                // Lỗi không lấy được hardware info
                MessageBox.Show(
                    ex.Message,
                    "Lỗi Khởi Động",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Shutdown();
            }
            catch (Exception ex)
            {
                // Các lỗi khác
                MessageBox.Show(
                    $"Lỗi khởi động ứng dụng: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Shutdown();
            }
        }

        private void ShowLicenseWindow(LicenseManager licenseManager, bool required)
        {
            var licenseWindow = new LicenseWindow(licenseManager);

            // Hiển thị license window như dialog
            bool? result = licenseWindow.ShowDialog();

            if (required && result != true)
            {
                // Nếu bắt buộc activate mà user không activate → Thoát app
                MessageBox.Show(
                    "Phần mềm chưa được kích hoạt.\n\nỨng dụng sẽ thoát.",
                    "Chưa kích hoạt",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                Shutdown();
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Đã xảy ra lỗi: {e.Exception.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
