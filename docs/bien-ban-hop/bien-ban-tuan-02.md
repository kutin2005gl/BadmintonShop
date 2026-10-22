# Biên bản họp nhóm – Tuần 02

> Phần SV1 điền theo lịch sử git và kết quả review; phần SV3, SV4 điền theo commit đã có trên `develop` (nên mỗi bạn đọc lại và sửa cho đúng). Phần SV2 do bạn tự điền.

- **Tuần:** 02 (kế hoạch 13/10 → 19/10/2026)
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

**Đã làm:**

- Review PR #2 (SV4, `feature/sv4-desktop`) và PR #3 (SV3, `feature/sv3-khuyenmai`) đã merge: build `BadmintonShop.sln` 0 lỗi 0 cảnh báo; không có SQL trong Form/Controller; form Desktop nạp dữ liệu trong `OnLoad` + kiểm quyền; không commit `bin/obj`.
- Gộp `04_SV4_Migration.sql` vào `database/BadmintonShop.sql` (01+02+03+04), chạy thử trên CSDL trống không lỗi: 15 bảng, 3 trigger, 24 hóa đơn, 4 CTKM, tồn kho không âm.

**Sẽ làm (tuần 03):**

- Hẹn buổi chạy thử Web + Desktop (đặt hàng web → xác nhận desktop → kiểm tồn kho).
- Chờ SV2 giao diện đăng nhập web + dữ liệu mẫu rồi gộp lại, tạo `BadmintonShop_demo.bak`.
- Nhận báo cáo kiểm thử chéo phần SV1 từ SV4.

**Vướng mắc:**

- SV2 chưa có commit nào trên GitHub (đăng nhập web, dữ liệu KH/SP/phản hồi, duyệt bình luận).

### SV2 – ......................

**Đã làm:**

-

**Sẽ làm (tuần 03):**

-

**Vướng mắc:**

-

### SV3 – ......................

**Đã làm:** (theo git: PR #3 ngày 21/10)

- Giỏ hàng, đặt hàng (COD/Online), danh sách + chi tiết đơn, khách hủy đơn khi còn "Chờ xác nhận".
- Core: `HoaDonRepository`, `DonHangOnlineRepository`, `KhuyenMaiRepository`, `GioHangService`, `TinhKhuyenMaiService`, `QuyTacBanHang`.
- Dữ liệu mẫu: 4 CTKM, 24 đơn online đủ 5 trạng thái.

**Sẽ làm (tuần 03):**

- Cấu hình gửi email trong `appsettings.json`; form Khuyến mãi Desktop (`mnuKhuyenMai`).

**Vướng mắc:**

-

### SV4 – ......................

**Đã làm:** (theo git: PR #2, commit 18/10)

- Desktop: Sản phẩm, Danh mục, POS, Đơn hàng, Thống kê; nối menu `FrmMain`.
- `04_SV4_Migration.sql`, `05_SV4_Demo.sql`; project kiểm tra tự động (35 kiểm tra đạt); tài liệu đặc tả, kiểm thử, hướng dẫn sử dụng, tổng kết.

**Sẽ làm (tuần 03):**

- Kiểm thử chéo phần SV1 (36 TC trong `docs/bao-cao/dac-ta-sv1.md`, 12 TC giao diện còn "–") và gửi danh sách lỗi.
- Dữ liệu mẫu hóa đơn tại quầy trải 6–12 tháng trong `03_SampleData.sql`.

**Vướng mắc:**

-

## 3. Quyết định của nhóm

| # | Nội dung quyết định | Người phụ trách | Hạn chót |
|---|---|---|---|
| 1 | Dùng đúng 5 trạng thái đơn: Chờ xác nhận → Đã xác nhận → Đang giao → Hoàn thành / Đã hủy (`QuyTacBanHang`) | SV3 + SV4 | đã áp dụng |
| 2 | | | |
| 3 | | | |

## 4. Xác nhận

Biên bản đã được đọc lại cuối buổi họp và các thành viên đồng ý với nội dung trên.
