# MÔ TẢ CƠ SỞ DỮ LIỆU

> Người viết: SV1. Lấy từ metadata thật của CSDL sau khi chạy `database/01_CreateDatabase.sql` (15/10/2026). Bản Word: [`mo-ta-csdl.docx`](mo-ta-csdl.docx), xuất bằng `node tools/XuatBaoCao/xuat-docx.js docs/bao-cao/mo-ta-csdl.md`.
> Khi thêm cột/bảng mới phải cập nhật file này cùng PR.

## 1. Quy ước

CSDL **BadmintonShop** chạy trên SQL Server, gồm 15 bảng. Các ký hiệu dùng trong cột "Ràng buộc":

- **PK**: khóa chính; **FK → Bảng**: khóa ngoại tham chiếu tới bảng đó; **UNIQUE**: giá trị không được trùng.
- **IDENTITY**: số tự tăng bắt đầu từ 1, do SQL Server tự cấp khi thêm dòng.
- **NOT NULL** / **NULL**: bắt buộc có giá trị / được bỏ trống.
- **DEFAULT**: giá trị mặc định khi không truyền vào; **CHECK**: điều kiện giá trị phải thỏa.
- **Cột tính**: giá trị do SQL Server tự tính từ các cột khác, lưu sẵn (PERSISTED), không ghi trực tiếp được.

Kiểu tiền tệ dùng `DECIMAL(18,0)` (đơn vị đồng, không có phần lẻ). Phần trăm giảm giá dùng `DECIMAL(5,2)` trong khoảng 0–100. Chuỗi tiếng Việt dùng `NVARCHAR`; chuỗi chỉ có ký tự không dấu (email, số điện thoại, tên đăng nhập, mã voucher) dùng `VARCHAR`.

## 2. Nhân sự và tài khoản

### 2.1. Bảng NhanVien – Nhân viên

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaNV | INT | PK, IDENTITY | Mã nhân viên |
| 2 | HoTen | NVARCHAR(100) | NOT NULL | Họ tên nhân viên |
| 3 | SDT | VARCHAR(15) | NULL | Số điện thoại (ứng dụng kiểm tra 10–11 chữ số) |
| 4 | Email | VARCHAR(100) | NULL | Email liên hệ |
| 5 | DiaChi | NVARCHAR(255) | NULL | Địa chỉ |
| 6 | ChucVu | NVARCHAR(50) | NOT NULL | Chức vụ: Quản lý, Nhân viên bán hàng, Kho… |
| 7 | Luong | DECIMAL(18,0) | NULL, CHECK ≥ 0 | Lương tháng (đồng) |
| 8 | NgayVaoLam | DATE | NOT NULL, DEFAULT ngày hiện tại | Ngày bắt đầu làm việc |

### 2.2. Bảng TaiKhoan – Tài khoản đăng nhập ứng dụng desktop

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaTK | INT | PK, IDENTITY | Mã tài khoản |
| 2 | TenDangNhap | VARCHAR(50) | NOT NULL, UNIQUE | Tên đăng nhập |
| 3 | MatKhau | VARCHAR(255) | NOT NULL | Chuỗi băm BCrypt của mật khẩu (không lưu mật khẩu thô) |
| 4 | VaiTro | VARCHAR(20) | NOT NULL, CHECK ∈ {ADMIN, QUANLY, NHANVIEN} | Vai trò, quyết định menu được dùng |
| 5 | MaNV | INT | FK → NhanVien, NOT NULL, UNIQUE | Nhân viên sở hữu tài khoản; mỗi nhân viên có tối đa 1 tài khoản |
| 6 | TrangThai | BIT | NOT NULL, DEFAULT 1 | 1 = đang hoạt động, 0 = bị khóa |

## 3. Danh mục, sản phẩm, nhà cung cấp, khách hàng

### 3.1. Bảng DanhMuc – Danh mục (loại) sản phẩm

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaDM | INT | PK, IDENTITY | Mã danh mục |
| 2 | TenDanhMuc | NVARCHAR(100) | NOT NULL, UNIQUE | Tên danh mục: Vợt, Cầu, Giày, Quần áo, Phụ kiện |
| 3 | MoTa | NVARCHAR(255) | NULL | Mô tả danh mục |

### 3.2. Bảng NhaCungCap – Nhà cung cấp

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaNCC | INT | PK, IDENTITY | Mã nhà cung cấp |
| 2 | TenNCC | NVARCHAR(150) | NOT NULL | Tên nhà cung cấp |
| 3 | DiaChi | NVARCHAR(255) | NULL | Địa chỉ |
| 4 | SDT | VARCHAR(15) | NULL | Số điện thoại |
| 5 | Email | VARCHAR(100) | NULL | Email liên hệ |

### 3.3. Bảng SanPham – Sản phẩm

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaSP | INT | PK, IDENTITY | Mã sản phẩm |
| 2 | TenSP | NVARCHAR(150) | NOT NULL | Tên sản phẩm |
| 3 | MaDM | INT | FK → DanhMuc, NOT NULL | Danh mục của sản phẩm |
| 4 | ThuongHieu | NVARCHAR(50) | NULL | Thương hiệu: Yonex, Victor, Li-Ning… (dùng để lọc trên web) |
| 5 | DonGiaBan | DECIMAL(18,0) | NOT NULL, CHECK ≥ 0 | Giá bán hiện tại (đồng) |
| 6 | SoLuongTon | INT | NOT NULL, DEFAULT 0, CHECK ≥ 0 | Số lượng tồn kho; trigger tự tăng khi nhập, tự giảm khi bán |
| 7 | DonViTinh | NVARCHAR(20) | NOT NULL, DEFAULT 'Cái' | Đơn vị tính: Cái, Đôi, Hộp, Ống… |
| 8 | HinhAnh | NVARCHAR(255) | NULL | Đường dẫn ảnh sản phẩm |
| 9 | MoTa | NVARCHAR(MAX) | NULL | Mô tả, thông số (trọng lượng vợt 3U/4U, độ cứng, size giày…) |
| 10 | TrangThai | NVARCHAR(20) | NOT NULL, DEFAULT 'Đang bán' | Trạng thái kinh doanh: Đang bán / Ngừng bán |

### 3.4. Bảng KhachHang – Khách hàng (tài khoản đăng nhập web)

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaKH | INT | PK, IDENTITY | Mã khách hàng |
| 2 | HoTen | NVARCHAR(100) | NOT NULL | Họ tên khách hàng |
| 3 | SDT | VARCHAR(15) | NULL | Số điện thoại |
| 4 | Email | VARCHAR(100) | NOT NULL, UNIQUE | Email, dùng làm tên đăng nhập web |
| 5 | MatKhau | VARCHAR(255) | NOT NULL | Chuỗi băm BCrypt của mật khẩu |
| 6 | DiaChi | NVARCHAR(255) | NULL | Địa chỉ nhận hàng mặc định |
| 7 | NgayDangKy | DATETIME | NOT NULL, DEFAULT thời điểm hiện tại | Thời điểm đăng ký tài khoản |
| 8 | TrangThai | BIT | NOT NULL, DEFAULT 1 | 1 = hoạt động, 0 = bị khóa (không đăng nhập được) |

## 4. Nhập hàng

### 4.1. Bảng PhieuNhap – Phiếu nhập hàng

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaPN | INT | PK, IDENTITY | Số phiếu nhập |
| 2 | NgayNhap | DATETIME | NOT NULL, DEFAULT thời điểm hiện tại | Ngày giờ nhập hàng |
| 3 | MaNCC | INT | FK → NhaCungCap, NOT NULL | Nhà cung cấp hàng |
| 4 | MaNV | INT | FK → NhanVien, NOT NULL | Nhân viên lập phiếu |
| 5 | TongTien | DECIMAL(18,0) | NOT NULL, DEFAULT 0 | Tổng tiền phiếu; trigger tự tính bằng tổng thành tiền các dòng chi tiết |

### 4.2. Bảng ChiTietPhieuNhap – Chi tiết phiếu nhập

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaPN | INT | PK, FK → PhieuNhap (xóa phiếu thì xóa theo) | Phiếu nhập chứa dòng này |
| 2 | MaSP | INT | PK, FK → SanPham | Sản phẩm được nhập (mỗi sản phẩm 1 dòng trong 1 phiếu) |
| 3 | SoLuong | INT | NOT NULL, CHECK > 0 | Số lượng nhập |
| 4 | DonGia | DECIMAL(18,0) | NOT NULL, CHECK ≥ 0 | Đơn giá nhập (đồng) |
| 5 | ThanhTien | DECIMAL(29,0) | Cột tính = SoLuong × DonGia | Thành tiền của dòng |

## 5. Khuyến mãi

### 5.1. Bảng CTKM – Chương trình khuyến mãi

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaCTKM | INT | PK, IDENTITY | Mã chương trình khuyến mãi |
| 2 | TenCTKM | NVARCHAR(150) | NOT NULL | Tên chương trình |
| 3 | NgayBatDau | DATE | NOT NULL | Ngày bắt đầu áp dụng |
| 4 | NgayKetThuc | DATE | NOT NULL, CHECK ≥ NgayBatDau | Ngày kết thúc áp dụng |
| 5 | LoaiKM | VARCHAR(20) | NOT NULL, CHECK ∈ {SANPHAM, HOADON, VOUCHER} | Kiểu khuyến mãi, cho biết chi tiết nằm ở bảng KM_TheoSP, KM_TheoHD hay KM_TheoVoucher |
| 6 | TrangThai | BIT | NOT NULL, DEFAULT 1 | 1 = đang bật, 0 = tạm tắt |

### 5.2. Bảng KM_TheoSP – Khuyến mãi theo sản phẩm

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaCTKM | INT | PK, FK → CTKM (xóa CTKM thì xóa theo) | Chương trình khuyến mãi |
| 2 | MaSP | INT | PK, FK → SanPham | Sản phẩm được giảm giá |
| 3 | PhanTramGG | DECIMAL(5,2) | NOT NULL, CHECK 0–100 | Phần trăm giảm cho sản phẩm |

### 5.3. Bảng KM_TheoHD – Khuyến mãi theo giá trị hóa đơn

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaCTKM | INT | PK, FK → CTKM (xóa CTKM thì xóa theo) | Chương trình khuyến mãi |
| 2 | TongTienToiThieu | DECIMAL(18,0) | PK, CHECK ≥ 0 | Mức tổng tiền hóa đơn tối thiểu để được giảm; một chương trình có thể có nhiều bậc |
| 3 | PhanTramGG | DECIMAL(5,2) | NOT NULL, CHECK 0–100 | Phần trăm giảm khi đạt mức tiền |

### 5.4. Bảng KM_TheoVoucher – Khuyến mãi theo mã voucher

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaVoucher | VARCHAR(30) | PK | Mã voucher khách nhập khi đặt hàng |
| 2 | MaCTKM | INT | FK → CTKM (xóa CTKM thì xóa theo), NOT NULL | Chương trình phát hành voucher |
| 3 | PhanTramGG | DECIMAL(5,2) | NOT NULL, CHECK 0–100 | Phần trăm giảm trên hóa đơn |
| 4 | SoLuong | INT | NOT NULL, DEFAULT 1, CHECK ≥ 0 | Tổng số lượt được dùng |
| 5 | DaDung | INT | NOT NULL, DEFAULT 0, CHECK 0 ≤ DaDung ≤ SoLuong | Số lượt đã dùng |

## 6. Bán hàng và phản hồi

### 6.1. Bảng HoaDon – Hóa đơn (đơn hàng web và hóa đơn bán tại quầy)

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaHD | INT | PK, IDENTITY | Mã hóa đơn |
| 2 | MaKH | INT | FK → KhachHang, NULL | Khách mua; NULL khi bán tại quầy cho khách vãng lai |
| 3 | MaNV | INT | FK → NhanVien, NULL | Nhân viên lập/xử lý; NULL khi khách tự đặt trên web |
| 4 | NgayLap | DATETIME | NOT NULL, DEFAULT thời điểm hiện tại | Thời điểm lập hóa đơn |
| 5 | LoaiBan | NVARCHAR(20) | NOT NULL, DEFAULT 'Online' | Kênh bán: Online / Tại quầy |
| 6 | HinhThucTT | NVARCHAR(20) | NOT NULL, DEFAULT 'COD' | Hình thức thanh toán: COD / Online / Tiền mặt |
| 7 | DiaChiGiao | NVARCHAR(255) | NULL | Địa chỉ giao hàng (đơn web) |
| 8 | MaCTKM | INT | FK → CTKM, NULL | Khuyến mãi cấp hóa đơn (theo hóa đơn hoặc voucher) đã áp dụng |
| 9 | TienGiamGia | DECIMAL(18,0) | NOT NULL, DEFAULT 0, CHECK ≥ 0 | Số tiền giảm ở cấp hóa đơn |
| 10 | TongTien | DECIMAL(18,0) | NOT NULL, DEFAULT 0 | Tổng phải trả; trigger tự tính = tổng thành tiền chi tiết − TienGiamGia |
| 11 | TrangThai | NVARCHAR(30) | NOT NULL, DEFAULT 'Chờ xác nhận', CHECK ∈ {Chờ xác nhận, Đã xác nhận, Đang giao, Hoàn thành, Đã hủy} | Trạng thái đơn; chuyển sang "Đã hủy" thì trigger trả hàng về kho |

### 6.2. Bảng ChiTietHoaDon – Chi tiết hóa đơn

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaHD | INT | PK, FK → HoaDon (xóa hóa đơn thì xóa theo) | Hóa đơn chứa dòng này |
| 2 | MaSP | INT | PK, FK → SanPham | Sản phẩm bán |
| 3 | SoLuong | INT | NOT NULL, CHECK > 0 | Số lượng mua; trigger chặn nếu vượt tồn kho |
| 4 | DonGia | DECIMAL(18,0) | NOT NULL, CHECK ≥ 0 | Đơn giá bán tại thời điểm mua |
| 5 | GiamGia | DECIMAL(5,2) | NOT NULL, DEFAULT 0, CHECK 0–100 | Phần trăm giảm theo sản phẩm (từ KM_TheoSP) |
| 6 | ThanhTien | DECIMAL(18,0) | Cột tính = SoLuong × DonGia × (1 − GiamGia/100) | Thành tiền của dòng sau giảm |

### 6.3. Bảng PhanHoi – Đánh giá, bình luận sản phẩm

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaPH | INT | PK, IDENTITY | Mã phản hồi |
| 2 | MaKH | INT | FK → KhachHang, NOT NULL | Khách viết đánh giá |
| 3 | MaSP | INT | FK → SanPham, NOT NULL | Sản phẩm được đánh giá |
| 4 | SoSao | TINYINT | NOT NULL, CHECK 1–5 | Số sao |
| 5 | NoiDung | NVARCHAR(MAX) | NULL | Nội dung bình luận |
| 6 | NgayGui | DATETIME | NOT NULL, DEFAULT thời điểm hiện tại | Thời điểm gửi |
| 7 | TrangThai | NVARCHAR(20) | NOT NULL, DEFAULT 'Hiển thị' | Trạng thái duyệt: Hiển thị / Ẩn (nhân viên duyệt trên desktop) |

## 7. Chỉ mục, trigger và thủ tục

Ngoài chỉ mục tự tạo cho khóa chính và UNIQUE, CSDL có thêm các chỉ mục phục vụ truy vấn thường dùng:

| Chỉ mục | Bảng (cột) | Phục vụ |
|---|---|---|
| IX_SanPham_DanhMuc | SanPham(MaDM) | Lọc sản phẩm theo danh mục |
| IX_HoaDon_NgayLap | HoaDon(NgayLap) | Thống kê doanh thu theo ngày/tháng |
| IX_HoaDon_KH | HoaDon(MaKH) | Lịch sử mua hàng của khách |
| IX_PhieuNhap_Ngay | PhieuNhap(NgayNhap) | Lọc phiếu nhập theo khoảng ngày |
| IX_PhanHoi_SP | PhanHoi(MaSP) | Hiện đánh giá trên trang chi tiết sản phẩm |

| Đối tượng | Loại | Chức năng |
|---|---|---|
| trg_CTPN_TangTon | Trigger trên ChiTietPhieuNhap | Thêm chi tiết phiếu nhập thì cộng tồn kho sản phẩm và tính lại TongTien của phiếu |
| trg_CTHD_GiamTon | Trigger trên ChiTietHoaDon | Thêm chi tiết hóa đơn thì trừ tồn kho; không đủ hàng thì báo lỗi và hủy giao tác; tính lại TongTien hóa đơn |
| trg_HoaDon_CapNhat | Trigger trên HoaDon | Hóa đơn chuyển sang "Đã hủy" thì trả hàng về kho; không cho mở lại đơn đã hủy; đổi TienGiamGia thì tính lại TongTien |
| sp_KiemTraTonKho | Thủ tục | Kiểm tra một sản phẩm còn đủ số lượng để bán hay không (dùng trước khi đặt hàng) |
