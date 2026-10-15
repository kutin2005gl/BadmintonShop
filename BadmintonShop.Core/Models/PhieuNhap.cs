namespace BadmintonShop.Core.Models;

/// <summary>Bang PhieuNhap</summary>
public class PhieuNhap
{
    public int MaPN { get; set; }
    public DateTime NgayNhap { get; set; }
    public int MaNCC { get; set; }
    public int MaNV { get; set; }
    public decimal TongTien { get; set; }  // trigger tu tinh
}
