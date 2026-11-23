using BadmintonShop.Web.Services;
using Microsoft.AspNetCore.Http;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;
using BadmintonShop.Web.Infrastructure;
using BadmintonShop.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonShop.Web.Controllers;

public class TaiKhoanController : Controller
{
    private readonly KhachHangRepository _khachHangRepository = new();
    private readonly OtpService _otpService;
    private readonly IWebHostEnvironment _environment;

    public TaiKhoanController(
        OtpService otpService,
        IWebHostEnvironment environment)
    {
        _otpService = otpService;
        _environment = environment;
    }

    // GET: /TaiKhoan/DangNhap
    [HttpGet]
    [AllowAnonymous]
    public IActionResult DangNhap(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new DangNhapViewModel());
    }

    // POST: /TaiKhoan/DangNhap
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(
        DangNhapViewModel model,
        string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var ketQua = _khachHangRepository.DangNhap(
            model.Email,
            model.MatKhau,
            out var khachHang
        );

        if (ketQua == KetQuaDangNhap.BiKhoa)
        {
            ModelState.AddModelError(
                string.Empty,
                "Tài khoản của bạn đã bị khóa."
            );
            return View(model);
        }

        if (ketQua != KetQuaDangNhap.ThanhCong ||
            khachHang == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Email hoặc mật khẩu không chính xác."
            );
            return View(model);
        }

        await HttpContext.DangNhapKhachHangAsync(
            khachHang,
            model.GhiNho
        );

        if (!string.IsNullOrEmpty(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    // POST: /TaiKhoan/DangXuat
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangXuat()
    {
        await HttpContext.DangXuatKhachHangAsync();
        return RedirectToAction("Index", "Home");
    }

    // GET: /TaiKhoan/DangKy
    [HttpGet]
    [AllowAnonymous]
    public IActionResult DangKy()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new DangKyViewModel());
    }

    // POST: /TaiKhoan/DangKy
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangKy(DangKyViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (!MatKhauHelper.LaMatKhauManh(model.MatKhau))
        {
            ModelState.AddModelError(
                nameof(model.MatKhau),
                "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường và chữ số.");
            return View(model);
        }

        if (_khachHangRepository.EmailDaTonTai(model.Email))
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "Email này đã được đăng ký."
            );

            return View(model);
        }

        if (_khachHangRepository.SdtDaTonTai(model.SDT))
        {
            // Khach da mua vang lai bang SDT nay: cho nang cap len tai khoan that sau khi xac thuc OTP (chi demo/Development)
            var vangLai = _environment.IsDevelopment() ? _khachHangRepository.LayKhachVangLaiTheoSdt(model.SDT) : null;
            if (vangLai != null)
            {
                var s = HttpContext.Session;
                s.SetInt32(DkMaKHKey, vangLai.MaKH);
                s.SetString(DkEmailKey, model.Email.Trim().ToLowerInvariant());
                s.SetString(DkMatKhauKey, MatKhauHelper.BamMatKhau(model.MatKhau));
                s.Remove(DkDaXacThucKey);
                PhatOtpDangKy(vangLai.MaKH);
                return RedirectToAction(nameof(XacThucSdt));
            }
            ModelState.AddModelError(
                nameof(model.SDT),
                "Số điện thoại này đã được đăng ký.");
            return View(model);
        }

        var khachHangMoi = new KhachHang
        {
            HoTen = model.HoTen,
            Email = model.Email,
            // TV1: luu SDT dang chuan 0xxxxxxxxx de khop voi khach vang lai / tra cuu don
            SDT = KhachHangRepository.ChuanHoaSDT(model.SDT) ?? model.SDT,
            DiaChi = model.DiaChi
        };

        try
        {
            int maKH = _khachHangRepository.DangKy(
                khachHangMoi,
                model.MatKhau
            );

            var khachHang = _khachHangRepository.LayTheoMa(maKH);

            if (khachHang == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể lấy thông tin tài khoản vừa đăng ký."
                );

                return View(model);
            }

            await HttpContext.DangNhapKhachHangAsync(
                khachHang,
                false
            );

            return RedirectToAction("Index", "Home");
        }
        catch (Microsoft.Data.SqlClient.SqlException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Không thể hoàn tất đăng ký. Vui lòng kiểm tra thông tin hoặc thử lại."
            );

            return View(model);
        }
    }

    private const string DkMaKHKey = "DangKy.MaKH";
    private const string DkEmailKey = "DangKy.Email";
    private const string DkMatKhauKey = "DangKy.MatKhauBam";
    private const string DkDaXacThucKey = "DangKy.DaXacThuc";

    private const string DkOtpDemoKey = "DangKy.OtpDemo";
    private const string DkGuiLucKey = "DangKy.GuiLuc";
    private const int GiayChoGuiLai = 60;

    private static string DkOtpKhoa(int maKH) => "dangky:" + maKH;

    // TV1: sinh OTP moi cho khach vang lai, luu ma demo + thoi diem gui trong session (de hien lai khi nhap sai va tinh 60s gui lai)
    private void PhatOtpDangKy(int maKH)
    {
        var s = HttpContext.Session;
        s.SetString(DkOtpDemoKey, _otpService.TaoOtp(DkOtpKhoa(maKH)));
        s.SetString(DkGuiLucKey, DateTime.UtcNow.Ticks.ToString());
    }

    // TV1: so giay con phai cho truoc khi duoc gui lai OTP (0 = duoc gui)
    private int GiayConChoGuiLai()
    {
        if (!long.TryParse(HttpContext.Session.GetString(DkGuiLucKey), out var ticks)) return 0;
        var da = DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc);
        return Math.Max(0, (int)Math.Ceiling(GiayChoGuiLai - da.TotalSeconds));
    }

    // TV1: nap du lieu hien thi cho man hinh OTP (ma demo + dem nguoc gui lai)
    private void NapDuLieuOtp()
    {
        ViewData["OtpDemo"] = HttpContext.Session.GetString(DkOtpDemoKey);
        ViewData["GiayCho"] = GiayConChoGuiLai();
    }

    // GET: /TaiKhoan/XacThucSdt
    // TV1: man hinh nhap OTP xac nhan quyen so huu SDT cua khach vang lai dang muon dang ky
    [HttpGet]
    [AllowAnonymous]
    public IActionResult XacThucSdt()
    {
        if (!_environment.IsDevelopment() || HttpContext.Session.GetInt32(DkMaKHKey) == null)
            return RedirectToAction(nameof(DangKy));
        NapDuLieuOtp();
        return View(new XacThucOtpViewModel());
    }

    // POST: /TaiKhoan/GuiLaiOtpSdt
    // TV1: gui lai OTP, chi cho phep sau khi het 60 giay ke tu lan gui truoc (kiem o server)
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult GuiLaiOtpSdt()
    {
        if (!_environment.IsDevelopment() || HttpContext.Session.GetInt32(DkMaKHKey) is not int maKH)
            return RedirectToAction(nameof(DangKy));

        var cho = GiayConChoGuiLai();
        if (cho > 0)
            TempData["OtpLoi"] = $"Vui lòng đợi {cho} giây nữa để gửi lại mã.";
        else
        {
            PhatOtpDangKy(maKH);
            TempData["OtpThongBao"] = "Đã gửi mã OTP mới. Mã cũ không còn hiệu lực.";
        }
        return RedirectToAction(nameof(XacThucSdt));
    }

    // POST: /TaiKhoan/XacThucSdt
    // TV1: dung OTP -> nang cap khach vang lai thanh tai khoan that (email + mat khau da nhap) va dang nhap luon
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XacThucSdt(XacThucOtpViewModel model)
    {
        var s = HttpContext.Session;
        if (!_environment.IsDevelopment() || s.GetInt32(DkMaKHKey) is not int maKH
            || s.GetString(DkEmailKey) is not string email || s.GetString(DkMatKhauKey) is not string matKhauBam)
            return RedirectToAction(nameof(DangKy));

        NapDuLieuOtp();
        if (!ModelState.IsValid)
            return View(model);

        if (!_otpService.XacThucOtp(DkOtpKhoa(maKH), model.MaOtp))
        {
            ModelState.Remove(nameof(model.MaOtp));
            model.MaOtp = "";
            ModelState.AddModelError(nameof(model.MaOtp), "Mã OTP không chính xác, đã hết hạn hoặc vượt quá số lần thử. Hãy kiểm tra lại hoặc gửi lại mã mới.");
            return View(model);
        }

        string? loi = null;
        try
        {
            if (_khachHangRepository.EmailDaTonTai(email))
                loi = "Email này đã được đăng ký.";
            else if (!_khachHangRepository.NangCapKhachVangLaiDaBam(maKH, email, matKhauBam))
                loi = "Không thể tạo tài khoản cho số điện thoại này. Vui lòng liên hệ cửa hàng.";
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 2627 or 2601)
        {
            loi = "Email này đã được đăng ký.";
        }

        foreach (var k in new[] { DkMaKHKey, DkEmailKey, DkMatKhauKey, DkDaXacThucKey, DkOtpDemoKey, DkGuiLucKey }) s.Remove(k);

        if (loi != null)
        {
            ModelState.AddModelError(string.Empty, loi + " Vui lòng đăng ký lại.");
            return View("DangKy", new DangKyViewModel());
        }

        var khachHang = _khachHangRepository.LayTheoMa(maKH)!;
        await HttpContext.DangNhapKhachHangAsync(khachHang, false);
        TempData["ThongBao"] = "Đã tạo tài khoản. Các đơn bạn đã đặt bằng số điện thoại này nay nằm trong lịch sử đơn hàng của tài khoản.";
        return RedirectToAction("Index", "Home");
    }

    // GET: /TaiKhoan/HoSo
    [HttpGet]
    [Authorize]
    public IActionResult HoSo()
    {
        var maKH = User.LayMaKH();

        if (maKH == null)
            return Challenge();

        var khachHang = _khachHangRepository.LayTheoMa(maKH.Value);

        if (khachHang == null || !khachHang.TrangThai)
            return NotFound();

        var model = new HoSoViewModel
        {
            HoTen = khachHang.HoTen,
            Email = khachHang.Email,
            SDT = khachHang.SDT,
            DiaChi = khachHang.DiaChi
        };

        return View(model);
    }

    // POST: /TaiKhoan/HoSo
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult HoSo(HoSoViewModel model)
    {
        var maKH = User.LayMaKH();

        if (maKH == null)
            return Challenge();

        var khachHang = _khachHangRepository.LayTheoMa(maKH.Value);

        if (khachHang == null || !khachHang.TrangThai)
            return NotFound();

        // Email luôn lấy từ database, không tin dữ liệu gửi từ trình duyệt.
        model.Email = khachHang.Email;

        if (!ModelState.IsValid)
            return View(model);

        var thanhCong = _khachHangRepository.CapNhatHoSo(
            maKH.Value,
            model.HoTen,
            model.SDT,
            model.DiaChi
        );

        if (!thanhCong)
        {
            ModelState.AddModelError(
                string.Empty,
                "Không thể cập nhật hồ sơ. Vui lòng thử lại."
            );

            return View(model);
        }

        TempData["ThongBao"] = "Cập nhật hồ sơ thành công!";

        return RedirectToAction(nameof(HoSo));
    }


    // GET: /TaiKhoan/DoiMatKhau
    [HttpGet]
    [Authorize]
    public IActionResult DoiMatKhau()
    {
        return View(new DoiMatKhauViewModel());
    }

    // POST: /TaiKhoan/DoiMatKhau
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult DoiMatKhau(DoiMatKhauViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var maKH = User.LayMaKH();

        if (maKH == null)
            return Challenge();

        var khachHang = _khachHangRepository.LayTheoMa(maKH.Value);

        if (khachHang == null || !khachHang.TrangThai)
            return NotFound();

        // Kiem tra mat khau hien tai bang BCrypt
        if (!MatKhauHelper.KiemTra(
                model.MatKhauHienTai,
                khachHang.MatKhau))
        {
            ModelState.AddModelError(
                nameof(model.MatKhauHienTai),
                "Mật khẩu hiện tại không chính xác."
            );

            return View(model);
        }

        // Khong cho dat mat khau moi trung mat khau cu
        if (MatKhauHelper.KiemTra(
                model.MatKhauMoi,
                khachHang.MatKhau))
        {
            ModelState.AddModelError(
                nameof(model.MatKhauMoi),
                "Mật khẩu mới phải khác mật khẩu hiện tại."
            );

            return View(model);
        }

        // DatMatKhau da bam BCrypt trong Repository
        var thanhCong = _khachHangRepository.DatMatKhau(
            maKH.Value,
            model.MatKhauMoi
        );

        if (!thanhCong)
        {
            ModelState.AddModelError(
                string.Empty,
                "Không thể đổi mật khẩu. Vui lòng thử lại."
            );

            return View(model);
        }

        TempData["ThongBao"] = "Đổi mật khẩu thành công!";

        return RedirectToAction(nameof(DoiMatKhau));
    }


    // GET: /TaiKhoan/QuenMatKhau
    [HttpGet]
    [AllowAnonymous]
    public IActionResult QuenMatKhau()
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        return View(new QuenMatKhauViewModel());
    }

    // POST: /TaiKhoan/QuenMatKhau
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult QuenMatKhau(QuenMatKhauViewModel model)
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        var email = model.Email.Trim().ToLowerInvariant();
        var khachHang = _khachHangRepository.LayTheoEmail(email);

        if (khachHang == null || !khachHang.TrangThai)
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "Email không tồn tại hoặc tài khoản đã bị khóa."
            );

            return View(model);
        }

        // Tao OTP va luu tam thoi trong OtpService
        var otp = _otpService.TaoOtp(email);

        // Luu email vao session de su dung o cac buoc sau
        HttpContext.Session.SetString("QuenMK_Email", email);
        HttpContext.Session.Remove("QuenMK_DaXacThuc");

        // Chi hien OTP tren website trong moi truong Development
        TempData["OtpDemo"] = otp;

        return RedirectToAction(nameof(XacThucOtp));
    }

    // GET: /TaiKhoan/XacThucOtp
    [HttpGet]
    [AllowAnonymous]
    public IActionResult XacThucOtp()
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        if (string.IsNullOrEmpty(
            HttpContext.Session.GetString("QuenMK_Email")))
        {
            return RedirectToAction(nameof(QuenMatKhau));
        }

        return View(new XacThucOtpViewModel());
    }

    // POST: /TaiKhoan/XacThucOtp
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult XacThucOtp(XacThucOtpViewModel model)
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        var email = HttpContext.Session.GetString("QuenMK_Email");

        if (string.IsNullOrEmpty(email))
            return RedirectToAction(nameof(QuenMatKhau));

        if (!ModelState.IsValid)
            return View(model);

        var hopLe = _otpService.XacThucOtp(
            email,
            model.MaOtp
        );

        if (!hopLe)
        {
            ModelState.AddModelError(
                nameof(model.MaOtp),
                "OTP không chính xác, đã hết hạn hoặc vượt quá số lần thử."
            );

            return View(model);
        }

        // Danh dau phien da xac minh OTP
        HttpContext.Session.SetString(
            "QuenMK_DaXacThuc",
            "true"
        );

        return RedirectToAction(nameof(DatLaiMatKhau));
    }

    // GET: /TaiKhoan/DatLaiMatKhau
    [HttpGet]
    [AllowAnonymous]
    public IActionResult DatLaiMatKhau()
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        var email = HttpContext.Session.GetString("QuenMK_Email");
        var daXacThuc = HttpContext.Session.GetString(
            "QuenMK_DaXacThuc"
        );

        if (string.IsNullOrEmpty(email) || daXacThuc != "true")
            return RedirectToAction(nameof(QuenMatKhau));

        return View(new DatLaiMatKhauViewModel());
    }

    // POST: /TaiKhoan/DatLaiMatKhau
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult DatLaiMatKhau(DatLaiMatKhauViewModel model)
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        var email = HttpContext.Session.GetString("QuenMK_Email");
        var daXacThuc = HttpContext.Session.GetString(
            "QuenMK_DaXacThuc"
        );

        if (string.IsNullOrEmpty(email) || daXacThuc != "true")
            return RedirectToAction(nameof(QuenMatKhau));

        if (!ModelState.IsValid)
            return View(model);

        var khachHang = _khachHangRepository.LayTheoEmail(email);

        if (khachHang == null || !khachHang.TrangThai)
        {
            ModelState.AddModelError(
                string.Empty,
                "Tài khoản không tồn tại hoặc đã bị khóa."
            );

            return View(model);
        }

        // Cap nhat mat khau moi, Repository tu bam BCrypt
        var thanhCong = _khachHangRepository.DatMatKhau(
            khachHang.MaKH,
            model.MatKhauMoi
        );

        if (!thanhCong)
        {
            ModelState.AddModelError(
                string.Empty,
                "Không thể đặt lại mật khẩu. Vui lòng thử lại."
            );

            return View(model);
        }

        // Xoa trang thai khoi phuc mat khau sau khi thanh cong
        HttpContext.Session.Remove("QuenMK_Email");
        HttpContext.Session.Remove("QuenMK_DaXacThuc");

        TempData["ThongBao"] =
            "Đặt lại mật khẩu thành công! Hãy đăng nhập bằng mật khẩu mới.";

        return RedirectToAction(nameof(DangNhap));
    }


}