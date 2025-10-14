using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace HMI_ScrewingMonitor.Services
{
    /// <summary>
    /// Generate và validate license key
    /// Format: XXXXX-XXXXX-XXXXX-XXXXX-XXXXX (25 ký tự, 5 nhóm)
    /// </summary>
    public class LicenseKey
    {
        // Secret key - Thay đổi thành giá trị riêng của bạn
        private const string SECRET_KEY = "HMI_SCREWING_MONITOR_2025_SECRET_KEY_V1";

        /// <summary>
        /// Generate license key từ Hardware ID, Company Name và Expiry Date
        /// </summary>
        /// <param name="hardwareId">Hardware ID từ HardwareInfo.GetHardwareId()</param>
        /// <param name="companyName">Tên công ty khách hàng</param>
        /// <param name="expiryDate">Ngày hết hạn (null = vĩnh viễn)</param>
        /// <returns>License key format: XXXXX-XXXXX-XXXXX-XXXXX-XXXXX</returns>
        public static string GenerateLicenseKey(string hardwareId, string companyName, DateTime? expiryDate)
        {
            try
            {
                // Chuẩn hóa input
                hardwareId = hardwareId?.Trim().ToUpper() ?? "";
                companyName = companyName?.Trim() ?? "Licensed Customer";

                // Tạo expiry string (YYYYMMDD hoặc "99991231" nếu vĩnh viễn)
                string expiryStr = expiryDate.HasValue
                    ? expiryDate.Value.ToString("yyyyMMdd")
                    : "99991231"; // Vĩnh viễn

                // Kết hợp dữ liệu (không dùng company name để đơn giản hóa)
                string dataToSign = $"{hardwareId}|{expiryStr}|{SECRET_KEY}";

                // Tạo signature bằng SHA256
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
                    string hash = BitConverter.ToString(hashBytes).Replace("-", "");

                    // Lấy 20 ký tự đầu từ hash
                    string signature = hash.Substring(0, 20).ToUpper();

                    // Thêm expiry code (4 ký tự) - Encode YYMM
                    string expiryCode = expiryDate.HasValue
                        ? EncodeExpiryDate(expiryDate.Value)
                        : "PERM"; // Permanent

                    // Kết hợp: Signature (20) + Expiry (4) + Checksum (1)
                    string combined = signature + expiryCode;

                    // Tạo checksum
                    char checksum = CalculateChecksum(combined);
                    combined += checksum;

                    // Format thành XXXXX-XXXXX-XXXXX-XXXXX-XXXXX
                    return FormatLicenseKey(combined);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating license key: {ex.Message}");
            }
        }

        /// <summary>
        /// Validate license key
        /// </summary>
        /// <param name="licenseKey">License key cần validate</param>
        /// <param name="hardwareId">Hardware ID hiện tại</param>
        /// <param name="companyName">Tên công ty (output)</param>
        /// <param name="expiryDate">Ngày hết hạn (output)</param>
        /// <returns>True nếu license hợp lệ</returns>
        public static bool ValidateLicenseKey(string licenseKey, string hardwareId, out string companyName, out DateTime? expiryDate)
        {
            companyName = "";
            expiryDate = null;

            try
            {
                if (string.IsNullOrWhiteSpace(licenseKey))
                    return false;

                // Loại bỏ dấu gạch ngang
                string cleanKey = licenseKey.Replace("-", "").Trim().ToUpper();

                if (cleanKey.Length != 25)
                    return false;

                // Tách các phần
                string signature = cleanKey.Substring(0, 20);
                string expiryCode = cleanKey.Substring(20, 4);
                char checksum = cleanKey[24];

                // Verify checksum
                string dataToCheck = signature + expiryCode;
                if (CalculateChecksum(dataToCheck) != checksum)
                    return false;

                // Decode expiry date
                if (expiryCode == "PERM")
                {
                    expiryDate = null; // Vĩnh viễn
                }
                else
                {
                    expiryDate = DecodeExpiryDate(expiryCode);
                    if (!expiryDate.HasValue)
                        return false;

                    // Kiểm tra đã hết hạn chưa
                    if (expiryDate.Value < DateTime.Today)
                        return false;
                }

                // Validate signature với tất cả company name có thể
                // Vì chúng ta không lưu company name trong key, ta sẽ set default
                companyName = "Licensed Customer";

                // Regenerate key để so sánh signature
                // Thử với company name mặc định
                string expectedKey = GenerateLicenseKey(hardwareId, companyName, expiryDate);
                string expectedSignature = expectedKey.Replace("-", "").Substring(0, 20);

                // So sánh signature
                if (signature == expectedSignature)
                    return true;

                // Nếu không khớp, có thể hardware ID không đúng
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Encode expiry date thành 4 ký tự (YYMM format, base36)
        /// </summary>
        private static string EncodeExpiryDate(DateTime date)
        {
            // Format: YYMM
            int yy = date.Year % 100; // 2025 → 25
            int mm = date.Month;       // 1-12

            // Combine: YYMM → 2512 (Dec 2025)
            int combined = yy * 100 + mm;

            // Convert to Base36 (0-9, A-Z)
            string encoded = ToBase36(combined).PadLeft(4, '0');
            return encoded.Substring(0, 4).ToUpper();
        }

        /// <summary>
        /// Decode expiry date từ 4 ký tự
        /// </summary>
        private static DateTime? DecodeExpiryDate(string encoded)
        {
            try
            {
                int combined = FromBase36(encoded);
                int yy = combined / 100;
                int mm = combined % 100;

                if (mm < 1 || mm > 12)
                    return null;

                int year = 2000 + yy;
                return new DateTime(year, mm, DateTime.DaysInMonth(year, mm));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Tính checksum đơn giản
        /// </summary>
        private static char CalculateChecksum(string data)
        {
            int sum = 0;
            foreach (char c in data)
            {
                sum += (int)c;
            }
            int checksum = sum % 36; // 0-35
            return "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[checksum];
        }

        /// <summary>
        /// Format license key thành XXXXX-XXXXX-XXXXX-XXXXX-XXXXX
        /// </summary>
        private static string FormatLicenseKey(string key)
        {
            if (key.Length != 25)
                throw new ArgumentException("Key must be 25 characters");

            var formatted = new StringBuilder();
            for (int i = 0; i < 25; i += 5)
            {
                if (i > 0) formatted.Append("-");
                formatted.Append(key.Substring(i, 5));
            }
            return formatted.ToString().ToUpper();
        }

        /// <summary>
        /// Convert số sang Base36 (0-9, A-Z)
        /// </summary>
        private static string ToBase36(int value)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string result = "";
            while (value > 0)
            {
                result = chars[value % 36] + result;
                value /= 36;
            }
            return string.IsNullOrEmpty(result) ? "0" : result;
        }

        /// <summary>
        /// Convert Base36 về số
        /// </summary>
        private static int FromBase36(string value)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int result = 0;
            foreach (char c in value.ToUpper())
            {
                result = result * 36 + chars.IndexOf(c);
            }
            return result;
        }
    }
}
