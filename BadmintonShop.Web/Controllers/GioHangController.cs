using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
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
        // TV1: tra ket qua them hang kem thong bao
        IActionResult KetQuaThem(bool thanhCong, string thongBao)
            => TraKetQua(thanhCong, thongBao, gio, returnUrl, hienThongBao: true);

        if (!ModelState.IsValid || maSP <= 0 || soLuong <= 0)
            return KetQuaThem(false, "Sản phẩm hoặc số lượng không hợp lệ.");

        var sp = _sanPham.LayTheoMa(maSP);
        var i = gio.FindIndex(x => x.MaSP == maSP);
        var tongDong = (long)soLuong + (i >= 0 ? gio[i].SoLuong : 0);
        if (LoiTangSoLuong(sp, tongDong) is { } loi)
            return KetQuaThem(false, loi);

        if (i >= 0) gio[i] = gio[i] with { SoLuong = (int)tongDong };
        else gio.Add(new DongGioLuu(maSP, soLuong));
        HttpContext.Session.LuuGio(gio);

        return KetQuaThem(true, $"Đã thêm “{sp!.TenSP}” vào giỏ hàng.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    // TV1: doi so luong mot dong gio hoac xoa khi ve khong
    public IActionResult CapNhat(int maSP, int soLuong, string? returnUrl = null)
    {
        var gio = HttpContext.Session.LayGio();
        if (!ModelState.IsValid || maSP <= 0)
            return TraKetQua(false, "Sản phẩm hoặc số lượng không hợp lệ.", gio, returnUrl);

        var i = gio.FindIndex(x => x.MaSP == maSP);
        if (i < 0)
            return TraKetQua(false, "Sản phẩm không có trong giỏ hàng.", gio, returnUrl);

        if (soLuong > gio[i].SoLuong && LoiTangSoLuong(_sanPham.LayTheoMa(maSP), soLuong) is { } loi)
            return TraKetQua(false, loi, gio, returnUrl);

        if (soLuong <= 0) gio.RemoveAt(i);
        else gio[i] = gio[i] with { SoLuong = soLuong };
        HttpContext.Session.LuuGio(gio);
        return TraKetQua(true, "Đã cập nhật giỏ hàng.", gio, returnUrl);
    }

    [HttpPost, ValidateAntiForgeryToken]
    // TV1: xoa mot san pham khoi gio
    public IActionResult Xoa(int maSP, string? returnUrl = null)
    {
        var gio = HttpContext.Session.LayGio();
        if (!ModelState.IsValid || maSP <= 0)
            return TraKetQua(false, "Sản phẩm không hợp lệ.", gio, returnUrl);

        gio.RemoveAll(x => x.MaSP == maSP);
        HttpContext.Session.LuuGio(gio);
        return TraKetQua(true, "Đã xóa sản phẩm khỏi giỏ hàng.", gio, returnUrl);
    }

    // TV1: kiem tra ton kho va gioi han khi tang so luong
    private static string? LoiTangSoLuong(SanPham? sp, long soLuong)
    {
        if (sp == null || sp.TrangThai != "Đang bán") return "Sản phẩm không tồn tại hoặc đã ngừng bán.";
        if (sp.SoLuongTon <= 0) return $"“{sp.TenSP}” đã hết hàng.";
        if (soLuong > sp.SoLuongTon) return $"“{sp.TenSP}” chỉ còn {sp.SoLuongTon} sản phẩm.";
        if (soLuong > GioHangSession.SoLuongToiDa)
            return $"Mỗi sản phẩm được thêm tối đa {GioHangSession.SoLuongToiDa} món.";
        return null;
    }

    // TV1: tra JSON cho AJAX hoac quay lai URL noi bo
    private IActionResult TraKetQua(bool thanhCong, string thongBao, List<DongGioLuu> gio, string? returnUrl, bool hienThongBao = false)
    {
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            var data = new { thanhCong, thongBao, tongSoLuong = gio.Sum(d => d.SoLuong) };
            return thanhCong ? Json(data) : BadRequest(data);
        }

        if (!thanhCong || hienThongBao)
            TempData[thanhCong ? "GioHang.ThongBao" : "GioHang.Loi"] = thongBao;
        return QuayLai(returnUrl);
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
