using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using BadmintonShop.Web.Models;
using BadmintonShop.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonShop.Web.Controllers;

/// <summary>Noi dung bang tim kiem tren header (tai khi khach mo o tim kiem).</summary>
public class TimKiemController : Controller
{
    private const int SoGoiY = 5;
    private readonly DanhMucCongKhai _catalog;
    public TimKiemController(DanhMucCongKhai catalog) => _catalog = catalog;

    // TV1: tim kiem pho bien, danh muc va thuong hieu noi bat, xep theo so luong da ban
    [HttpGet]
    public IActionResult KhamPha()
    {
        var (sanPham, danhMuc) = _catalog.Lay();
        var daBan = new SanPhamRepository().LayTongSoLuongBan();
        var dangBan = sanPham.Where(sp => sp.TrangThai == "Đang bán").ToList();
        int TongBan(IEnumerable<SanPham> nhom) => nhom.Sum(sp => daBan.GetValueOrDefault(sp.MaSP));

        var cacDanhMuc = danhMuc
            .Select(dm => (dm, nhom: dangBan.Where(sp => sp.MaDM == dm.MaDM).ToList()))
            .Where(x => x.nhom.Count > 0)
            .Select(x => new MucTimKiem(x.dm.TenDanhMuc, x.dm.MaDM, TongBan(x.nhom), x.nhom.Count));
        var cacThuongHieu = dangBan
            .Where(sp => !string.IsNullOrWhiteSpace(sp.ThuongHieu))
            .GroupBy(sp => sp.ThuongHieu!.Trim(), StringComparer.CurrentCultureIgnoreCase)
            .Select(g => new MucTimKiem(g.Key, null, TongBan(g), g.Count()));

        List<MucTimKiem> XepHang(IEnumerable<MucTimKiem> ds, int soLuong) => ds
            .OrderByDescending(m => m.DaBan).ThenByDescending(m => m.SoSanPham).ThenBy(m => m.Ten)
            .Take(soLuong).ToList();

        var topDanhMuc = XepHang(cacDanhMuc, 4);
        var topThuongHieu = XepHang(cacThuongHieu, 4);
        return PartialView("_KhamPha", new KhamPhaTimKiemViewModel
        {
            PhoBien = XepHang(topDanhMuc.Concat(topThuongHieu), 5),
            DanhMuc = topDanhMuc,
            ThuongHieu = topThuongHieu,
        });
    }

    // TV1: goi y san pham theo tu khoa, cung dieu kien loc voi trang /SanPham
    [HttpGet]
    public IActionResult GoiY(string? tuKhoa)
    {
        var tk = (tuKhoa ?? "").Trim();
        if (tk.Length is 0 or > 100) return PartialView("_GoiY", new GoiYTimKiemViewModel { TuKhoa = tk });

        var (sanPham, danhMuc) = _catalog.Lay();
        var khop = sanPham.Where(sp => sp.TrangThai == "Đang bán"
            && $"{sp.TenSP} {sp.ThuongHieu}".Contains(tk, StringComparison.CurrentCultureIgnoreCase)).ToList();
        var daBan = khop.Count > 0 ? new SanPhamRepository().LayTongSoLuongBan() : new();

        return PartialView("_GoiY", new GoiYTimKiemViewModel
        {
            TuKhoa = tk,
            TongSo = khop.Count,
            SanPham = khop
                .OrderByDescending(sp => sp.TenSP.StartsWith(tk, StringComparison.CurrentCultureIgnoreCase))
                .ThenByDescending(sp => daBan.GetValueOrDefault(sp.MaSP))
                .ThenBy(sp => sp.TenSP)
                .Take(SoGoiY)
                .Select(sp => (sp, danhMuc.FirstOrDefault(d => d.MaDM == sp.MaDM)?.TenDanhMuc ?? ""))
                .ToList(),
        });
    }
}
