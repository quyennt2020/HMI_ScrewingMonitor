using System;
using System.Text;

namespace LicenseGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("         HMI Screwing Monitor - License Generator v1.0        ");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine();

            bool running = true;
            while (running)
            {
                Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                         MENU                               ║");
                Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
                Console.WriteLine("║  1. Tạo License Key cho khách hàng                         ║");
                Console.WriteLine("║  2. Kiểm tra Hardware ID của máy này                       ║");
                Console.WriteLine("║  3. Validate License Key (kiểm tra)                        ║");
                Console.WriteLine("║  4. Thoát                                                  ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
                Console.Write("\nChọn chức năng (1-4): ");

                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        GenerateLicense();
                        break;
                    case "2":
                        CheckHardwareId();
                        break;
                    case "3":
                        ValidateLicense();
                        break;
                    case "4":
                        running = false;
                        Console.WriteLine("\n✅ Đã thoát. Cảm ơn bạn đã sử dụng License Generator!");
                        break;
                    default:
                        Console.WriteLine("\n❌ Lựa chọn không hợp lệ. Vui lòng chọn 1-4.");
                        break;
                }
            }
        }

        /// <summary>
        /// Tạo license key cho khách hàng
        /// </summary>
        static void GenerateLicense()
        {
            Console.WriteLine("\n" + new string('─', 65));
            Console.WriteLine("          📝 TẠO LICENSE KEY CHO KHÁCH HÀNG");
            Console.WriteLine(new string('─', 65));

            try
            {
                // Nhập Hardware ID từ khách hàng
                Console.WriteLine("\n1️⃣ Nhập Hardware ID của khách hàng:");
                Console.WriteLine("   (Khách hàng gửi cho bạn từ phần mềm)");
                Console.Write("   Hardware ID: ");
                string hardwareId = Console.ReadLine()?.Trim().Replace("-", "").ToUpper();

                if (string.IsNullOrEmpty(hardwareId) || hardwareId.Length != 32)
                {
                    Console.WriteLine("❌ Hardware ID không hợp lệ. Phải có 32 ký tự.");
                    return;
                }

                // Nhập tên công ty
                Console.WriteLine("\n2️⃣ Nhập tên công ty khách hàng:");
                Console.Write("   Tên công ty: ");
                string companyName = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(companyName))
                {
                    companyName = "Licensed Customer";
                    Console.WriteLine($"   → Sử dụng tên mặc định: {companyName}");
                }

                // Chọn loại license
                Console.WriteLine("\n3️⃣ Chọn loại license:");
                Console.WriteLine("   1. Vĩnh viễn (không hết hạn)");
                Console.WriteLine("   2. Có thời hạn (nhập ngày hết hạn)");
                Console.Write("   Chọn (1 hoặc 2): ");
                string licenseType = Console.ReadLine()?.Trim();

                DateTime? expiryDate = null;

                if (licenseType == "2")
                {
                    // Nhập ngày hết hạn
                    Console.WriteLine("\n   Nhập ngày hết hạn (dd/MM/yyyy):");
                    Console.Write("   Ví dụ: 31/12/2025: ");
                    string dateStr = Console.ReadLine()?.Trim();

                    if (DateTime.TryParseExact(dateStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                    {
                        expiryDate = parsedDate;
                    }
                    else
                    {
                        Console.WriteLine("❌ Định dạng ngày không hợp lệ. Sử dụng license vĩnh viễn.");
                        expiryDate = null;
                    }
                }

                // Generate license key
                Console.WriteLine("\n⏳ Đang tạo license key...");
                string licenseKey = LicenseKey.GenerateLicenseKey(hardwareId, companyName, expiryDate);

                // Hiển thị kết quả
                Console.WriteLine("\n" + new string('═', 65));
                Console.WriteLine("                    ✅ THÀNH CÔNG!");
                Console.WriteLine(new string('═', 65));
                Console.WriteLine($"\n📋 Thông tin License:");
                Console.WriteLine($"   • Công ty     : {companyName}");
                Console.WriteLine($"   • Hardware ID : {HardwareInfo.FormatHardwareId(hardwareId)}");
                Console.WriteLine($"   • Loại        : {(expiryDate.HasValue ? $"Có hạn đến {expiryDate.Value:dd/MM/yyyy}" : "Vĩnh viễn")}");
                Console.WriteLine($"\n🔑 LICENSE KEY:");
                Console.WriteLine($"\n   ╔════════════════════════════════════╗");
                Console.WriteLine($"   ║  {licenseKey}  ║");
                Console.WriteLine($"   ╚════════════════════════════════════╝");
                Console.WriteLine($"\n💡 Gửi license key này cho khách hàng để kích hoạt phần mềm.");
                Console.WriteLine(new string('═', 65));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Kiểm tra Hardware ID của máy hiện tại
        /// </summary>
        static void CheckHardwareId()
        {
            Console.WriteLine("\n" + new string('─', 65));
            Console.WriteLine("          🖥️ KIỂM TRA HARDWARE ID CỦA MÁY NÀY");
            Console.WriteLine(new string('─', 65));

            try
            {
                Console.WriteLine("\n⏳ Đang lấy thông tin phần cứng...");
                string hardwareId = HardwareInfo.GetHardwareId();
                string formatted = HardwareInfo.FormatHardwareId(hardwareId);

                Console.WriteLine("\n✅ Hardware ID của máy này:");
                Console.WriteLine($"\n   ╔═════════════════════════════════════════════════════╗");
                Console.WriteLine($"   ║  {formatted}  ║");
                Console.WriteLine($"   ╚═════════════════════════════════════════════════════╝");
                Console.WriteLine($"\n💡 Khách hàng cần gửi Hardware ID này cho bạn để tạo license.");
                Console.WriteLine(new string('─', 65));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Validate một license key
        /// </summary>
        static void ValidateLicense()
        {
            Console.WriteLine("\n" + new string('─', 65));
            Console.WriteLine("          🔍 VALIDATE LICENSE KEY");
            Console.WriteLine(new string('─', 65));

            try
            {
                // Nhập license key
                Console.WriteLine("\n1️⃣ Nhập License Key cần kiểm tra:");
                Console.Write("   License Key: ");
                string licenseKey = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(licenseKey))
                {
                    Console.WriteLine("❌ License key không được để trống.");
                    return;
                }

                // Nhập Hardware ID
                Console.WriteLine("\n2️⃣ Nhập Hardware ID:");
                Console.Write("   Hardware ID: ");
                string hardwareId = Console.ReadLine()?.Trim().Replace("-", "").ToUpper();

                if (string.IsNullOrEmpty(hardwareId) || hardwareId.Length != 32)
                {
                    Console.WriteLine("❌ Hardware ID không hợp lệ.");
                    return;
                }

                // Validate
                Console.WriteLine("\n⏳ Đang validate...");
                bool isValid = LicenseKey.ValidateLicenseKey(licenseKey, hardwareId, out string companyName, out DateTime? expiryDate);

                Console.WriteLine("\n" + new string('═', 65));
                if (isValid)
                {
                    Console.WriteLine("                    ✅ LICENSE HỢP LỆ!");
                    Console.WriteLine(new string('═', 65));
                    Console.WriteLine($"\n📋 Thông tin:");
                    Console.WriteLine($"   • Công ty     : {companyName}");
                    Console.WriteLine($"   • Hardware ID : {HardwareInfo.FormatHardwareId(hardwareId)}");
                    Console.WriteLine($"   • Loại        : {(expiryDate.HasValue ? $"Có hạn đến {expiryDate.Value:dd/MM/yyyy}" : "Vĩnh viễn")}");

                    if (expiryDate.HasValue)
                    {
                        var daysRemaining = (expiryDate.Value - DateTime.Today).Days;
                        Console.WriteLine($"   • Còn lại     : {daysRemaining} ngày");
                    }
                }
                else
                {
                    Console.WriteLine("                    ❌ LICENSE KHÔNG HỢP LỆ!");
                    Console.WriteLine(new string('═', 65));
                    Console.WriteLine("\n⚠️ Lý do có thể:");
                    Console.WriteLine("   • License key không đúng format");
                    Console.WriteLine("   • Hardware ID không khớp");
                    Console.WriteLine("   • License đã hết hạn");
                    Console.WriteLine("   • Checksum không hợp lệ");
                }
                Console.WriteLine(new string('═', 65));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Lỗi: {ex.Message}");
            }
        }
    }
}
