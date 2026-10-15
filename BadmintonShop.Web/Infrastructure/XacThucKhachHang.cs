using System.Security.Claims;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BadmintonShop.Web.Infrastructure;

/// <summary>
/// Co che dang nhap khach hang bang Cookie Authentication (SV1 - ROADMAP 2.2).
/// SV2 lam giao dien (TaiKhoanController + View), chi can goi:
///   - dang nhap:  await HttpContext.DangNhapKhachHangAsync(kh, ghiNho);
///   - dang xuat:  await HttpContext.DangXuatKhachHangAsync();
///   - lay khach:  User.LayMaKH(), User.LayHoTen(), User.LayEmail()
/// Trang can dang nhap (gio hang, dat hang, ho so) them [Authorize] len Controller/Action.
/// </summary>
public static class XacThucKhachHang
{
    public const string ClaimMaKH  = "MaKH";
    public const string ClaimHoTen = ClaimTypes.Name;   // => User.Identity.Name = ho ten
    public const string ClaimEmail = ClaimTypes.Email;

    // TV1: dang ky Cookie Authentication cho Web (goi trong Program.cs)
    public static IServiceCollection AddXacThucKhachHang(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(o =>
            {
                o.LoginPath         = "/TaiKhoan/DangNhap";
                o.LogoutPath        = "/TaiKhoan/DangXuat";
                o.AccessDeniedPath  = "/TaiKhoan/DangNhap";
                o.Cookie.Name       = "BadmintonShop.Auth";
                o.Cookie.HttpOnly   = true;
                o.ExpireTimeSpan    = TimeSpan.FromDays(7);
                o.SlidingExpiration = true;
                o.Events.OnValidatePrincipal = KiemTraConHoatDongAsync;
            });
        return services;
    }

    // TV1: ghi cookie dang nhap cho khach hang (ghiNho = true thi cookie con sau khi tat trinh duyet)
    public static Task DangNhapKhachHangAsync(this HttpContext http, KhachHang kh, bool ghiNho = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimMaKH,  kh.MaKH.ToString()),
            new(ClaimHoTen, kh.HoTen),
            new(ClaimEmail, kh.Email),
        };
        var nguoiDung = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        return http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, nguoiDung,
                                new AuthenticationProperties { IsPersistent = ghiNho });
    }

    // TV1: xoa cookie dang nhap (dang xuat)
    public static Task DangXuatKhachHangAsync(this HttpContext http)
        => http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    // TV1: lay MaKH cua khach dang dang nhap, chua dang nhap tra ve null
    public static int? LayMaKH(this ClaimsPrincipal user)
        => int.TryParse(user.FindFirstValue(ClaimMaKH), out var ma) ? ma : null;

    // TV1: lay ho ten khach dang dang nhap
    public static string? LayHoTen(this ClaimsPrincipal user) => user.FindFirstValue(ClaimHoTen);

    // TV1: lay email khach dang dang nhap
    public static string? LayEmail(this ClaimsPrincipal user) => user.FindFirstValue(ClaimEmail);

    // TV1: moi request kiem lai khach con ton tai va chua bi khoa, bi khoa thi dang xuat ngay
    private static async Task KiemTraConHoatDongAsync(CookieValidatePrincipalContext ctx)
    {
        var maKH = ctx.Principal?.LayMaKH();
        var kh = maKH == null ? null : new KhachHangRepository().LayTheoMa(maKH.Value);
        if (kh is { TrangThai: true }) return;
        ctx.RejectPrincipal();
        await ctx.HttpContext.DangXuatKhachHangAsync();
    }
}
