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
