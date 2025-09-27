using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HMI_ScrewingMonitor.Models;

namespace HMI_ScrewingMonitor.Services
{
    public class LoggingService
    {
        private readonly string _logDirectory;
        private static readonly object _fileLock = new object();

        public LoggingService()
        {
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HistoryLogs");
            Directory.CreateDirectory(_logDirectory);
        }

        /// <summary>
        /// Ghi lại kết quả của một lần siết vít vào file CSV.
        /// </summary>
        public void LogScrewingEvent(ScrewingDevice device)
        {
            Task.Run(() =>
            {
                try
                {
                    string logFilePath = Path.Combine(_logDirectory, $"ScrewingHistory_{DateTime.Now:yyyy-MM-dd}.csv");
                    bool fileExists = File.Exists(logFilePath);

                    // Sử dụng lock để đảm bảo nhiều thread có thể ghi file an toàn
                    lock (_fileLock)
                    {
                        using (var writer = new StreamWriter(logFilePath, true, Encoding.UTF8))
                        {
                            // Ghi header nếu file mới được tạo
                            if (!fileExists)
                            {
                                writer.WriteLine("Timestamp,DeviceID,DeviceName,DeviceModel,ActualTorque,TargetTorque,MinTorque,MaxTorque,Result,Status,TotalCount,OKCount,NGCount");
                            }

                            // Tạo dòng CSV với thông tin chi tiết hơn
                            var csvLine = string.Format("{0},{1},{2},{3},{4:F2},{5:F1},{6:F1},{7:F1},{8},{9},{10},{11},{12}",
                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                device.DeviceId,
                                device.DeviceName,
                                device.DeviceModel,
                                device.ActualTorque,
                                device.TargetTorque,
                                device.MinTorque,
                                device.MaxTorque,
                                device.ResultText,
                                device.Status,
                                device.TotalCount,
                                device.OKDeviceCount,
                                device.NGDeviceCount);

                            writer.WriteLine(csvLine);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Ghi lại lỗi vào debug console để không làm ảnh hưởng đến ứng dụng chính
                    System.Diagnostics.Debug.WriteLine($"Error writing to log file: {ex.Message}");
                }
            });
        }
    }
}