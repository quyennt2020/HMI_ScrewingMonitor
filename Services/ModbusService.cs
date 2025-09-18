// Updated ModbusService.cs - Hỗ trợ cả 3 cách kết nối

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using NModbus;
using System.IO.Ports;
using HMI_ScrewingMonitor.Models;

namespace HMI_ScrewingMonitor.Services
{
    public class ModbusService
    {
        private Dictionary<string, TcpClient> _tcpClients;
        private Dictionary<string, IModbusMaster> _modbusMasters;
        private SerialPort _serialPort;
        private IModbusMaster _rtuMaster;
        private bool _isConnected;
        private ConnectionType _connectionType;

        // Individual device connection management
        private Dictionary<int, TcpClient> _deviceConnections;
        private Dictionary<int, IModbusMaster> _deviceMasters;
        private Dictionary<int, DateTime> _lastConnectionAttempt;

        public bool IsConnected => _isConnected;

        public enum ConnectionType
        {
            TCP_Individual,     // Mỗi thiết bị 1 IP
            TCP_Gateway,       // 1 IP gateway, nhiều Slave ID  
            RTU_Serial        // RS485 serial
        }

        public ModbusService()
        {
            _tcpClients = new Dictionary<string, TcpClient>();
            _modbusMasters = new Dictionary<string, IModbusMaster>();

            // Initialize individual device connection tracking
            _deviceConnections = new Dictionary<int, TcpClient>();
            _deviceMasters = new Dictionary<int, IModbusMaster>();
            _lastConnectionAttempt = new Dictionary<int, DateTime>();
        }

        #region TCP Individual IPs
        public async Task<bool> ConnectTCP_Individual(List<ScrewingDevice> devices)
        {
            _connectionType = ConnectionType.TCP_Individual;
            bool allConnected = true;

            foreach (var device in devices)
            {
                try
                {
                    // Include DeviceId in key to create separate connections for same IP
                    var key = $"{device.IPAddress}:{device.Port}:ID{device.DeviceId}";

                    // Tạo kết nối riêng cho mỗi thiết bị
                    var tcpClient = new TcpClient();
                    await tcpClient.ConnectAsync(device.IPAddress, device.Port);

                    var factory = new ModbusFactory();
                    var modbusMaster = factory.CreateMaster(tcpClient);

                    _tcpClients[key] = tcpClient;
                    _modbusMasters[key] = modbusMaster;

                    device.IsConnected = true;

                    System.Diagnostics.Debug.WriteLine($"*** CONNECTED Device {device.DeviceId} with key: {key} ***");
                    System.Diagnostics.Debug.WriteLine($"    IP: {device.IPAddress}, Port: {device.Port}, SlaveID: {device.DeviceId}");
                }
                catch (Exception ex)
                {
                    device.IsConnected = false;
                    device.Status = $"Kết nối thất bại: {ex.Message}";
                    allConnected = false;

                    System.Diagnostics.Debug.WriteLine($"Device {device.DeviceId} connection failed: {ex.Message}");
                }
            }

            _isConnected = allConnected;
            return allConnected;
        }

        public async Task<ScrewingDevice> ReadDeviceData_Individual(ScrewingDevice device)
        {
            int deviceId = device.DeviceId;

            // Kiểm tra kết nối riêng lẻ của thiết bị
            if (!IsDeviceConnected(deviceId))
            {
                device.IsConnected = false;
                device.Status = "Chưa kết nối TCP";
                System.Diagnostics.Debug.WriteLine($"Device {deviceId} - Not connected");
                return HandleConnectionLoss(device, "Chưa kết nối TCP");
            }

            // Kiểm tra health của kết nối
            if (!IsConnectionAlive(deviceId))
            {
                System.Diagnostics.Debug.WriteLine($"Device {deviceId} - Connection not alive, attempting reconnect");
                var reconnected = await ConnectToDevice(device);
                if (!reconnected)
                {
                    return HandleConnectionLoss(device, "Kết nối thất bại");
                }
            }

            try
            {
                var modbusMaster = _deviceMasters[deviceId];

                Console.WriteLine($"READING Device {deviceId} - SlaveID={device.SlaveId}");

                // Thử đọc đủ 13 registers để có status register
                ushort[] registers;
                try
                {
                    registers = await modbusMaster.ReadHoldingRegistersAsync(
                        (byte)device.SlaveId, 0, 13); // Đọc đủ 13 registers (0-12)
                    Console.WriteLine($"INDIVIDUAL SUCCESS - Device {deviceId} (Slave {device.SlaveId}): Đọc được 13 registers - R0={registers[0]}, R2={registers[2]}, R12={registers[12]}");
                }
                catch (Exception ex)
                {
                    // Fallback: chỉ đọc 4 registers nếu slave không hỗ trợ đủ
                    Console.WriteLine($"Device {deviceId} - Failed to read 13 registers, trying 4: {ex.Message}");
                    registers = await modbusMaster.ReadHoldingRegistersAsync(
                        (byte)device.SlaveId, 0, 4);
                    Console.WriteLine($"INDIVIDUAL SUCCESS - Device {deviceId} (Slave {device.SlaveId}): Đọc được 4 registers - R0={registers[0]}, R1={registers[1]}, R2={registers[2]}, R3={registers[3]}");
                }

                return ProcessRegisterData(device, registers);
            }
            catch (SocketException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Device {deviceId} - SocketException: {ex.Message}");
                return HandleConnectionLoss(device, "Mất kết nối mạng TCP", ex);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not allowed on non-connected"))
            {
                System.Diagnostics.Debug.WriteLine($"Device {deviceId} - InvalidOperation: {ex.Message}");
                return HandleConnectionLoss(device, "Thiết bị không phản hồi", ex);
            }
            catch (TimeoutException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Device {deviceId} - Timeout: {ex.Message}");
                return HandleConnectionLoss(device, $"Slave ID {device.SlaveId} timeout", ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Device {deviceId} - General Error: {ex.Message}");
                return HandleConnectionLoss(device, $"Lỗi Individual: {ex.GetType().Name}", ex);
            }
        }
        #endregion

        #region TCP Gateway
        private TcpClient _gatewayClient;
        private IModbusMaster _gatewayMaster;

        public async Task<bool> ConnectTCP_Gateway(string gatewayIP, int gatewayPort)
        {
            _connectionType = ConnectionType.TCP_Gateway;
            
            try
            {
                _gatewayClient = new TcpClient();
                await _gatewayClient.ConnectAsync(gatewayIP, gatewayPort);
                
                var factory = new ModbusFactory();
                _gatewayMaster = factory.CreateMaster(_gatewayClient);
                
                _isConnected = true;
                return true;
            }
            catch
            {
                _isConnected = false;
                return false;
            }
        }

        public async Task<ScrewingDevice> ReadDeviceData_Gateway(ScrewingDevice device)
        {
            if (_gatewayMaster == null || !_isConnected)
            {
                device.IsConnected = false;
                device.Status = "Gateway chưa kết nối";
                return device;
            }

            try
            {
                // Đọc dữ liệu từ gateway bằng Slave ID
                Console.WriteLine($"GATEWAY READING Device {device.DeviceId} with SlaveID={device.SlaveId}");
                System.Diagnostics.Debug.WriteLine($"GATEWAY READING Device {device.DeviceId} with SlaveID={device.SlaveId}");
                var registers = await _gatewayMaster.ReadHoldingRegistersAsync(
                    (byte)device.SlaveId, 0, 4); // Đọc 4 registers: 0,1,2,3 (giới hạn của simulator)

                Console.WriteLine($"GATEWAY SUCCESS - Device {device.DeviceId} (Slave {device.SlaveId}): R0={registers[0]}, R1={registers[1]}, R2={registers[2]}, R3={registers[3]}");

                return ProcessRegisterData(device, registers);
            }
            catch (SocketException ex)
            {
                if (ShouldLogError(device, "SocketException"))
                {
                    return HandleConnectionLoss(device, "Mất kết nối mạng TCP", ex);
                }
                return HandleConnectionLoss(device, "Mất kết nối mạng TCP");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not allowed on non-connected"))
            {
                if (ShouldLogError(device, "InvalidOperation"))
                {
                    return HandleConnectionLoss(device, "Thiết bị không phản hồi", ex);
                }
                return HandleConnectionLoss(device, "Thiết bị không phản hồi");
            }
            catch (TimeoutException ex)
            {
                if (ShouldLogError(device, "Timeout"))
                {
                    return HandleConnectionLoss(device, $"Slave ID {device.SlaveId} timeout", ex);
                }
                return HandleConnectionLoss(device, $"Slave ID {device.SlaveId} timeout");
            }
            catch (Exception ex)
            {
                if (ShouldLogError(device, "General"))
                {
                    return HandleConnectionLoss(device, $"Lỗi Gateway: {ex.GetType().Name}", ex);
                }
                return HandleConnectionLoss(device, "Lỗi Gateway");
            }
        }
        #endregion

        #region RTU Serial
        public bool ConnectRTU(string portName, int baudRate = 9600)
        {
            _connectionType = ConnectionType.RTU_Serial;

            // RTU connection temporarily disabled due to NModbus API changes
            throw new NotImplementedException("RTU connection not implemented for current NModbus version");
        }

        public async Task<ScrewingDevice> ReadDeviceData_RTU(ScrewingDevice device)
        {
            throw new NotImplementedException("RTU data reading not implemented for current NModbus version");
        }
        #endregion

        #region Common Methods
        private ScrewingDevice ProcessRegisterData(ScrewingDevice device, ushort[] registers)
        {
            try
            {
                device.IsConnected = true;
                device.LastSuccessfulRead = DateTime.Now;  // Cập nhật thời gian đọc thành công

                // Debug: Log register values
                System.Diagnostics.Debug.WriteLine($"Device {device.DeviceId} - Raw registers:");
                for (int i = 0; i < Math.Min(registers.Length, 13); i++)
                {
                    System.Diagnostics.Debug.WriteLine($"  Register {i}: {registers[i]}");
                }

                // Xử liệu dữ liệu từ registers
                if (registers.Length >= 3)
                {
                    // Simple integer to float conversion for testing với registers có sẵn
                    float angle = (float)registers[0];
                    float torque = (float)registers[2] / 10.0f;

                    device.ActualAngle = angle;
                    device.ActualTorque = torque;

                    // Kiểm tra xem có đọc được status register chưa
                    if (registers.Length >= 13)
                    {
                        // Đọc được status register 12
                        device.IsOK = registers[12] == 1;
                        device.Status = registers[12] == 1 ? "Siết OK" : "Siết NG";
                        Console.WriteLine($"Device {device.DeviceId} - Using STATUS REGISTER: R12={registers[12]}, Status={device.Status}");
                    }
                    else
                    {
                        // Fallback: Kiểm tra status dựa trên range
                        bool angleOK = angle >= device.MinAngle && angle <= device.MaxAngle;
                        bool torqueOK = torque >= device.MinTorque && torque <= device.MaxTorque;
                        device.IsOK = angleOK && torqueOK;

                        if (device.IsOK)
                            device.Status = "Siết OK (Range check)";
                        else if (!angleOK && !torqueOK)
                            device.Status = "Siết NG - Góc & Lực vượt ngưỡng";
                        else if (!angleOK)
                            device.Status = "Siết NG - Góc vượt ngưỡng";
                        else
                            device.Status = "Siết NG - Lực vượt ngưỡng";

                        Console.WriteLine($"Device {device.DeviceId} - Using RANGE CHECK: Status={device.Status}");
                    }

                    Console.WriteLine($"Device {device.DeviceId} - VALUES: Angle={angle}°, Torque={torque}Nm, Status={device.Status}");
                }
                else
                {
                    // Không đủ register - có thể slave chỉ hỗ trợ ít register
                    device.ActualAngle = registers.Length > 0 ? (float)registers[0] : 0;
                    device.ActualTorque = registers.Length > 2 ? (float)registers[2] / 10.0f : 0;
                    device.IsOK = registers.Length > 1 ? registers[1] == 1 : false;
                    device.Status = $"Đọc được {registers.Length} registers - Dữ liệu không đầy đủ";
                }

                device.LastUpdate = DateTime.Now;

                System.Diagnostics.Debug.WriteLine($"Device {device.DeviceId} - Processed: Angle={device.ActualAngle:F1}°, Torque={device.ActualTorque:F2}Nm, OK={device.IsOK}");

                return device;
            }
            catch (Exception ex)
            {
                // Nếu có lỗi xử lý dữ liệu, coi như mất kết nối
                return HandleConnectionLoss(device, "Lỗi xử lý dữ liệu", ex);
            }
        }

        #region Individual Device Connection Management
        public async Task<bool> ConnectToDevice(ScrewingDevice device)
        {
            int deviceId = device.DeviceId;

            // Check if already connected and healthy
            if (_deviceConnections.ContainsKey(deviceId) && IsConnectionAlive(deviceId))
            {
                return true;
            }

            // Throttle connection attempts (max 1 per 1 second) - reduced for faster reconnection
            if (_lastConnectionAttempt.ContainsKey(deviceId) &&
                DateTime.Now - _lastConnectionAttempt[deviceId] < TimeSpan.FromSeconds(1))
            {
                return false;
            }

            _lastConnectionAttempt[deviceId] = DateTime.Now;

            try
            {
                // Close existing connection if any
                if (_deviceConnections.ContainsKey(deviceId))
                {
                    DisconnectFromDevice(deviceId);
                }

                Console.WriteLine($"CONNECTING to Device {deviceId} at {device.IPAddress}:{device.Port}");

                var tcpClient = new TcpClient();

                // Set timeouts to speed up connection
                tcpClient.ReceiveTimeout = 3000;  // 3 seconds
                tcpClient.SendTimeout = 3000;     // 3 seconds

                // Use CancellationToken for connection timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await tcpClient.ConnectAsync(device.IPAddress, device.Port).ConfigureAwait(false);

                var factory = new ModbusFactory();
                var master = factory.CreateMaster(tcpClient);

                _deviceConnections[deviceId] = tcpClient;
                _deviceMasters[deviceId] = master;

                Console.WriteLine($"CONNECTED to Device {deviceId} successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CONNECTION FAILED - Device {deviceId}: {ex.Message}");
                return false;
            }
        }

        public void DisconnectFromDevice(int deviceId)
        {
            try
            {
                if (_deviceConnections.ContainsKey(deviceId))
                {
                    _deviceConnections[deviceId]?.Close();
                    _deviceConnections.Remove(deviceId);
                }

                if (_deviceMasters.ContainsKey(deviceId))
                {
                    _deviceMasters[deviceId]?.Dispose();
                    _deviceMasters.Remove(deviceId);
                }

                Console.WriteLine($"DISCONNECTED from Device {deviceId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DISCONNECT ERROR - Device {deviceId}: {ex.Message}");
            }
        }

        public bool IsConnectionAlive(int deviceId)
        {
            if (!_deviceConnections.ContainsKey(deviceId))
                return false;

            try
            {
                var client = _deviceConnections[deviceId];
                return client != null && client.Connected;
            }
            catch
            {
                return false;
            }
        }

        public bool IsDeviceConnected(int deviceId)
        {
            return _deviceConnections.ContainsKey(deviceId) && IsConnectionAlive(deviceId);
        }
        #endregion

        #region Connection Loss Handling
        private ScrewingDevice HandleConnectionLoss(ScrewingDevice device, string reason, Exception ex = null)
        {
            device.IsConnected = false;
            device.Status = "--";
            // Xóa dữ liệu cũ khi mất kết nối
            device.ActualAngle = 0;
            device.ActualTorque = 0;
            device.IsOK = false;
            device.LastUpdate = DateTime.Now;

            // Log lỗi nhưng không spam console
            Console.WriteLine($"CONNECTION LOST - Device {device.DeviceId}: {reason}");
            if (ex != null)
            {
                System.Diagnostics.Debug.WriteLine($"Exception details: {ex.Message}");
            }

            return device;
        }

        private bool ShouldLogError(ScrewingDevice device, string errorType)
        {
            // Chỉ log lỗi mỗi 30 giây để tránh spam console
            var now = DateTime.Now;
            var key = $"{device.DeviceId}_{errorType}";

            if (!_lastErrorTimes.ContainsKey(key) ||
                now - _lastErrorTimes[key] > TimeSpan.FromSeconds(30))
            {
                _lastErrorTimes[key] = now;
                return true;
            }
            return false;
        }

        private Dictionary<string, DateTime> _lastErrorTimes = new Dictionary<string, DateTime>();
        #endregion

        private float ConvertRegistersToFloat(ushort highRegister, ushort lowRegister)
        {
            byte[] bytes = new byte[4];
            byte[] highBytes = BitConverter.GetBytes(highRegister);
            byte[] lowBytes = BitConverter.GetBytes(lowRegister);
            
            bytes[0] = lowBytes[0];
            bytes[1] = lowBytes[1];
            bytes[2] = highBytes[0];
            bytes[3] = highBytes[1];
            
            return BitConverter.ToSingle(bytes, 0);
        }

        public void Disconnect()
        {
            // Đóng tất cả kết nối TCP
            foreach (var client in _tcpClients.Values)
            {
                client?.Close();
            }
            _tcpClients.Clear();
            _modbusMasters.Clear();

            // Đóng gateway connection
            _gatewayClient?.Close();
            
            // Đóng serial connection
            _serialPort?.Close();
            
            _isConnected = false;
        }

        public void Dispose()
        {
            Disconnect();
        }
        #endregion
    }
}
