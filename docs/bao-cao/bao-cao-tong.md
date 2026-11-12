# BÁO CÁO TỔNG HỢP ĐỒ ÁN BADMINTONSHOP

## Mục lục chương

1. Giới thiệu
2. SRS và use case tổng quát — SV2
3. Kiến trúc hệ thống — SV1
4. Cơ sở dữ liệu — SV1
5. Thiết kế sequence/activity — SV3 và class diagram — SV4
6. Cài đặt
7. Kiểm thử — SV4
8. Hướng dẫn sử dụng
9. Tổng kết — SV4
10. Bảng tự đánh giá
11. Phụ lục đặc tả phần SV1

## 1. Giới thiệu

BadmintonShop là hệ thống bán đồ thể thao cầu lông gồm Web khách hàng và Desktop quản lý. Hai ứng dụng cùng dùng thư viện BadmintonShop.Core và CSDL SQL Server. Web sử dụng ASP.NET Core MVC, Desktop sử dụng WinForms, các project ứng dụng hướng tới .NET 8.

TODO(SV1): Điền thông tin nhóm, giảng viên, phạm vi đã chốt và mục tiêu nghiệm thu.

## 2. SRS và use case tổng quát — SV2

TODO(SV2): Cung cấp SRS, tác nhân, yêu cầu chức năng/phi chức năng, use case tổng quát và use case khách hàng đã duyệt.

## 3. Kiến trúc hệ thống — SV1

### 1. Tổng quan kiến trúc

Hệ thống bán đồ thể thao cầu lông BadmintonShop gồm hai ứng dụng phục vụ hai nhóm người dùng khác nhau:

- **Ứng dụng web BadmintonShop.Web** (ASP.NET Core MVC, .NET 8) dành cho khách hàng: xem và tìm kiếm sản phẩm, đăng ký và đăng nhập, giỏ hàng, đặt hàng, thanh toán, đánh giá sản phẩm.
- **Ứng dụng desktop BadmintonShop.Desktop** (Windows Forms, .NET 8) dành cho nhân viên và quản lý cửa hàng: quản lý sản phẩm, nhập hàng, xử lý đơn hàng, bán tại quầy (POS), khuyến mãi, nhân sự, thống kê và sao lưu dữ liệu.

Hai ứng dụng không truy cập cơ sở dữ liệu trực tiếp mà cùng tham chiếu một **thư viện lớp dùng chung BadmintonShop.Core**. Thư viện này chứa các lớp thực thể, lớp truy xuất dữ liệu và các dịch vụ nghiệp vụ, và là nơi duy nhất làm việc với **một cơ sở dữ liệu SQL Server**. Nhờ vậy, một đơn hàng đặt trên web sẽ hiện ngay trên màn hình quản lý đơn của desktop, và tồn kho luôn là một con số thống nhất cho cả hai phía.

![Sơ đồ kiến trúc tổng thể của hệ thống BadmintonShop](../diagrams/png/01-kien-truc.png)

### 2. Các thành phần của hệ thống

| Thành phần | Công nghệ | Trách nhiệm chính | Phụ trách |
|---|---|---|---|
| BadmintonShop.Web | ASP.NET Core MVC, Razor, Cookie Authentication | Giao diện cho khách hàng; nhận yêu cầu HTTP, gọi Core, trả về trang HTML | SV2, SV3 (cơ chế đăng nhập: SV1) |
| BadmintonShop.Desktop | Windows Forms | Giao diện cho nhân viên; đăng nhập và phân quyền theo vai trò | SV1, SV4 (khuyến mãi: SV3; duyệt bình luận: SV2) |
| BadmintonShop.Core | Class Library .NET 8, ADO.NET (Microsoft.Data.SqlClient), BCrypt.Net-Next | Models, DataAccess (DbHelper và các Repository), Services (mật khẩu, phiên đăng nhập, phân quyền, khuyến mãi) | SV1 (mỗi thành viên viết Repository cho bảng mình) |
| CSDL BadmintonShop | SQL Server 2022 (máy phát triển có thể dùng LocalDB) | 15 bảng với khóa chính, khóa ngoại, CHECK, chỉ mục; trigger cập nhật tồn kho và tổng tiền | SV1 |
| Dịch vụ email | Gmail SMTP | Gửi email xác nhận đơn hàng cho khách | SV3 |

#### 2.1. Thư viện dùng chung BadmintonShop.Core

Thư viện được chia thành ba thư mục, ứng với ba trách nhiệm:

- **Models**: 15 lớp thực thể, mỗi lớp ứng với một bảng, tên thuộc tính trùng tên cột (ví dụ `SanPham`, `HoaDon`, `PhieuNhap`). Các lớp này chỉ chứa dữ liệu, không chứa logic.
- **DataAccess**: lớp tĩnh `DbHelper` gói việc mở kết nối, chạy câu lệnh có tham số (`ExecuteQuery`, `ExecuteNonQuery`, `ExecuteScalar`) và chạy nhiều câu lệnh trong một giao tác (`ExecuteTransaction`). Mỗi bảng có một lớp `XxxRepository` (ví dụ `NhaCungCapRepository`, `PhieuNhapRepository`) cung cấp các hàm đọc/ghi như `LayTatCa`, `LayTheoMa`, `Them`, `Sua`, `Xoa`.
- **Services**: các xử lý nghiệp vụ không gắn với một bảng cụ thể, gồm `MatKhauHelper` (băm và kiểm tra mật khẩu bằng BCrypt), `PhienDangNhap` (lưu người đang đăng nhập trên desktop), `PhanQuyen` (bảng quyền theo vai trò), `KiemTraDuLieu` (kiểm tra số điện thoại, email) và `TinhKhuyenMaiService` (tính giảm giá).

Chuỗi kết nối không viết cứng trong mã: ứng dụng đọc biến môi trường `ConnectionStrings__BadmintonShop` nếu có, nếu không thì đọc từ `appsettings.json` (web) hoặc `App.config` (desktop), rồi gán cho `DbHelper.ConnectionString` một lần khi khởi động. Mỗi máy tự đặt chuỗi kết nối của mình mà không phải sửa file trong kho mã.

### 3. Mô hình phân lớp

Mỗi ứng dụng được tổ chức theo mô hình nhiều lớp. Lớp trên chỉ gọi lớp ngay dưới nó:

| Lớp | Web | Desktop | Nhiệm vụ |
|---|---|---|---|
| Giao diện | Views (Razor) + Controllers | Các Form (`FrmPhieuNhap`, `FrmNhanVien`…) | Hiển thị, nhận dữ liệu nhập, kiểm tra định dạng, báo lỗi cho người dùng |
| Nghiệp vụ | Core/Services | Core/Services | Quy tắc nghiệp vụ dùng chung: mật khẩu, phân quyền, khuyến mãi |
| Truy xuất dữ liệu | Core/DataAccess | Core/DataAccess | Câu lệnh SQL có tham số, giao tác, chuyển dòng dữ liệu thành đối tượng Model |
| Dữ liệu | SQL Server | SQL Server | Lưu trữ; ràng buộc toàn vẹn; trigger cập nhật tồn kho và tổng tiền |

Quy ước bắt buộc của nhóm: **không viết câu lệnh SQL trong Controller hay Form**. Mọi truy cập dữ liệu đi qua Repository trong Core. Khi duyệt Pull Request, nhóm trưởng kiểm tra quy ước này trước khi gộp mã.

### 4. Lý do tách thư viện dùng chung

- **Viết một lần, hai ứng dụng cùng dùng.** Ví dụ `SanPhamRepository.LayTheoMa` được cả trang chi tiết sản phẩm trên web lẫn màn hình POS trên desktop gọi. Sửa câu truy vấn ở một chỗ thì cả hai ứng dụng cùng được sửa.
- **Tránh lệch logic giữa hai ứng dụng.** Việc băm mật khẩu, tính khuyến mãi, lưu hóa đơn trong giao tác chỉ có một bản cài đặt. Không xảy ra tình trạng web tính giảm giá một kiểu còn desktop tính một kiểu.
- **Dễ phân công và làm song song.** Mỗi thành viên viết Repository cho các bảng mình phụ trách. Giao diện web và desktop được phát triển độc lập trên cùng một nền đã chốt từ tuần đầu.
- **Dễ kiểm thử.** Thư viện Core không phụ thuộc giao diện nên có thể kiểm thử bằng một chương trình console nhỏ chạy trên CSDL thật, không cần mở form hay trình duyệt.
- **Ràng buộc quan trọng đặt ở CSDL.** Khóa ngoại, CHECK và trigger tồn kho nằm trong SQL Server, nên dù dữ liệu đến từ ứng dụng nào cũng không thể bán quá tồn kho hay làm sai tổng tiền.

### 5. Một số luồng xử lý tiêu biểu

#### 5.1. Đăng nhập và phân quyền trên desktop

Khi mở ứng dụng desktop, `FrmDangNhap` kiểm tra kết nối CSDL trước (`DbHelper.ThuKetNoi`) để báo lỗi rõ ràng nếu máy chủ chưa chạy. Sau đó `TaiKhoanRepository.DangNhap` tìm tài khoản theo tên đăng nhập, so mật khẩu bằng BCrypt và chặn tài khoản đã bị khóa. Đăng nhập thành công thì thông tin người dùng được lưu vào `PhienDangNhap`. `FrmMain` dựa vào vai trò để ẩn hoặc hiện từng nhóm menu theo bảng dưới đây. Khi người dùng đăng xuất, phiên bị xóa và ứng dụng quay về màn hình đăng nhập.

| Chức năng | ADMIN | QUANLY | NHANVIEN |
|---|:-:|:-:|:-:|
| Nhân viên và tài khoản | ✔ | | |
| Nhà cung cấp, phiếu nhập | ✔ | ✔ | |
| Sản phẩm, danh mục, khuyến mãi | ✔ | ✔ | |
| Đơn hàng, bán tại quầy (POS) | ✔ | ✔ | ✔ |
| Thống kê – báo cáo, duyệt bình luận | ✔ | ✔ | |
| Sao lưu – phục hồi CSDL | ✔ | | |

![Luồng đăng nhập và phân quyền trên ứng dụng desktop](../diagrams/png/05-dang-nhap.png)

Trên web, khách hàng đăng nhập bằng email và mật khẩu. Hệ thống dùng **Cookie Authentication** của ASP.NET Core: sau khi đăng nhập, cookie chứa các claim `MaKH`, họ tên và email, có hạn 7 ngày và tự gia hạn khi khách còn hoạt động. Các trang cần đăng nhập (giỏ hàng, đặt hàng, hồ sơ) được đánh dấu `[Authorize]`. Ở mỗi yêu cầu, hệ thống kiểm tra lại trạng thái tài khoản, nên khách bị khóa giữa chừng sẽ bị đăng xuất ngay ở yêu cầu kế tiếp.

#### 5.2. Lập phiếu nhập và cập nhật tồn kho

Nhân viên chọn nhà cung cấp và thêm các dòng sản phẩm trên `FrmPhieuNhap`; thành tiền và tổng tiền được tính ngay trên form. Khi lưu, `PhieuNhapRepository.Them` ghi phiếu nhập rồi ghi từng dòng chi tiết **trong cùng một giao tác**. Mỗi lần thêm chi tiết, trigger `trg_CTPN_TangTon` tự cộng số lượng vào tồn kho của sản phẩm và tính lại tổng tiền của phiếu. Nếu bất kỳ dòng nào lỗi, toàn bộ giao tác bị hoàn tác: không có phiếu "lưu một nửa" và tồn kho giữ nguyên.

![Luồng lập phiếu nhập và cập nhật tồn kho](../diagrams/png/04-ton-kho.png)

Luồng bán hàng làm theo cách tương tự nhưng theo chiều ngược lại. Trigger `trg_CTHD_GiamTon` trừ tồn kho khi thêm chi tiết hóa đơn và báo lỗi nếu không đủ hàng. Trigger `trg_HoaDon_CapNhat` trả hàng về kho khi đơn chuyển sang trạng thái "Đã hủy". Thủ tục `sp_KiemTraTonKho` cho phép kiểm tra tồn trước khi đặt hàng.

### 6. Bảo mật và toàn vẹn dữ liệu

- **Mật khẩu không lưu dạng thô.** Mật khẩu nhân viên và khách hàng được băm bằng BCrypt (work factor 11) trước khi lưu. Hệ thống yêu cầu mật khẩu mạnh: tối thiểu 8 ký tự, có chữ hoa, chữ thường và chữ số. Khi quản trị viên đặt lại mật khẩu, hệ thống sinh ngẫu nhiên một mật khẩu mới và chỉ hiển thị một lần.
- **Chống SQL Injection.** Mọi câu lệnh đều dùng tham số (`SqlParameter`), không ghép chuỗi dữ liệu người dùng vào câu SQL.
- **Phân quyền theo vai trò.** Có 3 vai trò ADMIN, QUANLY, NHANVIEN. Quản trị viên không thể tự khóa hoặc tự hạ quyền của chính mình, để hệ thống luôn còn người quản trị.
- **Giao tác.** Các nghiệp vụ ghi nhiều bảng (phiếu nhập và chi tiết, hóa đơn và chi tiết) chạy trong một giao tác.
- **Ràng buộc ở CSDL.** Khóa ngoại chặn xóa dữ liệu đang được tham chiếu, ví dụ không xóa được nhà cung cấp đã có phiếu nhập. CHECK chặn số lượng âm và trạng thái đơn không hợp lệ. Cột thành tiền là cột tính toán nên không thể ghi sai.
- **Sao lưu – phục hồi.** Quản trị viên sao lưu CSDL ra file `.bak` và phục hồi khi cần ngay từ ứng dụng desktop.

### 7. Thiết kế cơ sở dữ liệu

#### 7.1. Sơ đồ thực thể – liên kết (ERD)

Lược đồ nền gồm 15 bảng, chia thành 5 nhóm: nhân sự và tài khoản; danh mục, sản phẩm, nhà cung cấp, khách hàng; nhập hàng; khuyến mãi; bán hàng và phản hồi. Khóa chính đơn đều là số nguyên tự tăng (`INT IDENTITY`). Hai bảng chi tiết dùng khóa chính ghép gồm mã phiếu (hoặc mã hóa đơn) và mã sản phẩm, nên một sản phẩm chỉ xuất hiện một lần trong mỗi phiếu.

![Sơ đồ thực thể – liên kết của CSDL BadmintonShop](../diagrams/png/02-erd.png)

#### 7.2. Lược đồ quan hệ

Quy ước: khóa chính <ins>gạch chân</ins>, khóa ngoại _in nghiêng_; cột vừa là khóa chính vừa là khóa ngoại thì <ins>_gạch chân và in nghiêng_</ins>.

1. NhanVien(<ins>MaNV</ins>, HoTen, SDT, Email, DiaChi, ChucVu, Luong, NgayVaoLam)
2. TaiKhoan(<ins>MaTK</ins>, TenDangNhap, MatKhau, VaiTro, _MaNV_, TrangThai)
3. DanhMuc(<ins>MaDM</ins>, TenDanhMuc, MoTa)
4. NhaCungCap(<ins>MaNCC</ins>, TenNCC, DiaChi, SDT, Email)
5. SanPham(<ins>MaSP</ins>, TenSP, _MaDM_, ThuongHieu, DonGiaBan, SoLuongTon, DonViTinh, HinhAnh, MoTa, TrangThai)
6. KhachHang(<ins>MaKH</ins>, HoTen, SDT, Email, MatKhau, DiaChi, NgayDangKy, TrangThai)
7. PhieuNhap(<ins>MaPN</ins>, NgayNhap, _MaNCC_, _MaNV_, TongTien)
8. ChiTietPhieuNhap(<ins>_MaPN_</ins>, <ins>_MaSP_</ins>, SoLuong, DonGia, ThanhTien)
9. CTKM(<ins>MaCTKM</ins>, TenCTKM, NgayBatDau, NgayKetThuc, LoaiKM, TrangThai)
10. KM_TheoSP(<ins>_MaCTKM_</ins>, <ins>_MaSP_</ins>, PhanTramGG)
11. KM_TheoHD(<ins>_MaCTKM_</ins>, <ins>TongTienToiThieu</ins>, PhanTramGG)
12. KM_TheoVoucher(<ins>MaVoucher</ins>, _MaCTKM_, PhanTramGG, SoLuong, DaDung)
13. HoaDon(<ins>MaHD</ins>, _MaKH_, _MaNV_, NgayLap, LoaiBan, HinhThucTT, DiaChiGiao, _MaCTKM_, TienGiamGia, TongTien, TrangThai)
14. ChiTietHoaDon(<ins>_MaHD_</ins>, <ins>_MaSP_</ins>, SoLuong, DonGia, GiamGia, ThanhTien)
15. PhanHoi(<ins>MaPH</ins>, _MaKH_, _MaSP_, SoSao, NoiDung, NgayGui, TrangThai)

#### 7.3. Các ràng buộc khóa ngoại

| Bảng con | Khóa ngoại | Tham chiếu | Cho phép NULL | Khi xóa bản ghi cha |
|---|---|---|:-:|---|
| TaiKhoan | MaNV (duy nhất) | NhanVien | Không | Chặn |
| SanPham | MaDM | DanhMuc | Không | Chặn |
| PhieuNhap | MaNCC | NhaCungCap | Không | Chặn |
| PhieuNhap | MaNV | NhanVien | Không | Chặn |
| ChiTietPhieuNhap | MaPN | PhieuNhap | Không | Xóa theo |
| ChiTietPhieuNhap | MaSP | SanPham | Không | Chặn |
| KM_TheoSP | MaCTKM | CTKM | Không | Xóa theo |
| KM_TheoSP | MaSP | SanPham | Không | Chặn |
| KM_TheoHD | MaCTKM | CTKM | Không | Xóa theo |
| KM_TheoVoucher | MaCTKM | CTKM | Không | Xóa theo |
| HoaDon | MaKH | KhachHang | Có (khách vãng lai tại quầy) | Chặn |
| HoaDon | MaNV | NhanVien | Có (khách tự đặt trên web) | Chặn |
| HoaDon | MaCTKM | CTKM | Có (không áp khuyến mãi hóa đơn) | Chặn |
| ChiTietHoaDon | MaHD | HoaDon | Không | Xóa theo |
| ChiTietHoaDon | MaSP | SanPham | Không | Chặn |
| PhanHoi | MaKH | KhachHang | Không | Chặn |
| PhanHoi | MaSP | SanPham | Không | Chặn |

Bảng mô tả chi tiết từng cột (kiểu dữ liệu, ràng buộc, ý nghĩa) của cả 15 bảng được trình bày ở phần Mô tả cơ sở dữ liệu.

### Cập nhật kiến trúc dữ liệu — 05/11/2026

Cài đủ SQL hiện tại tạo 16 bảng người dùng: 15 bảng nền và `DanhGiaSanPham` (script 07). Bổ sung lược đồ quan hệ: DanhGiaSanPham(<ins>MaDG</ins>, _MaSP_, _MaKH_, SoSao, NoiDung, NgayDanhGia, TrangThai); hai FK tham chiếu SanPham và KhachHang. Ứng dụng đánh giá sử dụng bảng mới này, giữ PhanHoi nền. HoaDon bổ sung bốn cột/index POS và hủy đơn theo 04/06. Xem phần cập nhật trong [mô tả CSDL](mo-ta-csdl.md). Sơ đồ PNG và bản Word là bản nền, cần xuất lại trước khi nộp.

Tái sử dụng Core/Repository hoặc cache Form chưa tự chứng minh đạt yêu cầu tái sử dụng dữ liệu tìm kiếm trong 5–10 phút. Phần đối chiếu yêu cầu giảng viên nằm trong [báo cáo rà soát](ra-soat-yeu-cau-2026-11-05.md).

## 4. Cơ sở dữ liệu — SV1

### 1. Quy ước

CSDL **BadmintonShop** chạy trên SQL Server. Lược đồ nền 01 có 15 bảng; cài đủ script hiện tại có 16 bảng, gồm bảng bổ sung `DanhGiaSanPham` ở 07. Các ký hiệu dùng trong cột "Ràng buộc":

- **PK**: khóa chính; **FK → Bảng**: khóa ngoại tham chiếu tới bảng đó; **UNIQUE**: giá trị không được trùng.
- **IDENTITY**: số tự tăng bắt đầu từ 1, do SQL Server tự cấp khi thêm dòng.
- **NOT NULL** / **NULL**: bắt buộc có giá trị / được bỏ trống.
- **DEFAULT**: giá trị mặc định khi không truyền vào; **CHECK**: điều kiện giá trị phải thỏa.
- **Cột tính**: giá trị do SQL Server tự tính từ các cột khác, lưu sẵn (PERSISTED), không ghi trực tiếp được.

Kiểu tiền tệ dùng `DECIMAL(18,0)` (đơn vị đồng, không có phần lẻ). Phần trăm giảm giá dùng `DECIMAL(5,2)` trong khoảng 0–100. Chuỗi tiếng Việt dùng `NVARCHAR`; chuỗi chỉ có ký tự không dấu (email, số điện thoại, tên đăng nhập, mã voucher) dùng `VARCHAR`.

### 2. Nhân sự và tài khoản

#### 2.1. Bảng NhanVien – Nhân viên

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

#### 2.2. Bảng TaiKhoan – Tài khoản đăng nhập ứng dụng desktop

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaTK | INT | PK, IDENTITY | Mã tài khoản |
| 2 | TenDangNhap | VARCHAR(50) | NOT NULL, UNIQUE | Tên đăng nhập |
| 3 | MatKhau | VARCHAR(255) | NOT NULL | Chuỗi băm BCrypt của mật khẩu (không lưu mật khẩu thô) |
| 4 | VaiTro | VARCHAR(20) | NOT NULL, CHECK ∈ {ADMIN, QUANLY, NHANVIEN} | Vai trò, quyết định menu được dùng |
| 5 | MaNV | INT | FK → NhanVien, NOT NULL, UNIQUE | Nhân viên sở hữu tài khoản; mỗi nhân viên có tối đa 1 tài khoản |
| 6 | TrangThai | BIT | NOT NULL, DEFAULT 1 | 1 = đang hoạt động, 0 = bị khóa |

### 3. Danh mục, sản phẩm, nhà cung cấp, khách hàng

#### 3.1. Bảng DanhMuc – Danh mục (loại) sản phẩm

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaDM | INT | PK, IDENTITY | Mã danh mục |
| 2 | TenDanhMuc | NVARCHAR(100) | NOT NULL, UNIQUE | Tên danh mục: Vợt, Cầu, Giày, Quần áo, Phụ kiện |
| 3 | MoTa | NVARCHAR(255) | NULL | Mô tả danh mục |

#### 3.2. Bảng NhaCungCap – Nhà cung cấp

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaNCC | INT | PK, IDENTITY | Mã nhà cung cấp |
| 2 | TenNCC | NVARCHAR(150) | NOT NULL | Tên nhà cung cấp |
| 3 | DiaChi | NVARCHAR(255) | NULL | Địa chỉ |
| 4 | SDT | VARCHAR(15) | NULL | Số điện thoại |
| 5 | Email | VARCHAR(100) | NULL | Email liên hệ |

#### 3.3. Bảng SanPham – Sản phẩm

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

#### 3.4. Bảng KhachHang – Khách hàng (tài khoản đăng nhập web)

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

### 4. Nhập hàng

#### 4.1. Bảng PhieuNhap – Phiếu nhập hàng

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaPN | INT | PK, IDENTITY | Số phiếu nhập |
| 2 | NgayNhap | DATETIME | NOT NULL, DEFAULT thời điểm hiện tại | Ngày giờ nhập hàng |
| 3 | MaNCC | INT | FK → NhaCungCap, NOT NULL | Nhà cung cấp hàng |
| 4 | MaNV | INT | FK → NhanVien, NOT NULL | Nhân viên lập phiếu |
| 5 | TongTien | DECIMAL(18,0) | NOT NULL, DEFAULT 0 | Tổng tiền phiếu; trigger tự tính bằng tổng thành tiền các dòng chi tiết |

#### 4.2. Bảng ChiTietPhieuNhap – Chi tiết phiếu nhập

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaPN | INT | PK, FK → PhieuNhap (xóa phiếu thì xóa theo) | Phiếu nhập chứa dòng này |
| 2 | MaSP | INT | PK, FK → SanPham | Sản phẩm được nhập (mỗi sản phẩm 1 dòng trong 1 phiếu) |
| 3 | SoLuong | INT | NOT NULL, CHECK > 0 | Số lượng nhập |
| 4 | DonGia | DECIMAL(18,0) | NOT NULL, CHECK ≥ 0 | Đơn giá nhập (đồng) |
| 5 | ThanhTien | DECIMAL(29,0) | Cột tính = SoLuong × DonGia | Thành tiền của dòng |

### 5. Khuyến mãi

#### 5.1. Bảng CTKM – Chương trình khuyến mãi

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaCTKM | INT | PK, IDENTITY | Mã chương trình khuyến mãi |
| 2 | TenCTKM | NVARCHAR(150) | NOT NULL | Tên chương trình |
| 3 | NgayBatDau | DATE | NOT NULL | Ngày bắt đầu áp dụng |
| 4 | NgayKetThuc | DATE | NOT NULL, CHECK ≥ NgayBatDau | Ngày kết thúc áp dụng |
| 5 | LoaiKM | VARCHAR(20) | NOT NULL, CHECK ∈ {SANPHAM, HOADON, VOUCHER} | Kiểu khuyến mãi, cho biết chi tiết nằm ở bảng KM_TheoSP, KM_TheoHD hay KM_TheoVoucher |
| 6 | TrangThai | BIT | NOT NULL, DEFAULT 1 | 1 = đang bật, 0 = tạm tắt |

#### 5.2. Bảng KM_TheoSP – Khuyến mãi theo sản phẩm

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaCTKM | INT | PK, FK → CTKM (xóa CTKM thì xóa theo) | Chương trình khuyến mãi |
| 2 | MaSP | INT | PK, FK → SanPham | Sản phẩm được giảm giá |
| 3 | PhanTramGG | DECIMAL(5,2) | NOT NULL, CHECK 0–100 | Phần trăm giảm cho sản phẩm |

#### 5.3. Bảng KM_TheoHD – Khuyến mãi theo giá trị hóa đơn

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaCTKM | INT | PK, FK → CTKM (xóa CTKM thì xóa theo) | Chương trình khuyến mãi |
| 2 | TongTienToiThieu | DECIMAL(18,0) | PK, CHECK ≥ 0 | Mức tổng tiền hóa đơn tối thiểu để được giảm; một chương trình có thể có nhiều bậc |
| 3 | PhanTramGG | DECIMAL(5,2) | NOT NULL, CHECK 0–100 | Phần trăm giảm khi đạt mức tiền |

#### 5.4. Bảng KM_TheoVoucher – Khuyến mãi theo mã voucher

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaVoucher | VARCHAR(30) | PK | Mã voucher khách nhập khi đặt hàng |
| 2 | MaCTKM | INT | FK → CTKM (xóa CTKM thì xóa theo), NOT NULL | Chương trình phát hành voucher |
| 3 | PhanTramGG | DECIMAL(5,2) | NOT NULL, CHECK 0–100 | Phần trăm giảm trên hóa đơn |
| 4 | SoLuong | INT | NOT NULL, DEFAULT 1, CHECK ≥ 0 | Tổng số lượt được dùng |
| 5 | DaDung | INT | NOT NULL, DEFAULT 0, CHECK 0 ≤ DaDung ≤ SoLuong | Số lượt đã dùng |

### 6. Bán hàng và phản hồi

#### 6.1. Bảng HoaDon – Hóa đơn (đơn hàng web và hóa đơn bán tại quầy)

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

#### 6.2. Bảng ChiTietHoaDon – Chi tiết hóa đơn

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaHD | INT | PK, FK → HoaDon (xóa hóa đơn thì xóa theo) | Hóa đơn chứa dòng này |
| 2 | MaSP | INT | PK, FK → SanPham | Sản phẩm bán |
| 3 | SoLuong | INT | NOT NULL, CHECK > 0 | Số lượng mua; trigger chặn nếu vượt tồn kho |
| 4 | DonGia | DECIMAL(18,0) | NOT NULL, CHECK ≥ 0 | Đơn giá bán tại thời điểm mua |
| 5 | GiamGia | DECIMAL(5,2) | NOT NULL, DEFAULT 0, CHECK 0–100 | Phần trăm giảm theo sản phẩm (từ KM_TheoSP) |
| 6 | ThanhTien | DECIMAL(18,0) | Cột tính = SoLuong × DonGia × (1 − GiamGia/100) | Thành tiền của dòng sau giảm |

#### 6.3. Bảng PhanHoi – Đánh giá, bình luận sản phẩm

| STT | Tên cột | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|:-:|---|---|---|---|
| 1 | MaPH | INT | PK, IDENTITY | Mã phản hồi |
| 2 | MaKH | INT | FK → KhachHang, NOT NULL | Khách viết đánh giá |
| 3 | MaSP | INT | FK → SanPham, NOT NULL | Sản phẩm được đánh giá |
| 4 | SoSao | TINYINT | NOT NULL, CHECK 1–5 | Số sao |
| 5 | NoiDung | NVARCHAR(MAX) | NULL | Nội dung bình luận |
| 6 | NgayGui | DATETIME | NOT NULL, DEFAULT thời điểm hiện tại | Thời điểm gửi |
| 7 | TrangThai | NVARCHAR(20) | NOT NULL, DEFAULT 'Hiển thị' | Trạng thái duyệt: Hiển thị / Ẩn (nhân viên duyệt trên desktop) |

### 7. Chỉ mục, trigger và thủ tục

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

### Bổ sung theo script hiện tại — 05/11/2026

Phần bảng nền phía trên mô tả bản 15/10. Script tổng hiện có 16 bảng người dùng; ứng dụng đánh giá dùng `DanhGiaSanPham`, không dùng bảng `PhanHoi` cũ. Bản Word chưa xuất lại sau lần cập nhật Markdown này.

#### DanhGiaSanPham (script 07)

| Cột | Kiểu | Ràng buộc / ý nghĩa |
|---|---|---|
| MaDG | INT | PK, IDENTITY |
| MaSP | INT | NOT NULL, FK → SanPham |
| MaKH | INT | NOT NULL, FK → KhachHang |
| SoSao | INT | NOT NULL, CHECK 1–5 |
| NoiDung | NVARCHAR(1000) | NULL |
| NgayDanhGia | DATETIME | NOT NULL, DEFAULT GETDATE() |
| TrangThai | NVARCHAR(20) | NOT NULL, mặc định Chờ duyệt; CHECK Chờ duyệt / Đã duyệt / Từ chối |

Chỉ mục `IX_DanhGiaSanPham_SP(MaSP, TrangThai)` hỗ trợ đọc đánh giá theo sản phẩm và trạng thái.

#### Bổ sung HoaDon (script 04/06)

| Cột | Kiểu | Ý nghĩa |
|---|---|---|
| LyDoHuy | NVARCHAR(500) NULL | Lý do nhân viên hủy đơn |
| MaYeuCauPOS | UNIQUEIDENTIFIER NULL | Token chống ghi trùng POS |
| TienKhachDua | DECIMAL(18,0) NULL | Số tiền nhận tại quầy |
| DauVanTayPOS | VARCHAR(64) NULL | SHA-256 yêu cầu POS đã xác nhận |

Unique filtered index `UX_HoaDon_MaYeuCauPOS` áp dụng khi MaYeuCauPOS khác NULL. Các cột bổ sung không thay thế schema nền. Số lượng bảng/cột ở đây được đối chiếu tĩnh với SQL repo, chưa truy vấn lại CSDL đang chạy ngày 05/11.

### Kiểm chứng script tổng ngày 12/11/2026

Script `database/BadmintonShop.sql` được chạy trên CSDL riêng `BadmintonShop_GptTmp`, gồm 16 bảng, 3 trigger, 1 thủ tục, 40 SP, 23 KH, 5 NV, 11 phiếu nhập, 28 hóa đơn Online, 4 CTKM, 34 đánh giá trong DanhGiaSanPham và 0 SP tồn kho âm. Hóa đơn từ 22/06 đến 20/10/2026, thuộc 5 tháng có dữ liệu.

TODO(SV3): Phối hợp SV4 hoàn thiện phân bố hóa đơn 6–12 tháng.

TODO(SV4): Bổ sung dữ liệu bán tại quầy cho script nộp.

## 5. Thiết kế

### 5.1. Sequence và activity — SV3

TODO(SV3): Ghép sơ đồ tuần tự đặt hàng/thanh toán, sơ đồ hoạt động xử lý đơn và thuyết minh.

### 5.2. Class diagram — SV4

TODO(SV4): Ghép class diagram và giải thích quan hệ lớp.

## 6. Cài đặt

Tài liệu dành cho: thành viên nhóm cài môi trường lập trình, giảng viên chạy thử chương trình, và làm phần "Hướng dẫn cài đặt" của báo cáo.

### 1. Yêu cầu hệ thống

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

### 2. Cài đặt phần mềm nền

#### 2.1. Visual Studio 2022

- Tải Visual Studio 2022 Community tại visualstudio.microsoft.com.
- Trong Visual Studio Installer, chọn 2 workload **ASP.NET and web development** và **.NET desktop development**.
- Cài xong, mở Command Prompt, gõ `dotnet --list-sdks` và `dotnet --list-runtimes`. Phải có một SDK bản 8 trở lên và runtime `Microsoft.NETCore.App 8.x`, `Microsoft.WindowsDesktop.App 8.x`, `Microsoft.AspNetCore.App 8.x`.

#### 2.2. SQL Server

Chọn một trong hai cách:

- **Cách 1 – SQL Server Express/Developer** (máy đóng gói dùng `.\SQLEXPRESS`): tải tại microsoft.com/sql-server/sql-server-downloads. Ở bước Database Engine Configuration, chọn Mixed Mode và Add Current User. Ghi lại **tên server**: thường là `localhost` (Developer, kiểu Basic) hoặc `.\SQLEXPRESS` (Express). Sau đó cài thêm SQL Server Management Studio (SSMS).
- **Cách 2 – LocalDB** (nhẹ, có sẵn khi cài Visual Studio): tên server là `(localdb)\MSSQLLocalDB`. Kiểm tra bằng lệnh `sqllocaldb info`, nếu thấy `MSSQLLocalDB` là dùng được.

Sai tên server là lỗi kết nối hay gặp nhất khi chạy trên máy khác.

#### 2.3. Git

Cài Git tại git-scm.com (giữ mặc định), rồi khai báo tên và email:

```
git config --global user.name "Ho Ten"
git config --global user.email "email@gmail.com"
```

### 3. Lấy mã nguồn

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

### 4. Tạo cơ sở dữ liệu

Cách nhanh nhất: chạy **một file duy nhất** `database\BadmintonShop.sql` – bản nối sẵn của 01 → 02 → 03 → 04 → 06 → 07 → 08, dùng trên máy chưa có CSDL. Muốn chạy từng phần thì chạy đủ bảy script này **theo đúng thứ tự**. File 05 chỉ dành cho DB test/demo riêng, không thuộc chuỗi cài đặt thông thường.

Chú ý, file tổng và file 01 **xóa và tạo lại** CSDL `BadmintonShop` nếu đã có. Chạy các lệnh cài mới dưới đây trên máy chưa có CSDL cùng tên. Trên máy đang sử dụng, tạo bản script đổi tên CSDL theo mục 4.4 để kiểm tra trong CSDL riêng, rồi áp dụng migration sau khi đã rà soát.

#### 4.1. Cách 1 – bằng SSMS

Mở SSMS, kết nối tới `.\SQLEXPRESS` hoặc instance của máy cài mới, mở file `database\BadmintonShop.sql` (File → Open → File…) và bấm Execute (F5). Nếu chạy từng phần thì mở và Execute lần lượt 01, 02, 03, 04, 06, 07, 08.

#### 4.2. Cách 2 – bằng dòng lệnh sqlcmd

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

#### 4.3. Kiểm tra

Chạy câu lệnh sau, trên CSDL mới cài đủ script của repo phải có 16 bảng người dùng (15 bảng nền + DanhGiaSanPham). Tồn kho phải không âm; số sản phẩm còn hàng không bắt buộc bằng toàn bộ sản phẩm sau khi đã sử dụng hệ thống:

```
USE BadmintonShop;
SELECT COUNT(*) FROM sys.tables WHERE is_ms_shipped = 0; -- 16 trên DB mới
SELECT COUNT(*) FROM SanPham WHERE SoLuongTon < 0;    -- phải = 0
SELECT COUNT(*) FROM PhieuNhap WHERE TongTien > 0;    -- = số phiếu nhập
```

Mật khẩu trong dữ liệu mẫu đã được băm BCrypt sẵn. Khi cần tạo chuỗi băm cho mật khẩu mới (ví dụ thêm tài khoản mẫu), chạy `dotnet run --project tools/HashTool -- MatKhau@123` rồi chép chuỗi in ra vào script.

#### 4.4. Kiểm tra script trên CSDL tạm

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

### 5. Cấu hình chuỗi kết nối

Hai ứng dụng đọc chuỗi kết nối tên **BadmintonShop** theo thứ tự ưu tiên:

1. Biến môi trường `ConnectionStrings__BadmintonShop` (nếu có).
2. File cấu hình: `BadmintonShop.Web\appsettings.json` cho web và `BadmintonShop.Desktop\App.config` cho desktop.

Mặc định 2 file cấu hình trỏ tới `Server=localhost`. Nếu máy dùng tên server khác, nên dùng **biến môi trường**: đặt một lần cho cả hai ứng dụng và không phải sửa file trong kho mã. Nhờ vậy các thành viên không ghi đè cấu hình của nhau khi commit.

#### 5.1. Đặt biến môi trường User

Mở **Edit environment variables for your account** trong Windows. Tại **User variables**, tạo biến `ConnectionStrings__BadmintonShop` với giá trị kết nối của máy. Các thành phần cho Windows Authentication được điền theo bảng sau, giá trị hoàn chỉnh được lưu tại máy cài đặt.

| Thành phần | Giá trị trên máy dùng SQLEXPRESS |
|---|---|
| Server | .\SQLEXPRESS |
| Database | BadmintonShop hoặc tên CSDL demo riêng |
| Trusted_Connection | True |
| TrustServerCertificate | True |

Đóng và mở lại Visual Studio hoặc terminal sau khi đặt biến.

#### 5.2. Cấu hình riêng cho máy đóng gói

Máy hiện tại lấy cấu hình từ biến môi trường **User** `ConnectionStrings__BadmintonShop`. Để terminal đang mở nhận giá trị đã lưu, dùng PowerShell.

```powershell
$env:ConnectionStrings__BadmintonShop = [Environment]::GetEnvironmentVariable(
    'ConnectionStrings__BadmintonShop', 'User')
```

Giá trị thực và thông tin đăng nhập CSDL được quản lý ngoài kho mã. Với bản publish, đặt biến môi trường cho tài khoản Windows chạy ứng dụng, rồi mở lại tiến trình. Giữ file `BadmintonShop.Desktop.dll.config` cạnh DLL của desktop.

Hai ứng dụng có thể dùng `BadmintonShop__MediaRoot` trỏ đến thư mục ảnh dùng chung. Khi cài bản publish trên máy khác, chép thư mục ảnh và đặt cùng thư mục gốc cho Web, Desktop để các khóa `products/...` tìm đúng file.

### 6. Build và chạy chương trình

#### 6.1. Bằng Visual Studio

- Mở `BadmintonShop.sln`, chờ Visual Studio tự khôi phục các gói NuGet.
- Build → Build Solution (Ctrl + Shift + B): các project phải build thành công, gồm bộ kiểm tra SV4 đang có trong solution.
- Chạy web: chuột phải `BadmintonShop.Web` → Set as Startup Project → F5. Chọn profile `http` để mở http://localhost:5255 hoặc profile `https` để mở https://localhost:7150.
- Chạy desktop: chuột phải `BadmintonShop.Desktop` → Set as Startup Project → F5. Màn hình đăng nhập hiện ra.
- Chạy cả hai cùng lúc (demo đặt hàng trên web rồi xử lý trên desktop): chuột phải Solution → Properties → Startup Project → Multiple startup projects → đặt Web và Desktop là Start.

#### 6.2. Bằng dòng lệnh

```
dotnet build BadmintonShop.sln
dotnet run --project BadmintonShop.Web --launch-profile http
:: mở cửa sổ khác
dotnet run --project BadmintonShop.Desktop
```

#### 6.3. Tài khoản test

| Ứng dụng | Tài khoản | Mật khẩu | Dùng để demo |
|---|---|---|---|
| Desktop | admin | Admin@123 | Toàn quyền: nhân viên và tài khoản, sao lưu, mọi chức năng |
| Desktop | quanly | Quanly@123 | Sản phẩm, nhà cung cấp, phiếu nhập, đơn hàng, báo cáo |
| Desktop | kho | Quanly@123 | Vai trò Quản lý (nhân viên kho) |
| Desktop | nhanvien | Nhanvien@123 | Chỉ bán tại quầy (POS) và xử lý đơn hàng |
| Desktop | nhanvien2 | Nhanvien@123 | Tài khoản bị khóa – demo chặn đăng nhập |
| Web | kh01@gmail.com | Khach@123 | Khách hàng mua hàng, theo dõi đơn, đánh giá (đã có trong script 03 hiện tại) |
| Web | kh20@gmail.com | Khach@123 | Khách hàng bị khóa – demo chặn đăng nhập (đã có trong script 03 hiện tại) |

### 7. Sao lưu và phục hồi cơ sở dữ liệu

#### 7.1. Trong ứng dụng desktop (tài khoản ADMIN)

- Đăng nhập `admin`, trong nhóm **HỆ THỐNG** ở thanh điều hướng trái chọn **Sao lưu, phục hồi**.
- **Sao lưu:** chọn thư mục, bấm "Sao lưu ngay". File được đặt tên `BadmintonShop_yyyyMMdd_HHmmss.bak`.
- **Phục hồi:** chọn file `.bak`, bấm "Phục hồi" và xác nhận. Ứng dụng tự kiểm tra file, ngắt các kết nối khác, phục hồi và yêu cầu đăng nhập lại. Bản sao lưu cần tương thích phiên bản SQL Server đích.

Đường dẫn thư mục là đường dẫn trên **máy chạy SQL Server**. Với SQL Server cài dạng dịch vụ, tài khoản dịch vụ phải có quyền ghi vào thư mục; đơn giản nhất là dùng thư mục Backup mặc định mà form tự gợi ý. LocalDB chạy bằng tài khoản Windows của người dùng nên ghi được vào thư mục bất kỳ của người đó.

#### 7.2. File sao lưu demo và kiểm chứng

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

### 8. Đóng gói để nộp bài và demo

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

### 9. Lỗi thường gặp

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

### 10. Nâng cấp CSDL đã có và cấu hình email

Với CSDL có dữ liệu cần giữ: sao lưu, kiểm tra schema rồi áp dụng riêng 04 → 06 → 07 khi còn thiếu; không chạy lại 01 hoặc file tổng vì có DROP/CREATE DATABASE. Bảng đánh giá được Web/Desktop sử dụng là `DanhGiaSanPham`; bảng `PhanHoi` nền vẫn được giữ.

TODO(SV3): Hoàn thiện hướng dẫn cấu hình email qua biến môi trường hoặc user-secrets, ghi các khóa cần dùng và bằng chứng gửi email xác nhận đơn thực tế. Xác nhận luồng OTP tài khoản riêng theo môi trường chạy.

### 11. Cấu hình bộ nhớ và tài liệu nghiệm thu

TTL mặc định là 10 phút. Biến môi trường `BadmintonShop__CacheMinutes` nhận giá trị từ 1 đến 60. Các tiến trình cùng tài khoản Windows chia sẻ tín hiệu thay đổi trong thư mục CacheSignals dưới LocalApplicationData/BadmintonShop. Với các tiến trình khác tài khoản hoặc máy, đặt `BadmintonShop__CacheSignalRoot` tới thư mục chung có quyền đọc/ghi và dùng cùng tên server/catalog.

Lệnh tái lập và phạm vi các đợt nghiệm thu được lưu trong `quy-tac-va-tien-do-dot-2.md`, `quy-tac-va-tien-do-dot-3.md` và `quy-tac-va-tien-do-dot-6.md`. Kịch bản bảo vệ được lưu trong `kich-ban-bao-ve-dot-3.md`.

## 7. Kiểm thử — SV4

TODO(SV4): Ghép kế hoạch kiểm thử, bảng test case chung, kết quả kiểm thử chéo SV1 và bằng chứng tương ứng.

## 8. Hướng dẫn sử dụng

### 8.1. Phần SV1

### 1. Chuẩn bị và quyền sử dụng

Cài đặt theo `huong-dan-cai-dat.md`. Hai ứng dụng dùng cùng CSDL SQL Server. Tài khoản mẫu nằm trong `readme.txt` và mục 6.3 của hướng dẫn cài đặt.

| Chức năng | ADMIN | QUANLY | NHANVIEN |
|---|---|---|---|
| Đăng nhập, đổi mật khẩu, đăng xuất | Có | Có | Có |
| Nhà cung cấp, phiếu nhập | Có | Có | |
| Nhân viên, tài khoản | Có | | |
| Sao lưu, phục hồi | Có | | |

Giao diện hiện tại dùng thanh điều hướng bên trái. Các nhãn thao tác bên dưới lấy từ `FrmMain.Shell.cs` và các form SV1.

### 2. Đăng nhập desktop

1. Chạy `BadmintonShop.Desktop.exe`. Màn hình **Đăng nhập hệ thống** xuất hiện.
2. Nhập **Tên đăng nhập** và **Mật khẩu**, ví dụ `admin` và `Admin@123`.
3. Có thể chọn **Hiện mật khẩu** để kiểm tra ký tự đã nhập.
4. Bấm **Đăng nhập** hoặc Enter. Hệ thống mở màn hình chính và hiển thị quyền của tài khoản.
5. Bấm **Thoát** khi muốn kết thúc từ màn hình đăng nhập.

Bỏ trống tài khoản hiện “Vui lòng nhập tên đăng nhập.”. Bỏ trống mật khẩu hiện “Vui lòng nhập mật khẩu.”. Sai thông tin hiện “Sai tên đăng nhập hoặc mật khẩu.”. Với `nhanvien2` và mật khẩu mẫu, thông báo là “Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.”.

Nếu mất kết nối, hộp **Lỗi kết nối** bắt đầu bằng “Không kết nối được cơ sở dữ liệu.” và kèm chi tiết để kiểm tra SQL Server, cấu hình kết nối.

![TODO: chụp màn hình đăng nhập desktop và tài khoản bị khóa](anh-sv1/dang-nhap-desktop.png)

### 3. Đổi mật khẩu và đăng xuất desktop

#### 3.1. Đổi mật khẩu

1. Trong nhóm **HỆ THỐNG** ở thanh bên trái, chọn **Đổi mật khẩu**.
2. Nhập **Mật khẩu hiện tại**, **Mật khẩu mới** và **Nhập lại mật khẩu mới**.
3. Mật khẩu mới cần tối thiểu 8 ký tự, có chữ hoa, chữ thường và chữ số, đồng thời khác mật khẩu hiện tại.
4. Bấm **Lưu**. Thành công hiện “Đổi mật khẩu thành công.” và đóng hộp thoại.
5. Bấm **Hủy** để đóng khi chưa muốn lưu.

Nhập sai mật khẩu hiện tại hiện “Mật khẩu hiện tại không đúng.”. Hai ô mật khẩu mới khác nhau hiện “Nhập lại mật khẩu mới không khớp.”. Dùng lại mật khẩu cũ hiện “Mật khẩu mới phải khác mật khẩu hiện tại.”.

![TODO: chụp màn hình đổi mật khẩu desktop](anh-sv1/doi-mat-khau.png)

#### 3.2. Đăng xuất

1. Bấm **Đăng xuất** ở góc trên bên phải.
2. Tại thông báo “Bạn có chắc muốn đăng xuất?”, chọn **Yes** để trở về màn hình đăng nhập, hoặc **No** để tiếp tục phiên.

### 4. Nhà cung cấp

1. Với tài khoản ADMIN hoặc QUANLY, chọn **Nhà cung cấp** trong nhóm **NHẬP HÀNG**.
2. Tìm dữ liệu bằng ô **Tìm (tên / SĐT)**, bấm **Tìm** hoặc Enter.
3. Để thêm, bấm **Làm mới**, nhập **Tên NCC (*)**, **SĐT**, **Địa chỉ**, **Email**, rồi bấm **Thêm**. Thành công hiện “Đã thêm nhà cung cấp.”.
4. Để sửa, chọn một dòng, chỉnh thông tin rồi bấm **Lưu sửa**. Thành công hiện “Đã cập nhật nhà cung cấp.”.
5. Để xóa, chọn dòng rồi bấm **Xóa**, xác nhận **Yes** trong hộp **Xác nhận xóa**. Thành công hiện “Đã xóa nhà cung cấp.”.

Tên là bắt buộc. SĐT khi có giá trị cần gồm 10–11 chữ số. Email khi có giá trị cần đúng định dạng. NCC đã phát sinh phiếu nhập hiện “Không thể xóa: nhà cung cấp này đã có phiếu nhập hàng.” và “Bạn chỉ có thể sửa thông tin.”.

![TODO: chụp màn hình nhà cung cấp và các nút Thêm Lưu sửa Xóa](anh-sv1/nha-cung-cap.png)

### 5. Nhân viên và tài khoản

#### 5.1. Hồ sơ nhân viên

1. Đăng nhập ADMIN, chọn **Nhân viên, tài khoản** trong nhóm **HỆ THỐNG**.
2. Mở tab **Nhân viên**. Tìm theo ô **Tìm (tên / SĐT / email)** và bấm **Tìm**.
3. Bấm **Làm mới** để nhập nhân viên mới. Điền họ tên, SĐT, email, địa chỉ, chức vụ, lương và ngày vào làm.
4. Bấm **Thêm**. Thành công hiện “Đã thêm nhân viên.”.
5. Chọn một nhân viên và bấm **Lưu sửa** sau khi chỉnh thông tin. Thành công hiện “Đã cập nhật nhân viên.”.
6. Chọn dòng, bấm **Xóa** và xác nhận **Yes** nếu cần xóa. Thành công hiện “Đã xóa nhân viên.”.

Họ tên và chức vụ là bắt buộc. Lương cần là số không âm, ngày vào làm tối đa là ngày hiện tại. Khi nhân viên đã có tài khoản, phiếu nhập hoặc hóa đơn, thông báo hướng dẫn “Hãy khóa tài khoản thay vì xóa nhân viên.”.

![TODO: chụp màn hình tab Nhân viên](anh-sv1/nhan-vien.png)

#### 5.2. Cấp tài khoản

1. Mở tab **Tài khoản đăng nhập**, khu vực **Cấp tài khoản mới**.
2. Chọn **Nhân viên** chưa có tài khoản, nhập **Tên đăng nhập**, **Mật khẩu**, chọn **Vai trò** là ADMIN, QUANLY hoặc NHANVIEN.
3. Tên đăng nhập dài 3–50 ký tự, gồm chữ không dấu, số, dấu gạch dưới hoặc dấu chấm. Mật khẩu tối thiểu 8 ký tự, có chữ hoa, chữ thường và chữ số.
4. Bấm **Cấp tài khoản**. Thông báo có dạng “Đã cấp tài khoản "tên" (vai trò).”.

Nếu hết nhân viên có thể cấp tài khoản, thông báo là “Không còn nhân viên nào chưa có tài khoản.”. Tên bị trùng hiện “Tên đăng nhập đã được dùng.”.

#### 5.3. Đổi quyền, khóa hoặc mở khóa

1. Chọn một dòng tài khoản ở lưới.
2. Trong **Tài khoản đang chọn**, chọn **Vai trò**, bấm **Đổi**. Thông báo là “Đã đổi vai trò. Người dùng cần đăng nhập lại để quyền mới có hiệu lực.”.
3. Bấm **Khóa tài khoản** đối với tài khoản đang hoạt động, hoặc **Mở khóa** đối với tài khoản đã bị khóa.
4. Kết quả hiện “Đã khóa tài khoản.” hoặc “Đã mở khóa tài khoản.”.

Tự đổi vai trò bị chặn bằng “Bạn không thể tự đổi vai trò của chính mình.”. Tự khóa bị chặn bằng “Bạn không thể tự khóa tài khoản của chính mình.”.

#### 5.4. Đặt lại mật khẩu

1. Chọn tài khoản, bấm **Đặt lại mật khẩu**.
2. Chọn **Yes** tại câu hỏi có dạng “Đặt lại mật khẩu cho "tên"?”.
3. Hộp **Đã đặt lại mật khẩu** hiển thị mật khẩu ngẫu nhiên mới. Ghi lại để giao cho đúng nhân viên và nhắc đổi mật khẩu sau khi đăng nhập.

![TODO: chụp màn hình tab Tài khoản đăng nhập với mật khẩu đã che](anh-sv1/tai-khoan.png)

### 6. Phiếu nhập hàng

#### 6.1. Danh sách và xem chi tiết

1. Chọn **Phiếu nhập** trong nhóm **NHẬP HÀNG**.
2. Chọn **NCC**, hoặc **(Tất cả)** để xem mọi nhà cung cấp.
3. Bật ô chọn cạnh **Từ ngày** và **Đến ngày** để giới hạn ngày. Bỏ chọn từng ô để bỏ giới hạn tương ứng.
4. Bấm **Lọc**. Lưới hiển thị số phiếu, ngày nhập, nhà cung cấp, người lập và tổng tiền.
5. Chọn một dòng để xem vùng **Chi tiết phiếu nhập số ...** phía dưới. Dòng tổng kết hiển thị số phiếu và tổng giá trị nhập.

Nếu khoảng ngày đảo ngược, hệ thống hiện “"Từ ngày" phải trước hoặc bằng "Đến ngày".”.

![TODO: chụp màn hình danh sách và chi tiết phiếu nhập](anh-sv1/danh-sach-phieu-nhap.png)

#### 6.2. Lập phiếu nhập

1. Từ danh sách, bấm **+ Lập phiếu nhập**.
2. Chọn **Nhà cung cấp (*)**, kiểm tra **Ngày nhập** và **Người lập**. Người lập được lấy từ phiên đăng nhập.
3. Trong **Thêm sản phẩm vào phiếu**, chọn **Sản phẩm**, nhập **SL** và **Đơn giá**, bấm **Thêm vào phiếu**.
4. Chọn sản phẩm gợi ý đơn giá nhập gần nhất khi có dữ liệu. Số lượng và đơn giá cần lớn hơn 0.
5. Nếu thêm lại sản phẩm đang có, hệ thống cộng số lượng và áp dụng đơn giá vừa nhập cho dòng gộp.
6. Có thể sửa **Số lượng**, **Đơn giá nhập** ngay trên lưới. Bấm **Xóa dòng đang chọn** để bỏ dòng. Kiểm tra **Thành tiền** và **Tổng tiền**.
7. Bấm **Lưu phiếu** rồi chọn **Yes** tại hộp **Xác nhận** có tên NCC, số sản phẩm và tổng tiền.
8. Thành công hiện “Đã lưu phiếu nhập số ... Tồn kho đã được cập nhật.”. Bấm **OK** để trở về danh sách và xem phiếu vừa tạo.
9. Bấm **Đóng** khi muốn rời form. Phiếu đang có chi tiết sẽ hỏi “Phiếu nhập chưa được lưu. Đóng và bỏ phiếu này?”.

Chưa chọn NCC hiện “Vui lòng chọn nhà cung cấp.”. Ngày nhập vượt thời điểm hiện tại hơn 5 phút bị chặn với “Ngày nhập không được ở tương lai.”. Nếu lỗi SQL khi lưu, hộp **Lỗi** bắt đầu bằng “Không lưu được phiếu nhập, dữ liệu chưa thay đổi.”.

![TODO: chụp màn hình lập phiếu nhập và thông báo lưu thành công](anh-sv1/lap-phieu-nhap.png)

### 7. Sao lưu và phục hồi

#### 7.1. Sao lưu

1. Đăng nhập ADMIN, chọn **Sao lưu, phục hồi** trong nhóm **HỆ THỐNG**.
2. Kiểm tra dòng **CSDL đang dùng** trước khi thao tác.
3. Trong vùng **Sao lưu**, giữ **Thư mục lưu** gợi ý của SQL Server hoặc bấm **Chọn...** để chọn thư mục có quyền ghi cho tài khoản dịch vụ SQL Server.
4. Bấm **Sao lưu ngay**, chờ trạng thái **Đang sao lưu...**.
5. Hộp **Sao lưu** hiển thị “Sao lưu thành công” kèm đường dẫn file. Tên file có dạng `BadmintonShop_yyyyMMdd_HHmmss.bak`.

Thư mục cần tồn tại, nếu sai sẽ hiện “Thư mục không tồn tại.”. Đường dẫn phải truy cập được từ máy SQL Server và máy chạy desktop vì form kiểm tra tồn tại ở phía desktop.

#### 7.2. Phục hồi trên CSDL demo riêng

1. Trước buổi thử, cấu hình ứng dụng dùng CSDL demo riêng theo hướng dẫn cài đặt, rồi đăng nhập ADMIN.
2. Mở **Sao lưu, phục hồi** và kiểm tra **CSDL đang dùng** đúng CSDL demo.
3. Trong vùng **Phục hồi**, bấm **Chọn...** để chọn **File .bak**.
4. Bấm **Phục hồi**. Đọc hộp **Xác nhận phục hồi** có tên CSDL, đường dẫn file và cảnh báo ghi đè dữ liệu, ngắt kết nối mọi người đang dùng.
5. Chọn **Yes** để thực hiện trên CSDL demo đã chọn, hoặc **No** để dừng. Chờ trạng thái **Đang phục hồi...**.
6. Khi hiện “Phục hồi thành công. Vui lòng đăng nhập lại.”, bấm **OK** và đăng nhập lại.

Nếu đường dẫn sai, thông báo là “Vui lòng chọn file .bak hợp lệ.”. Repository kiểm tra bản sao lưu bằng `RESTORE VERIFYONLY` trước khi phục hồi và ánh xạ file dữ liệu sang đường dẫn của máy đích.

![TODO: chụp màn hình sao lưu phục hồi trên CSDL demo riêng](anh-sv1/sao-luu-phuc-hoi.png)

### 8. Đăng nhập web của khách hàng

1. Chạy web, mở `http://localhost:5255/TaiKhoan/DangNhap` với profile `http`, hoặc địa chỉ được in bởi tiến trình web.
2. Nhập **Email** là `kh01@gmail.com`, **Mật khẩu** là `Khach@123`.
3. Có thể chọn **Ghi nhớ đăng nhập**, rồi bấm **Đăng nhập**. Hệ thống chuyển tới trang được yêu cầu trước đó hoặc trang chủ.
4. Sai thông tin hiện “Email hoặc mật khẩu không chính xác.”. Dùng `kh20@gmail.com` cùng mật khẩu mẫu hiện “Tài khoản của bạn đã bị khóa.”.
5. Bỏ trống email hiện “Vui lòng nhập email”, email sai định dạng hiện “Email không hợp lệ”, bỏ trống mật khẩu hiện “Vui lòng nhập mật khẩu”.

Giao diện web thuộc SV2. Cơ chế xác thực dùng repository khách hàng và cookie do SV1 cung cấp.

![TODO: chụp màn hình đăng nhập web kh01 và thông báo khóa kh20](anh-sv1/dang-nhap-web.png)

### 8.2. Phần các thành viên khác

TODO(SV2): Bổ sung hướng dẫn chức năng khách hàng và đánh giá.

TODO(SV3): Bổ sung hướng dẫn giỏ hàng, đặt hàng, khuyến mãi.

TODO(SV4): Bổ sung hướng dẫn sản phẩm, POS, đơn hàng và thống kê.

## 9. Tổng kết — SV4

TODO(SV4): Ghép kết quả đạt được, hạn chế và hướng phát triển đã được nhóm xác nhận.

## 10. Bảng tự đánh giá

| Thành viên | Họ tên / MSSV | Công việc xác nhận | Tự đánh giá | Nhóm xác nhận |
|---|---|---|---|---|
| SV1 | TODO(SV1) | TODO(SV1) | TODO(SV1) | TODO(SV1) |
| SV2 | TODO(SV2) | TODO(SV2) | TODO(SV2) | TODO(SV2) |
| SV3 | TODO(SV3) | TODO(SV3) | TODO(SV3) | TODO(SV3) |
| SV4 | TODO(SV4) | TODO(SV4) | TODO(SV4) | TODO(SV4) |

## 11. Phụ lục đặc tả phần SV1

Cột kết quả trong đặc tả giữ mốc kiểm tra đã ghi ngày 15/10/2026. Kết quả kiểm thử chéo và tổng hợp do SV4 xác nhận tại chương 7.

### 1. Danh sách use case

| Mã | Tên use case | Tác nhân | Ứng dụng |
|---|---|---|---|
| UC01 | Đăng nhập, đăng xuất, đổi mật khẩu | Quản trị viên, Quản lý, Nhân viên | Desktop |
| UC02 | Đăng nhập, đăng ký tài khoản khách hàng (cơ chế xác thực) | Khách hàng | Web |
| UC03 | Quản lý nhà cung cấp | Quản trị viên, Quản lý | Desktop |
| UC04 | Quản lý nhân viên và tài khoản | Quản trị viên | Desktop |
| UC05 | Lập phiếu nhập hàng | Quản trị viên, Quản lý | Desktop |
| UC06 | Sao lưu – phục hồi cơ sở dữ liệu | Quản trị viên | Desktop |

Giao diện đăng ký/đăng nhập trên web do SV2 làm; SV1 làm phần cơ chế (băm mật khẩu, cookie, chặn tài khoản khóa). Đặc tả UC02 phía giao diện nằm trong phần của SV2.

### 2. UC01 – Đăng nhập, đăng xuất, đổi mật khẩu (desktop)

| Mục | Nội dung |
|---|---|
| Tác nhân | Quản trị viên (ADMIN), Quản lý (QUANLY), Nhân viên (NHANVIEN) |
| Mô tả | Nhân viên đăng nhập ứng dụng desktop bằng tài khoản được cấp; hệ thống hiển thị menu theo vai trò |
| Tiền điều kiện | Tài khoản đã được quản trị viên cấp; SQL Server đang chạy |
| Hậu điều kiện | Đăng nhập thành công: thông tin người dùng lưu trong phiên, form chính hiện đúng các menu được phép |

**Luồng sự kiện chính**

1. Người dùng mở ứng dụng desktop; hệ thống hiện màn hình đăng nhập.
2. Người dùng nhập tên đăng nhập, mật khẩu (ô mật khẩu ẩn ký tự, có ô tick để hiện) và bấm Đăng nhập hoặc Enter.
3. Hệ thống kiểm tra kết nối CSDL, tìm tài khoản theo tên đăng nhập, so mật khẩu với chuỗi băm BCrypt và kiểm tra trạng thái tài khoản.
4. Hệ thống lưu phiên đăng nhập và mở form chính; menu hiển thị theo bảng phân quyền của vai trò.

**Luồng thay thế và ngoại lệ**

- 2a. Bỏ trống tên đăng nhập hoặc mật khẩu: báo "Vui lòng nhập tên đăng nhập" / "Vui lòng nhập mật khẩu".
- 3c. Không kết nối được CSDL: báo "Không kết nối được cơ sở dữ liệu" kèm chi tiết lỗi, vẫn ở màn hình đăng nhập.
- 3a. Sai tên đăng nhập hoặc sai mật khẩu: báo chung "Sai tên đăng nhập hoặc mật khẩu" (không tiết lộ tài khoản có tồn tại hay không).
- 3b. Tài khoản bị khóa (mật khẩu đúng): báo tài khoản đã bị khóa, liên hệ quản trị viên.
- Đổi mật khẩu: người dùng vào Hệ thống → Đổi mật khẩu, nhập mật khẩu hiện tại, mật khẩu mới (tối thiểu 8 ký tự, có chữ hoa, chữ thường, chữ số) và nhập lại. Mật khẩu hiện tại sai, mật khẩu mới yếu, nhập lại không khớp hoặc trùng mật khẩu cũ thì báo lỗi tương ứng.
- Đăng xuất: người dùng chọn Hệ thống → Đăng xuất và xác nhận; hệ thống xóa phiên và quay về màn hình đăng nhập.

### 3. UC03 – Quản lý nhà cung cấp

| Mục | Nội dung |
|---|---|
| Tác nhân | Quản trị viên, Quản lý |
| Mô tả | Xem, tìm kiếm, thêm, sửa, xóa nhà cung cấp |
| Tiền điều kiện | Đã đăng nhập với vai trò ADMIN hoặc QUANLY |
| Hậu điều kiện | Danh sách nhà cung cấp trong CSDL được cập nhật |

**Luồng sự kiện chính**

1. Người dùng chọn menu Nhập hàng → Nhà cung cấp; hệ thống hiện danh sách nhà cung cấp.
2. Người dùng nhập từ khóa (tên hoặc số điện thoại) và bấm Tìm để lọc danh sách.
3. Thêm: người dùng nhập tên (bắt buộc), địa chỉ, số điện thoại, email rồi bấm Thêm; hệ thống kiểm tra và lưu.
4. Sửa: người dùng chọn một dòng, sửa thông tin, bấm Lưu sửa; hệ thống kiểm tra và lưu.
5. Xóa: người dùng chọn một dòng, bấm Xóa và xác nhận; hệ thống xóa nhà cung cấp.

**Luồng thay thế và ngoại lệ**

- 3a/4a. Bỏ trống tên, số điện thoại không phải 10–11 chữ số, email sai định dạng: báo lỗi và đưa con trỏ về ô cần sửa.
- 5a. Nhà cung cấp đã có phiếu nhập: không cho xóa, báo lý do và gợi ý chỉ sửa thông tin.

### 4. UC04 – Quản lý nhân viên và tài khoản

| Mục | Nội dung |
|---|---|
| Tác nhân | Quản trị viên |
| Mô tả | Quản lý hồ sơ nhân viên; cấp tài khoản, đổi vai trò, khóa/mở khóa, đặt lại mật khẩu |
| Tiền điều kiện | Đã đăng nhập với vai trò ADMIN |
| Hậu điều kiện | Bảng NhanVien, TaiKhoan được cập nhật; mật khẩu luôn lưu dạng băm |

**Luồng sự kiện chính**

1. Quản trị viên chọn menu Nhân sự → Nhân viên & tài khoản; hệ thống hiện 2 tab Nhân viên và Tài khoản đăng nhập.
2. Tab Nhân viên: tìm kiếm, thêm, sửa, xóa nhân viên (họ tên, SĐT, email, địa chỉ, chức vụ, lương, ngày vào làm).
3. Tab Tài khoản – cấp tài khoản: chọn nhân viên chưa có tài khoản, nhập tên đăng nhập, mật khẩu, chọn vai trò, bấm Cấp tài khoản.
4. Chọn một tài khoản để đổi vai trò, khóa/mở khóa, hoặc đặt lại mật khẩu (hệ thống sinh mật khẩu ngẫu nhiên 10 ký tự, hiển thị một lần).

**Luồng thay thế và ngoại lệ**

- 2a. Họ tên hoặc chức vụ trống, SĐT/email sai định dạng, lương âm hoặc không phải số, ngày vào làm ở tương lai: báo lỗi.
- 2b. Xóa nhân viên đã có tài khoản, phiếu nhập hoặc hóa đơn: không cho xóa, gợi ý khóa tài khoản thay vì xóa.
- 3a. Tên đăng nhập trùng, không đúng định dạng (3–50 ký tự, chữ không dấu, số, dấu _ hoặc .), hoặc mật khẩu yếu: báo lỗi.
- 3b. Không còn nhân viên nào chưa có tài khoản: báo không thể cấp thêm (mỗi nhân viên tối đa 1 tài khoản).
- 4a. Quản trị viên tự khóa hoặc tự đổi vai trò của chính mình: hệ thống chặn và báo lý do.

### 5. UC05 – Lập phiếu nhập hàng

| Mục | Nội dung |
|---|---|
| Tác nhân | Quản trị viên, Quản lý |
| Mô tả | Lập phiếu nhập hàng từ nhà cung cấp, cập nhật tồn kho; xem danh sách phiếu nhập |
| Tiền điều kiện | Đã đăng nhập với vai trò ADMIN hoặc QUANLY; đã có nhà cung cấp và sản phẩm |
| Hậu điều kiện | Phiếu nhập và chi tiết được lưu; tồn kho các sản phẩm tăng đúng số lượng nhập; tổng tiền phiếu được tính |

**Luồng sự kiện chính**

1. Người dùng chọn menu Nhập hàng → Phiếu nhập; hệ thống hiện danh sách phiếu nhập (lọc được theo khoảng ngày và nhà cung cấp, chọn một phiếu để xem chi tiết).
2. Người dùng bấm "+ Lập phiếu nhập"; hệ thống mở form lập phiếu, người lập là người đang đăng nhập.
3. Người dùng chọn nhà cung cấp và ngày nhập.
4. Người dùng chọn sản phẩm (hệ thống gợi ý đơn giá nhập lần gần nhất), nhập số lượng, đơn giá và bấm Thêm vào phiếu. Lặp lại cho các sản phẩm khác.
5. Hệ thống hiển thị thành tiền từng dòng và tổng tiền phiếu, cập nhật ngay khi thêm, xóa hoặc sửa dòng.
6. Người dùng bấm Lưu phiếu và xác nhận; hệ thống lưu phiếu và toàn bộ chi tiết trong một giao tác; trigger tăng tồn kho và tính tổng tiền.
7. Hệ thống báo số phiếu vừa lưu, quay lại danh sách và chọn sẵn phiếu mới.

**Luồng thay thế và ngoại lệ**

- 3a. Chưa chọn nhà cung cấp hoặc phiếu chưa có sản phẩm nào: không cho lưu, báo lỗi.
- 4a. Thêm sản phẩm đã có trong phiếu: gộp vào dòng cũ (cộng số lượng, lấy đơn giá mới).
- 5a. Sửa trực tiếp số lượng (phải > 0) hoặc đơn giá (phải > 0) trên lưới; giá trị sai thì không cho rời ô. Thêm dòng có đơn giá bằng 0 cũng bị chặn: "Đơn giá nhập phải lớn hơn 0."
- 6a. Lỗi CSDL trong lúc lưu: hoàn tác toàn bộ, không có phiếu nào được lưu, tồn kho giữ nguyên, báo lỗi.
- 7a. Đóng form khi phiếu chưa lưu: hệ thống hỏi lại trước khi bỏ phiếu.

### 6. UC06 – Sao lưu – phục hồi cơ sở dữ liệu

| Mục | Nội dung |
|---|---|
| Tác nhân | Quản trị viên |
| Mô tả | Sao lưu toàn bộ CSDL ra file .bak và phục hồi từ file .bak |
| Tiền điều kiện | Đã đăng nhập với vai trò ADMIN; tài khoản SQL Server có quyền sao lưu/phục hồi |
| Hậu điều kiện | Sao lưu: có file .bak mới. Phục hồi: dữ liệu trở về đúng thời điểm sao lưu, người dùng phải đăng nhập lại |

**Luồng sự kiện chính**

1. Quản trị viên chọn menu Hệ thống → Sao lưu - phục hồi CSDL.
2. Sao lưu: chọn thư mục (mặc định là thư mục Backup của SQL Server), bấm Sao lưu ngay; hệ thống tạo file `BadmintonShop_yyyyMMdd_HHmmss.bak` và báo đường dẫn.
3. Phục hồi: chọn file .bak, bấm Phục hồi, xác nhận cảnh báo ghi đè dữ liệu.
4. Hệ thống kiểm tra file, ngắt các kết nối khác, phục hồi CSDL, đưa CSDL về chế độ nhiều người dùng.
5. Hệ thống báo thành công, đăng xuất và yêu cầu đăng nhập lại.

**Luồng thay thế và ngoại lệ**

- 2a. Thư mục không tồn tại hoặc SQL Server không có quyền ghi: báo lỗi kèm gợi ý dùng thư mục mặc định.
- 3a. File không tồn tại hoặc file hỏng: báo lỗi, dữ liệu hiện tại giữ nguyên, CSDL vẫn dùng được.
- 4a. File .bak tạo trên máy khác (đường dẫn file dữ liệu khác): hệ thống tự chuyển file dữ liệu về đúng vị trí của máy hiện tại.

### 7. Test case

Ghi chú cột Kết quả: **Đạt** = SV1 đã chạy và đúng mong đợi (ghi rõ cách kiểm); **–** = chưa chạy trên giao diện, chờ SV4 kiểm thử chéo.

| Mã TC | UC | Mô tả / các bước | Dữ liệu vào | Kết quả mong đợi | Kết quả |
|---|---|---|---|---|---|
| TC01 | UC01 | Đăng nhập đúng tài khoản ADMIN | admin / Admin@123 | Vào form chính, thấy đủ menu: Nhân sự, Nhập hàng, Sản phẩm, Bán hàng, Báo cáo, Sao lưu | Đạt (test tự động) |
| TC02 | UC01 | Đăng nhập vai trò QUANLY | quanly / Quanly@123 | Không thấy menu Nhân sự và Sao lưu; thấy các menu còn lại | Đạt (test tự động) |
| TC03 | UC01 | Đăng nhập vai trò NHANVIEN | nhanvien / Nhanvien@123 | Chỉ thấy menu Bán hàng (đơn hàng, POS) và Hệ thống | Đạt (test tự động) |
| TC04 | UC01 | Sai mật khẩu | admin / sai123 | Báo sai tên đăng nhập hoặc mật khẩu, vẫn ở màn hình đăng nhập | Đạt (test tự động) |
| TC05 | UC01 | Tên đăng nhập không tồn tại | khongco / Admin@123 | Báo giống TC04 | Đạt (test tự động) |
| TC06 | UC01 | Tài khoản bị khóa | nhanvien2 / Nhanvien@123 | Báo tài khoản đã bị khóa | Đạt (test tự động) |
| TC07 | UC01 | Mất kết nối CSDL | Tắt SQL Server, mở app, bấm Đăng nhập | Báo không kết nối được cơ sở dữ liệu, không bị văng app | – |
| TC08 | UC01 | Đổi mật khẩu hợp lệ | MK cũ đúng, MK mới Abcd1234 | Báo thành công; đăng nhập lại bằng MK mới được, MK cũ không được | Đạt (test tự động) |
| TC09 | UC01 | Đổi mật khẩu – mật khẩu mới yếu | MK mới abc | Báo chưa đủ mạnh | – |
| TC10 | UC01 | Đăng xuất | Hệ thống → Đăng xuất → Yes | Quay về màn hình đăng nhập | – |
| TC11 | UC03 | Thêm NCC hợp lệ | Tên "NCC A", SĐT 0901234567, email a@b.com | Thêm thành công, NCC hiện trên lưới | – |
| TC12 | UC03 | Thêm NCC bỏ trống tên | Tên rỗng | Báo nhập tên, không lưu | – |
| TC13 | UC03 | SĐT sai định dạng | SĐT 09abc hoặc 123 | Báo SĐT 10–11 chữ số | Đạt (test hàm kiểm tra) |
| TC14 | UC03 | Email sai định dạng | a@b, "x y@b.com" | Báo email không hợp lệ | Đạt (test hàm kiểm tra) |
| TC15 | UC03 | Xóa NCC đã có phiếu nhập | NCC số 1 | Không xóa, báo đã có phiếu nhập | Đạt (test tự động) |
| TC16 | UC03 | Xóa NCC chưa có phiếu nhập | NCC số 4 | Hỏi xác nhận, xóa thành công | Đạt (test tự động, phần kiểm tra) |
| TC17 | UC03 | Tìm kiếm theo tên / SĐT | Từ khóa "Yonex" hoặc một phần SĐT | Lưới chỉ còn các NCC khớp | – |
| TC18 | UC04 | Thêm nhân viên hợp lệ | Họ tên, chức vụ Kho, lương 5000000 | Thêm thành công, xuất hiện trong danh sách cấp tài khoản | Đạt (test tự động) |
| TC19 | UC04 | Lương âm / không phải số | -1 hoặc "abc" | Báo lương phải là số không âm | – |
| TC20 | UC04 | Xóa nhân viên đã có tài khoản | Nhân viên số 1 | Không xóa, gợi ý khóa tài khoản | Đạt (test tự động) |
| TC21 | UC04 | Cấp tài khoản hợp lệ | NV mới, test_tv1 / Abcd1234, NHANVIEN | Cấp thành công; đăng nhập được bằng tài khoản mới | Đạt (test tự động) |
| TC22 | UC04 | Cấp tài khoản trùng tên đăng nhập | Tên "admin" | Báo tên đăng nhập đã được dùng | – |
| TC23 | UC04 | Khóa rồi mở khóa tài khoản | Tài khoản test_tv1 | Sau khóa: không đăng nhập được; sau mở: đăng nhập được | Đạt (test tự động) |
| TC24 | UC04 | Đổi vai trò | test_tv1: NHANVIEN → QUANLY | Đăng nhập lại thấy menu của Quản lý | Đạt (test tự động) |
| TC25 | UC04 | Đặt lại mật khẩu | Tài khoản test_tv1 | Hiện mật khẩu mới 10 ký tự đủ mạnh; MK cũ hết hiệu lực | Đạt (test tự động) |
| TC26 | UC04 | ADMIN tự khóa / tự đổi vai trò mình | Đăng nhập admin, chọn dòng admin | Hệ thống chặn, báo không thể tự khóa / tự đổi vai trò | – |
| TC27 | UC05 | Lập phiếu nhập hợp lệ | NCC 1; SP1 ×3 giá 120.000; SP2 ×3 giá 50.000 | Lưu thành công; TongTien = 510.000; tồn SP1 +3, SP2 +3 | Đạt (test tự động trên CSDL) |
| TC28 | UC05 | Thêm trùng sản phẩm trong phiếu | SP1 ×2 giá 100.000, rồi SP1 ×1 giá 120.000 | Còn 1 dòng SP1: SL 3, giá 120.000, thành tiền 360.000 | Đạt (test tự động trên form) |
| TC29 | UC05 | Lưu khi chưa chọn NCC / chưa có dòng | – | Báo lỗi, không lưu | – |
| TC30 | UC05 | Lỗi giữa chừng khi lưu | Dòng 2 có mã sản phẩm không tồn tại | Không lưu phiếu nào, tồn kho dòng 1 không đổi | Đạt (test tự động trên CSDL) |
| TC31 | UC05 | Gợi ý đơn giá nhập gần nhất | Chọn SP đã từng nhập | Ô đơn giá điền sẵn giá lần nhập gần nhất | Đạt (test tự động) |
| TC32 | UC05 | Lọc danh sách phiếu nhập | Khoảng ngày + NCC | Chỉ hiện phiếu khớp, tổng giá trị nhập đúng | – |
| TC33 | UC06 | Sao lưu | Thư mục hợp lệ | Tạo file .bak, báo đường dẫn | Đạt (test tự động trên LocalDB) |
| TC34 | UC06 | Phục hồi khi đang có kết nối khác | File .bak vừa tạo; thêm 1 NCC trước khi phục hồi | Phục hồi thành công, NCC vừa thêm biến mất, yêu cầu đăng nhập lại | Đạt (test tự động, phần dữ liệu) |
| TC35 | UC06 | Phục hồi file không tồn tại / file hỏng | File rác .bak | Báo lỗi, CSDL vẫn dùng được bình thường | Đạt (test tự động) |
| TC36 | UC06 | Tài khoản QUANLY mở sao lưu | Đăng nhập quanly | Không thấy menu Sao lưu - phục hồi | Đạt (test tự động phân quyền) |

TODO(SV1): Cập nhật mục lục tự động, đánh số hình/bảng, định dạng bản Word tổng hợp và xuất PDF sau khi nhận đủ phần đã duyệt của nhóm.
