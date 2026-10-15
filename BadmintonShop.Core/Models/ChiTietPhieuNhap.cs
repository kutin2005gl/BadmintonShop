namespace BadmintonShop.Core.Models;

/// <summary>Bang ChiTietPhieuNhap</summary>
public class ChiTietPhieuNhap
{
    public int MaPN { get; set; }
    public int MaSP { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }  // cot tinh toan trong CSDL, chi doc
}
