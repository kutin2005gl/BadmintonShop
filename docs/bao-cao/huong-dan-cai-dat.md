# HƯỚNG DẪN CÀI ĐẶT

> Người viết: SV1. Bản này **thay cho** `docs/CNPM_02_Huong_dan_cai_dat.docx` (bản kế hoạch tuần 1). Đã cập nhật theo mã nguồn thực tế ngày 15/10/2026.
> Bản Word: [`huong-dan-cai-dat.docx`](huong-dan-cai-dat.docx), xuất bằng `node tools/XuatBaoCao/xuat-docx.js docs/bao-cao/huong-dan-cai-dat.md`.
> Rà soát tài liệu ngày 05/11/2026: đã sửa thứ tự SQL và thông tin tài khoản theo mã nguồn hiện có. Bản Word phía trên là bản cũ, chưa xuất lại trong lượt review này. SMTP vẫn cần cấu hình và kiểm chứng gửi thật.

Tài liệu dành cho: thành viên nhóm cài môi trường lập trình, giảng viên chạy thử chương trình, và làm phần "Hướng dẫn cài đặt" của báo cáo.

## 1. Yêu cầu hệ thống

| Thành phần | Yêu cầu | Ghi chú |
|---|---|---|
| Hệ điều hành | Windows 10/11 64-bit | Ứng dụng desktop (WinForms) chỉ chạy trên Windows |
| Phần cứng | CPU 4 nhân, RAM ≥ 8 GB, ổ đĩa trống ≥ 15 GB | SQL Server và Visual Studio khá nặng |
| .NET | .NET 8 SDK (hoặc SDK mới hơn) và .NET 8 Runtime | Có sẵn khi cài Visual Studio 2022; SDK 9/10 vẫn build được project .NET 8 |
| IDE | Visual Studio 2022 (17.8 trở lên), bản Community | Workload ASP.NET and web development + .NET desktop development |
| CSDL | SQL Server 2022 Express/Developer + SSMS, hoặc SQL Server LocalDB | LocalDB cài kèm Visual Studio, đủ để chạy đồ án |
| Git | 2.40 trở lên | Chỉ cần cho thành viên nhóm |
| Node.js | 18 trở lên (không bắt buộc) | Chỉ cần khi xuất lại tài liệu Word trong `docs/bao-cao` |
| Trình duyệt | Chrome hoặc Edge bản mới | Để dùng ứng dụng web |

## 2. Cài đặt phần mềm nền

### 2.1. Visual Studio 2022

- Tải Visual Studio 2022 Community tại visualstudio.microsoft.com.
- Trong Visual Studio Installer, chọn 2 workload **ASP.NET and web development** và **.NET desktop development**.
- Cài xong, mở Command Prompt, gõ `dotnet --list-sdks` và `dotnet --list-runtimes`. Phải có một SDK bản 8 trở lên và runtime `Microsoft.NETCore.App 8.x`, `Microsoft.WindowsDesktop.App 8.x`, `Microsoft.AspNetCore.App 8.x`.

### 2.2. SQL Server

Chọn một trong hai cách:

- **Cách 1 – SQL Server 2022 Express/Developer** (giống môi trường thật): tải tại microsoft.com/sql-server/sql-server-downloads. Ở bước Database Engine Configuration, chọn Mixed Mode và Add Current User. Ghi lại **tên server**: thường là `localhost` (Developer, kiểu Basic) hoặc `.\SQLEXPRESS` (Express). Sau đó cài thêm SQL Server Management Studio (SSMS).
- **Cách 2 – LocalDB** (nhẹ, có sẵn khi cài Visual Studio): tên server là `(localdb)\MSSQLLocalDB`. Kiểm tra bằng lệnh `sqllocaldb info`, nếu thấy `MSSQLLocalDB` là dùng được.

Sai tên server là lỗi kết nối hay gặp nhất khi chạy trên máy khác.

### 2.3. Git

Cài Git tại git-scm.com (giữ mặc định), rồi khai báo tên và email:

```
git config --global user.name "Ho Ten"
git config --global user.email "email@gmail.com"
```

## 3. Lấy mã nguồn

**Thành viên nhóm:**

```
cd D:\
git clone https://github.com/kutin2005gl/BadmintonShop.git
cd BadmintonShop
git checkout develop
```

**Giảng viên / máy chấm:** giải nén file nộp bài vào `D:\BadmintonShop`, mở `BadmintonShop.sln` bằng Visual Studio 2022.

Cấu trúc thư mục:

```
BadmintonShop\
├── BadmintonShop.sln
├── BadmintonShop.Core\       Thư viện dùng chung (SV1)
│   ├── Models\               Models cho lược đồ nền và bảng đánh giá bổ sung
│   ├── DataAccess\           DbHelper + các XxxRepository
│   └── Services\             MatKhauHelper, PhienDangNhap, PhanQuyen...
├── BadmintonShop.Web\        Web khách hàng – ASP.NET Core MVC (SV2, SV3)
│   └── appsettings.json      chuỗi kết nối của web
├── BadmintonShop.Desktop\    Desktop quản lý – WinForms (SV1, SV4)
│   └── App.config            chuỗi kết nối của desktop
├── database\
│   ├── BadmintonShop.sql     file tổng = 01 + 02 + 03 + 04 + 06 + 07 + 08
│   ├── 01_CreateDatabase.sql tạo CSDL, 15 bảng, khóa, ràng buộc, chỉ mục
│   ├── 02_Triggers.sql       trigger tồn kho, tổng tiền + thủ tục
│   ├── 03_SampleData.sql     dữ liệu mẫu + tài khoản test
│   └── 08_SV1_KhachHangHaNoi.sql 08: 3 khách hàng Hà Nội + 4 hóa đơn để demo tìm kiếm nâng cao (chứa ... OR ...)
├── tools\
│   ├── HashTool\             tạo chuỗi băm BCrypt cho mật khẩu mẫu
│   └── XuatBaoCao\           xuất tài liệu Markdown sang Word
└── docs\                     ERD, sơ đồ, chương báo cáo
```

## 4. Tạo cơ sở dữ liệu

Cách nhanh nhất: chạy **một file duy nhất** `database\BadmintonShop.sql` – bản nối sẵn của 01 → 02 → 03 → 04 → 06 → 07 → 08, dùng trên máy chưa có CSDL. Muốn chạy từng phần thì chạy đủ bảy script này **theo đúng thứ tự**. File 05 chỉ dành cho DB test/demo riêng, không thuộc chuỗi cài đặt thông thường.

Chú ý: file tổng (và file 01) **xóa và tạo lại** CSDL `BadmintonShop` nếu đã có. Nếu đang có dữ liệu cần giữ, hãy sao lưu trước (mục 7).

### 4.1. Cách 1 – bằng SSMS

Mở SSMS, kết nối tới server, mở file `database\BadmintonShop.sql` (File → Open → File…) và bấm Execute (F5). Nếu chạy từng phần thì mở và Execute lần lượt 01, 02, 03, 04, 06, 07, 08.

### 4.2. Cách 2 – bằng dòng lệnh sqlcmd

Mở Command Prompt tại thư mục `BadmintonShop`, thay `<server>` bằng tên server của máy (ví dụ `localhost`, `.\SQLEXPRESS`, `(localdb)\MSSQLLocalDB`):

```
sqlcmd -S "<server>" -E -I -b -f 65001 -i database\BadmintonShop.sql
```

Hoặc chạy từng phần:

```
sqlcmd -S "<server>" -E -I -b -f 65001 -i database\01_CreateDatabase.sql
sqlcmd -S "<server>" -E -I -b -f 65001 -i database\02_Triggers.sql
sqlcmd -S "<server>" -E -I -b -f 65001 -i database\03_SampleData.sql
sqlcmd -S "<server>" -E -I -b -f 65001 -i database\04_SV4_Migration.sql
sqlcmd -S "<server>" -E -I -b -f 65001 -i database\06_SV4_POS_Request.sql
sqlcmd -S "<server>" -E -I -b -f 65001 -i database\07_SV2_DanhGiaSanPham.sql
sqlcmd -S "<server>" -E -I -b -f 65001 -i database\08_SV1_KhachHangHaNoi.sql
```

Ý nghĩa các tham số: `-E` đăng nhập bằng tài khoản Windows; `-I` **bắt buộc** (bật QUOTED_IDENTIFIER, nếu thiếu sẽ lỗi khi tạo chỉ mục/cột tính); `-b` dừng ngay khi có lỗi; `-f 65001` đọc file UTF-8 để tiếng Việt không bị lỗi font.

### 4.3. Kiểm tra

Chạy câu lệnh sau, trên CSDL mới cài đủ script của repo phải có 16 bảng người dùng (15 bảng nền + DanhGiaSanPham). Tồn kho phải không âm; số sản phẩm còn hàng không bắt buộc bằng toàn bộ sản phẩm sau khi đã sử dụng hệ thống:

```
USE BadmintonShop;
SELECT COUNT(*) FROM sys.tables WHERE is_ms_shipped = 0; -- 16 trên DB mới
SELECT COUNT(*) FROM SanPham WHERE SoLuongTon < 0;    -- phải = 0
SELECT COUNT(*) FROM PhieuNhap WHERE TongTien > 0;    -- = số phiếu nhập
```

Mật khẩu trong dữ liệu mẫu đã được băm BCrypt sẵn. Khi cần tạo chuỗi băm cho mật khẩu mới (ví dụ thêm tài khoản mẫu), chạy `dotnet run --project tools/HashTool -- MatKhau@123` rồi chép chuỗi in ra vào script.

## 5. Cấu hình chuỗi kết nối

Hai ứng dụng đọc chuỗi kết nối tên **BadmintonShop** theo thứ tự ưu tiên:

1. Biến môi trường `ConnectionStrings__BadmintonShop` (nếu có).
2. File cấu hình: `BadmintonShop.Web\appsettings.json` cho web và `BadmintonShop.Desktop\App.config` cho desktop.

Mặc định 2 file cấu hình trỏ tới `Server=localhost`. Nếu máy dùng tên server khác, nên dùng **biến môi trường**: đặt một lần cho cả hai ứng dụng và không phải sửa file trong kho mã. Nhờ vậy các thành viên không ghi đè cấu hình của nhau khi commit.

### 5.1. Cách 1 – biến môi trường (khuyên dùng)

Mở Command Prompt, chạy một lệnh phù hợp với máy, rồi **đóng và mở lại Visual Studio**:

```
:: SQL Server Express
setx ConnectionStrings__BadmintonShop "Server=.\SQLEXPRESS;Database=BadmintonShop;Trusted_Connection=True;TrustServerCertificate=True;"

:: LocalDB
setx ConnectionStrings__BadmintonShop "Server=(localdb)\MSSQLLocalDB;Database=BadmintonShop;Trusted_Connection=True;TrustServerCertificate=True;"
```

### 5.2. Cách 2 – sửa file cấu hình

Sửa phần `Server=...` trong `appsettings.json` (web) và `App.config` (desktop). Ví dụ dùng tài khoản `sa` thay vì tài khoản Windows:

```
Server=localhost;Database=BadmintonShop;User Id=sa;Password=<mat khau sa>;TrustServerCertificate=True;
```

`TrustServerCertificate=True` là bắt buộc với thư viện Microsoft.Data.SqlClient khi kết nối SQL Server cục bộ, thiếu sẽ báo lỗi chứng chỉ. **Không commit** file cấu hình có mật khẩu `sa` lên GitHub.

## 6. Build và chạy chương trình

### 6.1. Bằng Visual Studio

- Mở `BadmintonShop.sln`, chờ Visual Studio tự khôi phục các gói NuGet.
- Build → Build Solution (Ctrl + Shift + B): 3 project phải build thành công.
- Chạy web: chuột phải `BadmintonShop.Web` → Set as Startup Project → F5. Trình duyệt mở ở https://localhost:7150.
- Chạy desktop: chuột phải `BadmintonShop.Desktop` → Set as Startup Project → F5. Màn hình đăng nhập hiện ra.
- Chạy cả hai cùng lúc (demo đặt hàng trên web rồi xử lý trên desktop): chuột phải Solution → Properties → Startup Project → Multiple startup projects → đặt Web và Desktop là Start.

### 6.2. Bằng dòng lệnh

```
dotnet build BadmintonShop.sln
dotnet run --project BadmintonShop.Web
:: mở cửa sổ khác
dotnet run --project BadmintonShop.Desktop
```

### 6.3. Tài khoản test

| Ứng dụng | Tài khoản | Mật khẩu | Dùng để demo |
|---|---|---|---|
| Desktop | admin | Admin@123 | Toàn quyền: nhân viên và tài khoản, sao lưu, mọi chức năng |
| Desktop | quanly | Quanly@123 | Sản phẩm, nhà cung cấp, phiếu nhập, đơn hàng, báo cáo |
| Desktop | kho | Quanly@123 | Vai trò Quản lý (nhân viên kho) |
| Desktop | nhanvien | Nhanvien@123 | Chỉ bán tại quầy (POS) và xử lý đơn hàng |
| Desktop | nhanvien2 | Nhanvien@123 | Tài khoản bị khóa – demo chặn đăng nhập |
| Web | kh01@gmail.com | Khach@123 | Khách hàng mua hàng, theo dõi đơn, đánh giá (đã có trong script 03 hiện tại) |
| Web | kh20@gmail.com | Khach@123 | Khách hàng bị khóa – demo chặn đăng nhập (đã có trong script 03 hiện tại) |

## 7. Sao lưu và phục hồi cơ sở dữ liệu

### 7.1. Trong ứng dụng desktop (tài khoản ADMIN)

- Đăng nhập `admin`, vào menu **Hệ thống → Sao lưu - phục hồi CSDL**.
- **Sao lưu:** chọn thư mục, bấm "Sao lưu ngay". File được đặt tên `BadmintonShop_yyyyMMdd_HHmmss.bak`.
- **Phục hồi:** chọn file `.bak`, bấm "Phục hồi" và xác nhận. Ứng dụng tự kiểm tra file, ngắt các kết nối khác, phục hồi và yêu cầu đăng nhập lại. File `.bak` tạo trên máy khác vẫn phục hồi được.

Đường dẫn thư mục là đường dẫn trên **máy chạy SQL Server**. Với SQL Server cài dạng dịch vụ, tài khoản dịch vụ phải có quyền ghi vào thư mục; đơn giản nhất là dùng thư mục Backup mặc định mà form tự gợi ý. LocalDB chạy bằng tài khoản Windows của người dùng nên ghi được vào thư mục bất kỳ của người đó.

### 7.2. Bằng SSMS hoặc câu lệnh T-SQL

```
-- Sao lưu
BACKUP DATABASE BadmintonShop TO DISK = 'D:\Backup\BadmintonShop_demo.bak' WITH INIT;

-- Phục hồi (chạy trên master)
USE master;
ALTER DATABASE BadmintonShop SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE BadmintonShop FROM DISK = 'D:\Backup\BadmintonShop_demo.bak' WITH REPLACE;
ALTER DATABASE BadmintonShop SET MULTI_USER;
```

Nếu CSDL chưa từng có trên máy, chỉ cần chạy lệnh `RESTORE DATABASE ... WITH REPLACE` (bỏ 2 lệnh `ALTER`). Nếu SQL Server báo đường dẫn file `.mdf`/`.ldf` không tồn tại, thêm `MOVE` cho từng file theo đường dẫn của máy mình, hoặc dùng chức năng phục hồi trong ứng dụng (tự xử lý việc này).

## 8. Đóng gói để nộp bài và demo

- **Web:** `dotnet publish BadmintonShop.Web -c Release -o publish_web`, chạy bằng `dotnet publish_web\BadmintonShop.Web.dll`. Sửa chuỗi kết nối trong `publish_web\appsettings.json` nếu không dùng biến môi trường.
- **Desktop:** `dotnet publish BadmintonShop.Desktop -c Release -r win-x64 --self-contained false -o publish_desktop`, chạy `publish_desktop\BadmintonShop.Desktop.exe`. Khi publish, `App.config` được đổi tên thành **`BadmintonShop.Desktop.dll.config`**; sửa chuỗi kết nối trong file này.
- **File nộp:** mã nguồn (đã xóa `bin\`, `obj\`, `node_modules\`), file SQL tổng cùng các script 01/02/03/04/06/07, file `BadmintonShop_demo.bak`, báo cáo (PDF và Word), `readme.txt` ghi thông tin nhóm và tài khoản test.
- Trước khi nộp, chạy thử trên một máy khác theo đúng tài liệu này.

## 9. Lỗi thường gặp

| Hiện tượng / thông báo lỗi | Nguyên nhân | Cách xử lý |
|---|---|---|
| Màn hình đăng nhập báo "Không kết nối được cơ sở dữ liệu"; A network-related or instance-specific error (error 26/40/50) | Sai tên server, hoặc dịch vụ SQL Server / LocalDB chưa chạy | Kiểm tra tên server trong chuỗi kết nối (mục 5); mở services.msc khởi động SQL Server; với LocalDB chạy `sqllocaldb start MSSQLLocalDB` |
| The certificate chain was issued by an authority that is not trusted | Thiếu TrustServerCertificate | Thêm `TrustServerCertificate=True;` vào chuỗi kết nối |
| Đã đặt biến môi trường nhưng ứng dụng vẫn dùng chuỗi cũ | Visual Studio mở trước khi đặt biến | Đóng hẳn Visual Studio (và cửa sổ lệnh) rồi mở lại |
| Login failed for user… | Sai mật khẩu sa, hoặc SQL Server chỉ bật Windows Authentication | Bật Mixed Mode trong Server Properties → Security, khởi động lại dịch vụ |
| Cannot open database "BadmintonShop" requested by the login | Chưa tạo CSDL | Cài đủ script theo mục 4 trên DB mới; với DB có dữ liệu chỉ bổ sung migration phù hợp |
| CREATE INDEX failed because … SET options have incorrect settings: 'QUOTED_IDENTIFIER' | Chạy sqlcmd thiếu tham số `-I` | Chạy lại với `-I` (mục 4.2) |
| Chữ tiếng Việt thành ký tự lạ (?? hoặc Ã¡…) | sqlcmd đọc file sai bảng mã, hoặc chèn chuỗi thiếu tiền tố N | Thêm `-f 65001` khi chạy sqlcmd; chuỗi tiếng Việt viết dạng `N'Vợt'` |
| San pham khong du ton kho | Trigger chặn bán quá tồn kho – đúng như thiết kế | Nhập thêm hàng hoặc giảm số lượng |
| The INSERT statement conflicted with the FOREIGN KEY constraint | Thêm dữ liệu sai thứ tự bảng | Thêm bảng cha trước: DanhMuc → SanPham, NhanVien → TaiKhoan, HoaDon → ChiTietHoaDon |
| Sao lưu báo Operating system error 5 (Access is denied) | Tài khoản dịch vụ SQL Server không có quyền ghi thư mục | Dùng thư mục Backup mặc định của SQL Server, hoặc cấp quyền ghi cho thư mục |
| NU1101: Unable to find package … | Chưa khôi phục NuGet hoặc máy không có mạng | Chạy `dotnet restore` khi có mạng |
| Đăng nhập đúng mật khẩu mẫu vẫn báo sai | Script 03 còn chuỗi `<HASH_...>` chưa thay | Chạy `dotnet run --project tools/HashTool -- --patch database/03_SampleData.sql` rồi chạy lại file 03 |

## 10. Nâng cấp CSDL đã có và cấu hình email

Với CSDL có dữ liệu cần giữ: sao lưu, kiểm tra schema rồi áp dụng riêng 04 → 06 → 07 khi còn thiếu; không chạy lại 01 hoặc file tổng vì có DROP/CREATE DATABASE. Bảng đánh giá được Web/Desktop sử dụng là `DanhGiaSanPham`; bảng `PhanHoi` nền vẫn được giữ.

`EmailService.CauHinh` đã được Web đọc từ mục `Email`: `Bat`, `Host`, `Port`, `TaiKhoan`, `MatKhau`, `TenNguoiGui`. Đặt mật khẩu ngoài Git (user-secrets hoặc biến môi trường `Email__MatKhau`). Khi thiếu cấu hình hoặc lỗi SMTP, gửi xác nhận đơn trả false; không được báo đã kiểm chứng email thật. Quy trình OTP tài khoản cần đối chiếu riêng, không suy từ email đơn hàng.

## Bổ sung cấu hình bộ nhớ và demo — đợt 2

TTL mặc định tạm 10 phút, có thể đặt biến môi trường BadmintonShop__CacheMinutes (1–60). Các tiến trình cùng user dùng tín hiệu thay đổi trong thư mục CacheSignals dưới LocalApplicationData/BadmintonShop; nếu khác user/máy, đặt BadmintonShop__CacheSignalRoot tới thư mục chung có quyền đọc/ghi và dùng cùng tên server/catalog. Không thay chuỗi kết nối thật để chạy kiểm tra: cờ --server của bộ kiểm tra chỉ tạo DB có tên mới. Xem [lệnh tái lập, DB nguyên mẫu và giới hạn](quy-tac-va-tien-do-dot-2.md).

## Tiếp tục nghiệm thu — đợt 3

Xem [đợt 3](quy-tac-va-tien-do-dot-3.md) để chạy cờ --acceptance trên DB thử mới và phân biệt test host Desktop với ứng dụng sản phẩm. Setup acceptance cần cả DDL migration 07 (DanhGiaSanPham) để mở trang chi tiết sản phẩm. Không chạy phần USE BadmintonShop hoặc dữ liệu mẫu của migration vào DB cửa hàng khi đang nghiệm thu DB thử. Phần đầu script tạo DB vẫn chỉ được dùng cho DB mới.

Đã mở XLSX bằng Excel và PDF bằng Chrome; kịch bản bảo vệ ở [đây](kich-ban-bao-ve-dot-3.md). Luồng bấm xử lý Desktop/MessageBox từ đơn Web còn cần chạy tiếp sau khi Computer Use bị dừng bằng Escape. Không sửa cấu hình kết nối để dùng các chế độ tests.
