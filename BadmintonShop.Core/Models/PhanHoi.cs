namespace BadmintonShop.Core.Models;

/// <summary>Bang PhanHoi</summary>
public class PhanHoi
{
    public int MaPH { get; set; }
    public int MaKH { get; set; }
    public int MaSP { get; set; }
    public byte SoSao { get; set; }
    public string? NoiDung { get; set; }
    public DateTime NgayGui { get; set; }
    public string TrangThai { get; set; } = "";
}
