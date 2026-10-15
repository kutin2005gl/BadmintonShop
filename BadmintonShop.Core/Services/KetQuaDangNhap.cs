namespace BadmintonShop.Core.Services;

/// <summary>Ket qua dang nhap - dung chung cho TaiKhoanRepository (Desktop) va KhachHangRepository (Web).</summary>
public enum KetQuaDangNhap
{
    ThanhCong,
    SaiThongTin,  // sai ten dang nhap/email hoac mat khau (khong noi ro sai cai nao)
    BiKhoa,       // dung mat khau nhung TrangThai = 0
}
