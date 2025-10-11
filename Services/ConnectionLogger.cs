using System;
using System.IO;
using System.Text;

namespace HMI_ScrewingMonitor.Services
{
    /// <summary>
    /// Logger chuyên dụng cho việc debug connection issues
    /// </summary>
    public class ConnectionLogger
    {
        private static readonly object _lock = new object();
        private static readonly string _logDirectory = "ConnectionLogs";
        private static string _currentLogFile;
        private static DateTime _sessionStartTime;
        private static int _totalDevices;
        private static int _successCount;
        private static int _failCount;

        /// <summary>
        /// Bật/tắt ghi log kết nối. Mặc định: false (tắt)
        /// </summary>
        public static bool IsEnabled { get; set; } = false;

        static ConnectionLogger()
        {
            // Tạo thư mục ConnectionLogs nếu chưa có
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            // Tạo tên file log theo ngày
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
            _currentLogFile = Path.Combine(_logDirectory, $"connection_{dateStr}.log");
        }

        /// <summary>
        /// Bắt đầu session kết nối mới
        /// </summary>
        public static void StartConnectionSession(int totalDevices)
        {
            if (!IsEnabled) return;

            _sessionStartTime = DateTime.Now;
            _totalDevices = totalDevices;
            _successCount = 0;
            _failCount = 0;

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=".PadRight(70, '='));
            sb.AppendLine($"  CONNECTION SESSION START - {_sessionStartTime:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine("=".PadRight(70, '='));
            sb.AppendLine($"Total enabled devices: {totalDevices}");
            sb.AppendLine();

            WriteLog(sb.ToString());
        }

        /// <summary>
        /// Log thông tin bắt đầu kết nối device
        /// </summary>
        public static void LogConnectionStart(int deviceId, string deviceName, string ip, int port, int slaveId)
        {
            if (!IsEnabled) return;

            var sb = new StringBuilder();
            sb.AppendLine($"[{DateTime.Now:HH:mm:ss.fff}] [Device {deviceId}] Starting connection...");
            sb.AppendLine($"  Name: {deviceName}");
            sb.AppendLine($"  IP: {ip}, Port: {port}, SlaveId: {slaveId}");

            WriteLog(sb.ToString());
        }

        /// <summary>
        /// Log kết quả kết nối thành công
        /// </summary>
        public static void LogConnectionSuccess(int deviceId, long tcpTimeMs, long modbusTimeMs)
        {
            if (!IsEnabled) return;

            _successCount++;
            long totalMs = tcpTimeMs + modbusTimeMs;

            var sb = new StringBuilder();
            sb.AppendLine($"  TCP connected ({tcpTimeMs}ms)");
            sb.AppendLine($"  Modbus master created ({modbusTimeMs}ms)");
            sb.AppendLine($"  ✅ SUCCESS (Total: {totalMs}ms)");
            sb.AppendLine();

            WriteLog(sb.ToString());
        }

        /// <summary>
        /// Log kết quả kết nối thất bại
        /// </summary>
        public static void LogConnectionFailure(int deviceId, long elapsedMs, string errorMessage, Exception ex = null)
        {
            if (!IsEnabled) return;

            _failCount++;

            var sb = new StringBuilder();
            sb.AppendLine($"  ❌ FAILED ({elapsedMs}ms)");
            sb.AppendLine($"  Error: {errorMessage}");
            if (ex != null)
            {
                sb.AppendLine($"  Exception: {ex.GetType().Name}");
                sb.AppendLine($"  Message: {ex.Message}");
                if (ex.InnerException != null)
                {
                    sb.AppendLine($"  Inner: {ex.InnerException.Message}");
                }
            }
            sb.AppendLine();

            WriteLog(sb.ToString());
        }

        /// <summary>
        /// Kết thúc session và hiển thị tổng kết
        /// </summary>
        public static void EndConnectionSession()
        {
            if (!IsEnabled) return;

            var sessionEndTime = DateTime.Now;
            var totalSeconds = (sessionEndTime - _sessionStartTime).TotalSeconds;

            var sb = new StringBuilder();
            sb.AppendLine("=".PadRight(70, '='));
            sb.AppendLine("  CONNECTION SUMMARY");
            sb.AppendLine("=".PadRight(70, '='));
            sb.AppendLine($"Total devices: {_totalDevices}");
            sb.AppendLine($"✅ Success: {_successCount}");
            sb.AppendLine($"❌ Failed: {_failCount}");
            sb.AppendLine($"Total time: {totalSeconds:F3}s");
            sb.AppendLine($"Session end: {sessionEndTime:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine("=".PadRight(70, '='));
            sb.AppendLine();

            WriteLog(sb.ToString());

            // Console output summary
            Console.WriteLine();
            Console.WriteLine("📊 CONNECTION LOG SUMMARY:");
            Console.WriteLine($"   ✅ Success: {_successCount}/{_totalDevices}");
            Console.WriteLine($"   ❌ Failed: {_failCount}/{_totalDevices}");
            Console.WriteLine($"   📄 Log file: {Path.GetFullPath(_currentLogFile)}");
            Console.WriteLine();
        }

        /// <summary>
        /// Ghi log vào file (thread-safe)
        /// </summary>
        private static void WriteLog(string message)
        {
            lock (_lock)
            {
                try
                {
                    File.AppendAllText(_currentLogFile, message, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to write connection log: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Log thông tin bổ sung
        /// </summary>
        public static void LogInfo(string message)
        {
            if (!IsEnabled) return;

            var log = $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n";
            WriteLog(log);
        }
    }
}
