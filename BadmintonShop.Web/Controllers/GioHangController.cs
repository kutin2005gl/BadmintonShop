using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;
using BadmintonShop.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonShop.Web.Controllers;

/// <summary>Gio hang luu trong Session (SV3). Khach chua dang nhap van them vao gio duoc; chi dat hang moi can dang nhap.</summary>
public class GioHangController : Controller
{
    private readonly GioHangService _gioHang = new();
    private readonly SanPhamRepository _sanPham = new();

    // TV3: xem gio hang
    public IActionResult Index() => View(_gioHang.TinhTuSession(HttpContext.Session));

    // TV3: them vao gio - SV2 goi tu trang chi tiet san pham: POST /GioHang/Them (maSP, soLuong, returnUrl)
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Them(int maSP, int soLuong = 1, string? returnUrl = null)
    {
        if (maSP <= 0 || soLuong <= 0) return BadRequest();

        var sp = _sanPham.LayTheoMa(maSP);
        if (sp == null || sp.TrangThai != "Đang bán")
        {
            TempData["Loi"] = "Sản phẩm không tồn tại hoặc đã ngừng bán.";
            return RedirectToAction(nameof(Index));
        }

        var gio = HttpContext.Session.LayGio();
        var i = gio.FindIndex(x => x.MaSP == maSP);
        if (i >= 0) gio[i] = gio[i] with { SoLuong = Math.Min(GioHangSession.SoLuongToiDa, gio[i].SoLuong + soLuong) };
        else gio.Add(new DongGioLuu(maSP, Math.Min(GioHangSession.SoLuongToiDa, soLuong)));
        HttpContext.Session.LuuGio(gio);

        TempData["ThongBao"] = $"Đã thêm “{sp.TenSP}” vào giỏ hàng.";
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction(nameof(Index));
    }

    // TV3: doi so luong 1 dong (<= 0 thi xoa dong)
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CapNhat(int maSP, int soLuong)
    {
        var gio = HttpContext.Session.LayGio();
        var i = gio.FindIndex(x => x.MaSP == maSP);
        if (i >= 0)
        {
            if (soLuong <= 0) gio.RemoveAt(i);
            else gio[i] = gio[i] with { SoLuong = Math.Min(GioHangSession.SoLuongToiDa, soLuong) };
            HttpContext.Session.LuuGio(gio);
        }
        return RedirectToAction(nameof(Index));
    }

    // TV3: xoa 1 san pham khoi gio
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Xoa(int maSP)
    {
        var gio = HttpContext.Session.LayGio();
        gio.RemoveAll(x => x.MaSP == maSP);
        HttpContext.Session.LuuGio(gio);
        return RedirectToAction(nameof(Index));
    }

    // TV3: nhap ma giam gia (kiem tra hop le khi tinh lai gio)
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ApDungVoucher(string? maVoucher)
    {
        HttpContext.Session.LuuVoucher(maVoucher);
        return RedirectToAction(nameof(Index));
    }

    // TV3: bo ma giam gia
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult BoVoucher()
    {
        HttpContext.Session.LuuVoucher(null);
        return RedirectToAction(nameof(Index));
    }
}
