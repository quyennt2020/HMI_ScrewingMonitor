using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using HMI_ScrewingMonitor.Services;

namespace HMI_ScrewingMonitor.Views
{
    /// <summary>
    /// About Window - Hiển thị thông tin phần mềm và license
    /// </summary>
    public partial class AboutWindow : Window
    {
        private LicenseManager _licenseManager;

        public AboutWindow()
        {
            InitializeComponent();
            LoadInformation();
        }

        /// <summary>
        /// Load thông tin app và license
        /// </summary>
        private void LoadInformation()
        {
            try
            {
                // Version info (update này khi release phiên bản mới)
                VersionText.Text = "Version 1.0.0";
                ReleaseDateText.Text = "11/10/2025";

                // Load license info
                _licenseManager = new LicenseManager();

                // Hardware ID
                string hardwareId = HardwareInfo.GetHardwareId();
                HardwareIdText.Text = HardwareInfo.FormatHardwareId(hardwareId);

                // License Status
                if (_licenseManager.IsLicensed)
                {
                    // Đã kích hoạt
                    LicenseStatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(39, 174, 96)); // Green
                    LicenseStatusText.Text = "✓ Đã kích hoạt";
                    LicenseStatusText.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96));

                    // Hiển thị expiry date (nếu có)
                    ExpiryLabel.Visibility = Visibility.Visible;
                    ExpiryDateText.Visibility = Visibility.Visible;
                    if (_licenseManager.ExpiryDate.HasValue)
                    {
                        ExpiryDateText.Text = _licenseManager.ExpiryDate.Value.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        ExpiryDateText.Text = "Vĩnh viễn";
                    }
                }
                else
                {
                    // Trial mode - 10 phút mỗi lần chạy
                    LicenseStatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(230, 126, 34)); // Orange
                    LicenseStatusText.Text = "⏰ Dùng thử - 10 phút/lần chạy";
                    LicenseStatusText.Foreground = new SolidColorBrush(Color.FromRgb(230, 126, 34));

                    TrialLabel.Visibility = Visibility.Collapsed;
                    TrialDaysText.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi tải thông tin: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Copy Hardware ID to clipboard
        /// </summary>
        private void CopyHardwareId_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(HardwareIdText.Text);
                MessageBox.Show(
                    "Hardware ID đã được copy vào clipboard!",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi copy: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Mở License Window để kích hoạt
        /// </summary>
        private void OpenLicenseWindow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var licenseWindow = new LicenseWindow(_licenseManager);
                bool? result = licenseWindow.ShowDialog();

                if (result == true)
                {
                    // Refresh thông tin sau khi kích hoạt thành công
                    LoadInformation();

                    MessageBox.Show(
                        "License đã được kích hoạt thành công!",
                        "Thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi mở License Window: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Đóng cửa sổ
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Mở link website trong browser
        /// </summary>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Không thể mở link: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
