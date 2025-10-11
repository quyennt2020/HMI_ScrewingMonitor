using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using HMI_ScrewingMonitor.Services;

namespace HMI_ScrewingMonitor.Views
{
    public partial class LicenseWindow : Window
    {
        private readonly LicenseManager _licenseManager;
        private bool _isActivated = false;

        public LicenseWindow(LicenseManager licenseManager)
        {
            InitializeComponent();
            _licenseManager = licenseManager;

            LoadLicenseInfo();
        }

        private void LoadLicenseInfo()
        {
            // Hiển thị Hardware ID
            string hardwareId = _licenseManager.HardwareId;
            HardwareIdTextBox.Text = HardwareInfo.FormatHardwareId(hardwareId);

            // Kiểm tra trạng thái license
            if (_licenseManager.IsLicensed)
            {
                // Đã có license hợp lệ
                ShowLicenseActivated();
            }
            else if (_licenseManager.TamperDetected)
            {
                // Phát hiện tamper
                ShowTamperDetected();
            }
            else
            {
                // Trial mode
                ShowTrialStatus();
            }
        }

        private void ShowLicenseActivated()
        {
            TrialStatusPanel.Visibility = Visibility.Collapsed;
            LicenseInfoPanel.Visibility = Visibility.Visible;
            StatusMessagePanel.Visibility = Visibility.Collapsed;

            LicenseCompanyText.Text = $"Công ty: {_licenseManager.CompanyName}";

            if (_licenseManager.ExpiryDate.HasValue)
            {
                LicenseExpiryText.Text = $"Hết hạn: {_licenseManager.ExpiryDate.Value:dd/MM/yyyy}";
            }
            else
            {
                LicenseExpiryText.Text = "Hết hạn: Vĩnh viễn";
            }

            // Đổi button text
            ActivateButton.Content = "✅ Đã kích hoạt";
            ActivateButton.IsEnabled = false;
            CancelButton.Content = "✔️ Đóng";

            _isActivated = true;
        }

        private void ShowTrialStatus()
        {
            TrialStatusPanel.Visibility = Visibility.Visible;
            LicenseInfoPanel.Visibility = Visibility.Collapsed;

            if (_licenseManager.IsTrialExpired)
            {
                TrialStatusPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8D7DA"));
                TrialStatusPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC3545"));
                TrialStatusText.Text = "❌ Phiên bản dùng thử đã hết hạn";
                TrialStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#721C24"));
                TrialDaysText.Text = "Vui lòng kích hoạt phần mềm để tiếp tục sử dụng";
                TrialDaysText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#721C24"));
            }
            else
            {
                TrialDaysText.Text = $"Còn {_licenseManager.DaysRemaining} ngày dùng thử";
            }
        }

        private void ShowTamperDetected()
        {
            TrialStatusPanel.Visibility = Visibility.Visible;
            LicenseInfoPanel.Visibility = Visibility.Collapsed;

            TrialStatusPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8D7DA"));
            TrialStatusPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC3545"));
            TrialStatusText.Text = "⚠️ Phát hiện thay đổi ngày giờ hệ thống";
            TrialStatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#721C24"));
            TrialDaysText.Text = "Vui lòng liên hệ hỗ trợ hoặc kích hoạt phần mềm";
            TrialDaysText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#721C24"));
        }

        private void CopyHardwareId_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(HardwareIdTextBox.Text);
                ShowStatusMessage("✅ Đã copy Hardware ID vào clipboard", MessageType.Success);
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"❌ Lỗi copy: {ex.Message}", MessageType.Error);
            }
        }

        private void LicenseKeyTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Auto-format license key khi user nhập
            string text = LicenseKeyTextBox.Text.Replace("-", "").ToUpper();

            if (text.Length > 25)
            {
                text = text.Substring(0, 25);
            }

            // Format thành XXXXX-XXXXX-XXXXX-XXXXX-XXXXX
            string formatted = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0 && i % 5 == 0)
                    formatted += "-";
                formatted += text[i];
            }

            if (formatted != LicenseKeyTextBox.Text)
            {
                int cursorPos = LicenseKeyTextBox.SelectionStart;
                LicenseKeyTextBox.Text = formatted;
                LicenseKeyTextBox.SelectionStart = Math.Min(cursorPos, formatted.Length);
            }
        }

        private void Activate_Click(object sender, RoutedEventArgs e)
        {
            string licenseKey = LicenseKeyTextBox.Text.Trim();

            if (string.IsNullOrEmpty(licenseKey))
            {
                ShowStatusMessage("⚠️ Vui lòng nhập license key", MessageType.Warning);
                LicenseKeyTextBox.Focus();
                return;
            }

            // Remove dashes
            licenseKey = licenseKey.Replace("-", "");

            if (licenseKey.Length != 25)
            {
                ShowStatusMessage("⚠️ License key không đúng định dạng (phải có 25 ký tự)", MessageType.Warning);
                LicenseKeyTextBox.Focus();
                return;
            }

            // Validate và save license
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                bool success = _licenseManager.SaveLicense(licenseKey, "");

                if (success)
                {
                    ShowStatusMessage("✅ Kích hoạt thành công!", MessageType.Success);
                    ShowLicenseActivated();

                    // Đóng window sau 2 giây
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(2)
                    };
                    timer.Tick += (s, args) =>
                    {
                        timer.Stop();
                        DialogResult = true;
                        Close();
                    };
                    timer.Start();
                }
                else
                {
                    ShowStatusMessage("❌ License key không hợp lệ hoặc không khớp với máy tính này", MessageType.Error);
                    LicenseKeyTextBox.SelectAll();
                    LicenseKeyTextBox.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"❌ Lỗi kích hoạt: {ex.Message}", MessageType.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Nếu đang trong trial và chưa expired, cho phép đóng
            if (!_licenseManager.IsLicensed && _licenseManager.IsTrialExpired && !_isActivated)
            {
                var result = MessageBox.Show(
                    "Phiên bản dùng thử đã hết hạn.\n\nBạn có chắc muốn thoát không? Phần mềm sẽ không khởi động.",
                    "Xác nhận thoát",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    DialogResult = false;
                    Close();
                }
            }
            else
            {
                DialogResult = _isActivated;
                Close();
            }
        }

        private void ShowStatusMessage(string message, MessageType type)
        {
            StatusMessagePanel.Visibility = Visibility.Visible;
            StatusMessageText.Text = message;

            switch (type)
            {
                case MessageType.Success:
                    StatusMessagePanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4EDDA"));
                    StatusMessagePanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28A745"));
                    StatusMessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#155724"));
                    break;
                case MessageType.Error:
                    StatusMessagePanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8D7DA"));
                    StatusMessagePanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC3545"));
                    StatusMessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#721C24"));
                    break;
                case MessageType.Warning:
                    StatusMessagePanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3CD"));
                    StatusMessagePanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC107"));
                    StatusMessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#856404"));
                    break;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Nếu trial expired và chưa activate, không cho đóng window
            if (!_licenseManager.IsLicensed && _licenseManager.IsTrialExpired && !_isActivated)
            {
                e.Cancel = true;
                Cancel_Click(this, null);
            }

            base.OnClosing(e);
        }

        private enum MessageType
        {
            Success,
            Error,
            Warning
        }
    }
}
