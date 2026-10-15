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
