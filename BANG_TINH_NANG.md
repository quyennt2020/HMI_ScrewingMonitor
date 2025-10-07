# BẢNG TÍNH NĂNG HỆ THỐNG
## HMI SCREWING MONITOR - PHẦN MÊM GIÁM SÁT THIẾT BỊ VẶN VÍT TỰ ĐỘNG

---

## 1. THÔNG TIN DỰ ÁN

| Thông tin | Chi tiết |
|-----------|----------|
| **Tên dự án** | HMI Screwing Monitor |
| **Phiên bản** | 1.0.0 |
| **Ngày phát hành** | 2025 |
| **Nền tảng** | Windows 10/11 - .NET 6.0 |
| **Loại phần mềm** | Desktop Application (WPF) |

---

## 2. TỔNG QUAN HỆ THỐNG

Hệ thống HMI Screwing Monitor là phần mềm giám sát và kiểm soát chất lượng quá trình vặn vít tự động trong sản xuất. Phần mềm kết nối với các thiết bị vặn vít thông qua giao thức Modbus TCP/IP, thu thập và hiển thị dữ liệu lực xiết theo thời gian thực, đồng thời tự động lưu trữ lịch sử để phục vụ phân tích và báo cáo chất lượng.

**Đối tượng sử dụng:** Nhân viên giám sát sản xuất, kỹ sư QC, quản lý chất lượng

**Lợi ích:** Giảm lỗi sản phẩm, tăng hiệu quả kiểm soát, lưu trữ dữ liệu truy xuất nguồn gốc

---

## 3. TÍNH NĂNG CHÍNH

> **📌 LƯU Ý QUAN TRỌNG:**
> - Hệ thống hỗ trợ giám sát từ **1 đến 15 thiết bị**, có thể cấu hình linh hoạt
> - Tất cả thông số lực xiết (Min/Max/Target/Actual) và kết quả OK/NG được **đọc trực tiếp từ thiết bị** qua Modbus
> - Kết nối chỉ hỗ trợ **Modbus TCP/IP** (không hỗ trợ Serial RTU)

### 3.1. Giám sát và hiển thị

✅ **Giám sát đa thiết bị**
- Hỗ trợ từ **1 đến 15 thiết bị** (có thể cấu hình linh hoạt)
- Hiển thị đồng thời trên một màn hình
- Mỗi thiết bị hiển thị độc lập trong một thẻ (card)
- Layout linh hoạt (grid 5 cột × 3 hàng hoặc tùy chỉnh)

✅ **Hiển thị trạng thái real-time**
- Trạng thái kết nối: **ONLINE** (xanh) / **OFFLINE** (cam) / **DISABLED** (xám)
- Đèn LED trực quan hiển thị tình trạng thiết bị
- Cập nhật dữ liệu mỗi **1 giây** (có thể tùy chỉnh)

✅ **Kết quả vặn vít**
- Hiển thị lớn, rõ ràng: **✓ OK** (nền xanh) hoặc **✗ NG** (nền đỏ)
- Kết quả được đọc trực tiếp từ thiết bị qua Modbus
- Đồng bộ hoàn toàn với đánh giá của máy vặn vít
- Cảnh báo trực quan bằng màu sắc

### 3.2. Đo lường lực xiết (Torque)

✅ **Đọc thông số trực tiếp từ thiết bị**
- **Lực Min**: Đọc từ thiết bị qua Modbus TCP/IP
- **Lực Max**: Đọc từ thiết bị qua Modbus TCP/IP
- **Lực đặt**: Đọc từ thiết bị qua Modbus TCP/IP
- **Lực thực tế**: Đọc từ thiết bị qua Modbus TCP/IP
- **Kết quả OK/NG**: Đọc từ thiết bị qua Modbus TCP/IP
- Format hiển thị: Số thập phân 1 chữ số (VD: 12.5 Nm)

✅ **Tự động đồng bộ**
- Tất cả thông số được đồng bộ real-time từ thiết bị
- Không cần cấu hình thủ công
- Luôn chính xác với cài đặt trên máy vặn vít

### 3.3. Thống kê và đếm số liệu

✅ **Bộ đếm theo thiết bị**
- **Tổng**: Tổng số lần vặn vít trong ngày
- **OK**: Số lần đạt chuẩn (màu xanh)
- **NG**: Số lần không đạt (màu đỏ)
- Mỗi thiết bị có bộ đếm độc lập

✅ **Reset tự động hàng ngày**
- Bộ đếm tự động reset về 0 vào 00:00 mỗi ngày
- Dữ liệu cũ được lưu vào file log trước khi reset
- Không cần can thiệp thủ công

✅ **Thống kê tổng hợp**
- Thanh trạng thái hiển thị tổng OK/NG của tất cả thiết bị
- Số liệu "Tổng hôm nay" tách biệt theo ngày
- Tỷ lệ đạt (%) tính tự động

### 3.4. Biểu đồ và trực quan hóa

✅ **Biểu đồ lực xiết real-time**
- Hiển thị **30 lần vặn gần nhất** cho mỗi thiết bị
- Đường giới hạn Max và Min để tham chiếu
- Tự động cập nhật khi có dữ liệu mới
- Tự động điều chỉnh tỷ lệ (auto-scaling)

✅ **Giao diện trực quan**
- Màu sắc rõ ràng, dễ nhận biết trạng thái
- Font chữ lớn, dễ đọc từ xa
- Hiển thị model thiết bị, tên công đoạn
- Ngày tháng cập nhật trên mỗi thẻ

### 3.5. Logging và lưu trữ

✅ **Tự động ghi log CSV**
- Ghi mỗi lần vặn vít vào file CSV
- Tên file theo ngày: `ScrewingHistory_YYYY-MM-DD.csv`
- Thư mục: `HistoryLogs/` (tự động tạo)
- An toàn với cơ chế ghi bất đồng bộ (async)

✅ **Cấu trúc dữ liệu đầy đủ**
- Thời gian, mã thiết bị, tên thiết bị
- Lực xiết thực tế, Min, Max, Target
- Kết quả (OK/NG)
- Encoding UTF-8 (hỗ trợ tiếng Việt)

✅ **Dễ phân tích**
- Mở trực tiếp bằng Microsoft Excel
- Dùng PivotTable để tạo báo cáo
- Hỗ trợ import vào Power BI, Python, R

### 3.6. Kết nối Modbus TCP/IP

✅ **Hỗ trợ kết nối Modbus TCP**
- **TCP Individual**: Kết nối riêng đến từng thiết bị qua IP độc lập
- Mỗi thiết bị có địa chỉ IP riêng (VD: 192.168.1.100, 192.168.1.101...)
- Cổng Modbus mặc định: 502

✅ **Tự động kết nối lại**
- Phát hiện mất kết nối và thử kết nối lại tự động
- Cơ chế retry với timeout có thể cấu hình
- Hiển thị trạng thái kết nối real-time cho từng thiết bị

✅ **Cấu hình linh hoạt**
- Register Mapping có thể điều chỉnh qua giao diện
- Hỗ trợ Handy2000 và các thiết bị tương thích
- Timeout, Retry Count, Scan Interval tùy chỉnh
- Mỗi thiết bị có thể cấu hình IP, Port, Slave ID riêng

### 3.7. Quản lý và cấu hình

✅ **Quản lý thiết bị**
- Thêm/sửa/xóa thiết bị qua giao diện
- Bật/tắt từng thiết bị (Enable/Disable)
- Lưu cấu hình vào file JSON

✅ **Cấu hình hệ thống**
- Cài đặt Modbus (IP, Port, Slave ID, Timeout)
- Cài đặt Register Mapping (địa chỉ thanh ghi)
- Cài đặt giao diện (số cột, hàng, tần suất cập nhật)

✅ **Dễ sử dụng**
- Giao diện cấu hình trực quan
- Không cần chỉnh sửa code
- Có thể thay đổi cấu hình khi đang chạy

### 3.8. Các tính năng khác

✅ **Giao diện thân thiện**
- Thiết kế hiện đại, chuyên nghiệp
- Hỗ trợ tiếng Việt đầy đủ
- Responsive, không bị lag

✅ **Ổn định và tin cậy**
- Xử lý lỗi toàn diện
- Không crash khi mất kết nối
- Thread-safe cho ghi log

✅ **Hiệu suất cao**
- Đọc dữ liệu song song (parallel)
- Ghi log bất đồng bộ
- Tối ưu bộ nhớ và CPU

---

## 4. THÔNG SỐ KỸ THUẬT

### 4.1. Khả năng

| Thông số | Giá trị |
|----------|---------|
| **Số thiết bị giám sát** | 1 - 15 thiết bị (có thể cấu hình linh hoạt) |
| **Tần suất cập nhật** | 1 giây (mặc định), có thể tùy chỉnh 500ms - 5s |
| **Số điểm dữ liệu biểu đồ** | 30 lần vặn gần nhất |
| **Giao thức hỗ trợ** | Modbus TCP/IP |
| **Độ chính xác torque** | 0.1 Nm (1 chữ số thập phân) |
| **Định dạng log** | CSV (UTF-8) |

### 4.2. Công nghệ

| Thành phần | Công nghệ |
|------------|-----------|
| **Framework** | .NET 6.0 |
| **Giao diện** | WPF (Windows Presentation Foundation) |
| **Kiến trúc** | MVVM (Model-View-ViewModel) |
| **Modbus Library** | NModbus 3.0.72 |
| **Serialization** | Newtonsoft.Json 13.0.4 |

### 4.3. Thiết bị tương thích

- **Handy2000** (mặc định, đã test)
- Các thiết bị vặn vít hỗ trợ Modbus TCP/IP
- Yêu cầu: Cung cấp thanh ghi BUSY, COMP, OK, NG, Torque (Min/Max/Target/Actual)

---

## 5. YÊU CẦU HỆ THỐNG

### 5.1. Phần cứng tối thiểu

| Thành phần | Yêu cầu tối thiểu | Khuyến nghị |
|------------|-------------------|-------------|
| **Processor** | Intel Core i3 hoặc tương đương | Intel Core i5 trở lên |
| **RAM** | 4GB | 8GB |
| **Ổ cứng** | 500MB dung lượng trống | 2GB (cho log) |
| **Màn hình** | 1280×800 | 1920×1080 (Full HD) |
| **Card mạng** | Ethernet 10/100Mbps (bắt buộc) | Ethernet Gigabit |

### 5.2. Phần mềm

| Yêu cầu | Chi tiết |
|---------|----------|
| **Hệ điều hành** | Windows 10 (64-bit) hoặc Windows 11 |
| **.NET Runtime** | .NET 6.0 Desktop Runtime (miễn phí) |
| **Quyền truy cập** | Administrator (chỉ khi cài đặt) |
| **Phần mềm bổ sung** | Microsoft Excel (để xem file CSV) - tùy chọn |

### 5.3. Mạng

| Yêu cầu | Chi tiết |
|---------|----------|
| **Kết nối** | Ethernet LAN |
| **Băng thông** | 10Mbps trở lên |
| **Topology** | Cùng subnet với thiết bị Modbus |
| **IP Address** | Tĩnh (static) - khuyến nghị |
| **Firewall** | Cho phép kết nối đến cổng Modbus (502) |

---

## 6. PHẠM VI BÀN GIAO

### 6.1. Phần mềm

✅ **Ứng dụng chính**
- File thực thi: `HMI_ScrewingMonitor.exe`
- Thư viện đi kèm (DLL files)
- File cấu hình mặc định: `Config/devices.json`
- Toàn bộ phần mềm trong một thư mục, chạy trực tiếp không cần cài đặt

### 6.2. Tài liệu

✅ **Tài liệu người dùng**
- `HUONG_DAN_SU_DUNG.md` - Hướng dẫn sử dụng chi tiết (40+ trang)
- `BANG_TINH_NANG.md` - Bảng tính năng (tài liệu này)
- `README.md` - Tổng quan dự án

✅ **Tài liệu kỹ thuật**
- `CLAUDE.md` - Hướng dẫn phát triển
- `DEVICE_REGISTERS.md` - Mapping thanh ghi Modbus
- `PROJECT_STRUCTURE.md` - Cấu trúc dự án

### 6.3. File cấu hình mẫu

✅ **Cấu hình thiết bị**
- File: `Config/devices.json`
- Nội dung: 15 thiết bị mẫu với cấu hình đầy đủ
- Có thể tùy chỉnh theo môi trường thực tế

### 6.4. Hỗ trợ (nếu có trong hợp đồng)

⚠️ **Cần xác nhận trong hợp đồng:**
- [ ] Hỗ trợ cài đặt tại hiện trường
- [ ] Đào tạo người dùng (__ giờ)
- [ ] Hỗ trợ kỹ thuật (__ tháng)
- [ ] Bảo hành phần mềm (__ tháng)
- [ ] Cập nhật và nâng cấp
- [ ] Tùy chỉnh theo yêu cầu đặc biệt

---

## 7. TÍNH NĂNG KHÔNG BAO GỒM

Các tính năng sau **KHÔNG** có trong phiên bản hiện tại:

❌ Đếm số liệu OK/NG theo Model sản phẩm (hiện tại đếm theo thiết bị)
❌ Tính toán chỉ số CPK/XR/SPC
❌ Xuất báo cáo tự động (PDF, Word)
❌ Gửi email cảnh báo
❌ Cảnh báo âm thanh (sound alert)
❌ Remote monitoring qua web browser
❌ Mobile app (iOS/Android)
❌ Database server (SQL, MySQL) - chỉ có CSV
❌ Multi-language (chỉ Tiếng Việt)
❌ Dark theme (chỉ Light theme)
❌ User authentication (login/password)
❌ Multi-user concurrent access

**Lưu ý:** Các tính năng trên có thể được phát triển thêm trong phiên bản tương lai theo yêu cầu và thỏa thuận riêng.

---

## 8. ĐIỀU KHOẢN BẢN QUYỀN

- Phần mềm thuộc quyền sở hữu của nhà cung cấp
- Khách hàng được cấp quyền sử dụng theo thỏa thuận hợp đồng
- Không được sao chép, phân phối lại cho bên thứ ba
- Không được reverse engineering, decompile
- Source code không bao gồm trong bàn giao (trừ khi có thỏa thuận riêng)

---

## 9. XÁC NHẬN

### Bên A: Nhà cung cấp

Chúng tôi xác nhận đã cung cấp đầy đủ và chính xác thông tin về tính năng, thông số kỹ thuật, và phạm vi bàn giao của hệ thống HMI Screwing Monitor như đã liệt kê trong tài liệu này.

**Họ tên:** ________________________________

**Chức vụ:** ________________________________

**Chữ ký:** ________________________________

**Ngày:** _____ / _____ / _____

---

### Bên B: Khách hàng

Chúng tôi xác nhận đã đọc, hiểu rõ và đồng ý với các tính năng, thông số kỹ thuật, yêu cầu hệ thống, và phạm vi bàn giao được mô tả trong tài liệu này. Chúng tôi cam kết cung cấp đầy đủ môi trường và điều kiện cần thiết để triển khai hệ thống.

**Họ tên:** ________________________________

**Chức vụ:** ________________________________

**Chữ ký:** ________________________________

**Ngày:** _____ / _____ / _____

---

## PHỤ LỤC: DANH MỤC TÀI LIỆU THAM KHẢO

| STT | Tên tài liệu | File | Mô tả |
|-----|--------------|------|-------|
| 1 | Bảng tính năng | BANG_TINH_NANG.md | Tài liệu này |
| 2 | Hướng dẫn sử dụng | HUONG_DAN_SU_DUNG.md | Chi tiết 40+ trang |
| 3 | README | README.md | Tổng quan dự án |
| 4 | Hướng dẫn phát triển | CLAUDE.md | Cho developer |
| 5 | Thanh ghi Modbus | DEVICE_REGISTERS.md | Mapping registers |
| 6 | Cấu trúc dự án | PROJECT_STRUCTURE.md | Kiến trúc code |

---

**Tài liệu này được tạo cho mục đích ký kết hợp đồng và xác nhận tính năng**

**Phiên bản:** 1.0.0
**Ngày tạo:** 2025
**Số trang:** 5
