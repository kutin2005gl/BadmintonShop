BADMINTONSHOP — ĐỒ ÁN CÔNG NGHỆ PHẦN MỀM

Hệ thống bán đồ thể thao cầu lông gồm web khách hàng (ASP.NET Core MVC),
desktop quản lý (WinForms), thư viện Core và SQL Server, sử dụng .NET 8.

THÀNH VIÊN NHÓM
TODO(SV1): Điền họ tên, MSSV, lớp, giảng viên hướng dẫn và phân công nhóm.

CẤU TRÚC
- BadmintonShop.sln                  Solution ứng dụng và bộ kiểm tra hiện có.
- BadmintonShop.Core/                Models, repository và dịch vụ dùng chung.
- BadmintonShop.Web/                 Web khách hàng.
- BadmintonShop.Desktop/             Desktop quản lý.
- database/BadmintonShop.sql         Script tổng dùng cho gói nộp.
- docs/bao-cao/                      Tài liệu Markdown và Word.
- docs/bien-ban-hop/                 Biên bản theo tuần.
- tools/XuatBaoCao/                  Công cụ xuất Word.
- publish_web/, publish_desktop/     Kết quả publish Release trên máy đóng gói.
- BadmintonShop_demo.bak             Bản sao lưu đính kèm khi SV1 chốt gói nộp.

CÀI ĐẶT NHANH
1. Cài .NET 8 runtime phù hợp, SQL Server Express/Developer và sqlcmd/SSMS.
   Máy build cần SDK hỗ trợ .NET 8 và Windows desktop.
2. Trên máy cài mới, chạy database/BadmintonShop.sql bằng SSMS hoặc
   sqlcmd -S ".\SQLEXPRESS" -E -I -b -f 65001 -i database/BadmintonShop.sql
   Script tổng tạo lại CSDL cùng tên. Dùng bản script đổi tên CSDL để thử
   trên máy đang có dữ liệu cần giữ. File 05_SV4_Demo.sql dành cho demo riêng.
3. Đặt ConnectionStrings__BadmintonShop ở biến môi trường User của máy.
   Khởi động lại terminal/IDE để nhận cấu hình. Xem hướng dẫn để nhập giá trị.
4. Build bằng dotnet build BadmintonShop.sln hoặc mở solution trong VS.
5. Chạy Web bằng dotnet run --project BadmintonShop.Web --launch-profile http
   rồi mở http://localhost:5255. Chạy Desktop bằng
   dotnet run --project BadmintonShop.Desktop.
6. Với bản publish, chạy BadmintonShop.Desktop.exe trong publish_desktop,
   giữ BadmintonShop.Desktop.dll.config cạnh DLL. Chạy Web từ publish_web
   bằng dotnet BadmintonShop.Web.dll --urls http://localhost:5255.

Hướng dẫn chi tiết trong docs/bao-cao/huong-dan-cai-dat.md và bản .docx.
Hướng dẫn thao tác SV1 trong docs/bao-cao/huong-dan-su-dung-sv1.md và .docx.

TÀI KHOẢN MẪU
Ứng dụng | Tài khoản       | Mật khẩu     | Vai trò / trạng thái
Desktop  | admin           | Admin@123    | ADMIN, hoạt động
Desktop  | quanly          | Quanly@123   | QUANLY, hoạt động
Desktop  | kho             | Quanly@123   | QUANLY, hoạt động
Desktop  | nhanvien        | Nhanvien@123 | NHANVIEN, hoạt động
Desktop  | nhanvien2       | Nhanvien@123 | NHANVIEN, bị khóa
Web      | kh01@gmail.com  | Khach@123    | Khách hàng, hoạt động
Web      | kh20@gmail.com  | Khach@123    | Khách hàng, bị khóa

Nguồn tài khoản là 03_SampleData.sql trong script tổng.
Đây là tài khoản mẫu phục vụ chấm/demo đồ án.

DỮ LIỆU VÀ SAO LƯU
Kiểm tra script tổng ngày 12/11/2026 trên CSDL tạm cho kết quả
16 bảng, 3 trigger, 1 thủ tục, 40 sản phẩm, 23 khách hàng, 5 nhân viên,
11 phiếu nhập, 28 hóa đơn Online, 4 CTKM, 34 đánh giá trong DanhGiaSanPham,
0 sản phẩm tồn kho âm. Hóa đơn trải 5 tháng từ 06 đến 10/2026.
TODO(SV3): Phối hợp SV4 bổ sung dữ liệu hóa đơn trải 6–12 tháng.
TODO(SV4): Bổ sung hóa đơn tại quầy vào dữ liệu nộp và xác nhận phân bố tháng.
BadmintonShop_demo.bak tạo từ dữ liệu trên, đã RESTORE VERIFYONLY thành công.
SQL Server nguồn là Express 2025 (17.x), cần phiên bản tương thích để restore.
Máy SQL Server cũ hơn có thể cài từ script tổng.
TODO(SV1): Đính kèm .bak sau khi chốt dữ liệu và thử gói nộp trên máy sạch.
