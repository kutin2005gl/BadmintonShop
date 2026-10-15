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
