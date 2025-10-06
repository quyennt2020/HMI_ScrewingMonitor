# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Development Commands

### Building the Project
```bash
# Build and run interactively
build_and_run.bat

# Quick start (automated build + run)
start.bat

# Build only
dotnet build

# Build release configuration
dotnet build --configuration Release

# Restore NuGet packages
dotnet restore
```

### Publishing
```bash
# Create standalone executable
publish.bat

# Manual publish command
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### Running
```bash
# Run in development
dotnet run

# Run release configuration
dotnet run --configuration Release
```

### Testing
```bash
# Run console test application
cd HMI_TestRunner
dotnet run

# Run specific test project
dotnet run --project HMI_TestRunner
```

## Architecture Overview

This is a **WPF .NET 6 application** that monitors screwing devices via **Modbus communication** using the **MVVM pattern**. The application provides real-time monitoring of multiple screwing devices with visual status indicators.

### Core Architecture Components

- **MVVM Pattern**: Clean separation between UI (Views), business logic (ViewModels), and data (Models)
- **Modbus Communication**: Uses NModbus library for both TCP and RTU connections
- **Real-time Monitoring**: Timer-based polling of device data every second
- **Configuration-driven**: Device settings stored in JSON configuration files

### Key Architectural Decisions

1. **Service Layer**: `ModbusService.cs` handles all Modbus communication and device data conversion
2. **Data Binding**: Heavy use of INotifyPropertyChanged for real-time UI updates
3. **Command Pattern**: RelayCommand implementation for UI interactions
4. **Configuration Management**: JSON-based device configuration in `Config/devices.json`

### Project Structure

```
├── Models/               # Data models (ScrewingDevice, TorqueDataPoint)
├── Services/            # Business logic (ModbusService, LoggingService)
├── ViewModels/          # MVVM view models (MainViewModel, SettingsViewModel)
├── Views/               # XAML UI files (MainWindow, SettingsWindow)
├── Controls/            # Custom WPF controls (TorqueChart)
├── Converters/          # Value converters for data binding
├── Resources/           # UI themes and styles
├── Config/              # JSON configuration files
└── HMI_TestRunner/      # Console test application
```

## Key Components

### MainViewModel.cs
- Central view model managing device collection and monitoring state
- Handles connection management and timer-based data polling
- Loads device configuration from `Config/devices.json` at startup
- Implements rising edge detection for completion events

### ModbusService.cs
- Abstracts Modbus TCP/RTU communication
- Converts register data to float values for torque measurements
- Uses configurable register mapping from `Config/devices.json`
- Supports `ReloadRegisterMapping()` for runtime configuration changes
- Handles both Input Registers (status) and Holding Registers (data)

### ScrewingDevice.cs
- Model representing a screwing device with properties for torque, counters, and status
- Implements INotifyPropertyChanged for UI binding
- Tracks `PreviousCompletionState` for rising edge detection
- Contains computed properties for status display and result evaluation

### LoggingService.cs
- Handles CSV logging of screwing events to daily log files
- Thread-safe file writing using locks
- Automatic directory creation in `HistoryLogs/`
- Logs torque values, timestamps, and pass/fail results

### SettingsViewModel.cs
- Manages application settings and device configuration
- Provides UI for editing RegisterMapping (Modbus register addresses)
- Supports saving/loading configuration from `Config/devices.json`
- Includes `ModbusSettingsConfig` for connection settings and `RegisterMappingConfig` for register addresses
- Changes to RegisterMapping trigger `ModbusService.ReloadRegisterMapping()` for immediate effect

### TorqueChart.cs (Custom Control)
- Real-time visualization of torque data history
- Custom WPF Canvas-based control with data binding
- Displays min/max torque limits as reference lines
- Interactive tooltips showing value and timestamp
- Auto-scaling with padding around configured ranges

### Device Configuration
Device settings are stored in `Config/devices.json` with structure:
- **Devices**: Array of device objects with identification (ID, name, model, IP, port, SlaveId), torque ranges, counters, and enabled status
- **ModbusSettings**: Connection type (TCP_Individual, TCP_Gateway, RTU_Serial), gateway/serial settings, timeout, retry, scan interval
- **RegisterMapping**: Configurable Modbus register addresses (BUSY, COMP, OK, NG, torque registers) - allows supporting different device models
- **UI**: Theme, language, refresh interval, grid layout

**Important:** RegisterMapping is now configurable in SettingsViewModel, allowing runtime adjustment of register addresses without code changes.

## Modbus Register Mapping

The application supports **Handy2000 screwing devices** with configurable register mapping in `Config/devices.json`:

### Current Implementation (Handy2000)
**Input Registers (Control Status):**
- Register 100082 (Modbus addr 81): BUSY - Device is operating
- Register 100084 (Modbus addr 83): COMP - Completion signal
- Register 100086 (Modbus addr 85): OK - Screwing result OK
- Register 100087 (Modbus addr 86): NG - Screwing result NG

**Holding Registers (Measurement Data - Float32):**
- Register 308467 (Modbus addr 8466): LastFastenFinalTorque - Actual torque value
- Register 308481 (Modbus addr 8480): LastFastenTargetTorque - Target torque
- Register 308482 (Modbus addr 8481): LastFastenMinTorque - Min torque limit
- Register 308483 (Modbus addr 8482): LastFastenMaxTorque - Max torque limit

**Note:** PLC addresses differ from Modbus addresses by offset (Input: -100001, Holding: -300001)

### Event Detection Logic
The system uses **rising edge detection** to identify completion events:
1. Monitor BUSY and COMP registers
2. Detect completion: `COMP=true AND BUSY=false AND previousCOMP=false`
3. Read OK/NG status to determine result
4. Read detailed torque data from holding registers
5. Update previousCOMP for next cycle

See `DEVICE_REGISTERS.md` for detailed register documentation and implementation examples.

## Dependencies

### NuGet Packages
- **NModbus** (3.0.72): Modbus communication library
- **System.IO.Ports** (7.0.0): Serial port communication for RTU
- **Newtonsoft.Json** (13.0.4): JSON serialization/deserialization

### Framework
- **.NET 6.0 Windows**: Target framework with WPF support

## Development Notes

### Language and Localization
- The application uses Vietnamese language in UI and comments
- Error messages and status text are in Vietnamese
- Consider this when making UI changes or adding features

### Data Conversion
- The `ConvertRegistersToFloat` method in ModbusService handles 32-bit float conversion from two 16-bit registers
- Byte order for Handy2000: `[reg1_low, reg1_high, reg0_low, reg0_high]`
- Different devices may require different byte ordering - test with actual hardware

### Error Handling
- Global exception handling in App.xaml.cs
- Per-device error handling in timer polling
- Connection status tracking per device
- Automatic fallback from Input Registers to Holding Registers for simulators that don't support Input Registers
- Retry mechanism for Modbus communication failures

### Testing
The `HMI_TestRunner` project provides console-based unit tests for:
- ScrewingDevice model creation and property changes
- MainViewModel initialization and device management
- ModbusService creation and connection states
- Tests run automatically and display pass/fail results with detailed output

### Data Visualization
- Custom `TorqueChart` control for real-time torque graphing
- Automatic scaling based on configured min/max values with padding
- Interactive data points with hover tooltips
- Reference lines for acceptable torque ranges
- Chart automatically updates when new data points are added

### Logging and History
- Automatic CSV logging to `HistoryLogs/` directory
- Daily log files with timestamp, device info, torque values, and results
- Thread-safe logging operations to prevent data corruption
- Logs are created asynchronously to avoid blocking the UI thread