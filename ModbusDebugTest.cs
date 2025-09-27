using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using NModbus;
using System.Text;

namespace HMI_ScrewingMonitor
{
    public class ModbusDebugTest
    {
        public static async Task RunRegisterReadTest()
        {
            // Sửa lỗi hiển thị tiếng Việt trên Console
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("=============================================");
            Console.WriteLine("===   MODBUS REGISTER READ TEST           ===");
            Console.WriteLine("=============================================");
            Console.WriteLine("Mục tiêu: Kiểm tra đọc bit COMP (100084) từ Slave ID 1.");
            Console.WriteLine("Kết nối tới: 127.0.0.1, Port: 502");
            Console.WriteLine();

            try
            {
                using var tcpClient = new TcpClient();
                // Thêm timeout 5 giây để tránh bị treo
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await tcpClient.ConnectAsync("127.0.0.1", 502, cts.Token);

                Console.WriteLine("[OK] Đã kết nối TCP tới 127.0.0.1:502");

                var factory = new ModbusFactory();
                var master = factory.CreateMaster(tcpClient);
                Console.WriteLine("[OK] Đã tạo Modbus Master.");
                Console.WriteLine();
                Console.WriteLine("Bắt đầu đọc trạng thái bit COMP (địa chỉ 100084) mỗi giây...");
                Console.WriteLine("----------------------------------------------------------");
                Console.WriteLine("Bây giờ, hãy thử BẬT/TẮT bit ở địa chỉ 84 trong Modbus Simulator.");
                Console.WriteLine();

                while (true)
                {
                    // Đọc bit COMP (Input Status 100084 -> địa chỉ 83) từ Slave ID 1
                    bool[] compSignal = await master.ReadInputsAsync(1, 83, 1);
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Trạng thái bit COMP (100084) là: {compSignal[0]}");
                    await Task.Delay(1000); // Chờ 1 giây
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI] Không thể thực hiện bài test: {ex.Message}");
                Console.ReadKey();
            }
        }
    }
}