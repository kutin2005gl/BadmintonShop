namespace BadmintonShop.Core.Models;

/// <summary>Bang NhanVien</summary>
public class NhanVien
{
    public int MaNV { get; set; }
    public string HoTen { get; set; } = "";
    public string? SDT { get; set; }
    public string? Email { get; set; }
    public string? DiaChi { get; set; }
    public string ChucVu { get; set; } = "";
    public decimal? Luong { get; set; }
    public DateTime NgayVaoLam { get; set; }
}
