# Sơ đồ hoạt động – Đăng nhập Desktop

Nhân viên nhập tên đăng nhập và mật khẩu trên `FrmDangNhap`. Form kiểm tra ô trống, thử kết nối CSDL (`DbHelper.ThuKetNoi`), rồi gọi `TaiKhoanRepository.DangNhap`.
Mật khẩu được so bằng **BCrypt** với chuỗi băm trong CSDL; mật khẩu được kiểm **trước** trạng thái khóa để người nhập sai không dò được tài khoản nào đang bị khóa.
Đăng nhập thành công thì thông tin được lưu vào `PhienDangNhap`, mở `FrmMain` và `ApDungPhanQuyen()` ẩn/hiện menu theo vai trò (bảng quyền nằm trong `PhanQuyen.cs`).

```mermaid
flowchart TD
    A(["Bắt đầu – mở ứng dụng Desktop"]) --> B["FrmDangNhap: nhập tên đăng nhập và mật khẩu, bấm Đăng nhập hoặc Enter"]
    B --> C{"Có ô nào để trống?"}
    C -- Có --> C1["Báo: vui lòng nhập đủ thông tin"] --> B
    C -- Không --> D{"DbHelper.ThuKetNoi thành công?"}
    D -- Không --> D1["Báo lỗi kết nối CSDL, kiểm tra SQL Server và chuỗi kết nối"] --> B
    D -- Có --> E["Tìm tài khoản theo TenDangNhap, JOIN NhanVien lấy HoTen"]
    E --> F{"Tìm thấy tài khoản?"}
    F -- Không --> X["Báo: sai tên đăng nhập hoặc mật khẩu, xóa ô mật khẩu"] --> B
    F -- Có --> G{"BCrypt verify mật khẩu khớp chuỗi băm?"}
    G -- Không --> X
    G -- Có --> H{"TrangThai = 0, tài khoản bị khóa?"}
    H -- Có --> H1["Báo: tài khoản đã bị khóa, liên hệ quản trị viên"] --> B
    H -- Không --> I["PhienDangNhap.BatDau: lưu MaTK, MaNV, TenDangNhap, HoTen, VaiTro"]
    I --> J["Mở FrmMain, gọi ApDungPhanQuyen"]
    J --> K{"VaiTro?"}
    K -- ADMIN --> K1["Hiện tất cả menu: Nhân sự, Nhập hàng, Sản phẩm, Bán hàng, Báo cáo, Sao lưu – phục hồi"]
    K -- QUANLY --> K2["Hiện Nhập hàng, Sản phẩm, Bán hàng, Báo cáo<br/>Ẩn Nhân sự, Sao lưu – phục hồi"]
    K -- NHANVIEN --> K3["Chỉ hiện Bán hàng: POS, Đơn hàng"]
    K1 --> L["Làm việc trên FrmMain"]
    K2 --> L
    K3 --> L
    L --> M{"Chọn Đăng xuất?"}
    M -- Có --> M1["PhienDangNhap.DangXuat, xóa thông tin phiên"] --> B
    M -- Không, chọn Thoát --> Z(["Kết thúc"])
```

**Bảng phân quyền áp dụng** (`Core/Services/PhanQuyen.cs`)

| Nhóm chức năng (menu) | ADMIN | QUANLY | NHANVIEN |
|---|:-:|:-:|:-:|
| Nhân viên & tài khoản | ✔ | | |
| Nhà cung cấp, phiếu nhập | ✔ | ✔ | |
| Sản phẩm, danh mục, khuyến mãi | ✔ | ✔ | |
| Đơn hàng, POS | ✔ | ✔ | ✔ |
| Thống kê – báo cáo, duyệt bình luận | ✔ | ✔ | |
| Sao lưu – phục hồi CSDL | ✔ | | |
