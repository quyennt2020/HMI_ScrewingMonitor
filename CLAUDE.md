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
- Contains hardcoded device initialization (consider moving to config)

### ModbusService.cs
- Abstracts Modbus TCP/RTU communication
- Converts register data to float values for angle/torque measurements
- Maps specific register addresses to device properties

### ScrewingDevice.cs
- Model representing a screwing device with properties for angle, torque, and status
- Implements INotifyPropertyChanged for UI binding
- Contains computed properties for status display

### LoggingService.cs
- Handles CSV logging of screwing events to daily log files
- Thread-safe file writing using locks
- Automatic directory creation in `HistoryLogs/`
- Logs torque values, timestamps, and pass/fail results

### TorqueChart.cs (Custom Control)
- Real-time visualization of torque data history
- Custom WPF Canvas-based control with data binding
- Displays min/max torque limits as reference lines
- Interactive tooltips showing value and timestamp
- Auto-scaling with padding around configured ranges

### Device Configuration
Device settings are stored in `Config/devices.json` with structure:
- Device identification (ID, name, IP, port)
- Measurement ranges (min/max angle and torque)
- Modbus register mapping
- UI settings (theme, language, refresh interval)

## Modbus Register Mapping

The application expects the following register layout per device:
- Registers 0-1: Actual Angle (32-bit float)
- Registers 2-3: Actual Torque (32-bit float)
- Registers 4-5: Min Angle (32-bit float)
- Registers 6-7: Max Angle (32-bit float)
- Registers 8-9: Min Torque (32-bit float)
- Registers 10-11: Max Torque (32-bit float)
- Register 12: Status (0=NG, 1=OK)

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
- Byte order may need adjustment based on actual device implementation

### Error Handling
- Global exception handling in App.xaml.cs
- Per-device error handling in timer polling
- Connection status tracking per device

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