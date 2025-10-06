using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using HMI_ScrewingMonitor.Models;
using HMI_ScrewingMonitor.Services;
using HMI_ScrewingMonitor.ViewModels;

namespace HMI_TestRunner
{
    class Program
    {
        static void MainTest(string[] args)
        {
            RunTest();
        }

        static void RunTest()
        {
            Console.WriteLine("=== HMI SCREWING MONITOR TEST RUNNER ===");
            Console.WriteLine();

            int passed = 0;
            int failed = 0;

            // Test 1: ScrewingDevice Creation
            Console.WriteLine("TEST 1: ScrewingDevice Creation");
            try
            {
                var device = new ScrewingDevice
                {
                    DeviceId = 1,
                    DeviceName = "Test Device",
                    IPAddress = "192.168.1.100",
                    Port = 502
                };

                if (device.DeviceId == 1 &&
                    device.DeviceName == "Test Device" &&
                    device.IPAddress == "192.168.1.100" &&
                    device.Port == 502 &&
                    !device.IsConnected &&
                    !device.IsOK)
                {
                    Console.WriteLine("✅ PASSED - Device created with correct default values");
                    passed++;
                }
                else
                {
                    Console.WriteLine("❌ FAILED - Device properties not set correctly");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED - Exception: {ex.Message}");
                failed++;
            }

            // Test 2: PropertyChanged Events
            Console.WriteLine("\nTEST 2: PropertyChanged Events");
            try
            {
                var device = new ScrewingDevice();
                bool propertyChanged = false;
                device.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(ScrewingDevice.ActualTorque))
                        propertyChanged = true;
                };

                device.ActualTorque = 10.5f;

                if (device.ActualTorque == 10.5f && propertyChanged)
                {
                    Console.WriteLine("✅ PASSED - PropertyChanged events work correctly");
                    passed++;
                }
                else
                {
                    Console.WriteLine("❌ FAILED - PropertyChanged not fired or value not set");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED - Exception: {ex.Message}");
                failed++;
            }

            // Test 3: MainViewModel Initialization
            Console.WriteLine("\nTEST 3: MainViewModel Initialization");
            try
            {
                var viewModel = new MainViewModel();

                if (viewModel.Devices != null &&
                    viewModel.ConnectCommand != null &&
                    viewModel.DisconnectCommand != null &&
                    !viewModel.IsMonitoring)
                {
                    Console.WriteLine("✅ PASSED - MainViewModel initialized correctly");
                    passed++;
                }
                else
                {
                    Console.WriteLine("❌ FAILED - MainViewModel not initialized properly");
                    failed++;
                }

                viewModel.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED - Exception: {ex.Message}");
                failed++;
            }

            // Test 4: MainViewModel Device Management
            Console.WriteLine("\nTEST 4: MainViewModel Device Management");
            try
            {
                var viewModel = new MainViewModel();

                // Add test devices
                viewModel.Devices.Add(new ScrewingDevice { IsConnected = true, Enabled = true });
                viewModel.Devices.Add(new ScrewingDevice { IsConnected = false, Enabled = true });
                viewModel.Devices.Add(new ScrewingDevice { IsConnected = true, Enabled = true });

                var connectedCount = viewModel.ConnectedDevicesCount;
                var totalDevices = viewModel.TotalDevices;
                var canConnect = viewModel.CanConnect;
                var canDisconnect = viewModel.CanDisconnect;

                if (connectedCount == 2 && totalDevices == 3 && canConnect && canDisconnect)
                {
                    Console.WriteLine("✅ PASSED - Device management logic works");
                    passed++;
                }
                else
                {
                    Console.WriteLine($"❌ FAILED - Expected: Connected=2, Total=3, CanConnect=true, CanDisconnect=true");
                    Console.WriteLine($"           Actual: Connected={connectedCount}, Total={totalDevices}, CanConnect={canConnect}, CanDisconnect={canDisconnect}");
                    failed++;
                }

                viewModel.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED - Exception: {ex.Message}");
                failed++;
            }

            // Test 5: ModbusService Creation
            Console.WriteLine("\nTEST 5: ModbusService Creation");
            try
            {
                var modbusService = new ModbusService();

                if (!modbusService.IsConnected)
                {
                    Console.WriteLine("✅ PASSED - ModbusService created with correct default state");
                    passed++;
                }
                else
                {
                    Console.WriteLine("❌ FAILED - ModbusService should not be connected initially");
                    failed++;
                }

                modbusService.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED - Exception: {ex.Message}");
                failed++;
            }

            // Test 6: Configuration Loading
            Console.WriteLine("\nTEST 6: Configuration Loading");
            try
            {
                string configPath = "../Config/devices.json";
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);

                    if (config?.RegisterMapping != null)
                    {
                        Console.WriteLine($"✅ PASSED - RegisterMapping loaded: BUSY={config.RegisterMapping.BUSYRegister}, Final Torque={config.RegisterMapping.LastFastenFinalTorque}");
                        passed++;
                    }
                    else
                    {
                        Console.WriteLine("❌ FAILED - RegisterMapping is null");
                        failed++;
                    }
                }
                else
                {
                    Console.WriteLine("❌ FAILED - Config file not found");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED - Exception: {ex.Message}");
                failed++;
            }

            // Test 7: SettingsViewModel with ModbusService
            Console.WriteLine("\nTEST 7: SettingsViewModel with ModbusService");
            try
            {
                var modbusService = new ModbusService();
                var settingsViewModel = new SettingsViewModel(modbusService);

                if (settingsViewModel.RegisterMapping != null &&
                    settingsViewModel.RegisterMapping.BUSYRegister > 0)
                {
                    Console.WriteLine($"✅ PASSED - SettingsViewModel RegisterMapping: BUSY={settingsViewModel.RegisterMapping.BUSYRegister}");
                    passed++;
                }
                else
                {
                    Console.WriteLine("❌ FAILED - RegisterMapping not properly initialized in SettingsViewModel");
                    failed++;
                }

                modbusService.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED - Exception: {ex.Message}");
                failed++;
            }

            Console.WriteLine("\n=== TEST SUMMARY ===");
            Console.WriteLine($"Tests Passed: {passed}");
            Console.WriteLine($"Tests Failed: {failed}");
            Console.WriteLine($"Total Tests: {passed + failed}");

            if (failed == 0)
            {
                Console.WriteLine("🎉 ALL TESTS PASSED!");
            }
            else
            {
                Console.WriteLine($"⚠️ {failed} TEST(S) FAILED");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
