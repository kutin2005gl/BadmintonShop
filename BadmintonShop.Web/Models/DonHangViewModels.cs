using System.Data;
using BadmintonShop.Core.Models;

namespace BadmintonShop.Web.Models;

public class DatHangViewModel
{
    public KetQuaGioHang Gio { get; set; } = new();
    public string HoTen { get; set; } = "";
    public string DiaChi { get; set; } = "";
    public string HinhThucTT { get; set; } = "COD";
    public string? Loi { get; set; }
}

public class ChiTietDonViewModel
{
    public HoaDon Don { get; set; } = new();
    public DataTable Dong { get; set; } = new();
}
