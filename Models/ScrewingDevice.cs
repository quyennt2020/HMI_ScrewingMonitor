using System;
using System.ComponentModel;

namespace HMI_ScrewingMonitor.Models
{
    public class ScrewingDevice : INotifyPropertyChanged
    {
        private bool _isConnected = false;
        private string _status = "--";
        private bool _isOK = false;
        private float _actualAngle = 0.0f;
        private float _actualTorque = 0.0f;
        private float _minAngle = 40.0f;
        private float _maxAngle = 50.0f;
        private float _minTorque = 7.0f;
        private float _maxTorque = 10.0f;
        private DateTime _lastUpdate = DateTime.Now;
        private DateTime _lastSuccessfulRead = DateTime.MinValue;
        private bool _enabled = true;

        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; } = 502;
        public int SlaveId { get; set; } = 1;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
                OnPropertyChanged(nameof(DeviceStatusText));
                OnPropertyChanged(nameof(DeviceStatusColor));
                OnPropertyChanged(nameof(DeviceBorderColor));
                OnPropertyChanged(nameof(StatusTextColor));
                OnPropertyChanged(nameof(ResultBackgroundColor));
            }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(DeviceStatusText));
                OnPropertyChanged(nameof(DeviceStatusColor));
                OnPropertyChanged(nameof(DeviceBorderColor));
                OnPropertyChanged(nameof(StatusTextColor));
                OnPropertyChanged(nameof(ResultBackgroundColor));
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public bool IsOK
        {
            get => _isOK;
            set
            {
                _isOK = value;
                OnPropertyChanged(nameof(IsOK));
                OnPropertyChanged(nameof(ResultText));
                OnPropertyChanged(nameof(ResultColor));
                OnPropertyChanged(nameof(ResultBackgroundColor));
            }
        }

        public float ActualAngle
        {
            get => _actualAngle;
            set
            {
                _actualAngle = value;
                System.Diagnostics.Debug.WriteLine($"Device {DeviceId} - UI UPDATE: ActualAngle set to {value}°");
                OnPropertyChanged(nameof(ActualAngle));
                OnPropertyChanged(nameof(AngleStatus));
            }
        }

        public float ActualTorque
        {
            get => _actualTorque;
            set
            {
                _actualTorque = value;
                System.Diagnostics.Debug.WriteLine($"Device {DeviceId} - UI UPDATE: ActualTorque set to {value} Nm");
                OnPropertyChanged(nameof(ActualTorque));
                OnPropertyChanged(nameof(TorqueStatus));
            }
        }

        public float MinAngle
        {
            get => _minAngle;
            set
            {
                _minAngle = value;
                OnPropertyChanged(nameof(MinAngle));
                OnPropertyChanged(nameof(AngleStatus));
            }
        }

        public float MaxAngle
        {
            get => _maxAngle;
            set
            {
                _maxAngle = value;
                OnPropertyChanged(nameof(MaxAngle));
                OnPropertyChanged(nameof(AngleStatus));
            }
        }

        public float MinTorque
        {
            get => _minTorque;
            set
            {
                _minTorque = value;
                OnPropertyChanged(nameof(MinTorque));
                OnPropertyChanged(nameof(TorqueStatus));
            }
        }

        public float MaxTorque
        {
            get => _maxTorque;
            set
            {
                _maxTorque = value;
                OnPropertyChanged(nameof(MaxTorque));
                OnPropertyChanged(nameof(TorqueStatus));
            }
        }

        public DateTime LastUpdate
        {
            get => _lastUpdate;
            set
            {
                _lastUpdate = value;
                OnPropertyChanged(nameof(LastUpdate));
            }
        }

        public DateTime LastSuccessfulRead
        {
            get => _lastSuccessfulRead;
            set
            {
                _lastSuccessfulRead = value;
                OnPropertyChanged(nameof(LastSuccessfulRead));
                OnPropertyChanged(nameof(IsHealthy));
            }
        }

        // Computed Properties
        public string ResultText => !IsConnected ? "--" : (IsOK ? "OK" : "NG");
        public string ResultColor => !IsConnected ? "Gray" : (IsOK ? "Green" : "Red");
        public string StatusColor => IsConnected ? "Green" : "Red";
        public string AngleStatus => $"{ActualAngle:F1}° ({MinAngle:F1}-{MaxAngle:F1})";
        public string TorqueStatus => $"{ActualTorque:F2}Nm ({MinTorque:F2}-{MaxTorque:F2})";

        // Device Status Visual Indicators
        public string DeviceStatusText
        {
            get
            {
                if (!Enabled) return "DISABLED";
                if (IsConnected) return "ONLINE";
                return "OFFLINE";
            }
        }

        public string DeviceStatusColor
        {
            get
            {
                if (!Enabled) return "#95A5A6"; // Professional Gray
                if (IsConnected) return "#2ECC71"; // Professional Green
                return "#F39C12"; // Professional Orange
            }
        }

        public string DeviceBorderColor
        {
            get
            {
                if (!Enabled) return "#BDC3C7"; // Light Gray border
                if (IsConnected) return "#27AE60"; // Darker Green border
                return "#E67E22"; // Darker Orange border
            }
        }

        // New property for status text color (better contrast)
        public string StatusTextColor
        {
            get
            {
                if (!IsConnected) return "#7F8C8D"; // Gray text for disconnected
                return "#FFFFFF"; // White text for connected states
            }
        }

        // Enhanced result colors
        public string ResultBackgroundColor
        {
            get
            {
                if (!IsConnected) return "#95A5A6"; // Gray for disconnected
                return IsOK ? "#2ECC71" : "#E74C3C"; // Professional Green/Red
            }
        }

        // Health check - thiết bị healthy nếu đọc thành công trong vòng 10 giây
        public bool IsHealthy => LastSuccessfulRead != DateTime.MinValue &&
                                DateTime.Now - LastSuccessfulRead < TimeSpan.FromSeconds(10);

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
