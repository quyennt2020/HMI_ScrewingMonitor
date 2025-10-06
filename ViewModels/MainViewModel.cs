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
        private ModbusSettingsConfig _modbusSettings; // Cache the settings
        private readonly DispatcherTimer _timer;
        private readonly LoggingService _loggingService;
        private bool _isMonitoring = false;
        private string _connectionStatus = "Chưa kết nối";
        private int _gridColumns = 2;
        private int _gridRows = 0; // 0 = auto
        private int _dailyOKCount = 0;
        private int _dailyNGCount = 0;
        private DateTime _currentDate = DateTime.Today;

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

        public int DailyOKCount
        {
            get => _dailyOKCount;
            set
            {
                _dailyOKCount = value;
                OnPropertyChanged(nameof(DailyOKCount));
            }
        }

        public int DailyNGCount
        {
            get => _dailyNGCount;
            set
            {
                _dailyNGCount = value;
                OnPropertyChanged(nameof(DailyNGCount));
            }
        }

        public int ConnectedDevicesCount => Devices.Count(d => d.IsConnected && d.Enabled);
        public int TotalDevices => Devices.Count(d => d.Enabled);
        public int OKCount => Devices.Sum(d => d.OKDeviceCount);  // Tổng số lần OK của tất cả thiết bị
        public int NGCount => Devices.Sum(d => d.NGDeviceCount);  // Tổng số lần NG của tất cả thiết bị

        // Progress properties for connection status
        // Button visibility properties
        public bool CanConnect => ConnectedDevicesCount < TotalDevices; // Cho phép kết nối khi chưa kết nối hết tất cả thiết bị
        public bool CanDisconnect => ConnectedDevicesCount > 0; // Cho phép ngắt kết nối khi có ít nhất 1 thiết bị đang kết nối
        public bool CanStartMonitoring => ConnectedDevicesCount > 0 && !IsMonitoring; // Cho phép giám sát khi có ít nhất 1 thiết bị kết nối
        public bool CanStopMonitoring => IsMonitoring;

        public MainViewModel()
        {
            _modbusService = new ModbusService();
            _loggingService = new LoggingService();
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
                MinTorque = 7.0f,
                MaxTorque = 10.0f
            });
            
            Devices.Add(new ScrewingDevice 
            { 
                DeviceId = 2, 
                DeviceName = "Máy vặn vít #2",
                IPAddress = "192.168.1.101", 
                Port = 502,
                MinTorque = 8.0f,
                MaxTorque = 12.0f
            });

            Devices.Add(new ScrewingDevice 
            { 
                DeviceId = 3, 
                DeviceName = "Máy vặn vít #3",
                IPAddress = "192.168.1.102", 
                Port = 502,
                MinTorque = 7.5f,
                MaxTorque = 11.0f
            });

            Devices.Add(new ScrewingDevice 
            { 
                DeviceId = 4, 
                DeviceName = "Máy vặn vít #4",
                IPAddress = "192.168.1.103", 
                Port = 502,
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
                    // Simulate realistic torque values around target
                    float targetTorque = device.TargetTorque;
                    float variation = (device.MaxTorque - device.MinTorque) * 0.2f;
                    device.ActualTorque = (float)(targetTorque + (random.NextDouble() - 0.5) * variation);

                    // Ensure within bounds
                    device.ActualTorque = Math.Max(device.MinTorque, Math.Min(device.MaxTorque, device.ActualTorque));

                    // Update statistics occasionally
                    if (random.NextDouble() < 0.1) // 10% chance
                    {
                        if (device.ActualTorque >= device.MinTorque && device.ActualTorque <= device.MaxTorque)
                        {
                            device.OKDeviceCount++;
                            device.TotalCount++;
                        }
                        else
                        {
                            device.NGDeviceCount++;
                            device.TotalCount++;
                        }
                    }

                    device.Status = device.DeviceId <= 2 ? "Siết OK" : "Siết NG";
                    device.IsOK = device.DeviceId <= 2;
                    device.LastUpdate = DateTime.Now;
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
                // Use cached connection settings
                _modbusSettings = LoadModbusConfig();
                bool connected = false;

                switch (_modbusSettings.ConnectionType)
                {
                    case "TCP_Individual":
                        // Kết nối tất cả thiết bị song song
                        var enabledDevices = Devices.Where(d => d.Enabled).ToList();
                        var totalDevices = enabledDevices.Count;
                        Console.WriteLine($"[CONNECT] Found {enabledDevices.Count} enabled devices out of {Devices.Count} total devices");
                        foreach (var dev in enabledDevices)
                        {
                            Console.WriteLine($"[CONNECT] Will connect to Device {dev.DeviceId}: {dev.DeviceName} at {dev.IPAddress}:{dev.Port}");
                        }

                        // Update UI with progress
                        ConnectionStatus = $"Đang kết nối 0/{totalDevices} thiết bị...";

                        // Create parallel connection tasks
                        var connectionTasks = enabledDevices.Select(async device =>
                        {
                            device.Status = "Đang kết nối...";
                            OnPropertyChanged(nameof(ConnectionStatus));

                            var deviceConnected = await _modbusService.ConnectToDevice(device);
                            if (deviceConnected)
                            {
                                device.IsConnected = true;
                                device.Status = "Sẵn sàng";

                                return true;
                            }
                            else
                            {
                                device.IsConnected = false;
                                device.Status = "Kết nối thất bại";
                                return false;
                            }
                        }).ToList();

                        // Cập nhật trạng thái UI trong khi chờ kết nối
                        while (connectionTasks.Any(t => !t.IsCompleted))
                        {
                            var connectedCount = Devices.Count(d => d.IsConnected);
                            ConnectionStatus = $"Đang kết nối {connectedCount}/{totalDevices} thiết bị...";
                            OnPropertyChanged(nameof(ConnectionStatus));
                            OnPropertyChanged(nameof(ConnectedDevicesCount));
                            await Task.Delay(100); // Chờ một chút trước khi cập nhật lại
                        }

                        // Wait for all connections to complete
                        var results = await Task.WhenAll(connectionTasks);
                        var finalConnectedCount = results.Count(r => r);
                        connected = finalConnectedCount > 0;
                        break;

                    case "TCP_Gateway":
                        connected = await _modbusService.ConnectTCP_Gateway(_modbusSettings.GatewayIP, _modbusSettings.GatewayPort);
                        if (connected)
                        {
                            foreach (var device in Devices.Where(d => d.Enabled))
                            {
                                device.IsConnected = true;
                                device.Status = "Sẵn sàng";
                            }
                        }
                        break;

                    case "RTU_Serial":
                        connected = _modbusService.ConnectRTU(_modbusSettings.SerialPort, _modbusSettings.BaudRate);
                        if (connected)
                        {
                            foreach (var device in Devices.Where(d => d.Enabled))
                            {
                                device.IsConnected = true;
                                device.Status = "Sẵn sàng";
                            }
                        }
                        break;

                    default:
                        ConnectionStatus = "Lỗi: Loại kết nối không xác định.";
                        connected = false;
                        break;
                }

                // Kiểm tra số thiết bị kết nối thành công
                int connectedCountAfterAll = Devices.Count(d => d.IsConnected);

                if (connectedCountAfterAll > 0)
                {
                    ConnectionStatus = $"Đã kết nối {connectedCountAfterAll}/{TotalDevices} thiết bị ({_modbusSettings.ConnectionType})";
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
                device.ActualTorque = 0;
                device.IsOK = false;
                // Xóa cả dữ liệu thống kê
                device.TotalCount = 0;
                device.OKDeviceCount = 0;
                device.NGDeviceCount = 0;

                device.LastSuccessfulRead = DateTime.MinValue;  // Reset health tracking
            }

            // Ngắt kết nối chung (cho Gateway)
            _modbusService.Disconnect();

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

                // Đảm bảo timer được cấu hình và đăng ký sự kiện đúng cách MỖI KHI bắt đầu.
                // Điều này tránh các lỗi do cấu hình lại ở nơi khác.
                _timer.Stop(); // Dừng timer cũ nếu đang chạy
                _timer.Interval = _modbusSettings.ScanInterval > 0 ? TimeSpan.FromMilliseconds(_modbusSettings.ScanInterval) : TimeSpan.FromSeconds(1);
                _timer.Tick -= Timer_Tick; // Hủy đăng ký cũ để tránh gọi nhiều lần
                _timer.Tick += Timer_Tick; // Đăng ký mới
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
            var settingsViewModel = new SettingsViewModel(_modbusService);
            var settingsWindow = new HMI_ScrewingMonitor.Views.SettingsWindow(settingsViewModel);
            settingsWindow.Owner = Application.Current.MainWindow;
            settingsWindow.ShowDialog();

            // Reload devices after settings window closes
            LoadDevicesFromConfig();

            // Reload modbus settings as they might have changed
            _modbusSettings = LoadModbusConfig();
            _timer.Interval = _modbusSettings.ScanInterval > 0 
                ? TimeSpan.FromMilliseconds(_modbusSettings.ScanInterval) 
                : TimeSpan.FromSeconds(1);
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
                                DeviceModel = deviceConfig.DeviceModel,
                                IPAddress = deviceConfig.IPAddress,
                                Port = deviceConfig.Port,
                                SlaveId = deviceConfig.SlaveId,
                                MinTorque = (float)deviceConfig.MinTorque,
                                MaxTorque = (float)deviceConfig.MaxTorque,
                                TargetTorque = (float)deviceConfig.TargetTorque,
                                TotalCount = deviceConfig.TotalCount,
                                OKDeviceCount = deviceConfig.OKCount,
                                NGDeviceCount = deviceConfig.NGCount,
                                Enabled = deviceConfig.Enabled,  // Set enabled status from config
                                // Trạng thái ban đầu
                                IsConnected = false,
                                Status = "--",
                                IsOK = false,
                                ActualTorque = 0
                            };
                            Console.WriteLine($"[CONFIG] Loaded Device {device.DeviceId}: {device.DeviceName}, Enabled={device.Enabled}, IP={device.IPAddress}");
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

            // Chỉ xử lý các thiết bị được kích hoạt
            foreach (var device in Devices.Where(d => d.Enabled).ToList())
            {
                // Chỉ đọc dữ liệu từ các thiết bị đang có kết nối
                if (!device.IsConnected)
                {
                    // Thử kết nối lại nếu thiết bị đang offline
                    bool reconnected = await _modbusService.ConnectToDevice(device);
                    // Nếu kết nối lại thất bại, bỏ qua thiết bị này trong chu kỳ hiện tại
                    if (!reconnected)
                    {
                        continue;
                    }
                }

                try
                {
                    DeviceReadEvent eventResult = null;
                    switch (_modbusSettings.ConnectionType)
                    {
                        case "TCP_Individual":
                            eventResult = await _modbusService.ReadDeviceEvent_Individual(device);
                            break;

                        case "TCP_Gateway":
                            eventResult = await _modbusService.ReadDeviceEvent_Gateway(device);
                            break;

                        case "RTU_Serial":
                            // Phương thức RTU chưa được triển khai, tạm thời gán null
                            // và thay thế readResult bằng eventResult cho đúng.
                            eventResult = null; // await _modbusService.ReadDeviceEvent_RTU(device);
                            break;

                        default:
                            eventResult = await _modbusService.ReadDeviceEvent_Individual(device);
                            break;
                    }

                    if (eventResult != null && eventResult.Success)
                    {
                        // Nếu đây là một sự kiện hoàn thành (COMP vừa bật ON)
                        if (eventResult.IsCompletionEvent)
                        {
                            // Validation: Tránh duplicate events trong 1 giây
                            var now = DateTime.Now;
                            if (device.LastUpdate != DateTime.MinValue &&
                                (now - device.LastUpdate).TotalSeconds < 1.0)
                            {
                                Console.WriteLine($"[SKIP] Device {device.DeviceId} - Duplicate completion event detected within 1 second");
                                return; // Skip duplicate event
                            }

                            Console.WriteLine($"[EVENT] Device {device.DeviceId} - Processing NEW completion event");

                            // Cập nhật tất cả dữ liệu từ sự kiện
                            device.IsConnected = true;
                            device.LastSuccessfulRead = now;
                            device.ActualTorque = eventResult.ActualTorque;
                            device.IsOK = eventResult.IsOK;
                            device.Status = eventResult.StatusMessage;
                            // Cập nhật TotalCount: ưu tiên từ thiết bị, nếu không có thì tự tăng
                            if (eventResult.TotalCount > 0)
                                device.TotalCount = eventResult.TotalCount;
                            else
                                device.TotalCount++; // Tự động tăng nếu thiết bị không cung cấp

                            // Cập nhật các giá trị cài đặt từ thiết bị (nếu có)
                            if (eventResult.TargetTorque > 0)
                                device.TargetTorque = eventResult.TargetTorque;
                            if (eventResult.MinTorque > 0)
                                device.MinTorque = eventResult.MinTorque;
                            if (eventResult.MaxTorque > 0)
                                device.MaxTorque = eventResult.MaxTorque;

                            device.LastUpdate = now;

                            // Cập nhật bộ đếm OK/NG cho chính thiết bị đó
                            if (eventResult.IsOK)
                            {
                                device.OKDeviceCount++;
                                Console.WriteLine($"[COUNTER] Device {device.DeviceId} - OK Count: {device.OKDeviceCount}");
                            }
                            else
                            {
                                device.NGDeviceCount++;
                                Console.WriteLine($"[COUNTER] Device {device.DeviceId} - NG Count: {device.NGDeviceCount}");
                            }

                            Console.WriteLine($"[COUNTER] Device {device.DeviceId} - Total Count: {device.TotalCount}");

                            // Ghi log completion event
                            Console.WriteLine($"[LOGGING] Device {device.DeviceId} - Writing completion event to log");
                            _loggingService.LogScrewingEvent(device);

                            // Cập nhật bộ đếm trong ngày
                            if (DateTime.Today != _currentDate)
                            {
                                _currentDate = DateTime.Today;
                                DailyOKCount = 0;
                                DailyNGCount = 0;
                            }
                            if (device.IsOK)
                                DailyOKCount++;
                            else
                                DailyNGCount++;

                            Console.WriteLine($"[DAILY] Daily counts - OK: {DailyOKCount}, NG: {DailyNGCount}");
                        }

                        // Luôn cập nhật trạng thái COMP trước đó cho lần kiểm tra tiếp theo
                        device.PreviousCompletionState = eventResult.CurrentCompletionState;
                    }
                    else
                    {
                        // Handle read failure
                        Console.WriteLine($"[ERROR] Device {device.DeviceId}: Read failed - Success={eventResult?.Success}, Status={eventResult?.StatusMessage}");
                        device.IsConnected = false; // Set to false on any read error
                        device.Status = eventResult?.StatusMessage ?? "Lỗi đọc dữ liệu";
                        device.ActualTorque = 0;
                        device.IsOK = false;
                        device.LastUpdate = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    device.IsConnected = false;
                    device.Status = "--";
                    // Xóa dữ liệu cũ khi có lỗi
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
            if (ConnectedDevicesCount == 0 && TotalDevices > 0)
            {
                ConnectionStatus = "Tất cả thiết bị đã mất kết nối";
            }
            else if (TotalDevices > 0)
            {
                ConnectionStatus = $"Đã kết nối {ConnectedDevicesCount}/{TotalDevices} thiết bị ({_modbusSettings.ConnectionType})";
            }
        }

        private void UpdateButtonStates()
        {
            OnPropertyChanged(nameof(CanConnect));
            OnPropertyChanged(nameof(CanDisconnect));
            OnPropertyChanged(nameof(CanStartMonitoring));
            OnPropertyChanged(nameof(CanStopMonitoring));
            OnPropertyChanged(nameof(ConnectedDevicesCount));
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
