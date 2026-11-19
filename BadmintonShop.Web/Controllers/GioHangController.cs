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

    [HttpGet, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    // TV1: tai noi dung ngan gio theo phien
    public IActionResult NganGio()
    {
        var session = HttpContext.Session;
        var gio = _gioHang.Tinh(session.LayGio().Select(d => (d.MaSP, d.SoLuong)), session.LayVoucher());
        return PartialView("_NganGio", gio);
    }

    [HttpPost, ValidateAntiForgeryToken]
    // TV1: them san pham vao gio va tra ket qua
    public IActionResult Them(int maSP, int soLuong = 1, string? returnUrl = null)
    {
        var gio = HttpContext.Session.LayGio();
        // TV1: tra ket qua them hang theo loai yeu cau
        IActionResult TraKetQua(bool thanhCong, string thongBao)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var data = new { thanhCong, thongBao, tongSoLuong = gio.Sum(d => d.SoLuong) };
                return thanhCong ? Json(data) : BadRequest(data);
            }

            TempData[thanhCong ? "GioHang.ThongBao" : "GioHang.Loi"] = thongBao;
            return QuayLai(returnUrl);
        }

        if (!ModelState.IsValid || maSP <= 0 || soLuong <= 0)
            return TraKetQua(false, "Sản phẩm hoặc số lượng không hợp lệ.");

        var sp = _sanPham.LayTheoMa(maSP);
        if (sp == null || sp.TrangThai != "Đang bán")
            return TraKetQua(false, "Sản phẩm không tồn tại hoặc đã ngừng bán.");

        var i = gio.FindIndex(x => x.MaSP == maSP);
        var tongDong = (long)soLuong + (i >= 0 ? gio[i].SoLuong : 0);
        if (sp.SoLuongTon <= 0) return TraKetQua(false, $"“{sp.TenSP}” đã hết hàng.");
        if (tongDong > sp.SoLuongTon) return TraKetQua(false, $"“{sp.TenSP}” chỉ còn {sp.SoLuongTon} sản phẩm.");
        if (tongDong > GioHangSession.SoLuongToiDa)
            return TraKetQua(false, $"Mỗi sản phẩm được thêm tối đa {GioHangSession.SoLuongToiDa} món.");

        if (i >= 0) gio[i] = gio[i] with { SoLuong = (int)tongDong };
        else gio.Add(new DongGioLuu(maSP, soLuong));
        HttpContext.Session.LuuGio(gio);

        return TraKetQua(true, $"Đã thêm “{sp.TenSP}” vào giỏ hàng.");
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
