using BadmintonShop.Core.Models;

namespace BadmintonShop.Web.Models;

/// <summary>Mot danh muc hoac thuong hieu trong bang tim kiem: co MaDM la danh muc, khong co la thuong hieu.</summary>
public record MucTimKiem(string Ten, int? MaDM, int DaBan, int SoSanPham);

public class KhamPhaTimKiemViewModel
{
    public List<MucTimKiem> PhoBien { get; set; } = new();
    public List<MucTimKiem> DanhMuc { get; set; } = new();
    public List<MucTimKiem> ThuongHieu { get; set; } = new();
}

public class GoiYTimKiemViewModel
{
    public string TuKhoa { get; set; } = "";
    public int TongSo { get; set; }
    public List<(SanPham SanPham, string TenDanhMuc)> SanPham { get; set; } = new();
}
