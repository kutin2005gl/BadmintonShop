/* SV4: dữ liệu demo RIÊNG, tùy chọn. Chạy sau 04 và 06. Không xóa/tạo lại CSDL.
   Danh mục [SV4-DEMO] là dấu nhận diện; chạy lại không thêm bản sao.
   Nhập hàng trước khi bán, không cập nhật tồn trực tiếp. Không phụ thuộc khách SV2.
   Chọn CSDL đích bằng sqlcmd -d hoặc dropdown SSMS; không chọn CSDL cửa hàng. */
IF DB_NAME() NOT LIKE N'BadmintonShop[_]SV4[_]Test[_]%'
   AND DB_NAME() NOT LIKE N'BadmintonShop[_]SV4[_]Demo%'
    THROW 51004,N'Chỉ chạy demo trên CSDL BadmintonShop_SV4_Test_* hoặc BadmintonShop_SV4_Demo*.',1;
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @lock int;
    EXEC @lock=sys.sp_getapplock @Resource=N'SV4_DEMO_V1',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
    IF @lock<0 THROW 50001,N'Không lấy được khóa tạo demo SV4.',1;
    IF EXISTS(SELECT 1 FROM DanhMuc WHERE TenDanhMuc=N'[SV4-DEMO] Vợt')
    BEGIN
        COMMIT;
        PRINT N'Demo SV4 đã tồn tại; không thêm lại.';
        RETURN;
    END;
    DECLARE @dm1 int,@dm2 int,@sp1 int,@sp2 int,@nv int,@ncc int,@pn int,@hd int;
    INSERT DanhMuc(TenDanhMuc,MoTa) VALUES(N'[SV4-DEMO] Vợt',N'Dữ liệu minh họa SV4');SET @dm1=SCOPE_IDENTITY();
    INSERT DanhMuc(TenDanhMuc,MoTa) VALUES(N'[SV4-DEMO] Phụ kiện',N'Dữ liệu minh họa SV4');SET @dm2=SCOPE_IDENTITY();
    INSERT SanPham(TenSP,MaDM,ThuongHieu,DonGiaBan,DonViTinh) VALUES(N'[SV4-DEMO] Vợt tập luyện',@dm1,N'SV4 Demo',500000,N'Cây');SET @sp1=SCOPE_IDENTITY();
    INSERT SanPham(TenSP,MaDM,ThuongHieu,DonGiaBan,DonViTinh) VALUES(N'[SV4-DEMO] Quấn cán',@dm2,N'SV4 Demo',30000,N'Cái');SET @sp2=SCOPE_IDENTITY();
    INSERT NhanVien(HoTen,ChucVu) VALUES(N'[SV4-DEMO] Nhân viên minh họa',N'Nhân viên bán hàng');SET @nv=SCOPE_IDENTITY();
    INSERT NhaCungCap(TenNCC) VALUES(N'[SV4-DEMO] Nhà cung cấp minh họa');SET @ncc=SCOPE_IDENTITY();
    INSERT PhieuNhap(NgayNhap,MaNCC,MaNV) VALUES(DATEADD(month,-12,GETDATE()),@ncc,@nv);SET @pn=SCOPE_IDENTITY();
    INSERT ChiTietPhieuNhap(MaPN,MaSP,SoLuong,DonGia) VALUES(@pn,@sp1,200,350000),(@pn,@sp2,500,10000);
    -- Offers are only for the isolated demo; never create accounts or write to the store database.
    DECLARE @km int;
    INSERT CTKM(TenCTKM,NgayBatDau,NgayKetThuc,LoaiKM)
        VALUES(N'[SV4-DEMO] Giảm vợt 10%',CAST(GETDATE() AS date),DATEADD(day,30,CAST(GETDATE() AS date)),'SANPHAM');
    SET @km=SCOPE_IDENTITY();INSERT KM_TheoSP(MaCTKM,MaSP,PhanTramGG) VALUES(@km,@sp1,10);
    INSERT CTKM(TenCTKM,NgayBatDau,NgayKetThuc,LoaiKM)
        VALUES(N'[SV4-DEMO] Hóa đơn từ 1 triệu',CAST(GETDATE() AS date),DATEADD(day,30,CAST(GETDATE() AS date)),'HOADON');
    SET @km=SCOPE_IDENTITY();INSERT KM_TheoHD(MaCTKM,TongTienToiThieu,PhanTramGG) VALUES(@km,1000000,5);
    INSERT CTKM(TenCTKM,NgayBatDau,NgayKetThuc,LoaiKM)
        VALUES(N'[SV4-DEMO] Voucher 15%',CAST(GETDATE() AS date),DATEADD(day,30,CAST(GETDATE() AS date)),'VOUCHER');
    SET @km=SCOPE_IDENTITY();INSERT KM_TheoVoucher(MaCTKM,MaVoucher,PhanTramGG,SoLuong) VALUES(@km,'SV4DEMO15',15,30);
    DECLARE @i int=0;
    WHILE @i<12
    BEGIN
        INSERT HoaDon(MaKH,MaNV,NgayLap,LoaiBan,HinhThucTT,TrangThai,TienGiamGia,TienKhachDua,MaYeuCauPOS)
        VALUES(NULL,@nv,DATEADD(month,-@i,DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1)),N'Tại quầy',N'Tiền mặt',N'Hoàn thành',10000,2000000,NEWID());
        SET @hd=SCOPE_IDENTITY();
        INSERT ChiTietHoaDon(MaHD,MaSP,SoLuong,DonGia,GiamGia) VALUES(@hd,@sp1,1+@i%3,500000,0),(@hd,@sp2,2+@i%4,30000,0);
        SET @i+=1;
    END;
    -- Đơn Online minh họa để kiểm thử độc lập luồng nhân viên (không giả tài khoản khách).
    SET @i=0;
    WHILE @i<5
    BEGIN
        INSERT HoaDon(MaKH,NgayLap,LoaiBan,HinhThucTT,TrangThai)
        VALUES(NULL,GETDATE(),N'Online',N'COD',N'Chờ xác nhận');SET @hd=SCOPE_IDENTITY();
        INSERT ChiTietHoaDon(MaHD,MaSP,SoLuong,DonGia) VALUES(@hd,@sp2,1,30000);
        IF @i IN(1,2,3) UPDATE HoaDon SET TrangThai=N'Đã xác nhận' WHERE MaHD=@hd;
        IF @i IN(2,3) UPDATE HoaDon SET TrangThai=N'Đang giao' WHERE MaHD=@hd;
        IF @i=3 UPDATE HoaDon SET TrangThai=N'Hoàn thành' WHERE MaHD=@hd;
        IF @i=4 UPDATE HoaDon SET TrangThai=N'Đã hủy',LyDoHuy=N'[SV4-DEMO] Khách yêu cầu trước khi giao' WHERE MaHD=@hd;
        SET @i+=1;
    END;
    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK;
    THROW;
END CATCH;
GO
