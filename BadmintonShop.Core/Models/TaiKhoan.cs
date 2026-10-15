namespace BadmintonShop.Core.Models;

/// <summary>Bang TaiKhoan</summary>
public class TaiKhoan
{
    public int MaTK { get; set; }
    public string TenDangNhap { get; set; } = "";
    public string MatKhau { get; set; } = "";  // chuoi bam BCrypt
    public string VaiTro { get; set; } = "";  // ADMIN / QUANLY / NHANVIEN (xem VaiTro.cs)
    public int MaNV { get; set; }
    public bool TrangThai { get; set; }  // false = bi khoa
}
