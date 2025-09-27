using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using HMI_ScrewingMonitor.Models;

namespace HMI_ScrewingMonitor.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private DeviceConfig _selectedDevice;
        private string _configFilePath = "Config/devices.json";

        public ObservableCollection<DeviceConfig> Devices { get; set; }
        public ModbusSettingsConfig ModbusSettings { get; set; }
        public UISettingsConfig UISettings { get; set; }

        public DeviceConfig SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                OnPropertyChanged(nameof(SelectedDevice));
            }
        }

        // Commands
        public ICommand AddDeviceCommand { get; }
        public ICommand EditDeviceCommand { get; }
        public ICommand DeleteDeviceCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand CancelCommand { get; }

        public SettingsViewModel()
        {
            Devices = new ObservableCollection<DeviceConfig>();
            ModbusSettings = new ModbusSettingsConfig();
            UISettings = new UISettingsConfig();

            // Initialize commands
            AddDeviceCommand = new RelayCommand(AddDevice);
            EditDeviceCommand = new RelayCommand(EditDevice, () => SelectedDevice != null);
            DeleteDeviceCommand = new RelayCommand(DeleteDevice, () => SelectedDevice != null);
            SaveCommand = new RelayCommand(SaveSettings);
            ReloadCommand = new RelayCommand(LoadSettings);
            CancelCommand = new RelayCommand(Cancel);

            LoadSettings();
        }

        private void AddDevice()
        {
            var newDevice = new DeviceConfig
            {
                DeviceId = Devices.Count > 0 ? Devices.Max(d => d.DeviceId) + 1 : 1,
                DeviceName = $"Máy vặn vít #{Devices.Count + 1}",
                IPAddress = "192.168.1.100",
                Port = 502,
                SlaveId = Devices.Count + 1,
                DeviceModel = "ABC",
                TargetTorque = 10.0,
                TotalCount = 100,
                OKCount = 90,
                NGCount = 10,
                MinTorque = 7.0,
                MaxTorque = 10.0,
                Enabled = true
            };

            Devices.Add(newDevice);
            SelectedDevice = newDevice;
        }

        private void EditDevice()
        {
            // Device is already bound to UI, changes are automatic
            // This command could open a detailed edit dialog if needed
        }

        private void DeleteDevice()
        {
            if (SelectedDevice != null)
            {
                var result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa thiết bị '{SelectedDevice.DeviceName}'?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Devices.Remove(SelectedDevice);
                    SelectedDevice = Devices.FirstOrDefault();
                }
            }
        }

        private void SaveSettings()
        {
            try
            {
                var config = new AppConfig
                {
                    Devices = Devices.ToList(),
                    ModbusSettings = ModbusSettings,
                    UI = UISettings
                };

                // Ensure directory exists
                var directory = Path.GetDirectoryName(_configFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_configFilePath, json);

                MessageBox.Show("Cấu hình đã được lưu thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Close window
                Application.Current.Windows.OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu cấu hình: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var json = File.ReadAllText(_configFilePath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);

                    if (config != null)
                    {
                        Devices.Clear();
                        foreach (var device in config.Devices)
                        {
                            Devices.Add(device);
                        }

                        ModbusSettings = config.ModbusSettings ?? new ModbusSettingsConfig();
                        UISettings = config.UI ?? new UISettingsConfig();

                        SelectedDevice = Devices.FirstOrDefault();
                    }
                }
                else
                {
                    LoadDefaultSettings();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải cấu hình: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                LoadDefaultSettings();
            }
        }

        private void LoadDefaultSettings()
        {
            // Load default devices
            Devices.Clear();
            for (int i = 1; i <= 4; i++)
            {
                Devices.Add(new DeviceConfig
                {
                    DeviceId = i,
                    DeviceName = $"Máy vặn vít #{i}",
                    IPAddress = $"192.168.1.{99 + i}",
                    Port = 502,
                    SlaveId = i,
                    DeviceModel = $"D{i:00}",
                    TargetTorque = 8.0 + i * 0.3,
                    TotalCount = 100 + i * 5,
                    OKCount = 85 + i * 3,
                    NGCount = 10 + i,
                    MinTorque = 7.0 + i * 0.5,
                    MaxTorque = 10.0 + i * 0.5,
                    Enabled = i <= 3 // Enable first 3 devices by default
                });
            }

            ModbusSettings = new ModbusSettingsConfig();
            UISettings = new UISettingsConfig();

            SelectedDevice = Devices.FirstOrDefault();
        }

        private void Cancel()
        {
            // Close window without saving
            Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)?.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Configuration classes
    public class AppConfig
    {
        public List<DeviceConfig> Devices { get; set; } = new();
        public ModbusSettingsConfig ModbusSettings { get; set; } = new();
        public UISettingsConfig UI { get; set; } = new();
    }

    public class DeviceConfig
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; } = "";
        public string DeviceModel { get; set; } = "ABC";
        public string IPAddress { get; set; } = "";
        public int Port { get; set; } = 502;
        public int SlaveId { get; set; }
        public double MinTorque { get; set; }
        public double MaxTorque { get; set; }
        public double TargetTorque { get; set; }
        public int TotalCount { get; set; } = 100;
        public int OKCount { get; set; } = 90;
        public int NGCount { get; set; } = 10;
        public bool Enabled { get; set; } = true;
    }

    public class ModbusSettingsConfig
    {
        public string ConnectionType { get; set; } = "TCP_Individual";
        public string GatewayIP { get; set; } = "192.168.1.100";
        public int GatewayPort { get; set; } = 502;
        public string SerialPort { get; set; } = "COM1";
        public int BaudRate { get; set; } = 9600;
        public int Timeout { get; set; } = 5000;
        public int RetryCount { get; set; } = 3;
        public int ScanInterval { get; set; } = 1000;
        public string Note { get; set; } = "ConnectionType options: TCP_Individual, TCP_Gateway, RTU_Serial";
    }

    public class UISettingsConfig
    {
        public string Theme { get; set; } = "Light";
        public string Language { get; set; } = "Vietnamese";
        public int RefreshInterval { get; set; } = 1000;
        public int GridColumns { get; set; } = 2;
        public int GridRows { get; set; } = 0; // 0 = auto
    }
}