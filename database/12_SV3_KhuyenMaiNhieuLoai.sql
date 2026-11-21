/* SV3: mot chuong trinh khuyen mai co the co ca 3 nhom uu dai (san pham, hoa don, voucher)
   va ghi nhan dung ma voucher da dung cho tung hoa don.
   - CTKM.LoaiKM: them gia tri 'HONHOP' (chuong trinh co tu 2 nhom tro len). Cot van giu de tuong thich.
   - Bang HoaDon_Voucher: hoa don nao da dung ma voucher nao (chi ghi ma THANG).
   Chay sau 01/02/03 (va 04) tren CSDL hien co; chay lai duoc, khong xoa du lieu. */
USE BadmintonShop;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_CTKM_Loai' AND parent_object_id = OBJECT_ID('dbo.CTKM')
           AND definition NOT LIKE '%HONHOP%')
    ALTER TABLE dbo.CTKM DROP CONSTRAINT CK_CTKM_Loai;
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_CTKM_Loai' AND parent_object_id = OBJECT_ID('dbo.CTKM'))
    ALTER TABLE dbo.CTKM ADD CONSTRAINT CK_CTKM_Loai CHECK (LoaiKM IN ('SANPHAM','HOADON','VOUCHER','HONHOP'));

IF OBJECT_ID(N'dbo.HoaDon_Voucher', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HoaDon_Voucher (
        MaHD      INT NOT NULL,
        MaVoucher VARCHAR(30) NOT NULL,
        CONSTRAINT PK_HoaDon_Voucher PRIMARY KEY (MaHD),
        CONSTRAINT FK_HDVC_HD FOREIGN KEY (MaHD) REFERENCES dbo.HoaDon(MaHD) ON DELETE CASCADE,
        CONSTRAINT FK_HDVC_VC FOREIGN KEY (MaVoucher) REFERENCES dbo.KM_TheoVoucher(MaVoucher)
    );
    CREATE INDEX IX_HDVC_Voucher ON dbo.HoaDon_Voucher(MaVoucher);
END

COMMIT;
GO
