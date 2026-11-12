# HƯỚNG DẪN CÀI ĐẶT

> Người viết: SV1. Bản này **thay cho** `docs/CNPM_02_Huong_dan_cai_dat.docx` (bản kế hoạch tuần 1). Rà soát theo mã nguồn và kiểm tra đóng gói ngày 12/11/2026.
> Bản Word: [`huong-dan-cai-dat.docx`](huong-dan-cai-dat.docx), xuất bằng `node tools/XuatBaoCao/xuat-docx.js docs/bao-cao/huong-dan-cai-dat.md`.
> Bản Word được xuất lại trong lượt đóng gói 12/11/2026. Cấu hình email và kiểm chứng gửi thật chờ SV3.

Tài liệu dành cho: thành viên nhóm cài môi trường lập trình, giảng viên chạy thử chương trình, và làm phần "Hướng dẫn cài đặt" của báo cáo.

## 1. Yêu cầu hệ thống

| Thành phần | Yêu cầu | Ghi chú |
|---|---|---|
| Hệ điều hành | Windows 10/11 64-bit | Ứng dụng desktop (WinForms) chỉ chạy trên Windows |
| Phần cứng | CPU 4 nhân, RAM ≥ 8 GB, ổ đĩa trống ≥ 15 GB | SQL Server và Visual Studio khá nặng |
| .NET | .NET 8 SDK (hoặc SDK mới hơn) và .NET 8 Runtime | Có sẵn khi cài Visual Studio 2022; SDK 9/10 vẫn build được project .NET 8 |
| IDE | Visual Studio 2022 (17.8 trở lên), bản Community | Workload ASP.NET and web development + .NET desktop development |
| CSDL | SQL Server Express/Developer + SSMS | Máy đóng gói dùng `.\SQLEXPRESS`, phiên bản 17.x. Cài từ SQL tổng cho máy SQL Server cũ hơn; restore .bak cần phiên bản tương thích |
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

- **Cách 1 – SQL Server Express/Developer** (máy đóng gói dùng `.\SQLEXPRESS`): tải tại microsoft.com/sql-server/sql-server-downloads. Ở bước Database Engine Configuration, chọn Mixed Mode và Add Current User. Ghi lại **tên server**: thường là `localhost` (Developer, kiểu Basic) hoặc `.\SQLEXPRESS` (Express). Sau đó cài thêm SQL Server Management Studio (SSMS).
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
│   └── appsettings.json      cấu hình web, máy dùng biến môi trường User
├── BadmintonShop.Desktop\    Desktop quản lý – WinForms (SV1, SV4)
│   └── App.config            cấu hình desktop, máy dùng biến môi trường User
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

Chú ý, file tổng và file 01 **xóa và tạo lại** CSDL `BadmintonShop` nếu đã có. Chạy các lệnh cài mới dưới đây trên máy chưa có CSDL cùng tên. Trên máy đang sử dụng, tạo bản script đổi tên CSDL theo mục 4.4 để kiểm tra trong CSDL riêng, rồi áp dụng migration sau khi đã rà soát.

### 4.1. Cách 1 – bằng SSMS

Mở SSMS, kết nối tới `.\SQLEXPRESS` hoặc instance của máy cài mới, mở file `database\BadmintonShop.sql` (File → Open → File…) và bấm Execute (F5). Nếu chạy từng phần thì mở và Execute lần lượt 01, 02, 03, 04, 06, 07, 08.

### 4.2. Cách 2 – bằng dòng lệnh sqlcmd

Mở Command Prompt tại thư mục `BadmintonShop`. Các lệnh dưới đây dùng instance `.\SQLEXPRESS`, thay tên instance theo máy cài đặt khi cần.

```
sqlcmd -S ".\SQLEXPRESS" -E -I -b -f 65001 -i database\BadmintonShop.sql
```

Hoặc chạy từng phần:

```
sqlcmd -S ".\SQLEXPRESS" -E -I -b -f 65001 -i database\01_CreateDatabase.sql
sqlcmd -S ".\SQLEXPRESS" -E -I -b -f 65001 -i database\02_Triggers.sql
sqlcmd -S ".\SQLEXPRESS" -E -I -b -f 65001 -i database\03_SampleData.sql
sqlcmd -S ".\SQLEXPRESS" -E -I -b -f 65001 -i database\04_SV4_Migration.sql
sqlcmd -S ".\SQLEXPRESS" -E -I -b -f 65001 -i database\06_SV4_POS_Request.sql
sqlcmd -S ".\SQLEXPRESS" -E -I -b -f 65001 -i database\07_SV2_DanhGiaSanPham.sql
sqlcmd -S ".\SQLEXPRESS" -E -I -b -f 65001 -i database\08_SV1_KhachHangHaNoi.sql
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

### 4.4. Kiểm tra script trên CSDL tạm

Trong PowerShell tại gốc repo, tạo bản script chỉ đổi tên CSDL, giữ nguyên dữ liệu mẫu.

```powershell
$sqlDemo = Get-Content -Encoding UTF8 database/BadmintonShop.sql -Raw
$sqlDemo = [regex]::Replace($sqlDemo, '\bBadmintonShop\b', 'BadmintonShop_GptTmp')
[IO.File]::WriteAllText("$env:TEMP\BadmintonShop_GptTmp.sql", $sqlDemo)
sqlcmd -S ".\SQLEXPRESS" -E -I -b -f 65001 -i "$env:TEMP\BadmintonShop_GptTmp.sql"
```

Trước khi chạy, kiểm tra tên `BadmintonShop_GptTmp` chưa được ai sử dụng vì bản script cũng có DROP/CREATE cho tên đã thay. Kiểm chứng ngày 12/11/2026 trên CSDL tạm cho kết quả 16 bảng, 3 trigger, 1 thủ tục, 40 sản phẩm, 23 khách hàng, 5 nhân viên, 11 phiếu nhập, 28 hóa đơn Online, 4 CTKM, 34 đánh giá trong `DanhGiaSanPham`, 0 sản phẩm tồn kho âm. `kh01@gmail.com` hoạt động, `kh20@gmail.com` bị khóa. Hóa đơn từ 22/06 đến 20/10/2026, gồm 5 tháng có dữ liệu.

TODO(SV3): Phối hợp SV4 bổ sung hóa đơn trải 6–12 tháng theo tiêu chí dữ liệu nộp.

TODO(SV4): Bổ sung hóa đơn tại quầy vào dữ liệu nộp và kiểm tra lại phân bố tháng.

## 5. Cấu hình chuỗi kết nối

Hai ứng dụng đọc chuỗi kết nối tên **BadmintonShop** theo thứ tự ưu tiên:

1. Biến môi trường `ConnectionStrings__BadmintonShop` (nếu có).
2. File cấu hình: `BadmintonShop.Web\appsettings.json` cho web và `BadmintonShop.Desktop\App.config` cho desktop.

Mặc định 2 file cấu hình trỏ tới `Server=localhost`. Nếu máy dùng tên server khác, nên dùng **biến môi trường**: đặt một lần cho cả hai ứng dụng và không phải sửa file trong kho mã. Nhờ vậy các thành viên không ghi đè cấu hình của nhau khi commit.

### 5.1. Đặt biến môi trường User

Mở **Edit environment variables for your account** trong Windows. Tại **User variables**, tạo biến `ConnectionStrings__BadmintonShop` với giá trị kết nối của máy. Các thành phần cho Windows Authentication được điền theo bảng sau, giá trị hoàn chỉnh được lưu tại máy cài đặt.

| Thành phần | Giá trị trên máy dùng SQLEXPRESS |
|---|---|
| Server | .\SQLEXPRESS |
| Database | BadmintonShop hoặc tên CSDL demo riêng |
| Trusted_Connection | True |
| TrustServerCertificate | True |

Đóng và mở lại Visual Studio hoặc terminal sau khi đặt biến.

### 5.2. Cấu hình riêng cho máy đóng gói

Máy hiện tại lấy cấu hình từ biến môi trường **User** `ConnectionStrings__BadmintonShop`. Để terminal đang mở nhận giá trị đã lưu, dùng PowerShell.

```powershell
$env:ConnectionStrings__BadmintonShop = [Environment]::GetEnvironmentVariable(
    'ConnectionStrings__BadmintonShop', 'User')
```

Giá trị thực và thông tin đăng nhập CSDL được quản lý ngoài kho mã. Với bản publish, đặt biến môi trường cho tài khoản Windows chạy ứng dụng, rồi mở lại tiến trình. Giữ file `BadmintonShop.Desktop.dll.config` cạnh DLL của desktop.

Hai ứng dụng có thể dùng `BadmintonShop__MediaRoot` trỏ đến thư mục ảnh dùng chung. Khi cài bản publish trên máy khác, chép thư mục ảnh và đặt cùng thư mục gốc cho Web, Desktop để các khóa `products/...` tìm đúng file.

## 6. Build và chạy chương trình

### 6.1. Bằng Visual Studio

- Mở `BadmintonShop.sln`, chờ Visual Studio tự khôi phục các gói NuGet.
- Build → Build Solution (Ctrl + Shift + B): các project phải build thành công, gồm bộ kiểm tra SV4 đang có trong solution.
- Chạy web: chuột phải `BadmintonShop.Web` → Set as Startup Project → F5. Chọn profile `http` để mở http://localhost:5255 hoặc profile `https` để mở https://localhost:7150.
- Chạy desktop: chuột phải `BadmintonShop.Desktop` → Set as Startup Project → F5. Màn hình đăng nhập hiện ra.
- Chạy cả hai cùng lúc (demo đặt hàng trên web rồi xử lý trên desktop): chuột phải Solution → Properties → Startup Project → Multiple startup projects → đặt Web và Desktop là Start.

### 6.2. Bằng dòng lệnh

```
dotnet build BadmintonShop.sln
dotnet run --project BadmintonShop.Web --launch-profile http
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

- Đăng nhập `admin`, trong nhóm **HỆ THỐNG** ở thanh điều hướng trái chọn **Sao lưu, phục hồi**.
- **Sao lưu:** chọn thư mục, bấm "Sao lưu ngay". File được đặt tên `BadmintonShop_yyyyMMdd_HHmmss.bak`.
- **Phục hồi:** chọn file `.bak`, bấm "Phục hồi" và xác nhận. Ứng dụng tự kiểm tra file, ngắt các kết nối khác, phục hồi và yêu cầu đăng nhập lại. Bản sao lưu cần tương thích phiên bản SQL Server đích.

Đường dẫn thư mục là đường dẫn trên **máy chạy SQL Server**. Với SQL Server cài dạng dịch vụ, tài khoản dịch vụ phải có quyền ghi vào thư mục; đơn giản nhất là dùng thư mục Backup mặc định mà form tự gợi ý. LocalDB chạy bằng tài khoản Windows của người dùng nên ghi được vào thư mục bất kỳ của người đó.

### 7.2. File sao lưu demo và kiểm chứng

Ngày 12/11/2026 đã tạo `BadmintonShop_demo.bak` từ CSDL tạm cài bằng script tổng, dùng `BACKUP DATABASE [BadmintonShop_GptTmp] ... WITH COPY_ONLY, CHECKSUM`. `RESTORE VERIFYONLY ... WITH CHECKSUM` trả về “The backup set on file 1 is valid.”.

File được lưu ở thư mục **Backup mặc định của instance SQL Server**. SV1 lấy đường dẫn thực và dung lượng từ báo cáo đóng gói để chép vào gói nộp. Bản .bak mang tên CSDL nguồn `BadmintonShop_GptTmp`. Khi phục hồi sang máy demo, chọn tên CSDL đích và ánh xạ file dữ liệu bằng `WITH MOVE`.

```sql
-- Chạy bằng sqlcmd -S ".\SQLEXPRESS" -E -I -b -f 65001 -i <file>
-- Thay <thu-muc-Backup> bằng thư mục Backup mặc định của instance.
RESTORE VERIFYONLY FROM DISK = N'<thu-muc-Backup>\BadmintonShop_demo.bak'
WITH CHECKSUM;
RESTORE FILELISTONLY FROM DISK = N'<thu-muc-Backup>\BadmintonShop_demo.bak';
-- Dùng LogicalName vừa đọc và đường dẫn dữ liệu của máy đích.
RESTORE DATABASE [BadmintonShop_Demo]
FROM DISK = N'<thu-muc-Backup>\BadmintonShop_demo.bak'
WITH MOVE N'BadmintonShop_GptTmp' TO N'<thu-muc-du-lieu>\BadmintonShop_Demo.mdf',
     MOVE N'BadmintonShop_GptTmp_log' TO N'<thu-muc-du-lieu>\BadmintonShop_Demo_log.ldf';
```

Dùng tên đích `BadmintonShop_Demo` chưa tồn tại, rồi cấu hình biến môi trường trỏ tới CSDL đó để thử trên máy demo. SQL Server nguồn là 17.x, cần kiểm tra tương thích trước khi restore lên máy khác. Dữ liệu trong bản sao lưu hiện có hóa đơn trải 5 tháng, cần SV3/SV4 hoàn thiện theo mục 4.4.

## 8. Đóng gói để nộp bài và demo

Các lệnh đã dùng thành công ngày 12/11/2026 từ gốc repo.

```powershell
dotnet publish BadmintonShop.Web/BadmintonShop.Web.csproj -c Release -o publish_web --nologo
dotnet publish BadmintonShop.Desktop/BadmintonShop.Desktop.csproj -c Release -o publish_desktop --nologo
```

- **Web**, mở terminal tại `publish_web`, chạy `dotnet BadmintonShop.Web.dll --urls http://localhost:5255`. Đặt cấu hình kết nối qua biến môi trường User.
- **Desktop**, chạy `publish_desktop\BadmintonShop.Desktop.exe`, giữ `BadmintonShop.Desktop.dll.config` cạnh DLL. Bản publish framework-dependent cần .NET 8 Desktop Runtime. Web cần ASP.NET Core Runtime 8.
- **File nộp**, mã nguồn, `database/BadmintonShop.sql`, bản sao lưu demo, báo cáo Word/PDF đã duyệt và `readme.txt`. Nếu đính kèm script rời, dùng 01/02/03/04/06/07/08 theo đúng thứ tự. Lọc `bin/`, `obj/`, `node_modules/` và `database/05_SV4_Demo.sql` khi tạo bản sao đóng gói.
- **Ảnh sản phẩm**, mang theo thư mục ảnh dùng chung và thiết lập `BadmintonShop__MediaRoot` theo mục 5.2.
- **Thông tin nhóm**, điền TODO trong `readme.txt`, biên bản và bảng tự đánh giá trước khi nộp.

TODO(SV1): Chốt dữ liệu, ghép báo cáo cuối, tạo gói nộp và chạy thử trên máy sạch.

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
| Đăng nhập đúng mật khẩu mẫu vẫn báo sai | Script 03 còn chuỗi `<HASH_...>` chưa thay | Đối chiếu script tổng và mật khẩu mẫu; với DB đang dùng, thực hiện đổi/reset mật khẩu qua ứng dụng |

## 10. Nâng cấp CSDL đã có và cấu hình email

Với CSDL có dữ liệu cần giữ: sao lưu, kiểm tra schema rồi áp dụng riêng 04 → 06 → 07 khi còn thiếu; không chạy lại 01 hoặc file tổng vì có DROP/CREATE DATABASE. Bảng đánh giá được Web/Desktop sử dụng là `DanhGiaSanPham`; bảng `PhanHoi` nền vẫn được giữ.

TODO(SV3): Hoàn thiện hướng dẫn cấu hình email qua biến môi trường hoặc user-secrets, ghi các khóa cần dùng và bằng chứng gửi email xác nhận đơn thực tế. Xác nhận luồng OTP tài khoản riêng theo môi trường chạy.

## 11. Cấu hình bộ nhớ đệm tìm kiếm

Dữ liệu tìm kiếm được giữ trong bộ nhớ đệm, mặc định 10 phút. Biến môi trường `BadmintonShop__CacheMinutes` nhận giá trị từ 1 đến 60. Các tiến trình cùng tài khoản Windows chia sẻ tín hiệu thay đổi trong thư mục CacheSignals dưới LocalApplicationData/BadmintonShop. Với các tiến trình khác tài khoản hoặc khác máy, đặt `BadmintonShop__CacheSignalRoot` tới thư mục chung có quyền đọc/ghi và dùng cùng tên server/catalog.
