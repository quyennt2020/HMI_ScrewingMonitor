# HMI Screwing Monitor

Ứng dụng HMI giám sát thiết bị vặn vít tự động qua giao thức Modbus.

## Tính năng chính

- **Giám sát real-time**: Theo dõi trạng thái thiết bị mỗi giây
- **Hiển thị trực quan**: Giao diện màu sắc rõ ràng cho trạng thái OK/NG
- **Thông số chi tiết**: Góc siết, lực siết thực tế và giá trị min/max
- **Kết nối Modbus**: Hỗ trợ cả TCP và RTU
- **Multi-device**: Giám sát nhiều thiết bị cùng lúc

## Cài đặt và chạy

### Yêu cầu hệ thống
- .NET 6.0 Runtime
- Windows 10/11
- Visual Studio 2022 (để phát triển)

### Cài đặt dependencies
```bash
# Cài đặt .NET 6.0 SDK
# Download từ: https://dotnet.microsoft.com/download/dotnet/6.0

# Restore NuGet packages
dotnet restore
```

### Build và chạy
```bash
# Build project
dotnet build

# Chạy ứng dụng
dotnet run

# Hoặc build release
dotnet publish -c Release -r win-x64 --self-contained
```

## Cấu hình thiết bị

### Chỉnh sửa danh sách thiết bị
Mở file `MainViewModel.cs` và chỉnh sửa method `InitializeDevices()`:

```csharp
private void InitializeDevices()
{
    Devices.Add(new ScrewingDevice 
    { 
        DeviceId = 1, 
        DeviceName = "Máy vặn vít #1",
        IPAddress = "192.168.1.100",  // IP của thiết bị
        Port = 502,                   // Port Modbus TCP
        MinAngle = 40.0f,            // Góc min
        MaxAngle = 50.0f,            // Góc max
        MinTorque = 7.0f,            // Lực min
        MaxTorque = 10.0f            // Lực max
    });
}
```

### Mapping dữ liệu Modbus
Trong file `ModbusService.cs`, method `ReadDeviceDataAsync()`:

```csharp
// Địa chỉ register có thể cần điều chỉnh theo thiết bị thực tế:
// Register 0-1: Actual Angle (float 32-bit)
// Register 2-3: Actual Torque (float 32-bit)  
// Register 4-5: Min Angle (float 32-bit)
// Register 6-7: Max Angle (float 32-bit)
// Register 8-9: Min Torque (float 32-bit)
// Register 10-11: Max Torque (float 32-bit)
// Register 12: Status (0=NG, 1=OK)
```

## Cấu trúc project

```
HMI_ScrewingMonitor/
├── Models/
│   └── ScrewingDevice.cs       # Model dữ liệu thiết bị
├── Services/
│   └── ModbusService.cs        # Service kết nối Modbus
├── ViewModels/
│   └── MainViewModel.cs        # ViewModel chính
├── MainWindow.xaml             # Giao diện chính
├── MainWindow.xaml.cs          # Code-behind
├── App.xaml                    # Cấu hình ứng dụng
├── App.xaml.cs                 # Code-behind App
└── HMI_ScrewingMonitor.csproj  # File project
```

## Sử dụng

1. **Khởi động ứng dụng**
2. **Kết nối**: Click "🔗 Kết nối" để kết nối với thiết bị Modbus
3. **Bắt đầu giám sát**: Click "▶️ Bắt đầu giám sát" để bắt đầu đọc dữ liệu
4. **Theo dõi**: Quan sát trạng thái OK/NG và các thông số trên giao diện
5. **Dừng**: Click "⏹️ Dừng giám sát" để dừng
6. **Ngắt kết nối**: Click "❌ Ngắt kết nối" để ngắt kết nối

## Tùy chỉnh giao diện

### Thay đổi màu sắc
Chỉnh sửa trong `MainWindow.xaml`:

```xml
<!-- Màu OK -->
<DataTrigger Binding="{Binding IsOK}" Value="True">
    <Setter Property="Background" Value="Green"/>  <!-- Đổi màu tại đây -->
</DataTrigger>

<!-- Màu NG -->
<DataTrigger Binding="{Binding IsOK}" Value="False">
    <Setter Property="Background" Value="Red"/>    <!-- Đổi màu tại đây -->
</DataTrigger>
```

### Thay đổi layout
```xml
<!-- Thay đổi số cột hiển thị -->
<UniformGrid Columns="3"/>  <!-- 3 cột thay vì 2 -->
```

### Thay đổi tần suất cập nhật
Trong `MainViewModel.cs`:

```csharp
_timer = new DispatcherTimer
{
    Interval = TimeSpan.FromSeconds(2) // Cập nhật mỗi 2 giây thay vì 1 giây
};
```

## Troubleshooting

### Lỗi kết nối Modbus
- Kiểm tra IP address và port
- Đảm bảo thiết bị đã được bật và cấu hình đúng
- Kiểm tra firewall
- Thử ping tới IP của thiết bị

### Lỗi build
- Đảm bảo đã cài .NET 6.0 SDK
- Chạy `dotnet restore` để cài packages
- Kiểm tra file .csproj có đúng framework target

### Lỗi giao diện
- Kiểm tra binding trong XAML
- Đảm bảo DataContext được set đúng
- Xem Output window trong Visual Studio để debug

## Phát triển thêm

### Thêm logging
```csharp
// Thêm NLog hoặc Serilog package
// Log các sự kiện quan trọng
```

### Thêm database
```csharp
// Lưu lịch sử dữ liệu
// Báo cáo thống kê
```

### Thêm alarm
```csharp
// Cảnh báo khi có NG
// Email notification
// Sound alert
```

## Liên hệ hỗ trợ

- Email: support@example.com
- Phone: +84 xxx xxx xxx

## License

Copyright (c) 2024. All rights reserved.
