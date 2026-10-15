namespace BadmintonShop.Core.Models;

/// <summary>Bang SanPham</summary>
public class SanPham
{
    public int MaSP { get; set; }
    public string TenSP { get; set; } = "";
    public int MaDM { get; set; }
    public string? ThuongHieu { get; set; }
    public decimal DonGiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public string DonViTinh { get; set; } = "";
    public string? HinhAnh { get; set; }
    public string? MoTa { get; set; }
    public string TrangThai { get; set; } = "";
}
