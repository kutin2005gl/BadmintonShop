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
DECLARE @HashAdmin    VARCHAR(255) = '$2a$11$rqyt7g0mbAXFJe3EguhF.eXDov6ABDcdRjdfWtG19Qoyf8cC.EGBy';
DECLARE @HashQuanLy   VARCHAR(255) = '$2a$11$Lcck6fMwKmmglip/bgzkzOJkpRLNCCyCvssvpaZIHPKwbTP6QSPMy';
DECLARE @HashNhanVien VARCHAR(255) = '$2a$11$7f3ZzY2JhUj9sbPzPIlNvO4OKwQSJOoouBb.h405mGOp6Y/Ac5IUy';

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
-- Khach hang (20): mat khau chung Khach@123 (BCrypt). kh20 bi khoa de demo.
DECLARE @HashKhach VARCHAR(255) = '$2a$11$TpPQ4ebyyJrq1vST5eoba.ugzkM1yyHXN5m4Fp8q4/17Sp20LQwb6';
INSERT INTO KhachHang (HoTen, SDT, Email, MatKhau, DiaChi, NgayDangKy, TrangThai) VALUES
 (N'Nguyễn Minh Anh', '0912000001', 'kh01@gmail.com', @HashKhach, N'12 Nguyễn Trãi, Q.5, TP.HCM', '2026-03-05', 1),
 (N'Trần Quốc Bảo', '0912000002', 'kh02@gmail.com', @HashKhach, N'45 Lê Lợi, Q.1, TP.HCM', '2026-03-18', 1),
 (N'Lê Thị Cẩm Tú', '0912000003', 'kh03@gmail.com', @HashKhach, N'78 Cách Mạng Tháng 8, Q.3, TP.HCM', '2026-04-02', 1),
 (N'Phạm Hoàng Dũng', '0912000004', 'kh04@gmail.com', @HashKhach, N'9 Phạm Văn Đồng, TP.Thủ Đức, TP.HCM', '2026-04-20', 1),
 (N'Võ Thị Ngọc Hà', '0912000005', 'kh05@gmail.com', @HashKhach, N'33 Quang Trung, Gò Vấp, TP.HCM', '2026-05-06', 1),
 (N'Đặng Văn Hùng', '0912000006', 'kh06@gmail.com', @HashKhach, N'27 Hoàng Văn Thụ, Tân Bình, TP.HCM', '2026-05-22', 1),
 (N'Bùi Thanh Khoa', '0912000007', 'kh07@gmail.com', @HashKhach, N'101 Lý Thường Kiệt, Q.10, TP.HCM', '2026-06-03', 1),
 (N'Huỳnh Thị Lan', '0912000008', 'kh08@gmail.com', @HashKhach, N'56 Nguyễn Thị Minh Khai, Q.3, TP.HCM', '2026-06-17', 1),
 (N'Ngô Đức Long', '0912000009', 'kh09@gmail.com', @HashKhach, N'18 Trần Hưng Đạo, Q.1, TP.HCM', '2026-06-30', 1),
 (N'Đỗ Thị Mai', '0912000010', 'kh10@gmail.com', @HashKhach, N'240 Lê Văn Sỹ, Phú Nhuận, TP.HCM', '2026-07-08', 1),
 (N'Dương Văn Nam', '0912000011', 'kh11@gmail.com', @HashKhach, N'72 Nguyễn Văn Cừ, Q.5, TP.HCM', '2026-07-19', 1),
 (N'Lý Thị Oanh', '0912000012', 'kh12@gmail.com', @HashKhach, N'15 Phan Văn Trị, Bình Thạnh, TP.HCM', '2026-07-28', 1),
 (N'Phan Thanh Phong', '0912000013', 'kh13@gmail.com', @HashKhach, N'88 Tô Hiến Thành, Q.10, TP.HCM', '2026-08-04', 1),
 (N'Trương Thị Quỳnh', '0912000014', 'kh14@gmail.com', @HashKhach, N'64 Nguyễn Oanh, Gò Vấp, TP.HCM', '2026-08-15', 1),
 (N'Hồ Văn Sơn', '0912000015', 'kh15@gmail.com', @HashKhach, N'31 Võ Văn Ngân, TP.Thủ Đức, TP.HCM', '2026-08-27', 1),
 (N'Mai Thị Thảo', '0912000016', 'kh16@gmail.com', @HashKhach, N'49 Bà Hom, Q.6, TP.HCM', '2026-09-05', 1),
 (N'Cao Minh Tuấn', '0912000017', 'kh17@gmail.com', @HashKhach, N'120 Âu Cơ, Tân Phú, TP.HCM', '2026-09-14', 1),
 (N'Vũ Thị Uyên', '0912000018', 'kh18@gmail.com', @HashKhach, N'7 Huỳnh Tấn Phát, Q.7, TP.HCM', '2026-09-25', 1),
 (N'Đinh Quang Vinh', '0912000019', 'kh19@gmail.com', @HashKhach, N'95 Nguyễn Thị Thập, Q.7, TP.HCM', '2026-10-03', 1),
 (N'Lương Thị Yến', '0912000020', 'kh20@gmail.com', @HashKhach, N'3 Lê Đức Thọ, Gò Vấp, TP.HCM', '2026-10-10', 0);

-- San pham bo sung (28) -> tong 40. Ton kho = 0, cac phieu nhap ben duoi se tang ton.
INSERT INTO SanPham (TenSP, MaDM, ThuongHieu, DonGiaBan, DonViTinh, HinhAnh, MoTa) VALUES
 (N'Vợt Yonex Astrox 99 Pro', 1, N'Yonex', 4590000, N'Cây', 'astrox99pro.jpg', N'4U, thân cứng, thiên công'),
 (N'Vợt Yonex Arcsaber 11 Pro', 1, N'Yonex', 3990000, N'Cây', 'arc11pro.jpg', N'4U, thân trung bình, kiểm soát tốt'),
 (N'Vợt Victor Auraspeed 90K', 1, N'Victor', 3490000, N'Cây', 'auraspeed90k.jpg', N'4U, thiên tốc độ'),
 (N'Vợt Victor Jetspeed S 12', 1, N'Victor', 2790000, N'Cây', 'jetspeeds12.jpg', N'4U, phù hợp người chơi phong trào'),
 (N'Vợt Li-Ning Windstorm 72', 1, N'Li-Ning', 2890000, N'Cây', 'windstorm72.jpg', N'4U, nhẹ đầu, linh hoạt'),
 (N'Vợt Kumpoo Power Control 980', 1, N'Kumpoo', 1250000, N'Cây', 'pc980.jpg', N'3U, giá mềm cho người mới'),
 (N'Cầu Victor Gold No.1', 2, N'Victor', 520000, N'Ống', 'goldno1.jpg', N'Ống 12 quả, lông ngỗng, thi đấu'),
 (N'Cầu Li-Ning A+300', 2, N'Li-Ning', 480000, N'Ống', 'aplus300.jpg', N'Ống 12 quả, độ bền cao'),
 (N'Cầu Thành Công 88', 2, N'Thành Công', 310000, N'Ống', 'tc88.jpg', N'Ống 12 quả, tập luyện nâng cao'),
 (N'Cầu nhựa Yonex Mavis 350', 2, N'Yonex', 290000, N'Ống', 'mavis350.jpg', N'Ống 6 quả, cầu nhựa tập luyện'),
 (N'Giày Yonex Eclipsion Z3', 3, N'Yonex', 2790000, N'Đôi', 'eclipsionz3.jpg', N'Size 38-44, đế bám sân tốt'),
 (N'Giày Victor P9200', 3, N'Victor', 3190000, N'Đôi', 'p9200.jpg', N'Size 38-45, hỗ trợ cổ chân'),
 (N'Giày Li-Ning Saga', 3, N'Li-Ning', 1850000, N'Đôi', 'saga.jpg', N'Size 38-44, êm chân'),
 (N'Giày Kumpoo KH-E36', 3, N'Kumpoo', 990000, N'Đôi', 'khe36.jpg', N'Size 37-43, giá phổ thông'),
 (N'Giày Yonex Comfort Z3', 3, N'Yonex', 2390000, N'Đôi', 'comfortz3.jpg', N'Size 36-42, form ôm chân'),
 (N'Áo thi đấu Yonex 10504', 4, N'Yonex', 420000, N'Cái', 'ao10504.jpg', N'Size S-XXL, thấm hút mồ hôi'),
 (N'Áo thi đấu Victor T-33001', 4, N'Victor', 380000, N'Cái', 't33001.jpg', N'Size S-XXL'),
 (N'Váy cầu lông Li-Ning AQXT', 4, N'Li-Ning', 520000, N'Cái', 'aqxt.jpg', N'Size S-XL, kèm quần trong'),
 (N'Quần short Victor R-3096', 4, N'Victor', 360000, N'Cái', 'r3096.jpg', N'Size S-XL'),
 (N'Áo thi đấu Kumpoo KW-2102', 4, N'Kumpoo', 290000, N'Cái', 'kw2102.jpg', N'Size S-XXL'),
 (N'Quần short Li-Ning AKSP', 4, N'Li-Ning', 340000, N'Cái', 'aksp.jpg', N'Size S-XL'),
 (N'Cước Victor VBS-66N', 5, N'Victor', 170000, N'Sợi', 'vbs66n.jpg', N'0.66mm, nảy tốt'),
 (N'Cước Li-Ning No.1', 5, N'Li-Ning', 150000, N'Sợi', 'lnno1.jpg', N'0.65mm, bền'),
 (N'Quấn cán Yonex AC102', 5, N'Yonex', 55000, N'Cái', 'ac102.jpg', N'Quấn cán thấm hút'),
 (N'Balo Yonex BA82212', 5, N'Yonex', 790000, N'Cái', 'ba82212.jpg', N'Balo đựng vợt và giày'),
 (N'Túi vợt Victor BR9213', 5, N'Victor', 1290000, N'Cái', 'br9213.jpg', N'Túi 6 cây, 2 ngăn'),
 (N'Băng trán Yonex', 5, N'Yonex', 90000, N'Cái', 'bangtran.jpg', N'Cotton co giãn'),
 (N'Vớ Victor SK', 5, N'Victor', 150000, N'Đôi', 'vosk.jpg', N'Vớ dày, chống trượt');

-- Phieu nhap cho cac san pham moi (moi hang 1 phieu, nhan vien kho = 4)
DECLARE @pn INT;
INSERT INTO PhieuNhap (NgayNhap, MaNCC, MaNV) VALUES ('2026-10-05', 1, 4);
SET @pn = SCOPE_IDENTITY();
INSERT INTO ChiTietPhieuNhap (MaPN, MaSP, SoLuong, DonGia) VALUES
 (@pn, 13, 12, 3213000),
 (@pn, 14, 12, 2793000),
 (@pn, 22, 40, 203000),
 (@pn, 23, 10, 1953000),
 (@pn, 27, 10, 1673000),
 (@pn, 28, 25, 294000),
 (@pn, 36, 30, 38000),
 (@pn, 37, 30, 553000),
 (@pn, 39, 30, 63000);
INSERT INTO PhieuNhap (NgayNhap, MaNCC, MaNV) VALUES ('2026-10-08', 2, 4);
SET @pn = SCOPE_IDENTITY();
INSERT INTO ChiTietPhieuNhap (MaPN, MaSP, SoLuong, DonGia) VALUES
 (@pn, 15, 12, 2443000),
 (@pn, 16, 12, 1953000),
 (@pn, 19, 40, 364000),
 (@pn, 24, 10, 2233000),
 (@pn, 29, 25, 266000),
 (@pn, 31, 25, 252000),
 (@pn, 34, 30, 119000),
 (@pn, 38, 30, 903000),
 (@pn, 40, 30, 105000);
INSERT INTO PhieuNhap (NgayNhap, MaNCC, MaNV) VALUES ('2026-10-10', 3, 4);
SET @pn = SCOPE_IDENTITY();
INSERT INTO ChiTietPhieuNhap (MaPN, MaSP, SoLuong, DonGia) VALUES
 (@pn, 17, 12, 2023000),
 (@pn, 20, 40, 336000),
 (@pn, 25, 10, 1295000),
 (@pn, 30, 25, 364000),
 (@pn, 33, 25, 238000),
 (@pn, 35, 30, 105000);
INSERT INTO PhieuNhap (NgayNhap, MaNCC, MaNV) VALUES ('2026-10-12', 4, 4);
SET @pn = SCOPE_IDENTITY();
INSERT INTO ChiTietPhieuNhap (MaPN, MaSP, SoLuong, DonGia) VALUES
 (@pn, 18, 12, 875000),
 (@pn, 26, 10, 693000),
 (@pn, 32, 25, 203000);
INSERT INTO PhieuNhap (NgayNhap, MaNCC, MaNV) VALUES ('2026-10-12', 5, 4);
SET @pn = SCOPE_IDENTITY();
INSERT INTO ChiTietPhieuNhap (MaPN, MaSP, SoLuong, DonGia) VALUES
 (@pn, 21, 40, 217000);
GO

/* ---------------------------------------------------------------------
   PHAN SV3: CTKM (>= 3: 1 SANPHAM, 1 HOADON, 1 VOUCHER), KM_TheoSP, KM_TheoHD,
             KM_TheoVoucher, HoaDon + ChiTietHoaDon online (>= 20, du cac trang thai)
   --------------------------------------------------------------------- */
-- ===== SV3: CTKM + khuyen mai =====
SET NOCOUNT ON;
DECLARE @ctSP INT, @ctHD INT, @ctVC INT, @ctVCHet INT;

-- 1) Giam theo SAN PHAM: tat ca san pham Yonex giam 10% (con hieu luc)
INSERT INTO CTKM (TenCTKM, NgayBatDau, NgayKetThuc, LoaiKM, TrangThai)
VALUES (N'Giảm giá sản phẩm Yonex tháng 10', '20261001', '20261231', 'SANPHAM', 1);
SET @ctSP = SCOPE_IDENTITY();
INSERT INTO KM_TheoSP (MaCTKM, MaSP, PhanTramGG)
SELECT @ctSP, MaSP, 10 FROM SanPham WHERE ThuongHieu = N'Yonex';

-- 2) Giam theo HOA DON: 3 bac muc (bac cao nhat dat duoc se duoc ap)
INSERT INTO CTKM (TenCTKM, NgayBatDau, NgayKetThuc, LoaiKM, TrangThai)
VALUES (N'Mua nhiều giảm nhiều 2026', '20260101', '20261231', 'HOADON', 1);
SET @ctHD = SCOPE_IDENTITY();
INSERT INTO KM_TheoHD (MaCTKM, TongTienToiThieu, PhanTramGG) VALUES
 (@ctHD, 2000000, 3), (@ctHD, 5000000, 5), (@ctHD, 10000000, 8);

-- 3) VOUCHER: con han (WELCOME10, BADMINTON20, HETLUOT da het luot) + 1 CTKM da het han (HE2026)
INSERT INTO CTKM (TenCTKM, NgayBatDau, NgayKetThuc, LoaiKM, TrangThai)
VALUES (N'Voucher chào khách mới', '20261001', '20261231', 'VOUCHER', 1);
SET @ctVC = SCOPE_IDENTITY();
INSERT INTO KM_TheoVoucher (MaCTKM, MaVoucher, PhanTramGG, SoLuong, DaDung) VALUES
 (@ctVC, 'WELCOME10',   10, 100, 0),
 (@ctVC, 'BADMINTON20', 20,   5, 0),
 (@ctVC, 'HETLUOT',     15,   3, 3);

INSERT INTO CTKM (TenCTKM, NgayBatDau, NgayKetThuc, LoaiKM, TrangThai)
VALUES (N'Voucher hè 2026 (đã hết hạn)', '20260601', '20260831', 'VOUCHER', 1);
SET @ctVCHet = SCOPE_IDENTITY();
INSERT INTO KM_TheoVoucher (MaCTKM, MaVoucher, PhanTramGG, SoLuong, DaDung) VALUES
 (@ctVCHet, 'HE2026', 10, 50, 0);

-- ===== SV3: khach hang cho don mau =====
-- Dung 5 khach dang hoat dong dau tien (SV2 them o tren); neu chua co thi tao khach thu (khong dang nhap duoc)
IF (SELECT COUNT(*) FROM KhachHang WHERE TrangThai = 1) < 5
    INSERT INTO KhachHang (HoTen, SDT, Email, MatKhau, DiaChi)
    SELECT v.HoTen, v.SDT, v.Email, 'KHONG_DANG_NHAP', v.DiaChi
    FROM (VALUES
        (N'Khách thử 1', '0900000001', 'sv3.test1@example.com', N'12 Nguyễn Văn Cừ, Q.5, TP.HCM'),
        (N'Khách thử 2', '0900000002', 'sv3.test2@example.com', N'45 Lê Lợi, Q.1, TP.HCM'),
        (N'Khách thử 3', '0900000003', 'sv3.test3@example.com', N'78 Cách Mạng Tháng 8, Q.3, TP.HCM'),
        (N'Khách thử 4', '0900000004', 'sv3.test4@example.com', N'9 Phạm Văn Đồng, Thủ Đức, TP.HCM'),
        (N'Khách thử 5', '0900000005', 'sv3.test5@example.com', N'33 Quang Trung, Gò Vấp, TP.HCM')
    ) v(HoTen, SDT, Email, DiaChi)
    WHERE NOT EXISTS (SELECT 1 FROM KhachHang k WHERE k.Email = v.Email);

CREATE TABLE #kh (idx INT, MaKH INT, DiaChi NVARCHAR(255));
INSERT INTO #kh (idx, MaKH, DiaChi)
SELECT x.idx, x.MaKH, ISNULL(x.DiaChi, N'123 Đường Mẫu, TP.HCM')
FROM (SELECT ROW_NUMBER() OVER (ORDER BY MaKH) AS idx, MaKH, DiaChi
      FROM KhachHang WHERE TrangThai = 1) x
WHERE x.idx <= 5;

-- ===== SV3: 24 don online, du 5 trang thai (ngay tinh lui tu 20/10/2026) =====
CREATE TABLE #don (stt INT, idx INT, ngay INT, tt NVARCHAR(20), trangThai NVARCHAR(30));
INSERT INTO #don (stt, idx, ngay, tt, trangThai) VALUES
 ( 1, 1, 120, N'COD',    N'Hoàn thành'),
 ( 2, 2, 110, N'Online', N'Hoàn thành'),
 ( 3, 3, 100, N'COD',    N'Hoàn thành'),
 ( 4, 4,  90, N'Online', N'Hoàn thành'),
 ( 5, 5,  80, N'COD',    N'Hoàn thành'),
 ( 6, 1,  70, N'COD',    N'Hoàn thành'),
 ( 7, 2,  60, N'Online', N'Hoàn thành'),
 ( 8, 3,  55, N'COD',    N'Hoàn thành'),
 ( 9, 4,  45, N'Online', N'Hoàn thành'),
 (10, 5,  40, N'COD',    N'Hoàn thành'),
 (11, 1,  30, N'COD',    N'Đã hủy'),
 (12, 2,  25, N'Online', N'Đã hủy'),
 (13, 3,  20, N'COD',    N'Đã hủy'),
 (14, 4,  15, N'COD',    N'Hoàn thành'),
 (15, 5,  12, N'Online', N'Đang giao'),
 (16, 1,  10, N'COD',    N'Đang giao'),
 (17, 2,   8, N'Online', N'Đang giao'),
 (18, 3,   6, N'COD',    N'Đã xác nhận'),
 (19, 4,   5, N'Online', N'Đã xác nhận'),
 (20, 5,   4, N'COD',    N'Đã xác nhận'),
 (21, 1,   3, N'COD',    N'Chờ xác nhận'),
 (22, 2,   2, N'Online', N'Chờ xác nhận'),   -- don nay dung voucher WELCOME10
 (23, 3,   1, N'COD',    N'Chờ xác nhận'),
 (24, 4,   0, N'COD',    N'Chờ xác nhận');

CREATE TABLE #ct (stt INT, MaSP INT, SL INT);
INSERT INTO #ct (stt, MaSP, SL) VALUES
 ( 1,5,2),( 1,6,1),
 ( 2,1,1),( 2,12,2),
 ( 3,9,2),( 3,10,1),
 ( 4,7,1),( 4,11,2),
 ( 5,3,1),
 ( 6,2,1),( 6,5,1),
 ( 7,8,1),( 7,12,3),
 ( 8,4,1),( 8,9,1),
 ( 9,6,3),( 9,11,3),
 (10,1,1),(10,7,1),
 (11,2,1),(11,10,2),
 (12,3,1),(12,5,1),
 (13,9,3),
 (14,5,2),(14,12,2),
 (15,1,1),(15,6,2),
 (16,8,1),(16,10,1),
 (17,4,1),
 (18,2,1),(18,9,2),
 (19,7,1),(19,11,1),
 (20,5,1),(20,6,1),(20,12,1),
 (21,1,1),(21,9,1),
 (22,10,2),(22,11,2),(22,12,2),
 (23,3,1),(23,5,1),
 (24,6,1),(24,12,1);

CREATE TABLE #map (stt INT, MaHD INT);
DECLARE @stt INT = 1, @maHD INT, @max INT = (SELECT MAX(stt) FROM #don);
WHILE @stt <= @max
BEGIN
    INSERT INTO HoaDon (MaKH, NgayLap, LoaiBan, HinhThucTT, DiaChiGiao)
    SELECT k.MaKH,
           DATEADD(HOUR, 9 + d.stt % 8, DATEADD(DAY, -d.ngay, CAST('20261020' AS DATETIME))),
           N'Online', d.tt, k.DiaChi
    FROM #don d JOIN #kh k ON k.idx = d.idx
    WHERE d.stt = @stt;
    SET @maHD = SCOPE_IDENTITY();

    -- trigger tru ton kho + tinh TongTien; giam theo SP (Yonex 10%) chi tu 01/10/2026 khi CTKM bat dau
    INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia, GiamGia)
    SELECT @maHD, c.MaSP, c.SL, sp.DonGiaBan,
           CASE WHEN sp.ThuongHieu = N'Yonex' AND h.NgayLap >= '20261001' THEN 10 ELSE 0 END
    FROM #ct c
    JOIN SanPham sp ON sp.MaSP = c.MaSP
    JOIN HoaDon h ON h.MaHD = @maHD
    WHERE c.stt = @stt;

    INSERT INTO #map (stt, MaHD) VALUES (@stt, @maHD);
    SET @stt += 1;
END

-- Giam theo bac hoa don (tru don 22 dung voucher); trigger tu tinh lai TongTien
UPDATE h
SET h.MaCTKM = b.MaCTKM,
    h.TienGiamGia = CAST(ROUND(h.TongTien * b.PhanTramGG / 100.0, 0) AS DECIMAL(18,0))
FROM HoaDon h
JOIN #map m ON m.MaHD = h.MaHD
CROSS APPLY (SELECT TOP 1 t.MaCTKM, t.PhanTramGG
             FROM KM_TheoHD t
             WHERE t.MaCTKM = @ctHD AND t.TongTienToiThieu <= h.TongTien
             ORDER BY t.TongTienToiThieu DESC) b
WHERE m.stt <> 22;

-- Don 22 dung voucher WELCOME10 (giam 10%), tang so luot da dung
UPDATE h
SET h.MaCTKM = @ctVC,
    h.TienGiamGia = CAST(ROUND(h.TongTien * 10 / 100.0, 0) AS DECIMAL(18,0))
FROM HoaDon h JOIN #map m ON m.MaHD = h.MaHD
WHERE m.stt = 22;
UPDATE KM_TheoVoucher SET DaDung = 1 WHERE MaVoucher = 'WELCOME10';

-- Chuyen sang trang thai cuoi (don "Đã hủy" duoc trigger hoan ton kho)
UPDATE h SET h.TrangThai = d.trangThai
FROM HoaDon h
JOIN #map m ON m.MaHD = h.MaHD
JOIN #don d ON d.stt = m.stt
WHERE d.trangThai <> N'Chờ xác nhận';

DROP TABLE #map; DROP TABLE #ct; DROP TABLE #don; DROP TABLE #kh;

SELECT TrangThai, COUNT(*) AS SoDon FROM HoaDon WHERE LoaiBan = N'Online' GROUP BY TrangThai;
SELECT MaCTKM, TenCTKM, LoaiKM, NgayBatDau, NgayKetThuc FROM CTKM;
GO

/* ---------------------------------------------------------------------
   PHAN SV4: HoaDon tai quay (LoaiBan = N'Tại quầy') trai deu 6-12 thang cho bieu do
   --------------------------------------------------------------------- */
-- TODO SV4

-- Kiem tra nhanh
SELECT TenSP, SoLuongTon FROM SanPham;
SELECT MaPN, NgayNhap, TongTien FROM PhieuNhap;
GO