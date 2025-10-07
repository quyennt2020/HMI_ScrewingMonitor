# HƯỚNG DẪN SỬ DỤNG HỆ THỐNG GIÁM SÁT THIẾT BỊ VẶN VÍT

## 1. GIỚI THIỆU HỆ THỐNG

### 1.1. Mục đích
Hệ thống HMI Screwing Monitor được thiết kế để giám sát và kiểm soát chất lượng quá trình vặn vít tự động trong sản xuất. Phần mềm kết nối với các thiết bị vặn vít thông qua giao thức Modbus, thu thập và hiển thị dữ liệu lực xiết (torque) theo thời gian thực.

### 1.2. Lợi ích
- **Giám sát trực quan**: Theo dõi trạng thái và kết quả của nhiều thiết bị đồng thời trên một màn hình
- **Kiểm soát chất lượng**: Phát hiện ngay lập tức các lần vặn vít không đạt chuẩn (NG)
- **Lưu trữ dữ liệu**: Tự động ghi lại lịch sử các lần vặn vít để phân tích và báo cáo
- **Cảnh báo trực quan**: Màu sắc và ký hiệu rõ ràng giúp nhận biết trạng thái nhanh chóng
- **Dễ dàng quản lý**: Giao diện thân thiện, cấu hình linh hoạt

### 1.3. Tổng quan chức năng
Hệ thống có khả năng:
- Giám sát đồng thời tối đa 15 thiết bị vặn vít
- Hiển thị thông số lực xiết thực tế và so sánh với tiêu chuẩn
- Thống kê số lần vặn vít đạt (OK) và không đạt (NG) cho từng thiết bị
- Vẽ biểu đồ lực xiết của 30 lần vặn gần nhất
- Tự động lưu dữ liệu vào file CSV theo ngày
- Hỗ trợ nhiều loại kết nối Modbus (TCP, Serial RTU)

---

## 2. TÍNH NĂNG CHÍNH

### 2.1. Giám sát đa thiết bị real-time
- Hiển thị đồng thời **tối đa 15 thiết bị** trên một màn hình
- Cập nhật dữ liệu **mỗi giây** (có thể tùy chỉnh)
- Tự động phát hiện sự kiện hoàn thành vặn vít (rising edge detection)
- Kết nối và ngắt kết nối từng thiết bị độc lập

### 2.2. Hiển thị trạng thái trực quan
**Trạng thái kết nối:**
- **ONLINE** (màu xanh): Thiết bị đang hoạt động bình thường
- **OFFLINE** (màu cam): Thiết bị mất kết nối
- **DISABLED** (màu xám): Thiết bị bị vô hiệu hóa trong cấu hình

**Kết quả vặn vít:**
- **✓ OK** (nền xanh): Lực xiết nằm trong khoảng cho phép
- **✗ NG** (nền đỏ): Lực xiết ngoài khoảng cho phép
- **⚠ --** (nền xám): Chưa có dữ liệu hoặc mất kết nối

### 2.3. Đo lường lực xiết (Torque)
Hệ thống hiển thị đầy đủ các thông số lực xiết:
- **Tiêu chuẩn (Standard)**: Khoảng giá trị cho phép (Min ~ Max) tính bằng Nm
- **Lực đặt (Target)**: Giá trị lực xiết mong muốn
- **Thực tế (Actual)**: Giá trị lực xiết đo được từ thiết bị

Ví dụ: Tiêu chuẩn 9.8~14 Nm, Lực đặt 12 Nm, Thực tế 12.5 Nm → OK

### 2.4. Thống kê theo thiết bị
Mỗi thiết bị có bộ đếm riêng biệt:
- **Tổng (Total)**: Tổng số lần vặn vít
- **OK**: Số lần đạt chuẩn
- **NG**: Số lần không đạt chuẩn

**Lưu ý quan trọng**:
- Bộ đếm **tự động reset về 0** vào 00:00 mỗi ngày
- Mỗi thiết bị có bộ đếm độc lập, không tổng hợp chung

### 2.5. Biểu đồ lực xiết real-time
- Hiển thị đồ thị của **30 lần vặn gần nhất**
- Tự động cập nhật khi có dữ liệu mới
- Hiển thị đường giới hạn Max và Min để dễ so sánh
- Tự động điều chỉnh tỷ lệ theo khoảng giá trị chuẩn

### 2.6. Logging và lưu trữ
- Tự động ghi log vào file CSV trong thư mục `HistoryLogs/`
- Tên file theo format: `ScrewingHistory_YYYY-MM-DD.csv`
- Ghi lại đầy đủ: thời gian, mã thiết bị, tên, lực xiết, kết quả
- File mới được tạo mỗi ngày
- An toàn với cơ chế ghi bất đồng bộ (async)

### 2.7. Kết nối Modbus linh hoạt
Hỗ trợ 3 chế độ kết nối:
- **TCP Individual**: Kết nối trực tiếp đến từng thiết bị qua IP riêng
- **TCP Gateway**: Kết nối qua một gateway chung, phân biệt bằng Slave ID
- **RTU Serial**: Kết nối qua cổng COM (RS485/RS232)

---

## 3. YÊU CẦU HỆ THỐNG

### 3.1. Yêu cầu phần cứng
**Máy tính:**
- Processor: Intel Core i3 hoặc tương đương trở lên
- RAM: Tối thiểu 4GB
- Ổ cứng: Tối thiểu 500MB dung lượng trống
- Màn hình: Độ phân giải tối thiểu 1280x800
- Card mạng: Ethernet cho kết nối TCP
- Cổng COM: Nếu sử dụng kết nối Serial RTU

**Mạng:**
- Kết nối Ethernet ổn định
- Dải IP cùng subnet với các thiết bị Modbus
- Bandwidth: 10Mbps trở lên

### 3.2. Yêu cầu phần mềm
- **Hệ điều hành**: Windows 10 hoặc Windows 11 (64-bit)
- **.NET Runtime**: .NET 6.0 Desktop Runtime
- **Quyền truy cập**: Administrator (cho lần cài đặt đầu tiên)

### 3.3. Yêu cầu thiết bị Modbus
- Hỗ trợ giao thức Modbus TCP hoặc RTU
- Cung cấp các thanh ghi (registers) theo chuẩn Handy2000 hoặc tương thích
- Địa chỉ IP tĩnh (với TCP) hoặc Slave ID duy nhất (với RTU)

---

## 4. GIAO DIỆN NGƯỜI DÙNG

### 4.1. Bảng điều khiển (Control Panel)
Nằm ở phía trên cùng của màn hình, bao gồm:

**Các nút lệnh:**
- **🔗 Kết nối**: Kết nối đến tất cả thiết bị được kích hoạt
- **❌ Ngắt kết nối**: Ngắt kết nối tất cả thiết bị
- **▶️ Bắt đầu giám sát**: Bật chế độ giám sát và đọc dữ liệu
- **⏹️ Dừng giám sát**: Tạm dừng giám sát
- **⚙️ Cấu hình**: Mở cửa sổ cấu hình hệ thống

**Thông tin trạng thái:**
- **Trạng thái**: Hiển thị tình trạng kết nối (số thiết bị đã kết nối/tổng số)
- **Giám sát**: Hiển thị BẬT/TẮT

### 4.2. Thẻ thiết bị (Device Card)
Mỗi thiết bị được hiển thị bằng một thẻ riêng, bao gồm:

**Phần đầu (Header):**
- Tên thiết bị và công đoạn (VD: "Máy #2 Công đoạn B")
- Đèn LED hiển thị trạng thái kết nối (xanh/đỏ)
- Badge trạng thái: ONLINE/OFFLINE/DISABLED
- Model thiết bị (VD: "ABC")

**Phần kết quả:**
- Vùng lớn hiển thị **✓ OK** (nền xanh) hoặc **✗ NG** (nền đỏ)

**Bảng thông số lực xiết:**
| Tiêu chuẩn Nm | Lực đặt Nm | Thực tế Nm |
|---------------|------------|------------|
| 9.8~14        | 12         | 12.5       |

**Thống kê:**
| Tổng | OK  | NG |
|------|-----|----|
| 100  | 90  | 10 |

**Biểu đồ:**
- Đồ thị đường thể hiện 30 lần vặn gần nhất
- Đường ngang Max và Min để tham chiếu
- Trục tung: Lực xiết (Nm)
- Trục hoành: Thứ tự các lần vặn

**Phần cuối:**
- Ngày tháng năm (VD: 28.05.2025)

### 4.3. Thanh trạng thái (Status Bar)
Nằm ở phía dưới cùng, hiển thị:
- **Tổng thiết bị**: Số lượng thiết bị trong hệ thống
- **Kết nối**: Số thiết bị đang online
- **OK**: Tổng số lần OK của tất cả thiết bị
- **NG**: Tổng số lần NG của tất cả thiết bị
- **Tổng hôm nay**: Số liệu OK/NG trong ngày
- **Thời gian**: Đồng hồ hiện tại

### 4.4. Màn hình cấu hình (Settings)
Mở bằng nút **⚙️ Cấu hình**, cho phép:
- Thêm/sửa/xóa thiết bị
- Cấu hình thông số Modbus
- Điều chỉnh địa chỉ thanh ghi (Register Mapping)
- Tùy chỉnh giao diện (số cột, hàng hiển thị)

---

## 5. HƯỚNG DẪN SỬ DỤNG

### 5.1. Khởi động hệ thống

**Bước 1: Khởi động phần mềm**
- Double-click vào file `HMI_ScrewingMonitor.exe`
- Hoặc sử dụng file `start.bat` để khởi động nhanh
- Chờ cửa sổ chính xuất hiện (khoảng 2-3 giây)

**Bước 2: Kiểm tra cấu hình ban đầu**
- Khi khởi động lần đầu, phần mềm sẽ tự động load cấu hình từ `Config/devices.json`
- Nếu file không tồn tại, hệ thống sẽ tạo cấu hình mặc định với 4 thiết bị mẫu

**Bước 3: Đảm bảo kết nối mạng**
- Kiểm tra máy tính đã kết nối vào mạng chung với các thiết bị
- Có thể thử ping đến địa chỉ IP của thiết bị để kiểm tra:
  ```
  ping 192.168.1.100
  ```

### 5.2. Kết nối thiết bị

**Bước 1: Click nút "🔗 Kết nối"**
- Hệ thống sẽ bắt đầu kết nối đến tất cả thiết bị được đánh dấu "Enabled" trong cấu hình
- Thanh trạng thái hiển thị tiến trình: "Đang kết nối X/Y thiết bị..."

**Bước 2: Theo dõi quá trình kết nối**
- Mỗi thẻ thiết bị sẽ hiển thị trạng thái "Đang kết nối..."
- Kết nối thành công: Badge chuyển sang **ONLINE** (xanh), trạng thái "Sẵn sàng"
- Kết nối thất bại: Badge **OFFLINE** (cam), trạng thái "Kết nối thất bại"

**Bước 3: Kiểm tra kết quả**
- Thanh trạng thái hiển thị: "Đã kết nối X/Y thiết bị (TCP_Individual)"
- Nếu có ít nhất 1 thiết bị kết nối thành công, hệ thống sẽ **tự động bắt đầu giám sát**

**Lưu ý:**
- Thiết bị bị vô hiệu hóa (Enabled=false) sẽ không được kết nối
- Có thể kết nối lại thiết bị lỗi bằng cách ngắt kết nối và kết nối lại

### 5.3. Bắt đầu giám sát

**Tự động:**
- Sau khi kết nối thành công, hệ thống tự động bắt đầu giám sát
- Không cần thao tác thêm

**Thủ công (nếu đã dừng trước đó):**
- Click nút **"▶️ Bắt đầu giám sát"**
- Trạng thái "Giám sát" chuyển sang **BẬT** (màu xanh)
- Hệ thống bắt đầu đọc dữ liệu từ thiết bị mỗi 1 giây (hoặc theo cấu hình)

**Khi đang giám sát:**
- Các thẻ thiết bị cập nhật dữ liệu real-time
- Biểu đồ tự động vẽ khi có dữ liệu mới
- Bộ đếm OK/NG tự động tăng
- Log file được ghi tự động

### 5.4. Đọc và hiểu dữ liệu

**Đọc thông số lực xiết:**
```
Tiêu chuẩn: 9.8~14 Nm   → Khoảng cho phép
Lực đặt:    12 Nm       → Giá trị mục tiêu
Thực tế:    12.5 Nm     → Giá trị đo được
```
→ Trong trường hợp này: 12.5 nằm trong khoảng 9.8~14 → **OK**

**Đọc thống kê:**
```
Tổng: 100   → Đã vặn 100 lần trong ngày
OK:   90    → 90 lần đạt chuẩn
NG:   10    → 10 lần không đạt
```
→ Tỷ lệ đạt = 90/100 = 90%

**Đọc biểu đồ:**
- Mỗi điểm trên đồ thị là một lần vặn vít
- Đường ngang trên: Max (14 Nm)
- Đường ngang dưới: Min (9.8 Nm)
- Các điểm nằm giữa 2 đường: OK
- Các điểm ngoài 2 đường: NG

**Màu sắc cảnh báo:**
- **Nền xanh (✓ OK)**: Lực xiết trong khoảng cho phép
- **Nền đỏ (✗ NG)**: Lực xiết ngoài khoảng cho phép
- **Nền xám (⚠ --)**: Không có dữ liệu hoặc mất kết nối

### 5.5. Dừng và ngắt kết nối

**Dừng giám sát tạm thời:**
1. Click nút **"⏹️ Dừng giám sát"**
2. Hệ thống ngừng đọc dữ liệu nhưng vẫn giữ kết nối
3. Các thẻ thiết bị hiển thị trạng thái "Sẵn sàng"
4. Bộ đếm và biểu đồ không bị xóa

**Ngắt kết nối hoàn toàn:**
1. Click nút **"❌ Ngắt kết nối"**
2. Hệ thống tự động dừng giám sát (nếu đang chạy)
3. Ngắt kết nối tất cả thiết bị
4. Thẻ thiết bị hiển thị badge **OFFLINE**, trạng thái "--"
5. Bộ đếm được reset về 0
6. Biểu đồ bị xóa trắng

**Thoát phần mềm:**
- Click nút X ở góc trên phải
- Hoặc nhấn Alt+F4
- Hệ thống tự động ngắt kết nối an toàn trước khi thoát

### 5.6. Xem lịch sử dữ liệu

**Vị trí file log:**
- Thư mục: `HistoryLogs/` (cùng cấp với file .exe)
- Tên file: `ScrewingHistory_2025-05-28.csv` (theo ngày)

**Mở file log:**
1. Mở thư mục `HistoryLogs/`
2. Double-click file CSV tương ứng ngày cần xem
3. File sẽ mở trong Microsoft Excel hoặc phần mềm xem CSV

**Cấu trúc dữ liệu trong file:**
```csv
Timestamp,DeviceID,DeviceName,ActualTorque,MinTorque,MaxTorque,TargetTorque,Result
2025-05-28 08:15:32,1,Máy #1 Công đoạn A,12.50,9.80,14.00,12.00,OK
2025-05-28 08:15:45,2,Máy #2 Công đoạn B,15.20,9.80,14.00,12.00,NG
```

**Phân tích dữ liệu:**
- Sử dụng Excel để tạo bảng PivotTable
- Lọc theo DeviceID hoặc DeviceName
- Tính toán tỷ lệ OK/NG
- Vẽ biểu đồ xu hướng theo thời gian

---

## 6. GIẢI THÍCH DỮ LIỆU HIỂN THỊ

### 6.1. Các thông số lực xiết

**Tiêu chuẩn (Standard Range):**
- Định nghĩa: Khoảng giá trị lực xiết được chấp nhận
- Format hiển thị: `Min~Max` (VD: 9.8~14 Nm)
- Ý nghĩa: Lực xiết thực tế phải nằm trong khoảng này để được đánh giá OK
- Nguồn: Cấu hình trong file `devices.json` hoặc đọc từ thiết bị

**Lực đặt (Target Torque):**
- Định nghĩa: Giá trị lực xiết mục tiêu
- Format hiển thị: Số thập phân với 1 chữ số (VD: 12.0 Nm)
- Ý nghĩa: Giá trị lý tưởng mà thiết bị cố gắng đạt được
- Thường nằm ở giữa khoảng Min~Max

**Thực tế (Actual Torque):**
- Định nghĩa: Giá trị lực xiết thực sự đo được sau mỗi lần vặn vít
- Format hiển thị: Số thập phân với 1 chữ số (VD: 12.5 Nm)
- Ý nghĩa: Giá trị này quyết định kết quả OK/NG
- Cập nhật mỗi khi có sự kiện hoàn thành vặn vít

**Quy tắc đánh giá:**
```
Nếu Min ≤ Actual ≤ Max  →  OK (đạt chuẩn)
Nếu Actual < Min hoặc Actual > Max  →  NG (không đạt)
```

### 6.2. Bộ đếm theo thiết bị

**Tổng (Total Count):**
- Định nghĩa: Tổng số lần vặn vít của thiết bị trong ngày
- Tính toán: Tăng 1 mỗi khi có sự kiện hoàn thành (COMP signal)
- Reset: Tự động về 0 vào 00:00 mỗi ngày
- Ứng dụng: Theo dõi năng suất của thiết bị

**OK Count:**
- Định nghĩa: Số lần vặn vít đạt chuẩn trong ngày
- Điều kiện: Actual Torque nằm trong khoảng Min~Max
- Màu hiển thị: Xanh dương (Blue)
- Tính toán: Tăng 1 khi kết quả là OK

**NG Count:**
- Định nghĩa: Số lần vặn vít không đạt chuẩn trong ngày
- Điều kiện: Actual Torque nằm ngoài khoảng Min~Max
- Màu hiển thị: Đỏ (Red)
- Tính toán: Tăng 1 khi kết quả là NG

**Công thức:**
```
Total = OK + NG
Tỷ lệ đạt = (OK / Total) × 100%
```

**Ví dụ:**
```
Máy #1: Tổng=100, OK=90, NG=10  →  Tỷ lệ đạt = 90%
Máy #2: Tổng=85,  OK=78, NG=7   →  Tỷ lệ đạt = 91.76%
```

**Lưu ý quan trọng:**
- Mỗi thiết bị có bộ đếm **độc lập**
- Không tổng hợp chung theo model sản phẩm
- Để xem tổng hợp nhiều thiết bị, cần xuất dữ liệu CSV và phân tích

### 6.3. Biểu đồ lực xiết

**Mục đích:**
- Theo dõi xu hướng lực xiết qua thời gian
- Phát hiện sớm dấu hiệu bất thường
- Đánh giá độ ổn định của quy trình

**Thông số biểu đồ:**
- **Số điểm**: 30 lần vặn gần nhất
- **Trục ngang**: Thứ tự các lần vặn (từ cũ đến mới)
- **Trục dọc**: Lực xiết (Nm)
- **Đường giới hạn trên**: Max Torque (đường ngang đỏ)
- **Đường giới hạn dưới**: Min Torque (đường ngang đỏ)
- **Đường dữ liệu**: Các điểm nối với nhau (màu xanh)

**Cách đọc:**
- Điểm nằm giữa 2 đường giới hạn: OK
- Điểm vượt quá đường trên hoặc dưới: NG
- Xu hướng lên dần: Lực xiết tăng
- Xu hướng xuống dần: Lực xiết giảm
- Dao động nhiều: Quy trình không ổn định

**Cập nhật:**
- Tự động thêm điểm mới khi có dữ liệu
- Xóa điểm cũ nhất khi vượt quá 30 điểm
- Biểu đồ được vẽ lại mỗi giây

**Tương tác:**
- Không hỗ trợ zoom hoặc pan
- Không hiển thị tooltip với giá trị chính xác
- Chỉ xem được 30 lần gần nhất (xem lịch sử đầy đủ trong file CSV)

### 6.4. Thời gian và ngày tháng

**Ngày tháng trên thẻ thiết bị:**
- Format: `dd.MM.yyyy` (VD: 28.05.2025)
- Hiển thị ngày của lần cập nhật cuối cùng
- Dùng để kiểm tra dữ liệu có còn mới hay không

**Đồng hồ trên Status Bar:**
- Format: `HH:mm:ss` (VD: 08:15:32)
- Cập nhật theo thời gian thực
- Đồng bộ với thời gian hệ thống Windows

---

## 7. CẤU HÌNH HỆ THỐNG

### 7.1. Mở màn hình cấu hình
1. Click nút **⚙️ Cấu hình** trên bảng điều khiển
2. Cửa sổ Settings xuất hiện với 4 tab:
   - **Devices**: Quản lý thiết bị
   - **Modbus**: Cấu hình kết nối Modbus
   - **Registers**: Địa chỉ thanh ghi
   - **UI**: Giao diện hiển thị

### 7.2. Quản lý thiết bị

**Thêm thiết bị mới:**
1. Vào tab **Devices**
2. Click nút **"Thêm thiết bị"**
3. Hệ thống tạo thiết bị mới với cấu hình mặc định
4. Chỉnh sửa các thông số:
   - **Device ID**: Mã số duy nhất (số nguyên)
   - **Device Name**: Tên hiển thị (VD: "Máy #1 Công đoạn A")
   - **Device Model**: Model thiết bị (VD: "Handy2000")
   - **IP Address**: Địa chỉ IP (VD: "192.168.1.100")
   - **Port**: Cổng Modbus (mặc định 502)
   - **Slave ID**: ID slave Modbus (1-247)
   - **Min Torque**: Lực xiết tối thiểu (Nm)
   - **Max Torque**: Lực xiết tối đa (Nm)
   - **Target Torque**: Lực xiết mục tiêu (Nm)
   - **Enabled**: Bật/tắt thiết bị (checkbox)

**Chỉnh sửa thiết bị:**
1. Chọn thiết bị trong danh sách
2. Sửa trực tiếp các trường
3. Click **"Lưu"** để áp dụng

**Xóa thiết bị:**
1. Chọn thiết bị cần xóa
2. Click nút **"Xóa thiết bị"**
3. Xác nhận trong hộp thoại
4. Thiết bị bị xóa khỏi danh sách

**Lưu ý:**
- Device ID phải duy nhất
- IP Address phải hợp lệ và khả dụng
- Min < Target < Max
- Thay đổi chỉ có hiệu lực sau khi click "Lưu" và khởi động lại giám sát

### 7.3. Cấu hình Modbus

**Tab Modbus Settings:**

**Connection Type (Loại kết nối):**
- **TCP_Individual**: Kết nối riêng lẻ đến từng thiết bị theo IP
- **TCP_Gateway**: Kết nối qua một gateway chung
- **RTU_Serial**: Kết nối qua cổng COM (RS485)

**Cấu hình TCP Individual:**
- Mỗi thiết bị có IP riêng
- Cổng (Port) thường là 502
- Không cần Gateway IP

**Cấu hình TCP Gateway:**
- **Gateway IP**: Địa chỉ IP của gateway (VD: 192.168.1.1)
- **Gateway Port**: Cổng của gateway (VD: 502)
- Phân biệt thiết bị bằng Slave ID

**Cấu hình RTU Serial:**
- **Serial Port**: Tên cổng COM (VD: COM1, COM3)
- **Baud Rate**: Tốc độ truyền (9600, 19200, 38400, 115200)
- Phân biệt thiết bị bằng Slave ID

**Thông số chung:**
- **Timeout**: Thời gian chờ phản hồi (milliseconds, mặc định 5000)
- **Retry Count**: Số lần thử lại khi lỗi (mặc định 3)
- **Scan Interval**: Khoảng thời gian đọc dữ liệu (ms, mặc định 1000)

### 7.4. Cấu hình thanh ghi (Register Mapping)

**Mục đích:**
- Điều chỉnh địa chỉ thanh ghi Modbus để phù hợp với thiết bị khác nhau
- Mặc định cấu hình cho thiết bị Handy2000

**Thanh ghi điều khiển (Input Registers):**
- **BUSY Register**: Địa chỉ thanh ghi trạng thái bận (mặc định: 100082)
- **COMP Register**: Địa chỉ thanh ghi hoàn thành (mặc định: 100084)
- **OK Register**: Địa chỉ thanh ghi kết quả OK (mặc định: 100085)
- **NG Register**: Địa chỉ thanh ghi kết quả NG (mặc định: 100086)

**Thanh ghi dữ liệu (Holding Registers - Float32):**
- **LastFastenFinalTorque**: Lực xiết thực tế (mặc định: 308467)
- **LastFastenTargetTorque**: Lực xiết mục tiêu (mặc định: 308481)
- **LastFastenMinTorque**: Lực xiết tối thiểu (mặc định: 308482)
- **LastFastenMaxTorque**: Lực xiết tối đa (mặc định: 308483)

**Chú ý:**
- Địa chỉ PLC khác với địa chỉ Modbus (có offset)
- Input Registers: PLC address - 100001 = Modbus address
- Holding Registers: PLC address - 300001 = Modbus address
- Giá trị Float32 chiếm 2 thanh ghi liên tiếp

**Khi nào cần thay đổi:**
- Sử dụng thiết bị không phải Handy2000
- Thiết bị có mapping thanh ghi khác
- Cần đọc thêm thanh ghi khác

### 7.5. Cấu hình giao diện

**Tab UI Settings:**

**Grid Layout:**
- **Grid Columns**: Số cột hiển thị (1-10, mặc định 5)
- **Grid Rows**: Số hàng hiển thị (0=auto, 1-10, mặc định 3)
- Tổng số ô = Columns × Rows (VD: 5×3 = 15 thiết bị)

**Hiển thị:**
- **Theme**: Light (chưa hỗ trợ Dark)
- **Language**: Vietnamese
- **Refresh Interval**: Tốc độ cập nhật UI (ms, mặc định 1000)

**Lưu ý:**
- Thay đổi Grid Layout có hiệu lực ngay lập tức
- Nếu số thiết bị > số ô, cần cuộn để xem hết
- Layout 5×3 (15 ô) là tối ưu cho màn hình Full HD

### 7.6. Lưu và áp dụng cấu hình

**Lưu cấu hình:**
1. Sau khi chỉnh sửa, click nút **"Lưu"**
2. Cấu hình được ghi vào file `Config/devices.json`
3. Hộp thoại xác nhận "Cấu hình đã được lưu thành công!"
4. Cửa sổ Settings tự động đóng

**Hủy thay đổi:**
1. Click nút **"Hủy"** hoặc đóng cửa sổ
2. Các thay đổi chưa lưu sẽ bị bỏ qua

**Tải lại cấu hình:**
1. Click nút **"Tải lại"** trong Settings
2. Hệ thống đọc lại file `Config/devices.json`
3. Tất cả thay đổi chưa lưu bị mất

**Áp dụng thay đổi:**
- Thay đổi UI Settings: Có hiệu lực ngay
- Thay đổi Devices/Modbus/Registers: Cần ngắt kết nối và kết nối lại
- Khuyến nghị: Ngắt kết nối → Lưu cấu hình → Kết nối lại

---

## 8. LOGGING VÀ BÁO CÁO

### 8.1. Tự động lưu CSV

**Cơ chế hoạt động:**
- Mỗi khi có sự kiện hoàn thành vặn vít (completion event), dữ liệu được ghi vào file CSV
- Ghi bất đồng bộ (async) để không làm chậm UI
- Thread-safe: Nhiều thiết bị có thể ghi đồng thời

**Thời điểm ghi log:**
- Chỉ ghi khi có kết quả hoàn thành (OK hoặc NG)
- Không ghi khi thiết bị đang chờ (BUSY) hoặc không có dữ liệu

**Thư mục lưu trữ:**
- Vị trí: `HistoryLogs/` (tự động tạo nếu chưa có)
- Đường dẫn đầy đủ: `<thư mục phần mềm>\HistoryLogs\`

### 8.2. Cấu trúc file log

**Tên file:**
- Format: `ScrewingHistory_YYYY-MM-DD.csv`
- Ví dụ: `ScrewingHistory_2025-05-28.csv`
- Mỗi ngày một file riêng
- Tự động tạo file mới vào 00:00

**Header (dòng đầu tiên):**
```csv
Timestamp,DeviceID,DeviceName,ActualTorque,MinTorque,MaxTorque,TargetTorque,Result
```

**Dòng dữ liệu:**
```csv
2025-05-28 08:15:32,1,Máy #1 Công đoạn A,12.50,9.80,14.00,12.00,OK
2025-05-28 08:15:45,2,Máy #2 Công đoạn B,15.20,9.80,14.00,12.00,NG
2025-05-28 08:16:01,1,Máy #1 Công đoạn A,11.80,9.80,14.00,12.00,OK
```

**Giải thích các cột:**
- **Timestamp**: Thời điểm ghi nhận (YYYY-MM-DD HH:mm:ss)
- **DeviceID**: Mã số thiết bị (số nguyên)
- **DeviceName**: Tên thiết bị
- **ActualTorque**: Lực xiết thực tế (Nm, 2 chữ số thập phân)
- **MinTorque**: Lực xiết tối thiểu (Nm, 1 chữ số)
- **MaxTorque**: Lực xiết tối đa (Nm, 1 chữ số)
- **TargetTorque**: Lực xiết mục tiêu (Nm, 1 chữ số)
- **Result**: Kết quả (OK hoặc NG)

**Encoding:**
- UTF-8 (hỗ trợ tiếng Việt có dấu)
- Phân cách bằng dấu phẩy (,)
- Không có dấu ngoặc kép quanh giá trị

### 8.3. Xem và phân tích dữ liệu

**Mở file CSV bằng Excel:**
1. Mở Microsoft Excel
2. File → Open → Chọn file CSV
3. Hoặc double-click trực tiếp vào file CSV

**Lọc dữ liệu:**
- Click vào header row
- Bật filter (Data → Filter)
- Chọn điều kiện lọc cho từng cột

**Tính toán tỷ lệ OK/NG:**
```excel
=COUNTIF(H:H,"OK")     // Đếm số lần OK
=COUNTIF(H:H,"NG")     // Đếm số lần NG
=COUNTIF(H:H,"OK")/COUNTA(H:H)-1  // Tỷ lệ OK (trừ 1 cho header)
```

**Tạo bảng Pivot:**
1. Chọn toàn bộ dữ liệu (Ctrl+A)
2. Insert → PivotTable
3. Kéo DeviceName vào Rows
4. Kéo Result vào Values (Count)
5. Được bảng thống kê OK/NG theo thiết bị

**Vẽ biểu đồ xu hướng:**
1. Chọn cột Timestamp và ActualTorque
2. Insert → Chart → Line Chart
3. Xem xu hướng lực xiết theo thời gian

**Phân tích nâng cao:**
- Import vào Power BI hoặc Tableau
- Sử dụng Python/R để tính toán CPK, PPK
- Tạo dashboard tự động

**Backup dữ liệu:**
- Copy thư mục `HistoryLogs/` ra ổ đĩa ngoài định kỳ
- Hoặc sử dụng cloud storage (Google Drive, OneDrive)
- Khuyến nghị backup hàng tuần

---

## 9. XỬ LÝ SỰ CỐ

### 9.1. Lỗi kết nối

**Triệu chứng:**
- Thẻ thiết bị hiển thị **OFFLINE** (cam)
- Trạng thái: "Kết nối thất bại" hoặc "Mất kết nối"
- Không có dữ liệu cập nhật

**Nguyên nhân và cách khắc phục:**

**A. Lỗi mạng:**
- **Kiểm tra**: Ping đến IP thiết bị
  ```
  ping 192.168.1.100
  ```
- **Nếu ping không thông**:
  - Kiểm tra cáp mạng
  - Kiểm tra switch/router
  - Kiểm tra IP của máy tính và thiết bị cùng subnet
- **Nếu ping thông**: Chuyển sang bước B

**B. Lỗi cấu hình Modbus:**
- **Kiểm tra**: Địa chỉ IP, Port, Slave ID trong Settings
- **Khắc phục**:
  - Vào Settings → Devices
  - Đối chiếu với thông số thực tế của thiết bị
  - Sửa lại và lưu
  - Ngắt kết nối → Kết nối lại

**C. Thiết bị đang bận hoặc tắt:**
- **Kiểm tra**: Đèn LED trên thiết bị vặn vít
- **Khắc phục**:
  - Bật nguồn thiết bị
  - Chờ thiết bị khởi động hoàn tất (30-60 giây)
  - Kết nối lại

**D. Firewall chặn kết nối:**
- **Kiểm tra**: Windows Defender Firewall
- **Khắc phục**:
  - Mở Control Panel → Windows Defender Firewall
  - Click "Allow an app through firewall"
  - Tìm và cho phép HMI_ScrewingMonitor.exe
  - Hoặc tắt firewall tạm thời để test

**E. Timeout quá ngắn:**
- **Kiểm tra**: Settings → Modbus → Timeout
- **Khắc phục**:
  - Tăng Timeout lên 10000ms (10 giây)
  - Tăng Retry Count lên 5
  - Lưu và kết nối lại

### 9.2. Lỗi đọc dữ liệu

**Triệu chứng:**
- Thiết bị hiển thị ONLINE nhưng không có dữ liệu mới
- Biểu đồ không cập nhật
- Bộ đếm không tăng
- Trạng thái: "Lỗi đọc dữ liệu" hoặc "--"

**Nguyên nhân và cách khắc phục:**

**A. Sai địa chỉ thanh ghi:**
- **Kiểm tra**: Settings → Registers
- **Khắc phục**:
  - Đối chiếu với tài liệu của thiết bị
  - Sửa lại Register Mapping
  - Lưu và khởi động lại giám sát

**B. Thiết bị không hoạt động:**
- **Kiểm tra**: Thiết bị có đang chạy vặn vít không?
- **Khắc phục**:
  - Thực hiện một lần vặn vít thử nghiệm
  - Quan sát xem dữ liệu có cập nhật không

**C. Tốc độ quét quá nhanh:**
- **Kiểm tra**: Settings → Modbus → Scan Interval
- **Khắc phục**:
  - Tăng Scan Interval lên 2000ms (2 giây)
  - Giảm tải cho thiết bị

**D. Dữ liệu không hợp lệ:**
- **Kiểm tra**: Giá trị Actual Torque = 0 hoặc số lạ
- **Khắc phục**:
  - Có thể do byte order sai
  - Liên hệ nhà cung cấp thiết bị để xác nhận format dữ liệu
  - Cần sửa code trong ModbusService.cs (ConvertRegistersToFloat)

### 9.3. Lỗi hiển thị

**Triệu chứng:**
- Giao diện bị lỗi, chữ chồng lên nhau
- Biểu đồ không hiển thị đúng
- Màu sắc bị sai

**Nguyên nhân và cách khắc phục:**

**A. Độ phân giải màn hình thấp:**
- **Yêu cầu tối thiểu**: 1280x800
- **Khuyến nghị**: 1920x1080 (Full HD)
- **Khắc phục**: Nâng cấp màn hình hoặc tăng độ phân giải

**B. Scaling Windows:**
- **Kiểm tra**: Settings → Display → Scale (nên để 100%)
- **Khắc phục**:
  - Giảm scale xuống 100%
  - Hoặc chỉnh DPI awareness của app

**C. Quá nhiều thiết bị hiển thị:**
- **Khắc phục**:
  - Vào Settings → UI → Tăng Grid Columns/Rows
  - Hoặc vô hiệu hóa các thiết bị không cần giám sát

### 9.4. Lỗi khác

**Phần mềm không khởi động:**
- Kiểm tra đã cài .NET 6.0 Runtime chưa
- Download từ: https://dotnet.microsoft.com/download/dotnet/6.0
- Chạy lại sau khi cài

**File log không tạo:**
- Kiểm tra quyền ghi vào thư mục HistoryLogs/
- Chạy phần mềm với quyền Administrator
- Kiểm tra ổ cứng còn dung lượng trống

**Bộ đếm không reset vào 00:00:**
- Máy tính phải chạy liên tục qua 00:00
- Nếu tắt máy qua đêm, bộ đếm sẽ reset khi khởi động lại
- Đảm bảo đồng hồ hệ thống chính xác

**Dữ liệu bị trùng lặp:**
- Do detection logic phát hiện nhiều lần
- Hệ thống có cơ chế chống duplicate trong 1 giây
- Nếu vẫn bị, liên hệ hỗ trợ kỹ thuật

---

## 10. PHỤ LỤC

### 10.1. Thuật ngữ kỹ thuật

**Modbus:**
- Giao thức truyền thông công nghiệp phổ biến
- Hỗ trợ TCP/IP (Modbus TCP) và Serial (Modbus RTU)

**Torque (Lực xiết):**
- Lực xoắn tác dụng lên bu-lông/vít khi vặn
- Đơn vị: Nm (Newton-mét)

**Register (Thanh ghi):**
- Vùng nhớ trong thiết bị Modbus
- Input Registers: Chỉ đọc (read-only)
- Holding Registers: Đọc/ghi (read-write)

**Slave ID:**
- Địa chỉ định danh thiết bị trên bus Modbus
- Giá trị từ 1 đến 247

**Rising Edge (Cạnh lên):**
- Chuyển trạng thái từ 0 sang 1 (OFF → ON)
- Dùng để phát hiện sự kiện hoàn thành

**CSV (Comma-Separated Values):**
- Định dạng file dữ liệu dạng bảng
- Mở được bằng Excel, Google Sheets

**Float32:**
- Số thực 32-bit (4 bytes)
- Chiếm 2 thanh ghi Modbus liên tiếp

**Scan Interval:**
- Khoảng thời gian giữa 2 lần đọc dữ liệu
- Đơn vị: milliseconds (ms)

### 10.2. Câu hỏi thường gặp (FAQ)

**Q1: Hệ thống hỗ trợ tối đa bao nhiêu thiết bị?**
A: Không giới hạn về mặt kỹ thuật, nhưng khuyến nghị tối đa 15 thiết bị để hiển thị tối ưu trên một màn hình. Có thể mở nhiều instance của phần mềm để giám sát nhiều nhóm thiết bị.

**Q2: Có thể thay đổi tần suất đọc dữ liệu không?**
A: Có, vào Settings → Modbus → Scan Interval. Giá trị mặc định là 1000ms (1 giây). Không nên đặt dưới 500ms để tránh quá tải thiết bị.

**Q3: Dữ liệu log lưu ở đâu?**
A: Trong thư mục `HistoryLogs/` cùng cấp với file .exe. Mỗi ngày một file CSV riêng.

**Q4: Bộ đếm OK/NG có lưu khi tắt phần mềm không?**
A: Có, bộ đếm được lưu vào file `Config/devices.json` khi tắt phần mềm. Khi khởi động lại, hệ thống sẽ load lại số liệu (trừ khi sang ngày mới thì reset về 0).

**Q5: Có thể xuất báo cáo tự động không?**
A: Hiện tại chưa hỗ trợ. Cần xuất file CSV và sử dụng Excel để tạo báo cáo.

**Q6: Phần mềm có chạy được trên Linux/Mac không?**
A: Không, hiện tại chỉ hỗ trợ Windows 10/11. Có thể chạy trên Linux bằng Wine nhưng không được kiểm chứng.

**Q7: Có thể kết nối thiết bị không phải Handy2000 không?**
A: Có, nhưng cần điều chỉnh Register Mapping trong Settings để phù hợp với thiết bị.

**Q8: Làm sao để backup dữ liệu?**
A: Copy thư mục `HistoryLogs/` ra ổ đĩa ngoài hoặc cloud storage định kỳ.

**Q9: Phần mềm có cần license không?**
A: Tùy theo thỏa thuận với nhà cung cấp. Liên hệ bộ phận bán hàng để biết chi tiết.

**Q10: Có hỗ trợ nhiều ngôn ngữ không?**
A: Hiện tại chỉ hỗ trợ tiếng Việt. Có thể thêm ngôn ngữ khác trong phiên bản tương lai.

### 10.3. Phím tắt

Hiện tại phần mềm chưa hỗ trợ phím tắt. Tất cả thao tác thực hiện bằng chuột.

### 10.4. Thông tin phiên bản

**Phiên bản hiện tại:** 1.0.0
**Ngày phát hành:** 2025
**Framework:** .NET 6.0
**Kiến trúc:** WPF (Windows Presentation Foundation)
**Pattern:** MVVM (Model-View-ViewModel)

**Thư viện sử dụng:**
- NModbus 3.0.72 - Modbus communication
- System.IO.Ports 7.0.0 - Serial port
- Newtonsoft.Json 13.0.4 - JSON serialization

### 10.5. Liên hệ hỗ trợ

**Hỗ trợ kỹ thuật:**
- Email: support@example.com
- Hotline: +84 xxx xxx xxx
- Thời gian: 8:00-17:00, Thứ 2 - Thứ 6

**Báo lỗi (Bug Report):**
- Mô tả chi tiết lỗi
- Đính kèm screenshot nếu có
- Gửi file log (HistoryLogs/) nếu cần
- Thông tin phiên bản phần mềm

**Yêu cầu tính năng mới:**
- Email: feedback@example.com
- Mô tả tính năng mong muốn
- Lý do và lợi ích của tính năng

**Tài liệu và cập nhật:**
- Website: https://example.com/hmi-screwing-monitor
- Tài liệu kỹ thuật: Xem file CLAUDE.md, DEVICE_REGISTERS.md
- Cập nhật phần mềm: Download từ website hoặc liên hệ hỗ trợ

---

## KẾT LUẬN

Hệ thống HMI Screwing Monitor cung cấp giải pháp giám sát toàn diện cho quy trình vặn vít tự động. Với giao diện trực quan, dễ sử dụng và khả năng lưu trữ dữ liệu đầy đủ, phần mềm giúp nâng cao hiệu quả kiểm soát chất lượng trong sản xuất.

Để sử dụng hiệu quả, người dùng cần:
1. Hiểu rõ các thông số lực xiết và ý nghĩa
2. Cấu hình đúng địa chỉ và thông số Modbus
3. Theo dõi thường xuyên và phân tích dữ liệu log
4. Bảo trì định kỳ (backup dữ liệu, kiểm tra kết nối)

Mọi thắc mắc và hỗ trợ, vui lòng liên hệ bộ phận kỹ thuật.

---

**Tài liệu này được tạo cho phiên bản 1.0.0**
**Cập nhật lần cuối: 2025**
