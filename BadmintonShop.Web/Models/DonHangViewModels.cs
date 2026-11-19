using System.Data;
using BadmintonShop.Core.Models;

namespace BadmintonShop.Web.Models;

public class DatHangViewModel
{
    public KetQuaGioHang Gio { get; set; } = new();
    public string HoTen { get; set; } = "";
    public string SDT { get; set; } = "";
    public bool DaDangNhap { get; set; }
    public string DiaChi { get; set; } = "";
    public string GhiChu { get; set; } = "";
    public string HinhThucTT { get; set; } = "COD";
    public string? Loi { get; set; }
}

public class ChiTietDonViewModel
{
    public HoaDon Don { get; set; } = new();
    public DataTable Dong { get; set; } = new();
}

/// <summary>Form tra cuu don cho khach chua dang nhap (ma don + SDT).</summary>
public class TraCuuDonViewModel
{
    public string MaDon { get; set; } = "";
    public string SDT { get; set; } = "";
    public string? Loi { get; set; }
}
