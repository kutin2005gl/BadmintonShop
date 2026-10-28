
namespace BadmintonShop.Core.Models;

public class DanhGia
{
    public int MaDG { get; set; }

    public int MaSP { get; set; }

    public int MaKH { get; set; }

    public int SoSao { get; set; }

    public string? NoiDung { get; set; }

    public DateTime NgayDanhGia { get; set; }

    public string TrangThai { get; set; } = "Chờ duyệt";
}
