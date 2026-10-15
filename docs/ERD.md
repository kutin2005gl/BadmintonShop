Đồ án · Hệ thống bán đồ cầu lông

# ERD vẽ lại + phần Khuyến mãi

Giữ nguyên các entity, thuộc tính và quan hệ trong bản vẽ tay. Phần mới là CTKM và 3 bảng con, nối vào SANPHAM và HOADON.

**||--o{** 1 – nhiều (0..n) **||--|{** 1 – nhiều (1..n) **||--o|** 1 – 0..1 **|o--o{** 0..1 – nhiều

```mermaid
erDiagram
    TAIKHOAN {
        string MaTK PK
        string TenDangNhap
        string MatKhau
        string VaiTro
        string MaNV FK
    }
    NHANVIEN {
        string MaNV PK
        string HoTen
        string SDT
        string Email
        string DiaChi
        string ChucVu
        decimal Luong
        date NgayVaoLam
    }
    NHACUNGCAP {
        string MaNCC PK
        string TenNCC
        string DiaChi
        string SDT
        string Email
    }
    DANHMUC {
        string MaDM PK
        string TenDanhMuc
    }
    KHACHHANG {
        string MaKH PK
        string HoTen
        string SDT
        string Email
        string MatKhau
        string DiaChi
        date NgayDangKy
        string TrangThai
    }
    SANPHAM {
        string MaSP PK
        string TenSP
        string MaDM FK "LoaiSP"
        decimal DonGia
        int SoLuongTon
        string HinhAnh
        string TrangThai
    }
    PHIEUNHAP {
        string MaPN PK
        date NgayNhap
        string MaNCC FK
        string MaNV FK
        decimal TongTien
    }
    CHITIETPHIEUNHAP {
        string MaPN PK, FK
        string MaSP PK, FK
        int SoLuong
        decimal DonGia
    }
    HOADON {
        string MaHD PK
        string MaKH FK
        string MaNV FK
        date NgayLap
        decimal TongTien
        decimal TienGiamGia "moi"
        string MaCTKM FK "moi, null"
        string TrangThai
    }
    CHITIETHOADON {
        string MaHD PK, FK
        string MaSP PK, FK
        int SoLuong
        decimal DonGia
        decimal ThanhTien "sau giam gia"
    }
    PHANHOI {
        string MaPH PK
        string MaKH FK
        string MaSP FK
        string NoiDung
        date NgayGui
    }
    CTKM {
        string MaCTKM PK
        string TenCTKM
        date NgayBatDau
        date NgayKetThuc
        string TrangThai
    }
    KM_THEO_SP {
        string MaCTKM PK, FK
        string MaSP PK, FK
        float PhanTramGG
    }
    KM_THEO_HD {
        string MaCTKM PK, FK
        decimal TongTienToiThieu
        float PhanTramGG
    }
    KM_THEO_VOUCHER {
        string MaVoucher PK
        string MaCTKM FK
        float PhanTramGG
    }

    NHANVIEN   ||--o| TAIKHOAN : "co"
    NHANVIEN   ||--o{ PHIEUNHAP : "lap"
    NHACUNGCAP ||--o{ PHIEUNHAP : "cung cap"
    NHANVIEN   ||--o{ HOADON : "lap"
    KHACHHANG  ||--o{ HOADON : "dat"
    DANHMUC    ||--o{ SANPHAM : "phan loai"
    KHACHHANG  ||--o{ PHANHOI : "gui"
    SANPHAM    ||--o{ PHANHOI : "nhan ve"
    PHIEUNHAP  ||--|{ CHITIETPHIEUNHAP : "gom"
    SANPHAM    ||--o{ CHITIETPHIEUNHAP : "co trong"
    HOADON     ||--|{ CHITIETHOADON : "gom"
    SANPHAM    ||--o{ CHITIETHOADON : "co trong"

    CTKM       ||--o{ KM_THEO_SP : "giam theo SP"
    SANPHAM    ||--o{ KM_THEO_SP : "duoc giam"
    CTKM       ||--o| KM_THEO_HD : "giam theo HD"
    CTKM       ||--o{ KM_THEO_VOUCHER : "phat hanh"
    CTKM       |o--o{ HOADON : "ap dung"
```

## Phần khuyến mãi nối vào như thế nào

- **CTKM** là bảng cha (tên, ngày bắt đầu, ngày kết thúc). 3 bảng con đều có `MaCTKM` trỏ về nó, đúng như mũi tên đỏ bạn vẽ.
- **KM_THEO_SP** nối với **SANPHAM** qua `MaSP`. Khi bán, giá trong CHITIETHOADON được trừ % này, nên mình thêm `ThanhTien` (đúng chỗ bạn ghi "thành tiền gg").
- **KM_THEO_HD**: chỗ bạn khoanh `MaHD` và ghi "Tổng", mình hiểu là điều kiện hóa đơn đạt tổng tiền tối thiểu, nên đổi thành `TongTienToiThieu`.
- **HOADON** thêm `MaCTKM` (cho phép null) để biết hóa đơn dùng CTKM theo HĐ hoặc voucher nào, và `TienGiamGia` (chỗ bạn ghi "gg").
- Ghi chú đỏ "Loại SP" ở SANPHAM: mình giữ `MaDM`, chỉ ghi chú là loại SP. Cột "Chi tiết ĐG" mình đọc là `DonGia`.