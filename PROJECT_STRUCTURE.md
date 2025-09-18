# HMI Screwing Monitor - Project Structure

## 📁 Cấu trúc thư mục được tổ chức lại:

```
HMI_ScrewingMonitor/
├── 📄 HMI_ScrewingMonitor.csproj    # File project chính
├── 📄 App.xaml                      # Cấu hình ứng dụng
├── 📄 App.xaml.cs                   # Code-behind App
├── 📄 README.md                     # Tài liệu hướng dẫn
├── 📄 build_and_run.bat            # Script build tự động
├── 📄 publish.bat                  # Script tạo .exe
│
├── 📂 Models/                       # Data Models
│   └── 📄 ScrewingDevice.cs        # Model thiết bị vặn vít
│
├── 📂 Services/                     # Business Logic Services
│   └── 📄 ModbusService.cs         # Service kết nối Modbus
│
├── 📂 ViewModels/                   # MVVM ViewModels
│   └── 📄 MainViewModel.cs         # ViewModel chính
│
├── 📂 Views/                        # UI Views
│   ├── 📄 MainWindow.xaml          # Giao diện chính
│   └── 📄 MainWindow.xaml.cs       # Code-behind MainWindow
│
├── 📂 Converters/                   # Value Converters
│   └── 📄 ValueConverters.cs       # Converters cho XAML binding
│
├── 📂 Resources/                    # Themes & Resources
│   └── 📄 LightTheme.xaml          # Theme sáng
│
└── 📂 Config/                       # Configuration Files
    └── 📄 devices.json             # Cấu hình thiết bị
```

## 🔧 Cải tiến đã thực hiện:

### ✅ **Cấu trúc theo chuẩn MVVM:**
- **Models**: Dữ liệu và business objects
- **Views**: Giao diện người dùng (XAML)
- **ViewModels**: Logic điều khiển và data binding

### ✅ **Separation of Concerns:**
- **Services**: Logic nghiệp vụ độc lập
- **Converters**: Chuyển đổi dữ liệu cho UI
- **Resources**: Themes và styles tái sử dụng
- **Config**: Cấu hình bên ngoài

### ✅ **Maintainability:**
- Code dễ bảo trì và mở rộng
- Cấu hình thiết bị bằng JSON
- Themes có thể thay đổi
- Build scripts tự động

### ✅ **Best Practices:**
- Namespace organization
- File naming conventions
- Clear folder structure
- Documentation

## 🚀 Hướng dẫn sử dụng:

### 1. Build và chạy:
```bash
cd HMI_ScrewingMonitor
build_and_run.bat
```

### 2. Cấu hình thiết bị:
Chỉnh sửa file `Config/devices.json` để thêm/sửa thiết bị

### 3. Tùy chỉnh giao diện:
- Sửa `Resources/LightTheme.xaml` để thay đổi màu sắc
- Sửa `Views/MainWindow.xaml` để thay đổi layout

### 4. Thêm tính năng:
- Models: Thêm data models mới
- Services: Thêm logic nghiệp vụ
- Views: Thêm giao diện mới
- ViewModels: Thêm logic điều khiển

## 📋 TODO List:

- [ ] Thêm Dark Theme
- [ ] Thêm Settings View
- [ ] Thêm Logging Service
- [ ] Thêm Database Support
- [ ] Thêm Export/Import Config
- [ ] Thêm Alarm System
- [ ] Thêm Multi-language Support
- [ ] Thêm Unit Tests

## 🛠️ Tech Stack:

- **.NET 6.0** - Framework
- **WPF** - UI Framework
- **MVVM** - Architecture Pattern
- **NModbus** - Modbus Communication
- **JSON** - Configuration
- **XAML** - UI Declaration

Cấu trúc này giúp project dễ maintain, mở rộng và collaborate trong team development.
