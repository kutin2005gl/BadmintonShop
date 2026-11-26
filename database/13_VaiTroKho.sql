/* Them vai tro KHO (thu kho): chi duoc dung Nha cung cap va Phieu nhap tren Desktop.
   - Mo rang buoc CK_TaiKhoan_VaiTro de nhan them 'KHO'.
   - Chuyen tai khoan mau 'kho' (dang la QUANLY) sang KHO.
   Chay sau 01/02/03 tren CSDL hien co; chay lai duoc, khong xoa du lieu. */
USE BadmintonShop;
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_TaiKhoan_VaiTro')
    ALTER TABLE TaiKhoan DROP CONSTRAINT CK_TaiKhoan_VaiTro;
GO

ALTER TABLE TaiKhoan ADD CONSTRAINT CK_TaiKhoan_VaiTro
    CHECK (VaiTro IN ('ADMIN','QUANLY','NHANVIEN','KHO'));
GO

UPDATE TaiKhoan SET VaiTro = 'KHO' WHERE TenDangNhap = 'kho' AND VaiTro = 'QUANLY';
GO
