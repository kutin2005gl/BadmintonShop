# Sơ đồ lớp Core và nghiệp vụ — SV4

Phản ánh mã nguồn nhánh `feature/sv4-final-integration` ngày 26/10/2026 (chưa commit, nền develop `920a576`). Ký hiệu: liên kết thực thể theo FK; dependency là lời gọi trong mã. Thuộc tính trình bày phần chính, không thay thế model đầy đủ.

## Các thực thể Core

```mermaid
classDiagram
    class DanhMuc {
        +int MaDM
        +string TenDanhMuc
        +string MoTa
    }
    class SanPham {
        +int MaSP
        +int MaDM
        +string TenSP
        +decimal DonGiaBan
        +int SoLuongTon
        +string HinhAnh
        +string TrangThai
    }
    class NhanVien {
        +int MaNV
        +string HoTen
        +string ChucVu
    }
    class TaiKhoan {
        +int MaTK
        +int MaNV
        +string VaiTro
        +bool TrangThai
    }
    class KhachHang {
        +int MaKH
        +string HoTen
        +string Email
    }
    class HoaDon {
        +int MaHD
        +int? MaKH
        +int? MaNV
        +int? MaCTKM
        +DateTime NgayLap
        +string LoaiBan
        +decimal TienGiamGia
        +decimal TongTien
        +string TrangThai
        +string LyDoHuy
        +Guid? MaYeuCauPOS
        +decimal? TienKhachDua
    }
    class ChiTietHoaDon {
        +int MaHD
        +int MaSP
        +int SoLuong
        +decimal DonGia
        +decimal GiamGia
        +decimal ThanhTien
    }
    class NhaCungCap {
        +int MaNCC
        +string TenNCC
    }
    class PhieuNhap {
        +int MaPN
        +int MaNV
        +int MaNCC
        +decimal TongTien
    }
    class ChiTietPhieuNhap {
        +int MaPN
        +int MaSP
        +int SoLuong
        +decimal DonGia
        +decimal ThanhTien
    }
    class CTKM {
        +int MaCTKM
        +string LoaiKM
        +DateTime NgayBatDau
        +DateTime NgayKetThuc
    }
    class KMTheoSP {
        +int MaCTKM
        +int MaSP
        +decimal PhanTramGG
    }
    class KMTheoHD {
        +int MaCTKM
        +decimal TongTienToiThieu
        +decimal PhanTramGG
    }
    class KMTheoVoucher {
        +int MaCTKM
        +string MaVoucher
        +int SoLuong
        +int DaDung
    }
    class PhanHoi {
        +int MaPH
        +int MaKH
        +int MaSP
        +byte SoSao
    }
    DanhMuc "1" --> "0..*" SanPham
    NhanVien "1" --> "0..1" TaiKhoan
    NhanVien "0..1" --> "0..*" HoaDon
    KhachHang "0..1" --> "0..*" HoaDon
    HoaDon "1" --> "0..*" ChiTietHoaDon
    SanPham "1" --> "0..*" ChiTietHoaDon
    NhanVien "1" --> "0..*" PhieuNhap
    NhaCungCap "1" --> "0..*" PhieuNhap
    PhieuNhap "1" --> "0..*" ChiTietPhieuNhap
    SanPham "1" --> "0..*" ChiTietPhieuNhap
    CTKM "0..1" --> "0..*" HoaDon
    CTKM "1" --> "0..*" KMTheoSP
    CTKM "1" --> "0..*" KMTheoHD
    CTKM "1" --> "0..*" KMTheoVoucher
    SanPham "1" --> "0..*" KMTheoSP
    KhachHang "1" --> "0..*" PhanHoi
    SanPham "1" --> "0..*" PhanHoi
```

Các model là POCO 1–1 với bảng, **không có navigation property/ORM**; các mũi tên trên là FK nghiệp vụ. ChiTietHoaDon.ThanhTien và ChiTietPhieuNhap.ThanhTien chỉ đọc về từ cột tính; HoaDon.TongTien và PhieuNhap.TongTien do trigger tính. CSDL cho phép hóa đơn chưa có chi tiết ở cấp lược đồ; thao tác ghi phải bảo đảm không để header rỗng sau lỗi.

## Repository, dịch vụ và giao diện

```mermaid
classDiagram
    class DbHelper {
        +string ConnectionString
        +TaoKetNoi() SqlConnection
        +ExecuteQuery() DataTable
        +ExecuteNonQuery() int
        +ExecuteScalar() object
        +ExecuteTransaction()
    }
    class DanhMucRepository {
        +LayTatCa() List~DanhMuc~
        +LayTheoMa(int maDM) DanhMuc
        +Luu(DanhMuc dm) int
        +Xoa(int maDM) void
    }
    class SanPhamRepository {
        +LayTatCa() List~SanPham~
        +LayTheoMa(int maSP) SanPham
        +LayDanhSachHienThi() DataTable
        +Luu(SanPham sp) int
        +NgungBan(int maSP) void
    }
    class HoaDonRepository {
        +LayDanhSachHienThi() DataTable
        +LayTheoMa(int ma) HoaDon
        +LayChiTietHienThi(int ma) DataTable
        +ChuyenTrangThai() void
        +ThanhToanPOS() int
    }
    class BaoCaoRepository {
        +LayDuLieu() DataTable
    }
    class BaoCaoService {
        +PhanBo() Dictionary
        +Tao() KetQuaBaoCao
    }
    class KetQuaBaoCao {
        +DateTime TuNgay
        +DateTime DenNgay
        +string LoaiBan
        +decimal TongDoanhThu
        +IReadOnlyDictionary Bang
    }
    class QuyTacBanHang {
        +int NguongSapHet
        +DuocChuyen() bool
        +BatBuoc() string
        +KiemTraTien() void
    }
    class KhoAnhSanPham {
        +string ThuMucGoc
        +DuongDanDayDu() string
    }
    class PhienDangNhap {
        +int MaNV
        +BatDau() void
        +DangXuat() void
        +CoQuyen() bool
    }
    class PhanQuyen {
        +CoQuyen() bool
    }
    class QuyenForm {
        +KiemTraKhiMo() bool
        +DuQuyen() bool
    }
    class Sv4Ui {
        +Chay() void
    }
    class FrmDanhMuc
    class FrmSanPham
    class FrmSanPhamSua
    class FrmDonHang
    class FrmPOS
    class FrmThongKe
    class DongPOS {
        +int MaSP
        +int SoLuong
        +decimal DonGia
        +decimal ThanhTien
    }
    class AnhSanPham {
        +Doc() Bitmap
        +Luu() string
    }
    class XuatTaiLieu {
        +Excel() void
        +BaoCaoPdf() void
        +HoaDonPdf() void
    }
    class BieuDoCot
    DanhMucRepository ..> DbHelper
    SanPhamRepository ..> DbHelper
    HoaDonRepository ..> DbHelper
    BaoCaoRepository ..> DbHelper
    DanhMucRepository ..> QuyTacBanHang
    SanPhamRepository ..> QuyTacBanHang
    SanPhamRepository ..> KhoAnhSanPham
    HoaDonRepository ..> QuyTacBanHang
    BaoCaoService ..> QuyTacBanHang
    BaoCaoService ..> KetQuaBaoCao
    PhienDangNhap ..> PhanQuyen
    QuyenForm ..> PhienDangNhap
    Sv4Ui ..> QuyenForm
    FrmDanhMuc ..> DanhMucRepository
    FrmSanPham ..> SanPhamRepository
    FrmSanPham ..> FrmSanPhamSua
    FrmSanPhamSua ..> SanPhamRepository
    FrmSanPhamSua ..> DanhMucRepository
    FrmSanPhamSua ..> AnhSanPham
    AnhSanPham ..> KhoAnhSanPham
    FrmDonHang ..> HoaDonRepository
    FrmDonHang ..> XuatTaiLieu
    FrmPOS ..> SanPhamRepository
    FrmPOS ..> HoaDonRepository
    FrmPOS ..> PhienDangNhap
    FrmPOS ..> DongPOS
    FrmPOS ..> XuatTaiLieu
    FrmThongKe ..> BaoCaoRepository
    FrmThongKe ..> BaoCaoService
    FrmThongKe ..> BieuDoCot
    FrmThongKe ..> XuatTaiLieu
    XuatTaiLieu ..> KetQuaBaoCao
```

Tất cả form SV4 gọi QuyenForm ở OnLoad và dùng Sv4Ui khi thực hiện thao tác CSDL; lược bỏ các cạnh lặp để dễ đọc. Các lớp NhaCungCapRepository, NhanVienRepository, TaiKhoanRepository, KhachHangRepository, PhieuNhapRepository, SaoLuuRepository (SV1) cũng truy cập DbHelper và model tương ứng; không bị thay thế bởi SV4.

Dịch vụ Core hiện có khác: MatKhauHelper, KiemTraDuLieu, LoiCsdl, VaiTro, ChucNang, KetQuaDangNhap. `TinhKhuyenMaiService` và `GioHangService` đã tính khuyến mãi thật dùng chung Web/POS; không còn service khung. Cột SQL `HoaDon.DauVanTayPOS` do repository ghi/đọc trực tiếp, chưa thêm property vào POCO HoaDon. `XuatTaiLieu`, `AnhSanPham`, `QuyenForm`, `BieuDoCot` nằm ở Desktop do phụ thuộc WinForms/GDI, không đưa vào Core đa nền tảng.

## Luồng thanh toán cuối cùng

```mermaid
classDiagram
    class FrmPOS
    class DatHangController
    class GioHangService {
        +Tinh() KetQuaGioHang
        ~TinhTrongGiaoDich() KetQuaGioHang
    }
    class TinhKhuyenMaiService {
        +LayPhanTramGiamSanPham() decimal
        +TinhGiamHoaDon()
        +LoiVoucher() string
        +TinhThanhTien() decimal
    }
    class KhuyenMaiRepository {
        +KhuyenMaiRepository(SqlConnection c, SqlTransaction t)
        +LayVoucherHopLe()
        +LoiVoucher() string
        +DungVoucher() bool
    }
    class KetQuaGioHang {
        +string VoucherYeuCau
        +decimal TongTienHang
        +decimal TienGiamHoaDon
        +decimal TongThanhToan
    }
    class XacNhanGia {
        +DauVanTay() string
        +DauVanTayPOS() string
        +KiemTra() void
    }
    class GiaThanhToanThayDoiException
    class HoaDonRepository
    class DonHangOnlineRepository
    class GiaoDichBanHang {
        ~TinhLai() KetQuaGioHang
        ~DungVoucher() void
        ~GhiChiTiet() void
    }
    FrmPOS ..> GioHangService
    FrmPOS ..> HoaDonRepository
    DatHangController ..> GioHangService
    DatHangController ..> DonHangOnlineRepository
    GioHangService ..> TinhKhuyenMaiService
    GioHangService ..> KetQuaGioHang
    TinhKhuyenMaiService ..> KhuyenMaiRepository
    HoaDonRepository ..> XacNhanGia
    HoaDonRepository ..> GiaoDichBanHang
    DonHangOnlineRepository ..> GiaoDichBanHang
    GiaoDichBanHang ..> GioHangService
    GiaoDichBanHang ..> XacNhanGia
    XacNhanGia ..> GiaThanhToanThayDoiException
```

GiaoDichBanHang internal chia sẻ điều kiện ghi, không thay ranh giới quyền: POS dùng MaNV/QuyenForm; web dùng Authorize/MaKH chủ đơn. KhuyenMaiRepository gắn connection/transaction để không đọc giá qua kết nối khác khi thanh toán. Trigger SV1 là nơi duy nhất sửa tồn và TongTien; không biểu diễn trigger như class C#.
