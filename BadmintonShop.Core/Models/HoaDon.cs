namespace BadmintonShop.Core.Models;

/// <summary>Bang HoaDon</summary>
public class HoaDon
{
    public int MaHD { get; set; }
    public int? MaKH { get; set; }
    public int? MaNV { get; set; }
    public DateTime NgayLap { get; set; }
    public string LoaiBan { get; set; } = "";  // Online / Tại quầy
    public string HinhThucTT { get; set; } = "";  // COD / Online / Tiền mặt
    public string? DiaChiGiao { get; set; }
    public int? MaCTKM { get; set; }
    public decimal TienGiamGia { get; set; }
    public decimal TongTien { get; set; }  // trigger tu tinh
    public string TrangThai { get; set; } = "";
}
