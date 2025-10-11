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
                    return; // MainWindow sẽ được khởi động tự động từ App.xaml
                }

                // Kiểm tra trial
                if (licenseManager.IsTrialExpired || licenseManager.TamperDetected)
                {
                    // Trial hết hạn hoặc phát hiện tamper → Bắt buộc phải activate
                    Console.WriteLine("[LICENSE] Trial expired or tamper detected. Showing license window...");
                    ShowLicenseWindow(licenseManager, required: true);
                }
                else
                {
                    // Đang trong trial → Cho phép sử dụng, hiển thị thông báo
                    Console.WriteLine($"[LICENSE] Trial mode. {licenseManager.DaysRemaining} days remaining.");

                    // Hiển thị thông báo trial (optional)
                    if (licenseManager.DaysRemaining <= 7)
                    {
                        MessageBox.Show(
                            $"Bạn đang sử dụng phiên bản dùng thử.\n\n" +
                            $"Còn lại: {licenseManager.DaysRemaining} ngày\n\n" +
                            $"Vui lòng kích hoạt phần mềm để tiếp tục sử dụng sau khi hết hạn.",
                            "Thông báo dùng thử",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );
                    }

                    // Cho phép vào app
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi kiểm tra license: {ex.Message}\n\nỨng dụng sẽ thoát.",
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
