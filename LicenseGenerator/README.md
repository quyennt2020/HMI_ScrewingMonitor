# License Generator - Công cụ Tạo License Key

## Giới thiệu

**LicenseGenerator** là công cụ console để tạo license key cho phần mềm HMI Screwing Monitor. Tool này cho phép bạn:
- ✅ Tạo license key cho khách hàng (vĩnh viễn hoặc có thời hạn)
- ✅ Kiểm tra Hardware ID của máy tính
- ✅ Validate license key (kiểm tra tính hợp lệ)

## Cách sử dụng

### 1. Build project

```bash
cd LicenseGenerator
dotnet build
```

### 2. Chạy tool

```bash
dotnet run
```

Hoặc chạy trực tiếp file executable:

```bash
cd bin/Debug/net6.0
LicenseGenerator.exe
```

### 3. Menu chức năng

Tool cung cấp 4 chức năng chính:

```
╔════════════════════════════════════════════════════════════╗
║                         MENU                               ║
╠════════════════════════════════════════════════════════════╣
║  1. Tạo License Key cho khách hàng                         ║
║  2. Kiểm tra Hardware ID của máy này                       ║
║  3. Validate License Key (kiểm tra)                        ║
║  4. Thoát                                                  ║
╚════════════════════════════════════════════════════════════╝
```

## Quy trình cấp License cho khách hàng

### Bước 1: Khách hàng gửi Hardware ID
1. Khách hàng chạy phần mềm **HMI Screwing Monitor**
2. Phần mềm sẽ hiển thị cửa sổ license với **Hardware ID**
3. Khách hàng copy Hardware ID và gửi cho bạn

**Ví dụ Hardware ID:**
```
A1B2-C3D4-E5F6-G7H8-I9J0-K1L2-M3N4-O5P6
```

### Bước 2: Tạo License Key
1. Mở **LicenseGenerator**
2. Chọn chức năng `1. Tạo License Key cho khách hàng`
3. Nhập thông tin:
   - **Hardware ID**: Paste Hardware ID từ khách hàng
   - **Tên công ty**: Nhập tên công ty khách hàng (VD: "ABC Company")
   - **Loại license**:
     - Chọn `1` cho license vĩnh viễn (không hết hạn)
     - Chọn `2` cho license có thời hạn (nhập ngày hết hạn)

4. Tool sẽ tạo ra **License Key** dạng:
   ```
   XXXXX-XXXXX-XXXXX-XXXXX-XXXXX
   ```

**Ví dụ:**
```
═══════════════════════════════════════════════════════════════
                    ✅ THÀNH CÔNG!
═══════════════════════════════════════════════════════════════

📋 Thông tin License:
   • Công ty     : ABC Company
   • Hardware ID : A1B2-C3D4-E5F6-G7H8-I9J0-K1L2-M3N4-O5P6
   • Loại        : Vĩnh viễn

🔑 LICENSE KEY:

   ╔════════════════════════════════════╗
   ║  AB12C-34DEF-56GHI-78JKL-90MNOP  ║
   ╚════════════════════════════════════╝

💡 Gửi license key này cho khách hàng để kích hoạt phần mềm.
═══════════════════════════════════════════════════════════════
```

### Bước 3: Gửi License Key cho khách hàng
1. Copy **License Key** từ tool
2. Gửi cho khách hàng qua email/chat
3. Khách hàng nhập License Key vào phần mềm
4. Phần mềm sẽ được kích hoạt

## Các chức năng chi tiết

### 1️⃣ Tạo License Key cho khách hàng

Tạo license key từ Hardware ID của khách hàng.

**Input:**
- Hardware ID (32 ký tự)
- Tên công ty
- Loại license (vĩnh viễn hoặc có thời hạn)

**Output:**
- License Key format: `XXXXX-XXXXX-XXXXX-XXXXX-XXXXX`

**Lưu ý:**
- Hardware ID phải có đúng 32 ký tự (không tính dấu gạch ngang)
- Nếu chọn license có thời hạn, nhập ngày theo format: `dd/MM/yyyy` (VD: `31/12/2025`)

### 2️⃣ Kiểm tra Hardware ID của máy này

Hiển thị Hardware ID của máy tính đang chạy tool.

**Mục đích:**
- Kiểm tra Hardware ID của máy dev/test
- Tạo license cho máy của chính bạn

### 3️⃣ Validate License Key

Kiểm tra tính hợp lệ của một license key.

**Input:**
- License Key
- Hardware ID

**Output:**
- ✅ License hợp lệ: Hiển thị thông tin công ty, loại license, số ngày còn lại
- ❌ License không hợp lệ: Hiển thị lý do (format sai, Hardware ID không khớp, hết hạn, checksum sai)

**Mục đích:**
- Kiểm tra license key trước khi gửi cho khách hàng
- Debug khi khách hàng báo lỗi license không hoạt động

## Cấu trúc License Key

License Key có format: `XXXXX-XXXXX-XXXXX-XXXXX-XXXXX` (25 ký tự, 5 nhóm)

**Cấu trúc:**
```
[Signature 20 ký tự][Expiry Code 4 ký tự][Checksum 1 ký tự]
```

- **Signature (20 ký tự)**: SHA256 hash của (Hardware ID + Company Name + Expiry + Secret Key)
- **Expiry Code (4 ký tự)**:
  - `PERM` = License vĩnh viễn
  - Base36 của YYMM = License có thời hạn (VD: `2512` = 12/2025)
- **Checksum (1 ký tự)**: Checksum để verify tính toàn vẹn

**Bảo mật:**
- Secret key được hard-code trong source code: `HMI_SCREWING_MONITOR_2025_SECRET_KEY_V1`
- License key được bind với Hardware ID cụ thể (không thể dùng cho máy khác)
- Có checksum để phát hiện key bị sửa đổi

## Ví dụ sử dụng

### Tạo license vĩnh viễn cho khách hàng

```
Chọn chức năng (1-4): 1

1️⃣ Nhập Hardware ID của khách hàng:
   Hardware ID: A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6

2️⃣ Nhập tên công ty khách hàng:
   Tên công ty: Toyota Vietnam

3️⃣ Chọn loại license:
   1. Vĩnh viễn (không hết hạn)
   2. Có thời hạn (nhập ngày hết hạn)
   Chọn (1 hoặc 2): 1

⏳ Đang tạo license key...

✅ THÀNH CÔNG!
🔑 LICENSE KEY: AB12C-34DEF-56GHI-78JKL-90MNO
```

### Tạo license có thời hạn 1 năm

```
Chọn chức năng (1-4): 1

1️⃣ Nhập Hardware ID của khách hàng:
   Hardware ID: A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6

2️⃣ Nhập tên công ty khách hàng:
   Tên công ty: Honda Vietnam

3️⃣ Chọn loại license:
   1. Vĩnh viễn (không hết hạn)
   2. Có thời hạn (nhập ngày hết hạn)
   Chọn (1 hoặc 2): 2

   Nhập ngày hết hạn (dd/MM/yyyy):
   Ví dụ: 31/12/2025: 31/12/2026

⏳ Đang tạo license key...

✅ THÀNH CÔNG!
🔑 LICENSE KEY: CD34E-56FGH-78IJK-90LMN-12OPQ
```

## Troubleshooting

### Lỗi: "Hardware ID không hợp lệ. Phải có 32 ký tự"

**Nguyên nhân:** Hardware ID không đúng format

**Giải pháp:**
- Kiểm tra Hardware ID có đúng 32 ký tự (không tính dấu gạch ngang)
- Loại bỏ tất cả dấu cách và ký tự đặc biệt
- Copy lại Hardware ID từ phần mềm HMI Screwing Monitor

### Lỗi: "Định dạng ngày không hợp lệ"

**Nguyên nhân:** Ngày hết hạn không đúng format

**Giải pháp:**
- Nhập ngày theo format: `dd/MM/yyyy`
- Ví dụ đúng: `31/12/2025`, `15/06/2026`
- Ví dụ sai: `2025-12-31`, `12/31/2025`

### License không hoạt động trên máy khách hàng

**Nguyên nhân:** Hardware ID không khớp

**Giải pháp:**
1. Yêu cầu khách hàng gửi lại Hardware ID từ phần mềm
2. Sử dụng chức năng `3. Validate License Key` để kiểm tra
3. Tạo lại license key với Hardware ID đúng

## Lưu ý quan trọng

⚠️ **Bảo mật:**
- Không chia sẻ source code của LicenseGenerator cho khách hàng
- Chỉ gửi License Key, không gửi Hardware ID hoặc thông tin khác
- Secret key phải được bảo mật (có thể thay đổi trong source code nếu cần)

⚠️ **Sao lưu:**
- Nên lưu lại thông tin: Hardware ID, Company Name, Expiry Date, License Key
- Để dễ dàng hỗ trợ khách hàng khi cần

⚠️ **Trial mode:**
- Phần mềm có 30 ngày trial
- Sau 30 ngày, khách hàng phải nhập license để tiếp tục sử dụng
- License vĩnh viễn sẽ không bao giờ hết hạn

## Tác giả

**HMI Screwing Monitor License System v1.0**

Developed for industrial screwing device monitoring application.
