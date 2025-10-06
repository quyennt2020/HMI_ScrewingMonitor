# TÀI LIỆU THANH GHI THIẾT BỊ HANDY2000

## Tổng quan
Tài liệu này mô tả chi tiết mapping thanh ghi Modbus cho thiết bị vặn vít tự động Handy2000 được sử dụng trong hệ thống HMI Screwing Monitor.

## Cấu hình kết nối

### TCP Individual Mode
```json
{
  "DeviceId": 1,
  "DeviceName": "Máy #1 Công đoạn A",
  "DeviceModel": "Handy2000",
  "IPAddress": "127.0.0.1",
  "Port": 502,
  "SlaveId": 1
}
```

## Mapping thanh ghi Modbus

### 1. Input Registers (Trạng thái điều khiển)

| Địa chỉ Modbus | Địa chỉ PLC | Tên thanh ghi | Mô tả | Kiểu dữ liệu |
|---------------|-------------|---------------|--------|--------------|
| 81 | 100082 | BUSY | Tín hiệu thiết bị đang hoạt động | Bool |
| 83 | 100084 | COMP | Tín hiệu hoàn thành quy trình siết | Bool |
| 85 | 100086 | OK | Kết quả siết OK | Bool |
| 86 | 100087 | NG | Kết quả siết NG | Bool |

**Ghi chú:** Địa chỉ Modbus = Địa chỉ PLC - 100001 (cho Input Registers)

### 2. Holding Registers (Dữ liệu đo lường)

| Địa chỉ Modbus | Địa chỉ PLC | Tên thanh ghi | Mô tả | Kiểu dữ liệu | Đơn vị |
|---------------|-------------|---------------|--------|--------------|--------|
| 8466 | 308467 | LastFastenFinalTorque | Mô-men xoắn cuối cùng của lần siết vừa rồi | Float32 | Nm |
| 8480 | 308481 | LastFastenTargetTorque | Mô-men xoắn mục tiêu | Float32 | Nm |
| 8481 | 308482 | LastFastenMinTorque | Mô-men xoắn tối thiểu cho phép | Float32 | Nm |
| 8482 | 308483 | LastFastenMaxTorque | Mô-men xoắn tối đa cho phép | Float32 | Nm |

**Ghi chú:** Địa chỉ Modbus = Địa chỉ PLC - 300001 (cho Holding Registers)

## Quy trình đọc dữ liệu Handy2000

### Bước 1: Phát hiện sự kiện hoàn thành
```
1.1. Đọc BUSY (100082) và COMP (100084)
1.2. Phát hiện "cạnh lên" của COMP: (COMP=ON) AND (BUSY=OFF) AND (COMP trước đó = OFF)
1.3. Khi phát hiện cạnh lên → chuyển sang Bước 2
```

### Bước 2: Đọc kết quả OK/NG
```
2.1. Đọc OK (100086) và NG (100087)
2.2. Xác định kết quả: OK=1 → PASS, NG=1 → FAIL
```

### Bước 3: Đọc dữ liệu chi tiết
```
3.1. Đọc LastFastenFinalTorque (308467) → ActualTorque
3.2. Đọc LastFastenTargetTorque (308481) → TargetTorque
3.3. Đọc LastFastenMinTorque (308482) → MinTorque
3.4. Đọc LastFastenMaxTorque (308483) → MaxTorque
```

## Ví dụ cấu hình devices.json

```json
{
  "Devices": [
    {
      "DeviceId": 1,
      "DeviceName": "Máy #1 Công đoạn A",
      "DeviceModel": "Handy2000",
      "IPAddress": "127.0.0.1",
      "Port": 502,
      "SlaveId": 1,
      "MinTorque": 9.8,
      "MaxTorque": 14.0,
      "TargetTorque": 12.0,
      "TotalCount": 100,
      "OKCount": 90,
      "NGCount": 10,
      "Enabled": true
    }
  ],
  "RegisterMapping": {
    "BUSYRegister": 100082,
    "COMPRegister": 100084,
    "OKRegister": 100086,
    "NGRegister": 100087,
    "LastFastenFinalTorque": 308467,
    "LastFastenTargetTorque": 308481,
    "LastFastenMinTorque": 308482,
    "LastFastenMaxTorque": 308483
  }
}
```

## Logic phát hiện sự kiện

### Rising Edge Detection
```csharp
// Phát hiện cạnh lên của tín hiệu COMP
bool isCompletionEvent = currentComp && !currentBusy && !device.PreviousCompletionState;

if (isCompletionEvent) {
    // Đây là sự kiện hoàn thành mới
    // Tiến hành đọc dữ liệu chi tiết
}

// Cập nhật trạng thái cho lần đọc tiếp theo
device.PreviousCompletionState = currentComp;
```

### Chuyển đổi dữ liệu Float32
```csharp
// Đọc 2 registers và chuyển thành Float32
ushort[] registers = await modbusMaster.ReadHoldingRegistersAsync(slaveId, address, 2);
byte[] bytes = new byte[4];
bytes[0] = (byte)(registers[1] & 0xFF);      // Low byte của register thứ 2
bytes[1] = (byte)(registers[1] >> 8);        // High byte của register thứ 2
bytes[2] = (byte)(registers[0] & 0xFF);      // Low byte của register thứ 1
bytes[3] = (byte)(registers[0] >> 8);        // High byte của register thứ 1
float value = BitConverter.ToSingle(bytes, 0);
```

## Fallback cho Simulator

Khi simulator không hỗ trợ Input Registers, hệ thống sẽ tự động fallback:

```csharp
try {
    // Thử đọc Input Registers trước
    bool[] statusBits = await modbusMaster.ReadInputsAsync(slaveId, 81, 3);
} catch {
    // Fallback: Đọc từ Holding Registers
    ushort[] fallbackRegisters = await modbusMaster.ReadHoldingRegistersAsync(slaveId, 0, 5);
    // Giả lập dữ liệu test
}
```

## Cấu hình Modbus Simulator

### Cho Input Registers:
- Địa chỉ 100082 (BUSY): 0/1
- Địa chỉ 100084 (COMP): 0/1
- Địa chỉ 100086 (OK): 0/1
- Địa chỉ 100087 (NG): 0/1

### Cho Holding Registers:
- Địa chỉ 308467: Float32 (Actual Torque)
- Địa chỉ 308481: Float32 (Target Torque)
- Địa chỉ 308482: Float32 (Min Torque)
- Địa chỉ 308483: Float32 (Max Torque)

## Debug và Monitoring

### Console Output mẫu:
```
[DEBUG] Device 1: Reading event. Prev COMP: False
[HANDY2000] Device 1: BUSY=False, COMP=True, PrevCOMP=False
[COMPLETION] Device 1: Completion event detected! Reading detailed data...
[DATA] Device 1: Torque=12.5Nm, Target=12.0Nm, Range=9.8-14.0Nm, Result=OK
[COUNTER] Device 1 - OK Count: 91, Total Count: 101
[LOGGING] Device 1 - Writing completion event to log
```

### Kiểm tra kết nối:
```
CONNECTING to Device 1 at 127.0.0.1:502
[DEBUG] Device 1: Starting TCP connection...
[DEBUG] Device 1: TCP connected, creating Modbus master...
CONNECTED to Device 1 successfully
```

## Ghi chú quan trọng

1. **Địa chỉ Modbus vs PLC**: Luôn chú ý sự chênh lệch offset
2. **Rising Edge**: Chỉ xử lý khi COMP chuyển từ 0→1
3. **Float32 Conversion**: Cần đảm bảo byte order đúng
4. **Thread Safety**: LoggingService sử dụng lock để đảm bảo an toàn
5. **Fallback Logic**: Hỗ trợ cả Input và Holding Registers
6. **Error Handling**: Có cơ chế xử lý lỗi và retry kết nối

---
*Tài liệu này được tự động sinh ra từ implementation hiện tại của hệ thống HMI Screwing Monitor.*