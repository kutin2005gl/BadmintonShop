# BadmintonShop – Hệ thống bán đồ thể thao cầu lông

Đồ án môn Công nghệ phần mềm – ĐH Sài Gòn, HK1 2026–2027 (nhóm 4 người).

- **BadmintonShop.Core** – Class Library dùng chung: Models, DataAccess (ADO.NET), Services (SV1)
- **BadmintonShop.Web** – ASP.NET Core MVC cho khách hàng (SV2, SV3)
- **BadmintonShop.Desktop** – WinForms cho nhân viên / quản lý (SV1, SV4)
- **database/** – `01_CreateDatabase.sql` → `02_Triggers.sql` → `03_SampleData.sql`
- **docs/** – ERD, phân công, hướng dẫn cài đặt

## Chạy nhanh

1. SSMS: chạy lần lượt 3 file trong `database/`.
2. Sửa `Server=` trong `BadmintonShop.Web/appsettings.json` và `BadmintonShop.Desktop/App.config` cho khớp máy mình (`localhost` hoặc `.\SQLEXPRESS`). **Không commit** phần sửa này nếu có mật khẩu.
3. Mở `BadmintonShop.sln` → Build → chạy Web hoặc Desktop.

Tài khoản test: `admin / Admin@123`, `quanly / Quanly@123`, `nhanvien / Nhanvien@123` (desktop).

## Quy tắc Git

- `main`: bản ổn định để nộp/demo. `develop`: nhánh tích hợp. Mỗi người làm trên `feature/svX-ten-chuc-nang` tách từ `develop`.
- Mọi thay đổi vào `develop` qua **Pull Request**; SV1 duyệt (SV4 duyệt thay khi SV1 bận).
- Commit nhỏ, rõ: `feat(sv3): ap ma giam gia theo voucher`, `fix(sv1): ...`, `docs(sv2): ...`.
- **Không viết SQL trong Controller/Form** – chỉ viết trong Repository của Core.
- Sau tuần 1: chỉ được **thêm** bảng/cột, không đổi tên hay xóa cột đã có. Lược đồ chốt: [`docs/ERD.md`](docs/ERD.md).

## Quy tắc viết Repository (Core/DataAccess)

- **Mỗi người tự tạo `XxxRepository.cs` cho bảng mình phụ trách**, copy khuôn `NhaCungCapRepository.cs`:
  hằng `Cot`, SQL có tham số `@ten` (không cộng chuỗi), 1 hàm `Map(DataRow)` riêng.
- Đã có sẵn (SV1): `NhaCungCap`, `NhanVien`, `TaiKhoan`, `PhieuNhap` (+ chi tiết, lưu bằng transaction),
  và phần **đọc** dùng chung `SanPhamRepository.LayTatCa/LayTheoMa`, `DanhMucRepository.LayTatCa/LayTheoMa`.
  SV2/SV4 **thêm hàm ghi** (Them/Sua/Xoa) vào chính 2 file này, không tạo file trùng.
- Ghi nhiều bảng cùng lúc (HoaDon + ChiTietHoaDon) → dùng `DbHelper.ExecuteTransaction`, xem mẫu `PhieuNhapRepository.Them`.
- Tồn kho và `TongTien` do **trigger** cập nhật – code không tự cộng/trừ. Không đủ tồn → trigger ném lỗi, bắt `SqlException` để báo người dùng.
- Mỗi hàm có **1 dòng comment ghi người làm + chức năng**, ví dụ: `// TV2: lay san pham theo thuong hieu`.
- Cần DataGridView có cột tên (JOIN)? Viết thêm hàm `LayDanhSachHienThi()` trả `DataTable`, giữ Model đúng 1–1 với bảng.

## Chuỗi kết nối trên máy mình

Không muốn sửa file config? Đặt biến môi trường **User** `ConnectionStrings__BadmintonShop`
(vd `Server=(localdb)\MSSQLLocalDB;Database=BadmintonShop;Trusted_Connection=True;TrustServerCertificate=True;`),
mở lại Visual Studio – cả Web và Desktop đều ưu tiên biến này.
Chạy script bằng `sqlcmd` nhớ thêm `-I` (bật QUOTED_IDENTIFIER), vd `sqlcmd -S . -E -I -f 65001 -i database\01_CreateDatabase.sql`.
