/* =====================================================================
   BadmintonShop.sql - SCRIPT CSDL DAY DU (ban giao, chay 1 lan tren may trong)
   = 01_CreateDatabase.sql + 02_Triggers.sql + 03_SampleData.sql (noi nguyen van).
   Sua CSDL thi sua 3 file goc roi ghep lai file nay (xem README).
   Chay: sqlcmd -S . -E -I -f 65001 -i database\BadmintonShop.sql
   LUU Y: XOA va TAO LAI database BadmintonShop neu da ton tai.
   ===================================================================== */

/* =====================================================================
   BadmintonShop - 01_CreateDatabase.sql
   Tao CSDL, bang, khoa chinh, khoa ngoai, rang buoc CHECK, chi muc.
   Nguoi phu trach: SV1
   Chay lan luot: 01_CreateDatabase.sql -> 02_Triggers.sql -> 03_SampleData.sql
   LUU Y: script nay XOA va TAO LAI database BadmintonShop neu da ton tai.
   ===================================================================== */
USE master;
GO
IF DB_ID(N'BadmintonShop') IS NOT NULL
BEGIN
    ALTER DATABASE BadmintonShop SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BadmintonShop;
END
GO
CREATE DATABASE BadmintonShop;
GO
USE BadmintonShop;
GO

-- ====== DANH MUC, NHA CUNG CAP, NHAN VIEN, TAI KHOAN ======
CREATE TABLE DanhMuc (
    MaDM        INT IDENTITY(1,1) PRIMARY KEY,
    TenDanhMuc  NVARCHAR(100) NOT NULL UNIQUE,      -- Vợt, Cầu, Giày, Quần áo, Phụ kiện
    MoTa        NVARCHAR(255) NULL
);

CREATE TABLE NhaCungCap (
    MaNCC   INT IDENTITY(1,1) PRIMARY KEY,
    TenNCC  NVARCHAR(150) NOT NULL,
    DiaChi  NVARCHAR(255) NULL,
    SDT     VARCHAR(15)   NULL,
    Email   VARCHAR(100)  NULL
);

CREATE TABLE NhanVien (
    MaNV        INT IDENTITY(1,1) PRIMARY KEY,
    HoTen       NVARCHAR(100) NOT NULL,
    SDT         VARCHAR(15)   NULL,
    Email       VARCHAR(100)  NULL,
    DiaChi      NVARCHAR(255) NULL,
    ChucVu      NVARCHAR(50)  NOT NULL,             -- Quản lý / Nhân viên bán hàng / Kho
    Luong       DECIMAL(18,0) NULL CHECK (Luong IS NULL OR Luong >= 0),
    NgayVaoLam  DATE NOT NULL DEFAULT GETDATE()
);

CREATE TABLE TaiKhoan (
    MaTK        INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap VARCHAR(50)  NOT NULL UNIQUE,
    MatKhau     VARCHAR(255) NOT NULL,              -- chuoi da bam BCrypt, KHONG luu tho
    VaiTro      VARCHAR(20)  NOT NULL
                CONSTRAINT CK_TaiKhoan_VaiTro CHECK (VaiTro IN ('ADMIN','QUANLY','NHANVIEN')),
    MaNV        INT NOT NULL UNIQUE,                -- 1 nhan vien co toi da 1 tai khoan (1 - 0..1)
    TrangThai   BIT NOT NULL DEFAULT 1,             -- 0 = bi khoa
    CONSTRAINT FK_TaiKhoan_NhanVien FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV)
);

-- ====== KHACH HANG, SAN PHAM ======
CREATE TABLE KhachHang (
    MaKH        INT IDENTITY(1,1) PRIMARY KEY,
    HoTen       NVARCHAR(100) NOT NULL,
    SDT         VARCHAR(15)   NULL,
    Email       VARCHAR(100)  NOT NULL UNIQUE,
    MatKhau     VARCHAR(255)  NOT NULL,
    DiaChi      NVARCHAR(255) NULL,
    NgayDangKy  DATETIME NOT NULL DEFAULT GETDATE(),
    TrangThai   BIT NOT NULL DEFAULT 1              -- 0 = bi khoa
);

CREATE TABLE SanPham (
    MaSP        INT IDENTITY(1,1) PRIMARY KEY,
    TenSP       NVARCHAR(150) NOT NULL,
    MaDM        INT NOT NULL,                       -- "Loai SP" tren ERD
    ThuongHieu  NVARCHAR(50)  NULL,                 -- Yonex, Victor, Li-Ning...
    DonGiaBan   DECIMAL(18,0) NOT NULL CHECK (DonGiaBan >= 0),
    SoLuongTon  INT NOT NULL DEFAULT 0 CHECK (SoLuongTon >= 0),
    DonViTinh   NVARCHAR(20)  NOT NULL DEFAULT N'Cái',
    HinhAnh     NVARCHAR(255) NULL,
    MoTa        NVARCHAR(MAX) NULL,
    TrangThai   NVARCHAR(20)  NOT NULL DEFAULT N'Đang bán',
    CONSTRAINT FK_SanPham_DanhMuc FOREIGN KEY (MaDM) REFERENCES DanhMuc(MaDM)
);

-- ====== NHAP HANG ======
CREATE TABLE PhieuNhap (
    MaPN      INT IDENTITY(1,1) PRIMARY KEY,
    NgayNhap  DATETIME NOT NULL DEFAULT GETDATE(),
    MaNCC     INT NOT NULL,
    MaNV      INT NOT NULL,
    TongTien  DECIMAL(18,0) NOT NULL DEFAULT 0,     -- trigger tu tinh
    CONSTRAINT FK_PhieuNhap_NCC FOREIGN KEY (MaNCC) REFERENCES NhaCungCap(MaNCC),
    CONSTRAINT FK_PhieuNhap_NV  FOREIGN KEY (MaNV)  REFERENCES NhanVien(MaNV)
);

CREATE TABLE ChiTietPhieuNhap (
    MaPN      INT NOT NULL,
    MaSP      INT NOT NULL,
    SoLuong   INT NOT NULL CHECK (SoLuong > 0),
    DonGia    DECIMAL(18,0) NOT NULL CHECK (DonGia >= 0),
    ThanhTien AS (SoLuong * DonGia) PERSISTED,
    CONSTRAINT PK_CTPN PRIMARY KEY (MaPN, MaSP),
    CONSTRAINT FK_CTPN_PN FOREIGN KEY (MaPN) REFERENCES PhieuNhap(MaPN) ON DELETE CASCADE,
    CONSTRAINT FK_CTPN_SP FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);

-- ====== KHUYEN MAI ======
CREATE TABLE CTKM (
    MaCTKM      INT IDENTITY(1,1) PRIMARY KEY,
    TenCTKM     NVARCHAR(150) NOT NULL,
    NgayBatDau  DATE NOT NULL,
    NgayKetThuc DATE NOT NULL,
    LoaiKM      VARCHAR(20) NOT NULL
                CONSTRAINT CK_CTKM_Loai CHECK (LoaiKM IN ('SANPHAM','HOADON','VOUCHER')),
    TrangThai   BIT NOT NULL DEFAULT 1,
    CONSTRAINT CK_CTKM_Ngay CHECK (NgayKetThuc >= NgayBatDau)
);

CREATE TABLE KM_TheoSP (            -- giam gia theo tung san pham
    MaCTKM      INT NOT NULL,
    MaSP        INT NOT NULL,
    PhanTramGG  DECIMAL(5,2) NOT NULL CHECK (PhanTramGG BETWEEN 0 AND 100),
    CONSTRAINT PK_KMSP PRIMARY KEY (MaCTKM, MaSP),
    CONSTRAINT FK_KMSP_CTKM FOREIGN KEY (MaCTKM) REFERENCES CTKM(MaCTKM) ON DELETE CASCADE,
    CONSTRAINT FK_KMSP_SP   FOREIGN KEY (MaSP)   REFERENCES SanPham(MaSP)
);

CREATE TABLE KM_TheoHD (            -- giam theo tong tien hoa don; 1 CTKM co the nhieu bac muc tien
    MaCTKM           INT NOT NULL,
    TongTienToiThieu DECIMAL(18,0) NOT NULL CHECK (TongTienToiThieu >= 0),
    PhanTramGG       DECIMAL(5,2) NOT NULL CHECK (PhanTramGG BETWEEN 0 AND 100),
    CONSTRAINT PK_KMHD PRIMARY KEY (MaCTKM, TongTienToiThieu),
    CONSTRAINT FK_KMHD_CTKM FOREIGN KEY (MaCTKM) REFERENCES CTKM(MaCTKM) ON DELETE CASCADE
);

CREATE TABLE KM_TheoVoucher (       -- giam theo ma voucher
    MaCTKM      INT NOT NULL,
    MaVoucher   VARCHAR(30) NOT NULL,
    PhanTramGG  DECIMAL(5,2) NOT NULL CHECK (PhanTramGG BETWEEN 0 AND 100),
    SoLuong     INT NOT NULL DEFAULT 1 CHECK (SoLuong >= 0),
    DaDung      INT NOT NULL DEFAULT 0,
    CONSTRAINT PK_KMVC PRIMARY KEY (MaVoucher),
    CONSTRAINT CK_KMVC_DaDung CHECK (DaDung BETWEEN 0 AND SoLuong),
    CONSTRAINT FK_KMVC_CTKM FOREIGN KEY (MaCTKM) REFERENCES CTKM(MaCTKM) ON DELETE CASCADE
);

-- ====== BAN HANG ======
CREATE TABLE HoaDon (
    MaHD        INT IDENTITY(1,1) PRIMARY KEY,
    MaKH        INT NULL,                           -- NULL khi ban tai quay cho khach vang lai
    MaNV        INT NULL,                           -- NULL khi khach tu dat tren web
    NgayLap     DATETIME NOT NULL DEFAULT GETDATE(),
    LoaiBan     NVARCHAR(20) NOT NULL DEFAULT N'Online',     -- Online / Tại quầy
    HinhThucTT  NVARCHAR(20) NOT NULL DEFAULT N'COD',        -- COD / Online / Tiền mặt
    DiaChiGiao  NVARCHAR(255) NULL,
    MaCTKM      INT NULL,                           -- CTKM theo HD hoac voucher da ap dung
    TienGiamGia DECIMAL(18,0) NOT NULL DEFAULT 0 CHECK (TienGiamGia >= 0),  -- giam o cap hoa don (ERD)
    TongTien    DECIMAL(18,0) NOT NULL DEFAULT 0,   -- trigger tu tinh = tong ThanhTien - TienGiamGia
    TrangThai   NVARCHAR(30) NOT NULL DEFAULT N'Chờ xác nhận',
    CONSTRAINT CK_HoaDon_TrangThai CHECK (TrangThai IN
        (N'Chờ xác nhận', N'Đã xác nhận', N'Đang giao', N'Hoàn thành', N'Đã hủy')),
    CONSTRAINT FK_HoaDon_KH   FOREIGN KEY (MaKH)   REFERENCES KhachHang(MaKH),
    CONSTRAINT FK_HoaDon_NV   FOREIGN KEY (MaNV)   REFERENCES NhanVien(MaNV),
    CONSTRAINT FK_HoaDon_CTKM FOREIGN KEY (MaCTKM) REFERENCES CTKM(MaCTKM)
);

CREATE TABLE ChiTietHoaDon (
    MaHD      INT NOT NULL,
    MaSP      INT NOT NULL,
    SoLuong   INT NOT NULL CHECK (SoLuong > 0),
    DonGia    DECIMAL(18,0) NOT NULL CHECK (DonGia >= 0),
    GiamGia   DECIMAL(5,2) NOT NULL DEFAULT 0 CHECK (GiamGia BETWEEN 0 AND 100),  -- % giam theo SP
    ThanhTien AS (CAST(SoLuong * DonGia * (1 - GiamGia / 100.0) AS DECIMAL(18,0))) PERSISTED,
    CONSTRAINT PK_CTHD PRIMARY KEY (MaHD, MaSP),
    CONSTRAINT FK_CTHD_HD FOREIGN KEY (MaHD) REFERENCES HoaDon(MaHD) ON DELETE CASCADE,
    CONSTRAINT FK_CTHD_SP FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);

-- ====== PHAN HOI / DANH GIA ======
CREATE TABLE PhanHoi (
    MaPH      INT IDENTITY(1,1) PRIMARY KEY,
    MaKH      INT NOT NULL,
    MaSP      INT NOT NULL,
    SoSao     TINYINT NOT NULL CHECK (SoSao BETWEEN 1 AND 5),
    NoiDung   NVARCHAR(MAX) NULL,
    NgayGui   DATETIME NOT NULL DEFAULT GETDATE(),
    TrangThai NVARCHAR(20) NOT NULL DEFAULT N'Hiển thị',
    CONSTRAINT FK_PhanHoi_KH FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
    CONSTRAINT FK_PhanHoi_SP FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);

-- ====== CHI MUC ======
CREATE INDEX IX_SanPham_DanhMuc  ON SanPham(MaDM);
CREATE INDEX IX_HoaDon_NgayLap   ON HoaDon(NgayLap);
CREATE INDEX IX_HoaDon_KH        ON HoaDon(MaKH);
CREATE INDEX IX_PhieuNhap_Ngay   ON PhieuNhap(NgayNhap);
CREATE INDEX IX_PhanHoi_SP       ON PhanHoi(MaSP);
GO

/* =====================================================================
   BadmintonShop - 02_Triggers.sql
   Trigger cap nhat ton kho + tong tien.  Nguoi phu trach: SV1
   Quy uoc cua nhom: KHONG sua/xoa dong chi tiet; muon bo thi HUY ca don.
   ===================================================================== */
USE BadmintonShop;
GO

-- 1) Nhap hang: tang ton kho + tinh lai tong tien phieu nhap
CREATE OR ALTER TRIGGER trg_CTPN_TangTon ON ChiTietPhieuNhap
AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;

    -- Gop theo MaSP: 1 lenh INSERT nhieu dong cung MaSP (nhieu phieu) phai cong du tat ca
    UPDATE sp SET sp.SoLuongTon = sp.SoLuongTon + x.SL
    FROM SanPham sp
    JOIN (SELECT MaSP, SUM(SoLuong) AS SL FROM inserted GROUP BY MaSP) x ON x.MaSP = sp.MaSP;

    UPDATE pn SET pn.TongTien = (SELECT ISNULL(SUM(ThanhTien), 0)
                                 FROM ChiTietPhieuNhap WHERE MaPN = pn.MaPN)
    FROM PhieuNhap pn WHERE pn.MaPN IN (SELECT MaPN FROM inserted);
END;
GO

-- 2) Ban hang (web + POS): chan ban qua ton, giam ton kho, tinh lai tong tien hoa don
CREATE OR ALTER TRIGGER trg_CTHD_GiamTon ON ChiTietHoaDon
AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;

    -- Gop theo MaSP: 1 lenh INSERT nhieu dong cung MaSP (nhieu hoa don) phai tru du tat ca
    IF EXISTS (SELECT 1 FROM (SELECT MaSP, SUM(SoLuong) AS SL FROM inserted GROUP BY MaSP) x
               JOIN SanPham sp ON sp.MaSP = x.MaSP
               WHERE sp.SoLuongTon < x.SL)
    BEGIN
        RAISERROR (N'San pham khong du ton kho.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    UPDATE sp SET sp.SoLuongTon = sp.SoLuongTon - x.SL
    FROM SanPham sp
    JOIN (SELECT MaSP, SUM(SoLuong) AS SL FROM inserted GROUP BY MaSP) x ON x.MaSP = sp.MaSP;

    UPDATE hd SET hd.TongTien =
        CASE WHEN x.Tong - hd.TienGiamGia < 0 THEN 0 ELSE x.Tong - hd.TienGiamGia END
    FROM HoaDon hd
    JOIN (SELECT MaHD, SUM(ThanhTien) AS Tong FROM ChiTietHoaDon
          WHERE MaHD IN (SELECT MaHD FROM inserted) GROUP BY MaHD) x ON x.MaHD = hd.MaHD;
END;
GO

-- 3) Hoa don: huy don thi tra hang ve kho; khong cho "mo lai" don da huy;
--    doi TienGiamGia thi tinh lai TongTien.
CREATE OR ALTER TRIGGER trg_HoaDon_CapNhat ON HoaDon
AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(TrangThai)
    BEGIN
        IF EXISTS (SELECT 1 FROM inserted i JOIN deleted d ON d.MaHD = i.MaHD
                   WHERE d.TrangThai = N'Đã hủy' AND i.TrangThai <> N'Đã hủy')
        BEGIN
            RAISERROR (N'Khong the mo lai don hang da huy.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Don vua chuyen sang "Đã hủy": cong lai ton kho
        UPDATE sp SET sp.SoLuongTon = sp.SoLuongTon + x.SL
        FROM SanPham sp
        JOIN (SELECT ct.MaSP, SUM(ct.SoLuong) AS SL
              FROM ChiTietHoaDon ct
              JOIN inserted i ON i.MaHD = ct.MaHD
              JOIN deleted  d ON d.MaHD = i.MaHD
              WHERE i.TrangThai = N'Đã hủy' AND d.TrangThai <> N'Đã hủy'
              GROUP BY ct.MaSP) x ON x.MaSP = sp.MaSP;
    END

    IF UPDATE(TienGiamGia)
    BEGIN
        UPDATE hd SET hd.TongTien =
            CASE WHEN ISNULL(x.Tong, 0) - hd.TienGiamGia < 0 THEN 0
                 ELSE ISNULL(x.Tong, 0) - hd.TienGiamGia END
        FROM HoaDon hd
        LEFT JOIN (SELECT MaHD, SUM(ThanhTien) AS Tong FROM ChiTietHoaDon GROUP BY MaHD) x
               ON x.MaHD = hd.MaHD
        WHERE hd.MaHD IN (SELECT MaHD FROM inserted);
    END
END;
GO

-- 4) Thu tuc kiem tra ton kho (SV3 / SV4 goi truoc khi dat hang / tinh tien POS)
CREATE OR ALTER PROCEDURE sp_KiemTraTonKho
    @MaSP INT, @SoLuong INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN SoLuongTon >= @SoLuong THEN 1 ELSE 0 END AS DuHang, SoLuongTon
    FROM SanPham WHERE MaSP = @MaSP;
END;
GO

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

