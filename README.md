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
- Sau tuần 1: chỉ được **thêm** bảng/cột, không đổi tên hay xóa cột đã có.
