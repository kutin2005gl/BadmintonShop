
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

    [HttpGet]
    public IActionResult Index()
    {
        var danhMuc = _danhMucRepository.LayTatCa();

        var sanPham = _sanPhamRepository
            .LayTatCa(null, null)
            .Where(sp => sp.TrangThai == "Đang bán")
            .OrderByDescending(sp => sp.MaSP)
            .Take(8)
            .ToList();

        ViewBag.DanhMuc = danhMuc;

        return View(sanPham);
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
