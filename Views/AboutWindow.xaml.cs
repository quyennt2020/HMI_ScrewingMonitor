using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
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

                    // Kiểm tra expiry date (nếu có)
                    // TODO: Nếu có thông tin expiry date từ license, hiển thị ở đây
                    ExpiryLabel.Visibility = Visibility.Visible;
                    ExpiryDateText.Visibility = Visibility.Visible;
                    ExpiryDateText.Text = "Vĩnh viễn";
                }
                else if (_licenseManager.TamperDetected)
                {
                    // Phát hiện tamper
                    LicenseStatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(231, 76, 60)); // Red
                    LicenseStatusText.Text = "⚠ Phát hiện hành vi gian lận";
                    LicenseStatusText.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60));

                    TrialLabel.Visibility = Visibility.Collapsed;
                    TrialDaysText.Visibility = Visibility.Collapsed;
                }
                else if (_licenseManager.IsTrialExpired)
                {
                    // Trial hết hạn
                    LicenseStatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(231, 76, 60)); // Red
                    LicenseStatusText.Text = "✗ Phiên bản dùng thử đã hết hạn";
                    LicenseStatusText.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60));

                    TrialLabel.Visibility = Visibility.Visible;
                    TrialDaysText.Visibility = Visibility.Visible;
                    TrialDaysText.Text = "Đã hết hạn";
                    TrialDaysText.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                }
                else
                {
                    // Trial mode
                    LicenseStatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(230, 126, 34)); // Orange
                    LicenseStatusText.Text = "⏰ Phiên bản dùng thử";
                    LicenseStatusText.Foreground = new SolidColorBrush(Color.FromRgb(230, 126, 34));

                    TrialLabel.Visibility = Visibility.Visible;
                    TrialDaysText.Visibility = Visibility.Visible;
                    TrialDaysText.Text = $"{_licenseManager.DaysRemaining} ngày";
                    TrialDaysText.Foreground = new SolidColorBrush(Color.FromRgb(230, 126, 34));
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
    }
}
