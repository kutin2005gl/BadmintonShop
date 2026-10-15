# Lược đồ quan hệ

Lược đồ quan hệ chuyển từ ERD sang dạng chữ, bám đúng `database/01_CreateDatabase.sql`. Mỗi bảng viết trên một dòng theo dạng `TenBang(KhoaChinh, KhoaNgoai, CotKhac...)`.
Quy ước: **in đậm** = khóa chính, _in nghiêng_ = khóa ngoại, **_đậm nghiêng_** = vừa là khóa chính vừa là khóa ngoại. Dưới mỗi bảng có chú thích khóa ngoại trỏ tới bảng nào.
Cột ghi `(UK)` là cột có ràng buộc UNIQUE.

## Nhân sự và tài khoản

1. NhanVien(**MaNV**, HoTen, SDT, Email, DiaChi, ChucVu, Luong, NgayVaoLam)
2. TaiKhoan(**MaTK**, TenDangNhap (UK), MatKhau, VaiTro, _MaNV_ (UK), TrangThai)
   - _MaNV_ → NhanVien(MaNV)

## Danh mục, sản phẩm, nhà cung cấp, khách hàng

3. DanhMuc(**MaDM**, TenDanhMuc (UK), MoTa)
4. NhaCungCap(**MaNCC**, TenNCC, DiaChi, SDT, Email)
5. SanPham(**MaSP**, TenSP, _MaDM_, ThuongHieu, DonGiaBan, SoLuongTon, DonViTinh, HinhAnh, MoTa, TrangThai)
   - _MaDM_ → DanhMuc(MaDM)
6. KhachHang(**MaKH**, HoTen, SDT, Email (UK), MatKhau, DiaChi, NgayDangKy, TrangThai)

## Nhập hàng

7. PhieuNhap(**MaPN**, NgayNhap, _MaNCC_, _MaNV_, TongTien)
   - _MaNCC_ → NhaCungCap(MaNCC)
   - _MaNV_ → NhanVien(MaNV)
8. ChiTietPhieuNhap(**_MaPN_**, **_MaSP_**, SoLuong, DonGia, ThanhTien)
   - _MaPN_ → PhieuNhap(MaPN), ON DELETE CASCADE
   - _MaSP_ → SanPham(MaSP)
   - ThanhTien là cột tính = SoLuong × DonGia

## Khuyến mãi

9. CTKM(**MaCTKM**, TenCTKM, NgayBatDau, NgayKetThuc, LoaiKM, TrangThai)
10. KM_TheoSP(**_MaCTKM_**, **_MaSP_**, PhanTramGG)
    - _MaCTKM_ → CTKM(MaCTKM), ON DELETE CASCADE
    - _MaSP_ → SanPham(MaSP)
11. KM_TheoHD(**_MaCTKM_**, **TongTienToiThieu**, PhanTramGG)
    - _MaCTKM_ → CTKM(MaCTKM), ON DELETE CASCADE
12. KM_TheoVoucher(**MaVoucher**, _MaCTKM_, PhanTramGG, SoLuong, DaDung)
    - _MaCTKM_ → CTKM(MaCTKM), ON DELETE CASCADE

## Bán hàng và phản hồi

13. HoaDon(**MaHD**, _MaKH_, _MaNV_, NgayLap, LoaiBan, HinhThucTT, DiaChiGiao, _MaCTKM_, TienGiamGia, TongTien, TrangThai)
    - _MaKH_ → KhachHang(MaKH), cho phép NULL (khách vãng lai mua tại quầy)
    - _MaNV_ → NhanVien(MaNV), cho phép NULL (khách tự đặt trên web)
    - _MaCTKM_ → CTKM(MaCTKM), cho phép NULL (không áp khuyến mãi cấp hóa đơn)
14. ChiTietHoaDon(**_MaHD_**, **_MaSP_**, SoLuong, DonGia, GiamGia, ThanhTien)
    - _MaHD_ → HoaDon(MaHD), ON DELETE CASCADE
    - _MaSP_ → SanPham(MaSP)
    - ThanhTien là cột tính = SoLuong × DonGia × (1 − GiamGia/100)
15. PhanHoi(**MaPH**, _MaKH_, _MaSP_, SoSao, NoiDung, NgayGui, TrangThai)
    - _MaKH_ → KhachHang(MaKH)
    - _MaSP_ → SanPham(MaSP)

## Tóm tắt khóa ngoại

| Bảng con | Cột FK | Bảng cha | NULL? | Khi xóa cha |
|---|---|---|:-:|---|
| TaiKhoan | MaNV | NhanVien | không | chặn |
| SanPham | MaDM | DanhMuc | không | chặn |
| PhieuNhap | MaNCC | NhaCungCap | không | chặn |
| PhieuNhap | MaNV | NhanVien | không | chặn |
| ChiTietPhieuNhap | MaPN | PhieuNhap | không | xóa theo (CASCADE) |
| ChiTietPhieuNhap | MaSP | SanPham | không | chặn |
| KM_TheoSP | MaCTKM | CTKM | không | xóa theo (CASCADE) |
| KM_TheoSP | MaSP | SanPham | không | chặn |
| KM_TheoHD | MaCTKM | CTKM | không | xóa theo (CASCADE) |
| KM_TheoVoucher | MaCTKM | CTKM | không | xóa theo (CASCADE) |
| HoaDon | MaKH | KhachHang | có | chặn |
| HoaDon | MaNV | NhanVien | có | chặn |
| HoaDon | MaCTKM | CTKM | có | chặn |
| ChiTietHoaDon | MaHD | HoaDon | không | xóa theo (CASCADE) |
| ChiTietHoaDon | MaSP | SanPham | không | chặn |
| PhanHoi | MaKH | KhachHang | không | chặn |
| PhanHoi | MaSP | SanPham | không | chặn |
