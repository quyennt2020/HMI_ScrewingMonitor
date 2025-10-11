using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace HMI_ScrewingMonitor.Models
{
    public class TorqueDataPoint
    {
        public float Value { get; set; }
        public DateTime Timestamp { get; set; }

        public TorqueDataPoint(float value, DateTime timestamp)
        {
            Value = value;
            Timestamp = timestamp;
        }
    }

    public class ScrewingDevice : INotifyPropertyChanged
    {
        private bool _isConnected = false;
        private string _status = "--";
        private bool _isOK = false;
        private float _actualTorque = 0.0f;
        private float _targetTorque = 12.0f;
        private float _minTorque = 9.8f;
        private float _maxTorque = 14.0f;
        private int _totalCount = 100;
        private int _okCount = 90;
        private int _ngCount = 10;
        private ObservableCollection<TorqueDataPoint> _torqueHistory = new ObservableCollection<TorqueDataPoint>();
        private string _deviceModel = "ABC";
        private DateTime _lastUpdate = DateTime.Now;
        private DateTime _lastSuccessfulRead = DateTime.MinValue;
        private bool _enabled = true;
        private bool _hasRealData = false;
        private DateTime _lastResetDate = DateTime.Today;

        // Thuộc tính để theo dõi trạng thái của tín hiệu COMP
        public bool PreviousCompletionState { get; set; } = false;

        // Thuộc tính để track ngày reset counters
        public DateTime LastResetDate
        {
            get => _lastResetDate;
            set
            {
                _lastResetDate = value;
                OnPropertyChanged(nameof(LastResetDate));
            }
        }

        public ScrewingDevice()
        {
            // Vô hiệu hóa việc tạo dữ liệu demo để biểu đồ bắt đầu trống
            // InitializeDemoData();
        }

        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; } = 502;
        public int SlaveId { get; set; } = 1;

        public string DeviceModel
        {
            get => _deviceModel;
            set
            {
                _deviceModel = value;
                OnPropertyChanged(nameof(DeviceModel));
            }
        }

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


        public float ActualTorque
        {
            get => _actualTorque;
            set
            {
                // Only update if value actually changed
                if (Math.Abs(_actualTorque - value) > 0.01f)
                {
                    _actualTorque = value;

                    // Only add to chart if value > 0 (skip disconnect/error values)
                    if (value > 0)
                    {
                        AddRealTorqueData(value);
                    }

                    OnPropertyChanged(nameof(ActualTorque));
                    OnPropertyChanged(nameof(StandardRange));
                }
            }
        }

        public float TargetTorque
        {
            get => _targetTorque;
            set
            {
                _targetTorque = value;
                OnPropertyChanged(nameof(TargetTorque));
            }
        }


        public float MinTorque
        {
            get => _minTorque;
            set
            {
                _minTorque = value;
                OnPropertyChanged(nameof(MinTorque));
                OnPropertyChanged(nameof(StandardRange));
            }
        }

        public float MaxTorque
        {
            get => _maxTorque;
            set
            {
                _maxTorque = value;
                OnPropertyChanged(nameof(MaxTorque));
                OnPropertyChanged(nameof(StandardRange));
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

        public int TotalCount
        {
            get => _totalCount;
            set
            {
                _totalCount = value;
                OnPropertyChanged(nameof(TotalCount));
            }
        }

        public int OKDeviceCount
        {
            get => _okCount;
            set
            {
                _okCount = value;
                OnPropertyChanged(nameof(OKDeviceCount));
            }
        }

        public int NGDeviceCount
        {
            get => _ngCount;
            set
            {
                _ngCount = value;
                OnPropertyChanged(nameof(NGDeviceCount));
            }
        }

        public ObservableCollection<TorqueDataPoint> TorqueHistory
        {
            get => _torqueHistory;
        }

        // Computed Properties
        public string ResultText => !IsConnected ? "--" : (IsOK ? "OK" : "NG");
        public string ResultColor => !IsConnected ? "Gray" : (IsOK ? "Green" : "Red");
        public string StatusColor => IsConnected ? "Green" : "Red";
        public string StandardRange => $"{MinTorque:F2}~{MaxTorque:F2}";

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

        private void InitializeDemoData()
        {
            // Initialize with 30 demo data points only if no real data exists
            // This ensures chart always shows full data for demo purposes
            var random = new Random();
            var baseTime = DateTime.Now.AddMinutes(-30);

            for (int i = 0; i < 30; i++)
            {
                // Create more realistic torque variation around target value
                var variation = (float)(random.NextDouble() * 4.0 - 2.0); // -2 to +2 Nm variation
                var value = _targetTorque + variation;

                // Ensure value stays within reasonable bounds
                value = Math.Max(8.0f, Math.Min(16.0f, value));

                var timestamp = baseTime.AddMinutes(i);
                _torqueHistory.Add(new TorqueDataPoint(value, timestamp));
            }
        }

        // Method to replace demo data with real data when device connects
        public void AddRealTorqueData(float torqueValue)
        {
            // Skip invalid values (0 or negative) - these come from disconnect/error states
            if (torqueValue <= 0)
            {
                Console.WriteLine($"Device {DeviceId} - SKIPPED INVALID DATA: {torqueValue:F1}Nm (not added to chart)");
                return;
            }

            // Force clear demo data on first real data
            if (!_hasRealData)
            {
                Console.WriteLine($"Device {DeviceId} - FIRST REAL DATA: Clearing {_torqueHistory.Count} demo points");
                _torqueHistory.Clear();
                _hasRealData = true;
            }

            var dataPoint = new TorqueDataPoint(torqueValue, DateTime.Now);
            _torqueHistory.Add(dataPoint);
            Console.WriteLine($"Device {DeviceId} - ADDED REAL DATA: {torqueValue:F1}Nm at {dataPoint.Timestamp:HH:mm:ss}, Total={_torqueHistory.Count}");

            if (_torqueHistory.Count > 30)
            {
                _torqueHistory.RemoveAt(0);
            }

            OnPropertyChanged(nameof(TorqueHistory));
        }

        // Thêm phương thức này vào lớp ScrewingDevice
        public void NotifyAllPropertiesChanged()
        {
            OnPropertyChanged(nameof(ActualTorque));
            OnPropertyChanged(nameof(IsOK));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(LastUpdate));
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(DeviceStatusColor));
            OnPropertyChanged(nameof(ResultText));
            OnPropertyChanged(nameof(ResultBackgroundColor));
            OnPropertyChanged(nameof(DeviceStatusText));
            OnPropertyChanged(nameof(DeviceBorderColor));
            // Quan trọng: Thông báo cho biểu đồ rằng collection đã thay đổi,
            // mặc dù không có mục mới nào được thêm vào ở đây,
            // nhưng nó đảm bảo biểu đồ được vẽ lại.
            OnPropertyChanged(nameof(TorqueHistory));
        }

        // Reset daily counters khi sang ngày mới
        public void ResetDailyCounters()
        {
            Console.WriteLine($"[DAILY RESET] Device {DeviceId} - Resetting counters. Old values: Total={TotalCount}, OK={OKDeviceCount}, NG={NGDeviceCount}");

            TotalCount = 0;
            OKDeviceCount = 0;
            NGDeviceCount = 0;
            LastResetDate = DateTime.Today;

            Console.WriteLine($"[DAILY RESET] Device {DeviceId} - Counters reset to 0");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
