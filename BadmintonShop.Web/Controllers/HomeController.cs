
using System.Diagnostics;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonShop.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly SanPhamRepository _sanPhamRepository = new();
    private readonly DanhMucRepository _danhMucRepository = new();

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // Giao dien: trang chu hien thi san pham dang ban va danh muc (chi doc, khong doi nghiep vu)
    public IActionResult Index()
    {
        var sp = new SanPhamRepository().LayTatCa().Where(x => x.TrangThai == "Đang bán").ToList();
        return View(new TrangChuViewModel { SanPham = sp, DanhMuc = new DanhMucRepository().LayTatCa() });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
        });
    }
}
