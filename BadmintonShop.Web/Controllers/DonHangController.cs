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
    private readonly KhachHangRepository _khachHang = new();

    private int MaKH => User.LayMaKH() ?? 0;

    // Tra cuu don cho khach chua dang nhap (CO-01): chi xem DUNG don da tra cuu, khong lo lich su cua SDT
    private const string TraCuuHDKey = "TraCuu.MaHD";
    private const string TraCuuKHKey = "TraCuu.MaKH";
    private const string SoLanSaiKey = "TraCuu.SoLanSai";
    private const string KhoaDenKey = "TraCuu.KhoaDen";
    private const string SdtChoOtpKey = "TraCuu.SdtChoOtp";       // SDT dang cho nhap OTP
    private const string SdtDaXacThucKey = "TraCuu.SdtDaXacThuc"; // SDT da qua OTP -> xem duoc moi don cua SDT nay
    private const int SoLanSaiToiDa = 5;
    private static readonly TimeSpan ThoiGianKhoa = TimeSpan.FromMinutes(15);
    private const string LoiChung = "Không tìm thấy đơn hàng với thông tin đã nhập.";

    // TV1: trang tra cuu don bang ma don + SDT, hoac chi SDT + OTP (khong can dang nhap)
    [HttpGet, AllowAnonymous]
    public IActionResult TraCuu() => View(new TraCuuDonViewModel());

    // TV1: chi co SDT -> chuyen sang nhap OTP; co ca ma don + SDT -> kiem khop thi luu quyen xem DUNG don do trong session; sai qua 5 lan khoa 15 phut
    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public IActionResult TraCuu(string? maDon, string? sdt)
    {
        var vm = new TraCuuDonViewModel { MaDon = maDon ?? "", SDT = sdt ?? "" };
        var s = HttpContext.Session;

        var conKhoa = PhutConKhoa();
        if (conKhoa > 0)
        {
            vm.Loi = $"Bạn đã nhập sai quá nhiều lần. Vui lòng thử lại sau {conKhoa} phút.";
            return View(vm);
        }
        if (string.IsNullOrWhiteSpace(sdt))
        {
            vm.Loi = "Vui lòng nhập số điện thoại đặt hàng.";
            return View(vm);
        }

        var so = KhachHangRepository.ChuanHoaSDT(sdt);

        // Chi nhap SDT -> xac thuc OTP roi xem danh sach don cua SDT do
        if (string.IsNullOrWhiteSpace(maDon))
        {
            if (so == null)
            {
                vm.Loi = "Số điện thoại không hợp lệ.";
                return View(vm);
            }
            s.SetString(SdtChoOtpKey, so);
            return RedirectToAction(nameof(NhapOtp));
        }

        int? maKH = null;
        if (int.TryParse(maDon.Trim().TrimStart('#'), out var maHD) && maHD > 0 && so != null)
            maKH = _donHang.TraCuuMaKHTheoSDT(maHD, so);

        if (maKH == null)
        {
            var sai = (s.GetInt32(SoLanSaiKey) ?? 0) + 1;
            if (sai >= SoLanSaiToiDa)
            {
                s.Remove(SoLanSaiKey);
                s.SetString(KhoaDenKey, DateTime.UtcNow.Add(ThoiGianKhoa).Ticks.ToString());
                vm.Loi = $"Bạn đã nhập sai quá nhiều lần. Vui lòng thử lại sau {(int)ThoiGianKhoa.TotalMinutes} phút.";
            }
            else
            {
                s.SetInt32(SoLanSaiKey, sai);
                vm.Loi = LoiChung;
            }
            return View(vm);
        }

        s.SetInt32(TraCuuHDKey, maHD);
        s.SetInt32(TraCuuKHKey, maKH.Value);
        return RedirectToAction(nameof(XemDon), new { id = maHD });
    }

    // TV1: trang nhap OTP gui toi SDT (DEMO: chua gui SMS that)
    [HttpGet, AllowAnonymous]
    public IActionResult NhapOtp()
    {
        var so = HttpContext.Session.GetString(SdtChoOtpKey);
        if (so == null) return RedirectToAction(nameof(TraCuu));
        return View(new TraCuuDonViewModel { SDT = DinhDang.CheSDT(so) });
    }

    // TV1: xac nhan OTP; DEMO chap nhan moi ma 6 chu so, dung thi cho xem danh sach don cua SDT trong phien nay
    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public IActionResult NhapOtp(string? otp)
    {
        var s = HttpContext.Session;
        var so = s.GetString(SdtChoOtpKey);
        if (so == null) return RedirectToAction(nameof(TraCuu));

        otp = otp?.Trim() ?? "";
        // TODO (sau demo): sinh OTP ngau nhien luc gui, luu session kem han 5 phut, so sanh o day + gioi han so lan sai
        if (otp.Length != 6 || !otp.All(char.IsDigit))
            return View(new TraCuuDonViewModel { SDT = DinhDang.CheSDT(so), Loi = "Mã OTP gồm 6 chữ số." });

        s.Remove(SdtChoOtpKey);
        s.SetString(SdtDaXacThucKey, so);
        return RedirectToAction(nameof(DanhSachTraCuu));
    }

    // TV1: danh sach don cua SDT da xac thuc OTP trong phien nay
    [HttpGet, AllowAnonymous, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult DanhSachTraCuu()
    {
        var so = HttpContext.Session.GetString(SdtDaXacThucKey);
        if (so == null) return RedirectToAction(nameof(TraCuu));
        ViewData["SDTChe"] = DinhDang.CheSDT(so);
        return View(_donHang.LayDonTheoSDT(so));
    }

    // TV1: xem 1 don da tra cuu dung (ma don + SDT) hoac thuoc SDT da xac thuc OTP trong phien nay (chi xem, khong huy)
    [HttpGet, AllowAnonymous, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult XemDon(int id)
    {
        var s = HttpContext.Session;
        int maKH;
        var sdtXacThuc = s.GetString(SdtDaXacThucKey);
        if (s.GetInt32(TraCuuHDKey) == id) maKH = s.GetInt32(TraCuuKHKey) ?? 0;
        else if (sdtXacThuc != null && _donHang.TraCuuMaKHTheoSDT(id, sdtXacThuc) is int kh)
        {
            maKH = kh;
            ViewData["TuDanhSach"] = true;
        }
        else return RedirectToAction(nameof(TraCuu));

        var hd = _donHang.LayDonCuaKhach(id, maKH);
        if (hd == null) return RedirectToAction(nameof(TraCuu));
        ViewData["TraCuu"] = true;
        ViewData["SDTChe"] = DinhDang.CheSDT(_khachHang.LayTheoMa(maKH)?.SDT);
        return View("ChiTiet", new ChiTietDonViewModel { Don = hd, Dong = _donHang.LayChiTietHienThi(id, maKH) });
    }

    // TV1: so phut con bi khoa tra cuu trong phien nay (0 = khong khoa)
    private int PhutConKhoa()
    {
        if (!long.TryParse(HttpContext.Session.GetString(KhoaDenKey), out var ticks)) return 0;
        var con = new DateTime(ticks, DateTimeKind.Utc) - DateTime.UtcNow;
        if (con <= TimeSpan.Zero) { HttpContext.Session.Remove(KhoaDenKey); return 0; }
        return (int)Math.Ceiling(con.TotalMinutes);
    }

    // TV3: danh sach don cua toi
    public IActionResult Index() => View(_donHang.LayLichSu(MaKH));

    // TV3: chi tiet + trang thai 1 don
    public IActionResult ChiTiet(int id)
    {
        var hd = _donHang.LayDonCuaKhach(id, MaKH);
        if (hd == null) return NotFound();
        return View(new ChiTietDonViewModel { Don = hd, Dong = _donHang.LayChiTietHienThi(id,MaKH) });
    }

    // Main–sub uses the same ownership checks as the full detail page; never cache personal orders.
    [HttpGet, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult ChiTietDong(int id)
    {
        var hd = _donHang.LayDonCuaKhach(id, MaKH);
        if (hd == null) return NotFound();
        ViewData["Nhung"] = true;
        return PartialView("ChiTiet", new ChiTietDonViewModel { Don = hd, Dong = _donHang.LayChiTietHienThi(id, MaKH) });
    }

    // TV3: khach huy don khi con "Cho xac nhan"
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Huy(int id, bool veDanhSach = false)
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
        // Huy tu panel main-sub o man hinh danh sach thi quay lai danh sach, khong nhay sang trang chi tiet
        return veDanhSach ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(ChiTiet), new { id });
    }
}
