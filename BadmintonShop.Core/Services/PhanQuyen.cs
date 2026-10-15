namespace BadmintonShop.Core.Services;

/// <summary>Cac nhom chuc nang tren Desktop - dung de an/hien menu theo vai tro.</summary>
public enum ChucNang
{
    NhanVienTaiKhoan,        // quan ly nhan vien & cap tai khoan (SV1)
    NhaCungCapPhieuNhap,     // nha cung cap, phieu nhap (SV1)
    SanPhamDanhMucKhuyenMai, // san pham, danh muc (SV4), khuyen mai (SV3)
    DonHangPOS,              // ban hang tai quay, xu ly don hang (SV3/SV4)
    ThongKeBinhLuan,         // thong ke - bao cao, duyet binh luan (SV2/SV4)
    SaoLuuPhucHoi,           // sao luu - phuc hoi CSDL (SV1)
}

/// <summary>
/// Bang phan quyen theo vai tro (ROADMAP 2.1). Moi form/menu chi goi PhanQuyen.CoQuyen,
/// KHONG tu so sanh chuoi vai tro rai rac trong code.
/// </summary>
public static class PhanQuyen
{
    // TV1: kiem tra vai tro co duoc dung nhom chuc nang hay khong
    public static bool CoQuyen(string? vaiTro, ChucNang chucNang) => vaiTro switch
    {
        VaiTro.Admin    => true,
        VaiTro.QuanLy   => chucNang is not (ChucNang.NhanVienTaiKhoan or ChucNang.SaoLuuPhucHoi),
        VaiTro.NhanVien => chucNang == ChucNang.DonHangPOS,
        _               => false,
    };

    // TV1: doi ma vai tro sang ten tieng Viet de hien thi
    public static string TenVaiTro(string? vaiTro) => vaiTro switch
    {
        VaiTro.Admin    => "Quản trị viên",
        VaiTro.QuanLy   => "Quản lý",
        VaiTro.NhanVien => "Nhân viên",
        _               => "Không xác định",
    };
}
