using BadmintonShop.Core.Models;

namespace BadmintonShop.Web.Models;

/// <summary>Du lieu trang chu: san pham dang ban + danh muc (giao dien SmashPro).</summary>
public class TrangChuViewModel
{
    public List<SanPham> SanPham { get; set; } = new();
    public List<DanhMuc> DanhMuc { get; set; } = new();
}

/// <summary>Du lieu cho 1 the san pham (partial _SanPhamCard).</summary>
public class TheSanPham
{
    public SanPham SanPham { get; set; } = new();
    public string? TenDanhMuc { get; set; }
    public string? Nhan { get; set; }
}
