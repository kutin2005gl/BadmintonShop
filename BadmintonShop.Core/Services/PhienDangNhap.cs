namespace BadmintonShop.Core.Services;

/// <summary>Ket qua tra ve cua TaiKhoanRepository.DangNhap.</summary>
public enum KetQuaDangNhap
{
    ThanhCong,
    SaiThongTin,  // sai ten dang nhap hoac mat khau (khong noi ro sai cai nao)
    BiKhoa,       // dung mat khau nhung TaiKhoan.TrangThai = 0
}

/// <summary>
/// Phien dang nhap Desktop (static, 1 nguoi dung / 1 lan chay app).
/// Cac form lay nguoi dang dang nhap qua day, vd. PhieuNhap.MaNV = PhienDangNhap.MaNV.
/// </summary>
public static class PhienDangNhap
{
    public static int MaTK { get; private set; }
    public static int MaNV { get; private set; }
    public static string TenDangNhap { get; private set; } = "";
    public static string HoTen { get; private set; } = "";
    public static string VaiTro { get; private set; } = "";

    public static bool DaDangNhap => MaTK > 0;

    // TV1: luu thong tin nguoi dung vao phien sau khi dang nhap thanh cong
    public static void BatDau(int maTK, int maNV, string tenDangNhap, string hoTen, string vaiTro)
    {
        MaTK        = maTK;
        MaNV        = maNV;
        TenDangNhap = tenDangNhap;
        HoTen       = hoTen;
        VaiTro      = vaiTro;
    }

    // TV1: xoa thong tin phien khi dang xuat
    public static void DangXuat() => BatDau(0, 0, "", "", "");

    // TV1: kiem tra nguoi dang dang nhap co quyen dung nhom chuc nang hay khong
    public static bool CoQuyen(ChucNang chucNang) => DaDangNhap && PhanQuyen.CoQuyen(VaiTro, chucNang);
}
