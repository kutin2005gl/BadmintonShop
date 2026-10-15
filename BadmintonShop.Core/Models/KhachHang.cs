namespace BadmintonShop.Core.Models;

/// <summary>Bang KhachHang</summary>
public class KhachHang
{
    public int MaKH { get; set; }
    public string HoTen { get; set; } = "";
    public string? SDT { get; set; }
    public string Email { get; set; } = "";
    public string MatKhau { get; set; } = "";  // chuoi bam BCrypt
    public string? DiaChi { get; set; }
    public DateTime NgayDangKy { get; set; }
    public bool TrangThai { get; set; }  // false = bi khoa
}
