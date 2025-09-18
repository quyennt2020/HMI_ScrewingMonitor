using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using NModbus;

namespace HMI_ScrewingMonitor
{
    public class ModbusDebugTest
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== MODBUS DEBUG TEST ===");
            Console.WriteLine("Testing TCP Gateway connection to 127.0.0.1:502");
            Console.WriteLine("Reading from Slave ID 1 and Slave ID 2");
            Console.WriteLine();

            try
            {
                // Kết nối TCP Gateway
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync("127.0.0.1", 502);
                Console.WriteLine("✓ TCP connected to 127.0.0.1:502");

                var factory = new ModbusFactory();
                var master = factory.CreateMaster(tcpClient);
                Console.WriteLine("✓ Modbus master created");
                Console.WriteLine();

                // Test đọc từ Slave ID 1
                Console.WriteLine("--- READING FROM SLAVE ID 1 ---");
                try
                {
                    var registers1 = await master.ReadHoldingRegistersAsync(1, 0, 13);
                    Console.WriteLine($"Slave 1 - Successfully read {registers1.Length} registers:");
                    for (int i = 0; i < registers1.Length; i++)
                    {
                        Console.WriteLine($"  R{i}: {registers1[i]}");
                    }

                    float angle1 = (float)registers1[0];
                    float torque1 = (float)registers1[2] / 10.0f;
                    bool status1 = registers1[12] == 1;
                    Console.WriteLine($"Slave 1 PROCESSED: Angle={angle1}°, Torque={torque1}Nm, Status={status1}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Slave 1 ERROR: {ex.Message}");
                }
                Console.WriteLine();

                // Test đọc từ Slave ID 2
                Console.WriteLine("--- READING FROM SLAVE ID 2 ---");
                try
                {
                    var registers2 = await master.ReadHoldingRegistersAsync(2, 0, 13);
                    Console.WriteLine($"Slave 2 - Successfully read {registers2.Length} registers:");
                    for (int i = 0; i < registers2.Length; i++)
                    {
                        Console.WriteLine($"  R{i}: {registers2[i]}");
                    }

                    float angle2 = (float)registers2[0];
                    float torque2 = (float)registers2[2] / 10.0f;
                    bool status2 = registers2[12] == 1;
                    Console.WriteLine($"Slave 2 PROCESSED: Angle={angle2}°, Torque={torque2}Nm, Status={status2}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Slave 2 ERROR: {ex.Message}");
                }

                Console.WriteLine();
                Console.WriteLine("=== TEST COMPLETE ===");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CONNECTION ERROR: {ex.Message}");
                Console.ReadKey();
            }
        }
    }
}