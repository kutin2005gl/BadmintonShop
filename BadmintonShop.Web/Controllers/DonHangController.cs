using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;
using BadmintonShop.Web.Infrastructure;
using BadmintonShop.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Web.Controllers;

/// <summary>Lich su mua hang, theo doi trang thai, huy don (SV3). Khach chi thay don cua chinh minh.</summary>
[Authorize]
public class DonHangController : Controller
{
    private readonly DonHangOnlineRepository _donHang = new();

    private int MaKH => User.LayMaKH() ?? 0;

    // TV3: danh sach don cua toi
    public IActionResult Index() => View(_donHang.LayLichSu(MaKH));

    // TV3: chi tiet + trang thai 1 don
    public IActionResult ChiTiet(int id)
    {
        var hd = _donHang.LayDonCuaKhach(id, MaKH);
        if (hd == null) return NotFound();
        return View(new ChiTietDonViewModel { Don = hd, Dong = _donHang.LayChiTietHienThi(id,MaKH) });
    }

    // TV3: khach huy don khi con "Cho xac nhan"
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Huy(int id)
    {
        try
        {
            if (_donHang.HuyDon(id, MaKH)) TempData["ThongBao"] = "Đã hủy đơn hàng. Hàng được hoàn lại kho.";
            else TempData["Loi"] = "Không thể hủy: đơn đã được xác nhận hoặc không tồn tại.";
        }
        catch (SqlException ex)
        {
            TempData["Loi"] = LoiCsdl.ThongBao(ex);
        }
        return RedirectToAction(nameof(ChiTiet), new { id });
    }
}
