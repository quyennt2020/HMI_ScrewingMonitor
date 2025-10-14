using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace HMI_ScrewingMonitor.Services
{
    /// <summary>
    /// Lấy thông tin phần cứng để tạo Hardware ID duy nhất cho từng máy tính
    /// </summary>
    public class HardwareInfo
    {
        /// <summary>
        /// Lấy Hardware ID duy nhất của máy tính
        /// Format: SHA256 hash của (CPU ID + Motherboard Serial + HDD Serial)
        /// </summary>
        public static string GetHardwareId()
        {
            string cpuId = GetCpuId();
            string motherboardSerial = GetMotherboardSerial();
            string hddSerial = GetHddSerial();

            // Kiểm tra xem có lấy được ít nhất 2/3 thông tin phần cứng không
            int validCount = 0;
            if (cpuId != "UNKNOWN_CPU") validCount++;
            if (motherboardSerial != "UNKNOWN_MOBO") validCount++;
            if (hddSerial != "UNKNOWN_HDD") validCount++;

            if (validCount < 2)
            {
                // Không đủ thông tin phần cứng → Throw exception
                string errorMsg = $"Không thể lấy thông tin phần cứng!\n\n" +
                                  $"CPU ID: {cpuId}\n" +
                                  $"Motherboard: {motherboardSerial}\n" +
                                  $"HDD Serial: {hddSerial}\n\n" +
                                  $"Vui lòng chạy ứng dụng với quyền Administrator.";

                Console.WriteLine($"[HARDWARE ERROR] {errorMsg}");
                throw new InvalidOperationException(errorMsg);
            }

            // Kết hợp các thông tin
            string combined = $"{cpuId}|{motherboardSerial}|{hddSerial}";
            Console.WriteLine($"[HARDWARE INFO] Combined: {combined}");

            // Hash bằng SHA256
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                string hash = BitConverter.ToString(hashBytes).Replace("-", "");

                // Lấy 32 ký tự đầu để dễ hiển thị
                return hash.Substring(0, 32);
            }
        }

        /// <summary>
        /// Lấy CPU ID
        /// </summary>
        private static string GetCpuId()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["ProcessorId"]?.ToString() ?? "UNKNOWN_CPU";
                    }
                }
            }
            catch
            {
                return "UNKNOWN_CPU";
            }
            return "UNKNOWN_CPU";
        }

        /// <summary>
        /// Lấy Motherboard Serial Number
        /// </summary>
        private static string GetMotherboardSerial()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string serial = obj["SerialNumber"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(serial))
                            return serial;
                    }
                }
            }
            catch
            {
                return "UNKNOWN_MOBO";
            }
            return "UNKNOWN_MOBO";
        }

        /// <summary>
        /// Lấy HDD Serial Number (ổ C:)
        /// </summary>
        private static string GetHddSerial()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_PhysicalMedia"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string serial = obj["SerialNumber"]?.ToString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(serial))
                            return serial;
                    }
                }
            }
            catch
            {
                return "UNKNOWN_HDD";
            }
            return "UNKNOWN_HDD";
        }

        /// <summary>
        /// Format Hardware ID thành dạng dễ đọc: XXXX-XXXX-XXXX-XXXX-XXXX-XXXX-XXXX-XXXX
        /// </summary>
        public static string FormatHardwareId(string hardwareId)
        {
            if (string.IsNullOrEmpty(hardwareId) || hardwareId.Length != 32)
                return hardwareId;

            // Chia thành 8 nhóm, mỗi nhóm 4 ký tự
            var formatted = new StringBuilder();
            for (int i = 0; i < 32; i += 4)
            {
                if (i > 0) formatted.Append("-");
                formatted.Append(hardwareId.Substring(i, 4));
            }
            return formatted.ToString();
        }
    }
}
