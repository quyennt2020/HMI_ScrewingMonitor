using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using HMI_ScrewingMonitor.Models;
using HMI_ScrewingMonitor.Services;

namespace HMI_ScrewingMonitor.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ModbusService _modbusService;
        private readonly DispatcherTimer _timer;
        private bool _isMonitoring = false;
        private string _connectionStatus = "Chưa kết nối";
        private int _gridColumns = 2;
        private int _gridRows = 0; // 0 = auto

        public ObservableCollection<ScrewingDevice> Devices { get; }
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand StartMonitoringCommand { get; }
        public ICommand StopMonitoringCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        public bool IsMonitoring
        {
            get => _isMonitoring;
            set
            {
                _isMonitoring = value;
                OnPropertyChanged(nameof(IsMonitoring));
            }
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set
            {
                _connectionStatus = value;
                OnPropertyChanged(nameof(ConnectionStatus));
            }
        }

        public int GridColumns
        {
            get => _gridColumns;
            set
            {
                _gridColumns = value;
                OnPropertyChanged(nameof(GridColumns));
            }
        }

        public int GridRows
        {
            get => _gridRows;
            set
            {
                _gridRows = value;
                OnPropertyChanged(nameof(GridRows));
            }
        }

        public int ConnectedDevicesCount => Devices.Count(d => d.IsConnected && d.Enabled);
        public int TotalDevices => Devices.Count(d => d.Enabled);
        public int OKCount => Devices.Count(d => d.IsOK);
        public int NGCount => Devices.Count(d => !d.IsOK && d.IsConnected && d.Status != "Sẵn sàng");

        // Button visibility properties
        public bool CanConnect => ConnectedDevicesCount < TotalDevices; // Cho phép kết nối khi chưa kết nối hết tất cả thiết bị
        public bool CanDisconnect => ConnectedDevicesCount > 0; // Cho phép ngắt kết nối khi có ít nhất 1 thiết bị đang kết nối
        public bool CanStartMonitoring => ConnectedDevicesCount > 0 && !IsMonitoring; // Cho phép giám sát khi có ít nhất 1 thiết bị kết nối
        public bool CanStopMonitoring => IsMonitoring;

        public MainViewModel()
        {
            _modbusService = new ModbusService();
            Devices = new ObservableCollection<ScrewingDevice>();

            ConnectCommand = new RelayCommand(() => ConnectAsync());
            DisconnectCommand = new RelayCommand(Disconnect);
            StartMonitoringCommand = new RelayCommand(StartMonitoring);
            StopMonitoringCommand = new RelayCommand(StopMonitoring);
            OpenSettingsCommand = new RelayCommand(OpenSettings);

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Cập nhật mỗi giây
            };
            _timer.Tick += Timer_Tick;

            // Load devices from config first, fallback to default if needed
            LoadDevicesFromConfig();

            // Initialize with placeholder data for better UI preview
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                InitializePlaceholderData();
            }
        }

        private void InitializeDevices()
        {
            // Khởi tạo danh sách thiết bị - có thể đọc từ config file
            Devices.Add(new ScrewingDevice 
            { 
                DeviceId = 1, 
                DeviceName = "Máy vặn vít #1",
                IPAddress = "192.168.1.100",
                Port = 502,
                MinAngle = 40.0f,
                MaxAngle = 50.0f,
                MinTorque = 7.0f,
                MaxTorque = 10.0f
            });
            
            Devices.Add(new ScrewingDevice 
            { 
                DeviceId = 2, 
                DeviceName = "Máy vặn vít #2",
                IPAddress = "192.168.1.101", 
                Port = 502,
                MinAngle = 40.0f,
                MaxAngle = 50.0f,
                MinTorque = 8.0f,
                MaxTorque = 12.0f
            });

            Devices.Add(new ScrewingDevice 
            { 
                DeviceId = 3, 
                DeviceName = "Máy vặn vít #3",
                IPAddress = "192.168.1.102", 
                Port = 502,
                MinAngle = 40.0f,
                MaxAngle = 50.0f,
                MinTorque = 7.5f,
                MaxTorque = 11.0f
            });

            Devices.Add(new ScrewingDevice 
            { 
                DeviceId = 4, 
                DeviceName = "Máy vặn vít #4",
                IPAddress = "192.168.1.103", 
                Port = 502,
                MinAngle = 40.0f,
                MaxAngle = 50.0f,
                MinTorque = 8.0f,
                MaxTorque = 12.0f
            });
        }

        private void InitializePlaceholderData()
        {
            // Add placeholder data for design-time preview
            foreach (var device in Devices)
            {
                var random = new Random(device.DeviceId);

                // Simulate some devices as connected with sample data
                if (device.DeviceId <= 2)
                {
                    device.IsConnected = true;
                    device.Status = device.DeviceId == 1 ? "Siết OK" : "Siết NG";
                    device.IsOK = device.DeviceId == 1;
                    device.ActualAngle = (float)(device.MinAngle + random.NextDouble() * (device.MaxAngle - device.MinAngle));
                    device.ActualTorque = (float)(device.MinTorque + random.NextDouble() * (device.MaxTorque - device.MinTorque));
                    device.LastUpdate = DateTime.Now.AddSeconds(-random.Next(1, 30));
                }
                else
                {
                    device.IsConnected = false;
                    device.Status = "Mất kết nối";
                    device.IsOK = false;
                }
            }

            // Set sample connection status
            ConnectionStatus = "Đã kết nối (2/4 thiết bị)";
            IsMonitoring = true;
        }

        private async void ConnectAsync()
        {
            try
            {
                // Load connection settings from config
                var config = LoadModbusConfig();
                bool connected = false;

                switch (config.ConnectionType)
                {
                    case "TCP_Individual":
                        // Kết nối từng thiết bị riêng lẻ sử dụng hệ thống mới
                        int connectedDevices = 0;
                        foreach (var device in Devices.Where(d => d.Enabled))
                        {
                            var deviceConnected = await _modbusService.ConnectToDevice(device);
                            if (deviceConnected)
                            {
                                connectedDevices++;
                                device.IsConnected = true;
                                device.Status = "Sẵn sàng";
                            }
                            else
                            {
                                device.IsConnected = false;
                                device.Status = "Kết nối thất bại";
                            }
                        }
                        connected = connectedDevices > 0;
                        break;

                    case "TCP_Gateway":
                        connected = await _modbusService.ConnectTCP_Gateway(config.GatewayIP, config.GatewayPort);
                        if (connected)
                        {
                            foreach (var device in Devices)
                            {
                                device.IsConnected = true;
                                device.Status = "Sẵn sàng";
                            }
                        }
                        break;

                    case "RTU_Serial":
                        connected = _modbusService.ConnectRTU(config.SerialPort, config.BaudRate);
                        if (connected)
                        {
                            foreach (var device in Devices)
                            {
                                device.IsConnected = true;
                                device.Status = "Sẵn sàng";
                            }
                        }
                        break;

                    default:
                        // Fallback to TCP_Individual với hệ thống mới
                        int fallbackConnectedDevices = 0;
                        foreach (var device in Devices.Where(d => d.Enabled))
                        {
                            var deviceConnected = await _modbusService.ConnectToDevice(device);
                            if (deviceConnected)
                            {
                                fallbackConnectedDevices++;
                                device.IsConnected = true;
                                device.Status = "Sẵn sàng";
                            }
                            else
                            {
                                device.IsConnected = false;
                                device.Status = "Kết nối thất bại";
                            }
                        }
                        connected = fallbackConnectedDevices > 0;
                        break;
                }

                // Kiểm tra số thiết bị kết nối thành công
                int connectedCount = Devices.Count(d => d.IsConnected);

                if (connectedCount > 0)
                {
                    ConnectionStatus = $"Đã kết nối {connectedCount}/{TotalDevices} thiết bị ({config.ConnectionType})";
                    // Auto-start monitoring nếu có ít nhất 1 thiết bị kết nối
                    StartMonitoring();
                }
                else
                {
                    ConnectionStatus = "Kết nối thất bại - Không có thiết bị nào kết nối được";
                }

                UpdateButtonStates();
                OnPropertyChanged(nameof(ConnectedDevicesCount));
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Lỗi kết nối: {ex.Message}";
            }
        }

        private HMI_ScrewingMonitor.ViewModels.ModbusSettingsConfig LoadModbusConfig()
        {
            try
            {
                string configPath = "Config/devices.json";
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<HMI_ScrewingMonitor.ViewModels.AppConfig>(json);
                    return config?.ModbusSettings ?? new HMI_ScrewingMonitor.ViewModels.ModbusSettingsConfig();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading Modbus config: {ex.Message}");
            }

            return new HMI_ScrewingMonitor.ViewModels.ModbusSettingsConfig();
        }

        private void Disconnect()
        {
            // Auto-stop monitoring before disconnect
            StopMonitoring();

            // Ngắt kết nối từng thiết bị riêng lẻ
            foreach (var device in Devices.Where(d => d.IsConnected))
            {
                _modbusService.DisconnectFromDevice(device.DeviceId);
                device.IsConnected = false;
                device.Status = "--";
                device.ActualAngle = 0;
                device.ActualTorque = 0;
                device.IsOK = false;
                device.LastSuccessfulRead = DateTime.MinValue;  // Reset health tracking
            }

            ConnectionStatus = "Đã ngắt kết nối";
            UpdateButtonStates();
            OnPropertyChanged(nameof(ConnectedDevicesCount));
        }

        private void StartMonitoring()
        {
            // Kiểm tra có ít nhất 1 thiết bị kết nối thay vì service connection
            if (ConnectedDevicesCount > 0)
            {
                IsMonitoring = true;
                _timer.Start();
                UpdateButtonStates();
                Console.WriteLine($"*** MONITORING STARTED - {ConnectedDevicesCount} devices connected ***");
            }
            else
            {
                Console.WriteLine("*** MONITORING NOT STARTED - No devices connected ***");
            }
        }

        private void StopMonitoring()
        {
            IsMonitoring = false;
            _timer.Stop();

            // Xóa dữ liệu các thiết bị khi dừng giám sát để phân biệt trạng thái
            foreach (var device in Devices)
            {
                device.ActualAngle = 0;
                device.ActualTorque = 0;
                device.Status = device.IsConnected ? "Sẵn sàng" : "--";
                device.IsOK = false;
            }

            // Cập nhật thống kê
            OnPropertyChanged(nameof(OKCount));
            OnPropertyChanged(nameof(NGCount));

            UpdateButtonStates();
            Console.WriteLine("*** MONITORING STOPPED - Đã xóa dữ liệu thiết bị ***");
        }

        private void OpenSettings()
        {
            var settingsWindow = new HMI_ScrewingMonitor.Views.SettingsWindow();
            settingsWindow.Owner = Application.Current.MainWindow;
            settingsWindow.ShowDialog();

            // Reload devices after settings window closes
            LoadDevicesFromConfig();
        }

        private void LoadDevicesFromConfig()
        {
            try
            {
                string configPath = "Config/devices.json";
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<HMI_ScrewingMonitor.ViewModels.AppConfig>(json);

                    if (config?.Devices != null && config.Devices.Count > 0)
                    {
                        Devices.Clear();
                        foreach (var deviceConfig in config.Devices)
                        {
                            var device = new ScrewingDevice
                            {
                                DeviceId = deviceConfig.DeviceId,
                                DeviceName = deviceConfig.DeviceName,
                                IPAddress = deviceConfig.IPAddress,
                                Port = deviceConfig.Port,
                                SlaveId = deviceConfig.SlaveId,
                                MinAngle = (float)deviceConfig.MinAngle,
                                MaxAngle = (float)deviceConfig.MaxAngle,
                                MinTorque = (float)deviceConfig.MinTorque,
                                MaxTorque = (float)deviceConfig.MaxTorque,
                                Enabled = deviceConfig.Enabled,  // Set enabled status from config
                                // Trạng thái ban đầu
                                IsConnected = false,
                                Status = "--",
                                IsOK = false,
                                ActualAngle = 0,
                                ActualTorque = 0
                            };
                            Devices.Add(device);
                        }

                        // Load UI settings
                        if (config.UI != null)
                        {
                            GridColumns = config.UI.GridColumns;
                            GridRows = config.UI.GridRows;
                            // Update timer interval if monitoring
                            if (IsMonitoring && config.UI.RefreshInterval > 0)
                            {
                                _timer.Interval = TimeSpan.FromMilliseconds(config.UI.RefreshInterval);
                            }
                        }

                        OnPropertyChanged(nameof(TotalDevices));
                        OnPropertyChanged(nameof(ConnectedDevicesCount));
                        return; // Successfully loaded from config
                    }
                }

                // If no config file or no devices in config, use default devices
                InitializeDevices();
            }
            catch (Exception ex)
            {
                // If config loading fails, use default devices
                System.Diagnostics.Debug.WriteLine($"Error loading config: {ex.Message}");
                InitializeDevices();
            }
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            if (!IsMonitoring)
            {
                Console.WriteLine("*** TIMER_TICK SKIPPED - IsMonitoring = false ***");
                return;
            }

            Console.WriteLine($"*** TIMER_TICK - IsMonitoring = {IsMonitoring}, ConnectedDevices = {ConnectedDevicesCount} ***");

            var config = LoadModbusConfig();

            // Chỉ xử lý các thiết bị được kích hoạt
            foreach (var device in Devices.Where(d => d.Enabled).ToList())
            {
                try
                {
                    ScrewingDevice updatedDevice = null;

                    switch (config.ConnectionType)
                    {
                        case "TCP_Individual":
                            updatedDevice = await _modbusService.ReadDeviceData_Individual(device);
                            break;

                        case "TCP_Gateway":
                            updatedDevice = await _modbusService.ReadDeviceData_Gateway(device);
                            break;

                        case "RTU_Serial":
                            updatedDevice = await _modbusService.ReadDeviceData_RTU(device);
                            break;

                        default:
                            updatedDevice = await _modbusService.ReadDeviceData_Individual(device);
                            break;
                    }

                    if (updatedDevice != null)
                    {
                        // Cập nhật dữ liệu
                        device.ActualAngle = updatedDevice.ActualAngle;
                        device.ActualTorque = updatedDevice.ActualTorque;
                        device.IsOK = updatedDevice.IsOK;
                        device.Status = updatedDevice.Status;
                        device.LastUpdate = updatedDevice.LastUpdate;
                        device.LastSuccessfulRead = updatedDevice.LastSuccessfulRead;
                        device.IsConnected = updatedDevice.IsConnected;
                    }
                    else
                    {
                        // Giữ nguyên trạng thái kết nối, chỉ báo lỗi đọc dữ liệu
                        device.Status = "Lỗi đọc dữ liệu";
                        device.LastUpdate = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    device.IsConnected = false;
                    device.Status = "--";
                    // Xóa dữ liệu cũ khi có lỗi
                    device.ActualAngle = 0;
                    device.ActualTorque = 0;
                    device.IsOK = false;
                    device.LastUpdate = DateTime.Now;
                    Console.WriteLine($"TIMER ERROR - Device {device.DeviceId}: {ex.Message}");
                }
            }

            // Cập nhật thống kê và trạng thái kết nối
            OnPropertyChanged(nameof(ConnectedDevicesCount));
            OnPropertyChanged(nameof(OKCount));
            OnPropertyChanged(nameof(NGCount));

            // Cập nhật ConnectionStatus realtime
            if (ConnectedDevicesCount == 0)
            {
                ConnectionStatus = "Tất cả thiết bị đã mất kết nối";
            }
            else
            {
                var currentConfig = LoadModbusConfig();
                ConnectionStatus = $"Đã kết nối {ConnectedDevicesCount}/{TotalDevices} thiết bị ({currentConfig.ConnectionType})";
            }
        }

        private void UpdateButtonStates()
        {
            OnPropertyChanged(nameof(CanConnect));
            OnPropertyChanged(nameof(CanDisconnect));
            OnPropertyChanged(nameof(CanStartMonitoring));
            OnPropertyChanged(nameof(CanStopMonitoring));
        }

        public void Dispose()
        {
            _timer?.Stop();
            _modbusService?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Helper class for ICommand
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object parameter) => _execute();
    }
}
