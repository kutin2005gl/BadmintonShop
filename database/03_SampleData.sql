/* =====================================================================
   BadmintonShop - 03_SampleData.sql
   Du lieu demo. Moi thanh vien BO SUNG phan cua minh vao dung muc.
   Them bang cha truoc: DanhMuc -> SanPham, NhanVien -> TaiKhoan, HoaDon -> ChiTietHoaDon
   ===================================================================== */
USE BadmintonShop;
GO

/* ---------------------------------------------------------------------
   PHAN SV1: DanhMuc, NhaCungCap, NhanVien, TaiKhoan, SanPham (khoi dau),
             PhieuNhap, ChiTietPhieuNhap
   --------------------------------------------------------------------- */

-- Danh muc (5)
INSERT INTO DanhMuc (TenDanhMuc, MoTa) VALUES
 (N'Vợt',      N'Vợt cầu lông các hãng'),
 (N'Cầu',      N'Quả cầu lông lông vũ / nhựa'),
 (N'Giày',     N'Giày chuyên dụng cầu lông'),
 (N'Quần áo',  N'Áo, quần, váy thi đấu'),
 (N'Phụ kiện', N'Cước, quấn cán, balo, túi vợt');

-- Nha cung cap (6)
INSERT INTO NhaCungCap (TenNCC, DiaChi, SDT, Email) VALUES
 (N'Công ty TNHH Yonex Việt Nam',   N'12 Nguyễn Văn Trỗi, Phú Nhuận, TP.HCM', '0281234567', 'sales@yonex.vn'),
 (N'Nhà phân phối Victor Sài Gòn',  N'45 Lý Thường Kiệt, Q.10, TP.HCM',       '0282345678', 'contact@victorsg.vn'),
 (N'Li-Ning Việt Nam',              N'88 Cách Mạng Tháng 8, Q.3, TP.HCM',     '0283456789', 'info@lining.vn'),
 (N'Kumpoo Store',                  N'21 Trần Hưng Đạo, Q.1, TP.HCM',         '0284567890', 'kumpoo@store.vn'),
 (N'Cầu lông Thành Công',           N'5 Hoàng Văn Thụ, Tân Bình, TP.HCM',     '0285678901', 'thanhcong@caulong.vn'),
 (N'Phụ kiện thể thao Hải Yến',     N'130 Lê Văn Sỹ, Q.3, TP.HCM',            '0286789012', 'haiyen@phukien.vn');

-- Nhan vien (5)
INSERT INTO NhanVien (HoTen, SDT, Email, DiaChi, ChucVu, Luong, NgayVaoLam) VALUES
 (N'Nguyễn Quản Trị',  '0901000001', 'admin@shopcaulong.vn',    N'Q.1, TP.HCM',       N'Quản lý',               15000000, '2024-01-02'),
 (N'Trần Thị Quản Lý', '0901000002', 'quanly@shopcaulong.vn',   N'Q.3, TP.HCM',       N'Quản lý',               12000000, '2024-03-01'),
 (N'Lê Văn Bán',       '0901000003', 'nhanvien@shopcaulong.vn', N'Q.5, TP.HCM',       N'Nhân viên bán hàng',     8000000, '2025-02-15'),
 (N'Phạm Thị Kho',     '0901000004', 'kho@shopcaulong.vn',      N'Tân Bình, TP.HCM',  N'Kho',                    8500000, '2025-06-01'),
 (N'Võ Văn Nghỉ',      '0901000005', 'nghi@shopcaulong.vn',     N'Gò Vấp, TP.HCM',    N'Nhân viên bán hàng',     7500000, '2025-09-10');

/* Tai khoan (5) - mat khau la chuoi BCrypt.
   3 chuoi HASH_ ben duoi duoc init-solution.bat tu thay bang chuoi bam that
   (tools/HashTool --patch). Neu chua thay, chay lai buoc do. Cac tai khoan con lai dung chung hash.
     admin    / Admin@123
     quanly   / Quanly@123
     nhanvien / Nhanvien@123                                                   */
DECLARE @HashAdmin    VARCHAR(255) = '<HASH_Admin@123>';
DECLARE @HashQuanLy   VARCHAR(255) = '<HASH_Quanly@123>';
DECLARE @HashNhanVien VARCHAR(255) = '<HASH_Nhanvien@123>';

INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro, MaNV, TrangThai) VALUES
 ('admin',     @HashAdmin,    'ADMIN',    1, 1),
 ('quanly',    @HashQuanLy,   'QUANLY',   2, 1),
 ('nhanvien',  @HashNhanVien, 'NHANVIEN', 3, 1),
 ('kho',       @HashQuanLy,   'QUANLY',   4, 1),
 ('nhanvien2', @HashNhanVien, 'NHANVIEN', 5, 0);   -- tai khoan bi khoa de demo

-- San pham khoi dau (12) - ton kho = 0, phieu nhap ben duoi se tang ton.
-- SV2/SV4 bo sung them cho du >= 40 san pham.
INSERT INTO SanPham (TenSP, MaDM, ThuongHieu, DonGiaBan, DonViTinh, HinhAnh, MoTa) VALUES
 (N'Vợt Yonex Astrox 88D Pro',     1, N'Yonex',  4290000, N'Cây',   'astrox88d.jpg',  N'4U, thân cứng, thiên công'),
 (N'Vợt Yonex Nanoflare 700',      1, N'Yonex',  3590000, N'Cây',   'nf700.jpg',      N'4U, thân trung bình, thiên tốc độ'),
 (N'Vợt Victor Thruster K 9900',   1, N'Victor', 3890000, N'Cây',   'tk9900.jpg',     N'3U, thân cứng'),
 (N'Vợt Li-Ning Axforce 80',       1, N'Li-Ning',3690000, N'Cây',   'axforce80.jpg',  N'4U, cân bằng'),
 (N'Cầu Yonex AS-30',              2, N'Yonex',   890000, N'Ống',   'as30.jpg',       N'Ống 12 quả, lông ngỗng'),
 (N'Cầu Thành Công 77',            2, N'Thành Công', 245000, N'Ống', 'tc77.jpg',      N'Ống 12 quả, tập luyện'),
 (N'Giày Yonex Power Cushion 65Z3',3, N'Yonex',  2990000, N'Đôi',   'pc65z3.jpg',     N'Size 39-44'),
 (N'Giày Victor A970 ACE',         3, N'Victor', 2590000, N'Đôi',   'a970.jpg',       N'Size 38-44'),
 (N'Áo thi đấu Li-Ning AAYU',      4, N'Li-Ning', 450000, N'Cái',   'aayu.jpg',       N'Size S-XXL'),
 (N'Quần short Yonex 15048',       4, N'Yonex',   390000, N'Cái',   'short15048.jpg', N'Size S-XL'),
 (N'Cước Yonex BG80',              5, N'Yonex',   180000, N'Sợi',   'bg80.jpg',       N'0.68mm'),
 (N'Quấn cán Kumpoo KG-104',       5, N'Kumpoo',   35000, N'Cái',   'kg104.jpg',      N'Quấn cán gai');

-- Phieu nhap (6) trai deu cac thang - trigger tu cong ton va tinh TongTien
INSERT INTO PhieuNhap (NgayNhap, MaNCC, MaNV) VALUES
 ('2026-01-08', 1, 4),
 ('2026-02-20', 2, 4),
 ('2026-04-03', 3, 2),
 ('2026-06-15', 5, 4),
 ('2026-08-01', 6, 2),
 ('2026-09-22', 1, 4);

INSERT INTO ChiTietPhieuNhap (MaPN, MaSP, SoLuong, DonGia) VALUES
 (1, 1, 15, 3500000), (1, 2, 15, 2900000), (1, 5, 40, 700000), (1, 7, 12, 2400000),
 (2, 3, 10, 3100000), (2, 8, 12, 2050000),
 (3, 4, 10, 2950000), (3, 9, 40,  300000),
 (4, 6, 80,  180000),
 (5, 11, 100, 120000), (5, 12, 150, 20000),
 (6, 1, 10, 3500000), (6, 10, 30, 260000), (6, 5, 30, 700000);
GO

/* ---------------------------------------------------------------------
   PHAN SV2: KhachHang (>= 20, kh20@gmail.com bi khoa), them SanPham (>= 40), PhanHoi (>= 30)
   --------------------------------------------------------------------- */
-- TODO SV2

/* ---------------------------------------------------------------------
   PHAN SV3: CTKM (>= 3: 1 SANPHAM, 1 HOADON, 1 VOUCHER), KM_TheoSP, KM_TheoHD,
             KM_TheoVoucher, HoaDon + ChiTietHoaDon online (>= 20, du cac trang thai)
   --------------------------------------------------------------------- */
-- TODO SV3

/* ---------------------------------------------------------------------
   PHAN SV4: HoaDon tai quay (LoaiBan = N'Tại quầy') trai deu 6-12 thang cho bieu do
   --------------------------------------------------------------------- */
-- TODO SV4

-- Kiem tra nhanh
SELECT TenSP, SoLuongTon FROM SanPham;
SELECT MaPN, NgayNhap, TongTien FROM PhieuNhap;
GO
