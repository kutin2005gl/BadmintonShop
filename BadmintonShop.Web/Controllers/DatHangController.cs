using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;
using BadmintonShop.Web.Infrastructure;
using BadmintonShop.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Web.Controllers;

/// <summary>
/// Dat hang online: nhap dia chi, chon COD / thanh toan online gia lap (SV3).
/// Khach CHUA dang nhap van dat duoc: nhap ho ten + SDT, he thong lien ket khach theo SDT (CO-01).
/// Khach vang lai chi xem lai duoc DON VUA DAT trong phien nay, khong xem duoc lich su cua SDT do.
/// </summary>
public class DatHangController : Controller
{
    private readonly GioHangService _gioHang = new();
    private readonly DonHangOnlineRepository _donHang = new();
    private readonly KhachHangRepository _khachHang = new();

    private const string QuoteKey = "Checkout.ConfirmedQuote";
    private const string GuestHDKey = "Checkout.GuestMaHD";
    private const string GuestKHKey = "Checkout.GuestMaKH";

    private bool DaDangNhap => User.Identity?.IsAuthenticated == true && User.LayMaKH() != null;
    private int MaKHDangNhap => User.LayMaKH() ?? 0;

    // MaKH duoc phep xem don 'maHD': khach dang nhap, hoac khach vang lai vua dat dung don do trong phien nay
    private int MaKHXemDon(int maHD)
    {
        if (DaDangNhap) return MaKHDangNhap;
        var s = HttpContext.Session;
        return s.GetInt32(GuestHDKey) == maHD ? s.GetInt32(GuestKHKey) ?? 0 : 0;
    }

    // TV3: trang xac nhan don, dien san dia chi trong ho so
    [HttpGet]
    public IActionResult Index()
    {
        var gio = _gioHang.TinhTuSession(HttpContext.Session);
        if (gio.Dong.Count == 0) return RedirectToAction("Index", "GioHang");
        HttpContext.Session.SetString(QuoteKey, XacNhanGia.DauVanTay(gio));
        if (!DaDangNhap) return View(new DatHangViewModel { Gio = gio });
        var kh = _khachHang.LayTheoMa(MaKHDangNhap);
        return View(new DatHangViewModel { Gio = gio, DaDangNhap = true, HoTen = kh?.HoTen ?? "", SDT = kh?.SDT ?? "", DiaChi = kh?.DiaChi ?? "" });
    }

    // TV3: bam "Dat hang" -> tao don trong 1 transaction
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult XacNhan(string? diaChi, string? hinhThucTT, string? hoTen, string? sdt, string? ghiChu)
    {
        var gio = _gioHang.TinhTuSession(HttpContext.Session);
        if (gio.Dong.Count == 0) return RedirectToAction("Index", "GioHang");

        var tt = DonHangOnlineRepository.HinhThucHopLe.Contains(hinhThucTT ?? "") ? hinhThucTT! : "COD";
        var vm = new DatHangViewModel
        {
            Gio = gio,
            DaDangNhap = DaDangNhap,
            HoTen = DaDangNhap ? User.LayHoTen() ?? "" : hoTen ?? "",
            SDT = DaDangNhap ? "" : sdt ?? "",
            DiaChi = diaChi ?? "",
            GhiChu = ghiChu ?? "",
            HinhThucTT = tt,
        };

        try
        {
            if (HttpContext.Session.GetString(QuoteKey) != XacNhanGia.DauVanTay(gio))
            {
                HttpContext.Session.SetString(QuoteKey, XacNhanGia.DauVanTay(gio));
                vm.Loi = "Giỏ hàng hoặc khuyến mãi đã thay đổi. Kiểm tra tổng mới và bấm Đặt hàng để xác nhận lại.";
                return View("Index", vm);
            }
            var maKH = DaDangNhap ? MaKHDangNhap : _khachHang.LayHoacTaoKhachVangLai(vm.HoTen, vm.SDT, vm.DiaChi);
            var maHD = _donHang.TaoDon(maKH, vm.DiaChi, tt, gio, vm.GhiChu);
            HttpContext.Session.XoaGio(); HttpContext.Session.Remove(QuoteKey);
            if (!DaDangNhap)
            {
                HttpContext.Session.SetInt32(GuestHDKey, maHD);
                HttpContext.Session.SetInt32(GuestKHKey, maKH);
            }
            _ = EmailService.GuiXacNhanDonAsync(maHD, maKH);   // SV3: gui email xac nhan (khong cho, loi gui mail khong lam hong don)
            return tt == "Online"
                ? RedirectToAction(nameof(ThanhToanOnline), new { id = maHD })
                : RedirectToAction(nameof(ThanhCong), new { id = maHD });
        }
        catch (GiaThanhToanThayDoiException ex)
        {
            vm.Gio = ex.GioMoi; vm.Loi = ex.Message;
            HttpContext.Session.SetString(QuoteKey, XacNhanGia.DauVanTay(ex.GioMoi));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            vm.Loi = ex.Message;
        }
        catch (SqlException ex)
        {
            vm.Loi = LoiCsdl.ThongBao(ex);
        }
        return View("Index", vm);
    }

    // TV3: man hinh thanh toan online GIA LAP (khong goi cong thanh toan that)
    [HttpGet]
    public IActionResult ThanhToanOnline(int id)
    {
        var hd = _donHang.LayDonCuaKhach(id, MaKHXemDon(id));
        if (hd == null) return NotFound();
        if (hd.HinhThucTT != "Online" || hd.TrangThai != "Chờ xác nhận")
            return RedirectToAction(nameof(ThanhCong), new { id });
        var maKH = MaKHXemDon(id);
        var dong = _donHang.LayChiTietHienThi(id, maKH);
        var truoc = dong.Rows.Cast<System.Data.DataRow>().Sum(r => Convert.ToInt32(r["SoLuong"]) * Convert.ToDecimal(r["DonGia"]));
        var sauGiamSP = dong.Rows.Cast<System.Data.DataRow>().Sum(r => Convert.ToDecimal(r["ThanhTien"]));
        return View(new ThanhToanOnlineViewModel
        {
            Don = hd,
            TienHangTruocGiam = truoc,
            GiamSanPham = truoc - sauGiamSP,
            GiamHoaDon = hd.TienGiamGia,
            MaVoucher = new KhuyenMaiRepository().LayVoucherCuaHoaDon(id),
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ThanhToanOnline(int id, bool xacNhan = true)
    {
        var hd = _donHang.LayDonCuaKhach(id, MaKHXemDon(id));
        if (hd == null) return NotFound();
        TempData["ThongBao"] = "Thanh toán online (giả lập) thành công.";
        return RedirectToAction(nameof(ThanhCong), new { id });
    }

    // TV3: dat hang xong
    [HttpGet]
    public IActionResult ThanhCong(int id)
    {
        var hd = _donHang.LayDonCuaKhach(id, MaKHXemDon(id));
        if (hd == null) return NotFound();
        if (!DaDangNhap) ViewData["NangCap"] = KhachVuaDatCoTheNangCap(id) ? "form" : "daCoTaiKhoan";
        return View(new ChiTietDonViewModel { Don = hd, Dong = _donHang.LayChiTietHienThi(id, MaKHXemDon(id)) });
    }

    // TV1: khach vang lai vua dat don 'maHD' trong phien nay va van con la khach email gia -> duoc tao tai khoan
    private bool KhachVuaDatCoTheNangCap(int maHD)
    {
        var s = HttpContext.Session;
        if (s.GetInt32(GuestHDKey) != maHD || s.GetInt32(GuestKHKey) is not int maKH) return false;
        var kh = _khachHang.LayTheoMa(maKH);
        return kh != null && kh.TrangThai && KhachHangRepository.LaEmailVangLai(kh.Email);
    }

    // TV1: nang cap khach vang lai vua dat don thanh tai khoan (email + mat khau), dang nhap luon roi chuyen toi /DonHang
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoTaiKhoan(int id, string? email, string? matKhau, string? xacNhanMatKhau)
    {
        if (DaDangNhap) return RedirectToAction("Index", "DonHang");
        if (!KhachVuaDatCoTheNangCap(id)) return RedirectToAction(nameof(ThanhCong), new { id });
        var maKH = HttpContext.Session.GetInt32(GuestKHKey)!.Value;

        email = (email ?? "").Trim();
        string? loi = null;
        if (email.Length == 0 || email.Length > 100 || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email)
            || KhachHangRepository.LaEmailVangLai(email))
            loi = "Email không hợp lệ.";
        else if (!MatKhauHelper.LaMatKhauManh(matKhau ?? ""))
            loi = "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường và chữ số.";
        else if (matKhau != xacNhanMatKhau)
            loi = "Mật khẩu xác nhận không khớp.";
        else if (_khachHang.EmailDaTonTai(email))
            loi = "Email này đã được đăng ký.";

        if (loi == null)
        {
            try
            {
                if (!_khachHang.NangCapKhachVangLai(maKH, email, matKhau!))
                    loi = "Không thể tạo tài khoản cho số điện thoại này. Vui lòng liên hệ cửa hàng.";
            }
            catch (SqlException ex) when (ex.Number is 2627 or 2601)   // email vua bi nguoi khac dang ky cung luc
            {
                loi = "Email này đã được đăng ký.";
            }
            catch (SqlException ex)
            {
                loi = LoiCsdl.ThongBao(ex);
            }
        }

        if (loi != null)
        {
            TempData["LoiNangCap"] = loi;
            TempData["EmailNangCap"] = email;
            return RedirectToAction(nameof(ThanhCong), new { id });
        }

        var kh = _khachHang.LayTheoMa(maKH)!;
        await HttpContext.DangNhapKhachHangAsync(kh);
        HttpContext.Session.Remove(GuestHDKey);
        HttpContext.Session.Remove(GuestKHKey);
        TempData["ThongBao"] = "Đã tạo tài khoản. Từ nay bạn đăng nhập bằng email " + kh.Email + " để theo dõi đơn hàng.";
        return RedirectToAction("Index", "DonHang");
    }
}
