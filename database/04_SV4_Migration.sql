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
