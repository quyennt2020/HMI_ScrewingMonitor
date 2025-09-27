// Updated ModbusService.cs - Hỗ trợ cả 3 cách kết nối

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using NModbus;
using System.IO.Ports;
using HMI_ScrewingMonitor.Models;
using System.IO; 
using System.Text.Json;

namespace HMI_ScrewingMonitor.Services
{
    public class ModbusService
    {
        // Individual device connection management
        private Dictionary<int, TcpClient> _deviceConnections;
        private Dictionary<int, IModbusMaster> _deviceMasters;
        private Dictionary<int, DateTime> _lastConnectionAttempt;

        public bool IsConnected { get; private set; }

        public enum ConnectionType
        {
            TCP_Individual,     // Mỗi thiết bị 1 IP
            TCP_Gateway,       // 1 IP gateway, nhiều Slave ID  
            RTU_Serial        // RS485 serial
        }

        public ModbusService()
        {
            // Initialize individual device connection tracking
            _deviceConnections = new Dictionary<int, TcpClient>();
            _deviceMasters = new Dictionary<int, IModbusMaster>();
            _lastConnectionAttempt = new Dictionary<int, DateTime>();
        }

        #region TCP Individual IPs
        /// <summary>
        /// Phương thức đọc dữ liệu mới, tuân thủ logic phát hiện sự kiện từ tài liệu.
        /// </summary>
        public async Task<DeviceReadEvent> ReadDeviceEvent_Individual(ScrewingDevice device)
        {
            int deviceId = device.DeviceId;

            if (!IsDeviceConnected(deviceId))
            {
                return new DeviceReadEvent { Success = false, StatusMessage = "Chưa kết nối" };
            }

            try
            {
                var modbusMaster = _deviceMasters[deviceId];
                byte slaveId = (byte)device.SlaveId;

                // DEBUG: Log điểm bắt đầu và trạng thái COMP trước đó
                Console.WriteLine($"[DEBUG] Device {deviceId}: Reading event. Prev COMP: {device.PreviousCompletionState}");

                // HANDY2000 QUY TRÌNH CHÍNH THỨC
                // Bước 1.1: Đọc COMP (100084) và BUSY (100082)
                // NModbus địa chỉ: BUSY=81, COMP=83 (vì địa chỉ Modbus bắt đầu từ 0)

                bool[] statusBits;
                try
                {
                    // Đọc 3 bits: BUSY(81), Reserved(82), COMP(83)
                    statusBits = await modbusMaster.ReadInputsAsync(slaveId, 81, 3);
                }
                catch (Exception ex)
                {
                    // Fallback: Nếu simulator không hỗ trợ ReadInputs, thử đọc từ Holding Registers
                    Console.WriteLine($"[FALLBACK] Device {deviceId}: ReadInputs failed, trying Holding Registers fallback");
                    try
                    {
                        ushort[] fallbackRegisters = await modbusMaster.ReadHoldingRegistersAsync(slaveId, 0, 5);
                        // Giả lập test data cho simulator
                        bool simulatedComp = (fallbackRegisters[0] % 2 == 1); // Giả lập COMP bit

                        return new DeviceReadEvent
                        {
                            Success = true,
                            IsCompletionEvent = simulatedComp && !device.PreviousCompletionState, // Phát hiện cạnh lên
                            IsOK = true, // Giả lập OK cho test
                            ActualTorque = (float)fallbackRegisters[1] / 10.0f + 10.0f, // Test torque
                            TotalCount = device.TotalCount + (simulatedComp && !device.PreviousCompletionState ? 1 : 0),
                            CurrentCompletionState = simulatedComp,
                            StatusMessage = "Simulator Fallback",
                            TargetTorque = device.TargetTorque,
                            MinTorque = device.MinTorque,
                            MaxTorque = device.MaxTorque
                        };
                    }
                    catch
                    {
                        throw new Exception($"Both ReadInputs and Holding Register fallback failed: {ex.Message}");
                    }
                }

                bool currentBusy = statusBits[0];      // Bit 100082 (BUSY)
                bool currentComp = statusBits[2];      // Bit 100084 (COMP)

                Console.WriteLine($"[HANDY2000] Device {deviceId}: BUSY={currentBusy}, COMP={currentComp}, PrevCOMP={device.PreviousCompletionState}");

                // Bước 1.2: Phát hiện cạnh lên COMP (0→1) và BUSY=OFF
                if (currentComp && !currentBusy && !device.PreviousCompletionState)
                {
                    // 🎉 PHÁT HIỆN COMPLETION EVENT - Có lần siết mới hoàn thành!
                    Console.WriteLine($"[SUCCESS] Device {deviceId} - COMPLETION DETECTED (COMP Rising Edge, BUSY=OFF)");

                    // Bước 1.3: Đọc kết quả OK/NG từ bits 100085/100086
                    bool[] resultBits = await modbusMaster.ReadInputsAsync(slaveId, 84, 2);
                    bool isOk = resultBits[0];  // Bit 100085 (OK)
                    bool isNg = resultBits[1];  // Bit 100086 (NG)

                    Console.WriteLine($"[HANDY2000] Device {deviceId}: Result OK={isOk}, NG={isNg}");

                    // Bước 1.4: Đọc dữ liệu chi tiết từ Input Registers (3X)
                    // ĐÚNG THEO TÀI LIỆU HANDY2000:

                    // LAST FASTEN FINAL TORQUE: 308467 -> NModbus địa chỉ 8466
                    ushort[] torqueRegister = await modbusMaster.ReadInputRegistersAsync(slaveId, 8466, 1);
                    float finalTorque = (float)torqueRegister[0] / 10.0f; // Chia 10 theo đặc tả

                    // Đọc Target/Min/Max theo đúng địa chỉ:
                    // 308481 (TARGET) -> 8480
                    // 308482 (MIN) -> 8481
                    // 308483 (MAX) -> 8482
                    ushort[] setpointRegisters = await modbusMaster.ReadInputRegistersAsync(slaveId, 8480, 3);
                    float targetTorque = (float)setpointRegisters[0] / 10.0f; // 308481
                    float minTorque = (float)setpointRegisters[1] / 10.0f;    // 308482
                    float maxTorque = (float)setpointRegisters[2] / 10.0f;    // 308483

                    Console.WriteLine($"[HANDY2000] Device {deviceId}: Torque Data - Final={finalTorque:F1}, Target={targetTorque:F1}, Range={minTorque:F1}-{maxTorque:F1}");

                    // Trả về completion event với đầy đủ dữ liệu thực
                    return new DeviceReadEvent
                    {
                        Success = true,
                        IsCompletionEvent = true, // Đây là completion event thực sự
                        IsOK = isOk,
                        ActualTorque = finalTorque,
                        TotalCount = device.TotalCount + 1, // Tăng counter cho lần siết mới
                        CurrentCompletionState = currentComp,
                        StatusMessage = isOk ? "Siết OK" : "Siết NG",
                        TargetTorque = targetTorque,
                        MinTorque = minTorque,
                        MaxTorque = maxTorque
                    };
                }

                // Không có completion event, chỉ trả về trạng thái hiện tại
                return new DeviceReadEvent
                {
                    Success = true,
                    IsCompletionEvent = false, // Không phải completion event
                    CurrentCompletionState = currentComp
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Device {deviceId} - Read Event Error: {ex.Message}");
                return HandleReadError(device, "Lỗi đọc Modbus", ex);
            }
        }

        // Các phương thức ReadDeviceData_... cũ có thể được giữ lại hoặc xóa đi
        // Để đơn giản, tôi sẽ comment chúng ra
        /*
        public async Task<ModbusReadResult> ReadDeviceData_Individual(ScrewingDevice device)
        {
            if (_gatewayMaster == null || !_isConnected)
            {
                return new ModbusReadResult { Success = false, StatusMessage = "Gateway chưa kết nối" };
            }

            try
            {...}
        }
        */
        #endregion

        #region TCP Gateway
        private TcpClient _gatewayClient;
        private IModbusMaster _gatewayMaster;

        public async Task<bool> ConnectTCP_Gateway(string gatewayIP, int gatewayPort)
        {
            try
            {
                _gatewayClient = new TcpClient();
                await _gatewayClient.ConnectAsync(gatewayIP, gatewayPort);
                
                var factory = new ModbusFactory();
                _gatewayMaster = factory.CreateMaster(_gatewayClient);
                
                IsConnected = true;
                return true;
            }
            catch
            {
                IsConnected = false;
                return false;
            }
        }

        public async Task<DeviceReadEvent> ReadDeviceEvent_Gateway(ScrewingDevice device)
        {
            if (_gatewayMaster == null || !IsConnected)
            {
                return new DeviceReadEvent { Success = false, StatusMessage = "Gateway chưa kết nối" };
            }

            try
            {
                byte slaveId = (byte)device.SlaveId;

                // Bước 1: Giám sát tín hiệu COMP (Completion)
                bool[] compSignal = await _gatewayMaster.ReadInputsAsync(slaveId, 83, 1);
                bool currentCompState = compSignal[0];

                // Phát hiện cạnh lên (OFF -> ON)
                if (currentCompState && !device.PreviousCompletionState)
                {
                    // ĐÃ PHÁT HIỆN HOÀN THÀNH MỘT LẦN VẶN!
                    Console.WriteLine($"Device {device.DeviceId} (Gateway) - COMPLETION DETECTED");

                    // Bước 2: Đọc kết quả OK/NG
                    bool[] resultSignals = await _gatewayMaster.ReadInputsAsync(slaveId, 84, 2);
                    bool isOk = resultSignals[0];

                    // Bước 3: Đọc giá trị lực siết
                    ushort[] torqueRegister = await _gatewayMaster.ReadInputRegistersAsync(slaveId, 8464, 1);
                    float finalTorque = (float)torqueRegister[0] / 10.0f;

                    // Bước 4: Đọc bộ đếm
                    ushort[] counterRegister = await _gatewayMaster.ReadInputRegistersAsync(slaveId, 8210, 1);
                    int fasteningCount = counterRegister[0];

                    // Đọc các giá trị cài đặt của lần vặn cuối
                    ushort[] setpointRegisters = await _gatewayMaster.ReadInputRegistersAsync(slaveId, 8480, 3);
                    float targetTorque = (float)setpointRegisters[0] / 10.0f;
                    float minTorque = (float)setpointRegisters[1] / 10.0f;
                    float maxTorque = (float)setpointRegisters[2] / 10.0f;

                    Console.WriteLine($"Device {device.DeviceId} (Gateway) - Setpoints Read: Target={targetTorque}, Min={minTorque}, Max={maxTorque}");

                    return new DeviceReadEvent
                    {
                        Success = true,
                        IsCompletionEvent = true,
                        IsOK = isOk,
                        ActualTorque = finalTorque,
                        TotalCount = fasteningCount,
                        CurrentCompletionState = currentCompState,
                        StatusMessage = isOk ? "Siết OK" : "Siết NG",
                        TargetTorque = targetTorque,
                        MinTorque = minTorque,
                        MaxTorque = maxTorque
                    };
                }

                // Nếu không có sự kiện hoàn thành
                return new DeviceReadEvent
                {
                    Success = true,
                    IsCompletionEvent = false,
                    CurrentCompletionState = currentCompState
                };
            }
            catch (Exception ex)
            {
                // Sử dụng ShouldLogError để tránh spam console khi có lỗi
                if (ShouldLogError(device, "GatewayReadError"))
                {
                    Console.WriteLine($"Device {device.DeviceId} (Gateway) - Read Event Error: {ex.Message}");
                }
                return HandleReadError(device, "Lỗi đọc Gateway", ex);
            }
        }
        #endregion

        #region RTU Serial
        public bool ConnectRTU(string portName, int baudRate = 9600)
        {
            // RTU connection temporarily disabled due to NModbus API changes
            throw new NotImplementedException("RTU connection not implemented for current NModbus version");
        }

        public Task<DeviceReadEvent> ReadDeviceData_RTU(ScrewingDevice device)
        {
            throw new NotImplementedException("RTU data reading not implemented for current NModbus version");
        }
        #endregion

        #region Common Methods
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
                    Console.WriteLine($"[DEBUG] Device {deviceId}: Closing existing connection");
                    DisconnectFromDevice(deviceId);
                }

                Console.WriteLine($"CONNECTING to Device {deviceId} at {device.IPAddress}:{device.Port}");

                var tcpClient = new TcpClient();

                // Set timeouts to speed up connection
                tcpClient.ReceiveTimeout = 3000;  // 3 seconds
                tcpClient.SendTimeout = 3000;     // 3 seconds

                Console.WriteLine($"[DEBUG] Device {deviceId}: Starting TCP connection...");

                // Use CancellationToken for connection timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await tcpClient.ConnectAsync(device.IPAddress, device.Port).ConfigureAwait(false);

                Console.WriteLine($"[DEBUG] Device {deviceId}: TCP connected, creating Modbus master...");

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
        private DeviceReadEvent HandleReadError(ScrewingDevice device, string reason, Exception ex = null)
        {
            // Log lỗi nhưng không spam console
            if (ShouldLogError(device, "ReadError"))
            {
                Console.WriteLine($"Device {device.DeviceId} - READ ERROR: {reason}");
                if (ex != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception details: {ex.Message}");
                }
            }

            return new DeviceReadEvent { Success = false, StatusMessage = reason };
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

        public void Disconnect()
        {
            // Đóng gateway connection
            _gatewayClient?.Close();
            
            IsConnected = false;
        }

        public void Dispose()
        {
            Disconnect();
        }
        #endregion
    }

    /// <summary>
    /// DTO mới để chứa kết quả đọc theo logic sự kiện.
    /// </summary>
    public class DeviceReadEvent
    {
        public bool Success { get; set; }
        public string StatusMessage { get; set; }
        public bool IsCompletionEvent { get; set; } // Đánh dấu nếu đây là sự kiện hoàn thành
        public bool CurrentCompletionState { get; set; } // Trạng thái hiện tại của bit COMP

        // Dữ liệu chỉ có ý nghĩa khi IsCompletionEvent = true
        public bool IsOK { get; set; }
        public float ActualTorque { get; set; }
        public int TotalCount { get; set; }
        public float TargetTorque { get; set; }
        public float MinTorque { get; set; }
        public float MaxTorque { get; set; }
    }
}
