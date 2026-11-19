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

    // TV3: nhap ma giam gia (kiem tra hop le khi tinh lai gio); goi tu trang Gio hang hoac trang Dat hang (returnUrl)
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ApDungVoucher(string? maVoucher, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(maVoucher))
        {
            TempData["Loi"] = "Vui lòng nhập mã giảm giá.";
            return QuayLai(returnUrl);
        }
        // CO-02: bao som neu khach dang nhap da dung ma nay (khach chua dang nhap duoc kiem luc dat hang)
        var maKH = User.Identity?.IsAuthenticated == true ? User.LayMaKH() : null;
        if (maKH != null && new KhuyenMaiRepository().KhachDaDungVoucher(maKH.Value, maVoucher))
        {
            HttpContext.Session.LuuVoucher(null);
            TempData["Loi"] = KhuyenMaiRepository.ThongBaoDaDungVoucher;
            return QuayLai(returnUrl);
        }
        HttpContext.Session.LuuVoucher(maVoucher);
        return QuayLai(returnUrl);
    }

    // TV3: bo ma giam gia
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult BoVoucher(string? returnUrl = null)
    {
        HttpContext.Session.LuuVoucher(null);
        return QuayLai(returnUrl);
    }

    // TV3: quay ve trang goi (chi nhan URL noi bo), mac dinh ve gio hang
    private IActionResult QuayLai(string? returnUrl)
        => !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction(nameof(Index));
}
