using System;

namespace LicenseGenerator
{
    class TestGenerate
    {
        static void Main(string[] args)
        {
            // Hardware ID từ screenshot
            string hardwareId = "C8631A88E173497DB35BDBDEA1827085";
            string companyName = "Test User";
            DateTime? expiryDate = null; // Vĩnh viễn

            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("         TEST: Tạo License Key cho máy của bạn");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine($"Hardware ID: {HardwareInfo.FormatHardwareId(hardwareId)}");
            Console.WriteLine($"Công ty    : {companyName}");
            Console.WriteLine($"Loại       : Vĩnh viễn");
            Console.WriteLine();
            Console.WriteLine("⏳ Đang tạo license key...");
            Console.WriteLine();

            try
            {
                string licenseKey = LicenseKey.GenerateLicenseKey(hardwareId, companyName, expiryDate);

                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.WriteLine("                    ✅ THÀNH CÔNG!");
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.WriteLine();
                Console.WriteLine("🔑 LICENSE KEY:");
                Console.WriteLine();
                Console.WriteLine($"   ╔════════════════════════════════════╗");
                Console.WriteLine($"   ║  {licenseKey}  ║");
                Console.WriteLine($"   ╚════════════════════════════════════╝");
                Console.WriteLine();
                Console.WriteLine("💡 Copy license key này và paste vào phần mềm để kích hoạt.");
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
