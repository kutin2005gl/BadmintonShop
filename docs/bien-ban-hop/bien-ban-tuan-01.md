# Biên bản họp nhóm – Tuần 01

> Mẫu dùng chung: tuần sau copy file này thành `bien-ban-tuan-02.md`, `bien-ban-tuan-03.md`... rồi điền lại.
> Phần SV1 điền theo lịch sử git (`git log`), phần SV2–SV4 do từng bạn tự điền.

- **Tuần:** 01 (kế hoạch 06/10 → 12/10/2026)
- **Ngày họp:** ......./......./2026 · **Giờ:** ............ · **Hình thức:** trực tiếp / online
- **Người ghi biên bản:** SV1 – Huỳnh Phúc Hoàng Nhật (nhóm trưởng)

## 1. Thành viên dự họp

| Vai trò | Họ tên | Có mặt |
|---|---|---|
| SV1 – Nhóm trưởng | Huỳnh Phúc Hoàng Nhật | ☑ |
| SV2 | | ☐ |
| SV3 | | ☐ |
| SV4 | | ☐ |

## 2. Báo cáo từng thành viên

### SV1 – Huỳnh Phúc Hoàng Nhật

**Đã làm** (commit trên `develop`, ngày ghi trong git: 15/10/2026):

- Khởi tạo repo, solution 3 project (Core / Web / Desktop), Web và Desktop dùng chung `BadmintonShop.Core`.
- CSDL: script tạo 15 bảng + trigger tồn kho/tổng tiền + dữ liệu mẫu phần SV1; chốt lược đồ, cập nhật `docs/ERD.md` khớp script.
- Repository `NhanVien`, `TaiKhoan`, `PhieuNhap`, `SanPham`, `DanhMuc` (PR #1 `feature/sv1-core` đã merge).
- Đăng nhập, phân quyền, đổi mật khẩu, đăng xuất Desktop; cơ chế đăng nhập Web khách hàng (Cookie Authentication).
- Form quản lý nhà cung cấp + `KiemTraDuLieu` (nhánh `feature/sv1-ncc` đã merge).
- Form quản lý nhân viên & tài khoản (cấp TK, đổi vai trò, khóa, reset mật khẩu).
- Lập phiếu nhập hàng + danh sách phiếu nhập (lọc ngày/NCC, xem chi tiết).
- Sao lưu – phục hồi CSDL (BACKUP/RESTORE).
- `LoiCsdl` (đổi lỗi SQL thành câu tiếng Việt), `QuyenForm` (form tự kiểm quyền khi mở), `MoForm` kiểm quyền theo `ChucNang`.
- Tài liệu: chương Kiến trúc hệ thống, mô tả 15 bảng CSDL, hướng dẫn cài đặt, đặc tả 6 use case + 36 test case phần SV1 (md + docx); sơ đồ kiến trúc, ERD, lược đồ quan hệ.
- Gộp script CSDL thành 1 file bàn giao `database/BadmintonShop.sql`.

**Sẽ làm (tuần 02):**

- Review + merge PR của SV2/SV3/SV4 trong 24h.
- Hỗ trợ SV3 ghi HoaDon + CTHD trong 1 transaction; chốt 5 trạng thái đơn hàng với SV3 + SV4.
- Gộp dữ liệu mẫu của cả nhóm vào `03_SampleData.sql` (ghép lại `BadmintonShop.sql`).

**Vướng mắc:**

- Chưa có nhánh/PR nào của SV2, SV3, SV4 trên GitHub → chưa ghép thử được phần của nhóm.
- Repo mới có 2 collaborator (`congvinh0302`, `minh2255`) → còn 1 bạn chưa được mời / chưa nhận lời mời.
- Chờ cả nhóm xác nhận lược đồ CSDL (từ giờ chỉ thêm bảng/cột, không đổi tên/xóa cột).

### SV2 – ......................

**Đã làm:**

-

**Sẽ làm (tuần 02):**

-

**Vướng mắc:**

-

### SV3 – ......................

**Đã làm:**

-

**Sẽ làm (tuần 02):**

-

**Vướng mắc:**

-

### SV4 – ......................

**Đã làm:**

-

**Sẽ làm (tuần 02):**

-

**Vướng mắc:**

-

## 3. Quyết định của nhóm

| # | Nội dung quyết định | Người phụ trách | Hạn chót |
|---|---|---|---|
| 1 | | | |
| 2 | | | |
| 3 | | | |

## 4. Xác nhận

Biên bản đã được đọc lại cuối buổi họp và các thành viên đồng ý với nội dung trên.
