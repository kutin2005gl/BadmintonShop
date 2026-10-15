namespace BadmintonShop.Core.Models;

/// <summary>Bang ChiTietHoaDon</summary>
public class ChiTietHoaDon
{
    public int MaHD { get; set; }
    public int MaSP { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal GiamGia { get; set; }  // % giam theo san pham
    public decimal ThanhTien { get; set; }  // cot tinh toan trong CSDL, chi doc
}
