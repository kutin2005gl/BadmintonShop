/* =====================================================================
   BadmintonShop - 09_NhanVienNgaySinh.sql
   Bo sung cot NgaySinh cho NhanVien theo bang/ERD co ve (NS).
   Chay 1 lan tren CSDL da co; chay lai nhieu lan van an toan.
   ===================================================================== */
USE BadmintonShop;
GO

IF COL_LENGTH('dbo.NhanVien', 'NgaySinh') IS NULL
    ALTER TABLE dbo.NhanVien ADD NgaySinh DATE NULL;
GO

-- Dien ngay sinh cho 5 nhan vien mau (chi khi chua co)
UPDATE dbo.NhanVien SET NgaySinh = '1985-04-12' WHERE Email = 'admin@shopcaulong.vn'    AND NgaySinh IS NULL;
UPDATE dbo.NhanVien SET NgaySinh = '1990-08-23' WHERE Email = 'quanly@shopcaulong.vn'   AND NgaySinh IS NULL;
UPDATE dbo.NhanVien SET NgaySinh = '1998-01-15' WHERE Email = 'nhanvien@shopcaulong.vn' AND NgaySinh IS NULL;
UPDATE dbo.NhanVien SET NgaySinh = '1995-11-05' WHERE Email = 'kho@shopcaulong.vn'      AND NgaySinh IS NULL;
UPDATE dbo.NhanVien SET NgaySinh = '1999-06-30' WHERE Email = 'nghi@shopcaulong.vn'     AND NgaySinh IS NULL;
GO
