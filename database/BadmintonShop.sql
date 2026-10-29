/* =====================================================================
   BadmintonShop.sql - SCRIPT CSDL DAY DU (ban giao, chay 1 lan tren may trong)
   = 01_CreateDatabase.sql + 02_Triggers.sql + 03_SampleData.sql + 04_SV4_Migration.sql (noi nguyen van).
   05_SV4_Demo.sql la du lieu demo tuy chon cua SV4, KHONG gop (chay rieng neu can).
   Sua CSDL thi sua cac file goc roi ghep lai file nay (xem README).
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

/* SV4: migration bổ sung, chạy sau 01/02/03 trên CSDL hiện có, chạy lại được.
   Không xóa/tạo lại dữ liệu. Sao lưu theo quy trình nhóm trước khi triển khai. */
USE BadmintonShop;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.HoaDon','LyDoHuy') IS NULL
    ALTER TABLE dbo.HoaDon ADD LyDoHuy NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.HoaDon','MaYeuCauPOS') IS NULL
    ALTER TABLE dbo.HoaDon ADD MaYeuCauPOS UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.HoaDon','TienKhachDua') IS NULL
    ALTER TABLE dbo.HoaDon ADD TienKhachDua DECIMAL(18,0) NULL;
COMMIT;
GO
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.HoaDon') AND name='UX_HoaDon_MaYeuCauPOS')
    CREATE UNIQUE INDEX UX_HoaDon_MaYeuCauPOS ON dbo.HoaDon(MaYeuCauPOS) WHERE MaYeuCauPOS IS NOT NULL;
GO

/* SV4: preserve the exact confirmed request for idempotent POS retry, including a losing voucher.
   Run after 04_SV4_Migration.sql. Additive, repeatable. Never automatically run on a store database. */
USE BadmintonShop;
GO
IF COL_LENGTH('dbo.HoaDon','DauVanTayPOS') IS NULL
    ALTER TABLE dbo.HoaDon ADD DauVanTayPOS VARCHAR(64) NULL;
GO

USE BadmintonShop;
GO

-- SV2: bang danh gia san pham (khach gui -> Cho duyet -> nhan vien Da duyet / Tu choi)
-- Chay lai duoc (bo qua neu bang da co). Khop DanhGiaRepository + Models/DanhGia.
IF OBJECT_ID(N'dbo.DanhGiaSanPham', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DanhGiaSanPham (
        MaDG        INT IDENTITY(1,1) PRIMARY KEY,
        MaSP        INT NOT NULL,
        MaKH        INT NOT NULL,
        SoSao       INT NOT NULL CONSTRAINT CK_DanhGiaSanPham_SoSao CHECK (SoSao BETWEEN 1 AND 5),
        NoiDung     NVARCHAR(1000) NULL,
        NgayDanhGia DATETIME NOT NULL CONSTRAINT DF_DanhGiaSanPham_Ngay DEFAULT GETDATE(),
        TrangThai   NVARCHAR(20) NOT NULL CONSTRAINT DF_DanhGiaSanPham_TT DEFAULT N'Chờ duyệt',
        CONSTRAINT CK_DanhGiaSanPham_TT CHECK (TrangThai IN (N'Chờ duyệt', N'Đã duyệt', N'Từ chối')),
        CONSTRAINT FK_DanhGiaSanPham_SP FOREIGN KEY (MaSP) REFERENCES dbo.SanPham(MaSP),
        CONSTRAINT FK_DanhGiaSanPham_KH FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH)
    );
    CREATE INDEX IX_DanhGiaSanPham_SP ON dbo.DanhGiaSanPham(MaSP, TrangThai);
END
GO

-- Danh gia mau (34): 24 'Đã duyệt', 6 'Chờ duyệt', 4 'Từ chối' de demo man hinh duyet binh luan.
-- Chi chen khi bang con trong; bo qua dong nao khach/san pham chua ton tai.
IF NOT EXISTS (SELECT 1 FROM dbo.DanhGiaSanPham)
BEGIN
    INSERT INTO dbo.DanhGiaSanPham (MaSP, MaKH, SoSao, NoiDung, NgayDanhGia, TrangThai)
    SELECT v.MaSP, k.MaKH, v.SoSao, v.NoiDung, v.Ngay, v.TrangThai
    FROM (VALUES
        (1, 'kh01@gmail.com', 5, N'Rất hài lòng, hàng chính hãng, đóng gói cẩn thận.', '20260712', N'Đã duyệt'),
        (1, 'kh02@gmail.com', 4, N'Dùng ổn, đúng mô tả. Trừ 1 sao vì hộp bị móp nhẹ.', '20260730', N'Đã duyệt'),
        (2, 'kh03@gmail.com', 5, N'Dùng rất thích, giao nhanh, nhân viên tư vấn nhiệt tình.', '20260803', N'Đã duyệt'),
        (5, 'kh04@gmail.com', 5, N'Sản phẩm xịn, đáng đồng tiền. Mình đã giới thiệu cho cả nhóm đánh cùng.', '20260809', N'Đã duyệt'),
        (5, 'kh05@gmail.com', 4, N'Dùng ổn, đúng mô tả. Trừ 1 sao vì hộp bị móp nhẹ.', '20260814', N'Đã duyệt'),
        (7, 'kh06@gmail.com', 5, N'Chất lượng tuyệt vời, đúng như mô tả. Sẽ ủng hộ shop tiếp.', '20260818', N'Đã duyệt'),
        (7, 'kh07@gmail.com', 3, N'Tạm được, dùng bình thường chưa thấy nổi bật.', '20260822', N'Đã duyệt'),
        (3, 'kh08@gmail.com', 5, N'Sản phẩm xịn, đáng đồng tiền. Mình đã giới thiệu cho cả nhóm đánh cùng.', '20260825', N'Đã duyệt'),
        (8, 'kh09@gmail.com', 4, N'Chất lượng tốt so với tầm giá, sẽ mua lại.', '20260829', N'Đã duyệt'),
        (9, 'kh10@gmail.com', 5, N'Chất lượng tuyệt vời, đúng như mô tả. Sẽ ủng hộ shop tiếp.', '20260901', N'Đã duyệt'),
        (10, 'kh11@gmail.com', 4, N'Dùng ổn, đúng mô tả. Trừ 1 sao vì hộp bị móp nhẹ.', '20260904', N'Đã duyệt'),
        (11, 'kh12@gmail.com', 5, N'Sản phẩm xịn, đáng đồng tiền. Mình đã giới thiệu cho cả nhóm đánh cùng.', '20260906', N'Đã duyệt'),
        (12, 'kh13@gmail.com', 4, N'Sản phẩm tốt, giao hàng hơi chậm một chút nhưng chấp nhận được.', '20260908', N'Đã duyệt'),
        (6, 'kh14@gmail.com', 3, N'Sản phẩm ổn nhưng mình kỳ vọng nhiều hơn.', '20260911', N'Đã duyệt'),
        (4, 'kh15@gmail.com', 5, N'Dùng rất thích, giao nhanh, nhân viên tư vấn nhiệt tình.', '20260913', N'Đã duyệt'),
        (13, 'kh16@gmail.com', 5, N'Sản phẩm xịn, đáng đồng tiền. Mình đã giới thiệu cho cả nhóm đánh cùng.', '20260916', N'Đã duyệt'),
        (19, 'kh17@gmail.com', 4, N'Dùng ổn, đúng mô tả. Trừ 1 sao vì hộp bị móp nhẹ.', '20260918', N'Đã duyệt'),
        (24, 'kh18@gmail.com', 5, N'Chất lượng tuyệt vời, đúng như mô tả. Sẽ ủng hộ shop tiếp.', '20260921', N'Đã duyệt'),
        (25, 'kh01@gmail.com', 4, N'Sản phẩm tốt, giao hàng hơi chậm một chút nhưng chấp nhận được.', '20260923', N'Đã duyệt'),
        (29, 'kh02@gmail.com', 5, N'Sản phẩm xịn, đáng đồng tiền. Mình đã giới thiệu cho cả nhóm đánh cùng.', '20260926', N'Đã duyệt'),
        (35, 'kh03@gmail.com', 4, N'Chất lượng tốt so với tầm giá, sẽ mua lại.', '20260928', N'Đã duyệt'),
        (38, 'kh04@gmail.com', 5, N'Chất lượng tuyệt vời, đúng như mô tả. Sẽ ủng hộ shop tiếp.', '20261001', N'Đã duyệt'),
        (16, 'kh05@gmail.com', 2, N'Hơi thất vọng, chất lượng chưa như quảng cáo.', '20261003', N'Đã duyệt'),
        (21, 'kh06@gmail.com', 1, N'Sản phẩm bị lỗi, đã liên hệ shop để đổi.', '20261004', N'Đã duyệt'),
        (2, 'kh07@gmail.com', 5, N'Rất hài lòng, hàng chính hãng, đóng gói cẩn thận.', '20261006', N'Chờ duyệt'),
        (14, 'kh08@gmail.com', 4, N'Dùng ổn, đúng mô tả. Trừ 1 sao vì hộp bị móp nhẹ.', '20261008', N'Chờ duyệt'),
        (17, 'kh09@gmail.com', 5, N'Dùng rất thích, giao nhanh, nhân viên tư vấn nhiệt tình.', '20261010', N'Chờ duyệt'),
        (26, 'kh10@gmail.com', 3, N'Sản phẩm ổn nhưng mình kỳ vọng nhiều hơn.', '20261012', N'Chờ duyệt'),
        (1, 'kh11@gmail.com', 4, N'Dùng ổn, đúng mô tả. Trừ 1 sao vì hộp bị móp nhẹ.', '20261014', N'Chờ duyệt'),
        (5, 'kh12@gmail.com', 5, N'Chất lượng tuyệt vời, đúng như mô tả. Sẽ ủng hộ shop tiếp.', '20261016', N'Chờ duyệt'),
        (22, 'kh13@gmail.com', 5, N'Shop lừa đảo, đừng mua!!! (không có đơn hàng)', '20261005', N'Từ chối'),
        (27, 'kh14@gmail.com', 1, N'Quảng cáo: cần mua sỉ liên hệ zalo 09xx để được giá tốt.', '20261007', N'Từ chối'),
        (31, 'kh15@gmail.com', 4, N'Hàng nhái, mua ở shop khác rẻ hơn, vào link để nhận ưu đãi!!!', '20261009', N'Từ chối'),
        (40, 'kh16@gmail.com', 2, N'Spam spam spam, xem video tại link bên dưới.', '20261011', N'Từ chối')
    ) v(MaSP, Email, SoSao, NoiDung, Ngay, TrangThai)
    JOIN dbo.KhachHang k ON k.Email = v.Email
    JOIN dbo.SanPham   p ON p.MaSP  = v.MaSP;
END
GO
