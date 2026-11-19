/* SV3: ghi chu don hang online (khach nhap khi dat). Them cot, KHONG doi/xoa cot cu.
   Chay sau 01/02/03 (va 04) tren CSDL hien co; chay lai duoc. */
USE BadmintonShop;
GO
IF COL_LENGTH('dbo.HoaDon','GhiChu') IS NULL
    ALTER TABLE dbo.HoaDon ADD GhiChu NVARCHAR(500) NULL;
GO
