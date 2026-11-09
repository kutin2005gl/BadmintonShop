USE BadmintonShop;
GO

-- TV1: Them du lieu Ha Noi trong mot giao dich de tranh don dang do.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @KhachMoi INT = 0, @HoaDonMoi INT = 0;

    -- TV1: Them khach thu nhat neu email chua ton tai.
    IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WITH (UPDLOCK, HOLDLOCK) WHERE Email = 'kh.hn01@example.com')
    BEGIN
        INSERT INTO dbo.KhachHang (HoTen, SDT, Email, MatKhau, DiaChi)
        VALUES (N'Nguyễn Minh Hà', '0911000001', 'kh.hn01@example.com', 'KHONG_DANG_NHAP', N'25 Phố Huế, Hai Bà Trưng, Hà Nội');
        SET @KhachMoi += 1;
    END;

    -- TV1: Them khach thu hai neu email chua ton tai.
    IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WITH (UPDLOCK, HOLDLOCK) WHERE Email = 'kh.hn02@example.com')
    BEGIN
        INSERT INTO dbo.KhachHang (HoTen, SDT, Email, MatKhau, DiaChi)
        VALUES (N'Trần Thu Hương', '0911000002', 'kh.hn02@example.com', 'KHONG_DANG_NHAP', N'18 Nguyễn Trãi, Thanh Xuân, Hà Nội');
        SET @KhachMoi += 1;
    END;

    -- TV1: Them khach thu ba neu email chua ton tai.
    IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WITH (UPDLOCK, HOLDLOCK) WHERE Email = 'kh.hn03@example.com')
    BEGIN
        INSERT INTO dbo.KhachHang (HoTen, SDT, Email, MatKhau, DiaChi)
        VALUES (N'Lê Hoàng Nam', '0911000003', 'kh.hn03@example.com', 'KHONG_DANG_NHAP', N'36 Cầu Giấy, Cầu Giấy, Hà Nội');
        SET @KhachMoi += 1;
    END;

    -- TV1: Khai bao bon don nam 2026 co tong tien trong va ngoai khoang demo.
    DECLARE @Don TABLE (STT INT PRIMARY KEY, Email VARCHAR(100), NgayLap DATETIME, TenSP NVARCHAR(150), SoLuong INT);
    INSERT INTO @Don (STT, Email, NgayLap, TenSP, SoLuong) VALUES
        (1, 'kh.hn01@example.com', '20261016 09:00:00', N'Cầu Yonex AS-30', 2),
        (2, 'kh.hn02@example.com', '20261017 10:00:00', N'Cầu Thành Công 77', 2),
        (3, 'kh.hn03@example.com', '20261018 11:00:00', N'Cầu Yonex AS-30', 2),
        (4, 'kh.hn01@example.com', '20261019 14:00:00', N'Vợt Yonex Astrox 88D Pro', 1);
    DECLARE @STT INT = 1, @MaKH INT, @MaHD INT, @MaSP INT, @SoLuong INT,
            @NgayLap DATETIME, @TenSP NVARCHAR(150), @DiaChi NVARCHAR(255), @DonGia DECIMAL(18,0);

    -- TV1: Tao tung don con thieu theo email va ngay lap de chay lai khong trung.
    WHILE @STT <= 4
    BEGIN
        SELECT @MaKH = k.MaKH, @DiaChi = k.DiaChi, @NgayLap = d.NgayLap,
               @TenSP = d.TenSP, @SoLuong = d.SoLuong
        FROM @Don d JOIN dbo.KhachHang k ON k.Email = d.Email
        WHERE d.STT = @STT;

        -- TV1: Chi them hoa don va tru ton khi chua co don mau cua khach.
        IF NOT EXISTS (SELECT 1 FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK)
                       WHERE MaKH = @MaKH AND NgayLap = @NgayLap AND LoaiBan = N'Online' AND HinhThucTT = 'COD')
        BEGIN
            SET @MaSP = NULL;
            SELECT TOP (1) @MaSP = MaSP, @DonGia = DonGiaBan
            FROM dbo.SanPham WITH (UPDLOCK, HOLDLOCK)
            WHERE TenSP = @TenSP AND TrangThai = N'Đang bán' AND SoLuongTon >= @SoLuong
            ORDER BY MaSP;
            IF @MaSP IS NULL
                THROW 50001, N'Sản phẩm mẫu không tồn tại, ngừng bán hoặc không đủ tồn kho.', 1;
            IF (@STT IN (1, 3) AND @DonGia * @SoLuong NOT BETWEEN 1000000 AND 2000000)
               OR (@STT IN (2, 4) AND @DonGia * @SoLuong BETWEEN 1000000 AND 2000000)
                THROW 50002, N'Giá sản phẩm không phù hợp khoảng tổng tiền demo.', 1;

            INSERT INTO dbo.HoaDon (MaKH, NgayLap, LoaiBan, HinhThucTT, DiaChiGiao)
            VALUES (@MaKH, @NgayLap, N'Online', 'COD', @DiaChi);
            SET @MaHD = SCOPE_IDENTITY();

            -- TV1: De trigger tru ton kho va tu tinh tong tien tu gia ban san pham.
            INSERT INTO dbo.ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia)
            SELECT @MaHD, MaSP, @SoLuong, DonGiaBan FROM dbo.SanPham WHERE MaSP = @MaSP;
            UPDATE dbo.HoaDon SET TrangThai = N'Hoàn thành' WHERE MaHD = @MaHD;
            SET @HoaDonMoi += 1;
        END;
        SET @STT += 1;
    END;
    COMMIT TRANSACTION;
    SELECT @KhachMoi AS KhachHangDaThem, @HoaDonMoi AS HoaDonDaThem, @HoaDonMoi AS ChiTietDaThem;
END TRY
-- TV1: Hoan tac toan bo du lieu mau neu co loi rang buoc hoac ton kho.
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
