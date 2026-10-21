using System.Text.Json;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Web.Infrastructure;

/// <summary>1 dong gio hang luu trong Session: chi giu ma + so luong, gia luon doc lai tu CSDL (SV3).</summary>
public record DongGioLuu(int MaSP, int SoLuong);

/// <summary>Luu / doc gio hang va ma voucher trong Session (SV3).</summary>
public static class GioHangSession
{
    private const string KeyGio = "GioHang";
    private const string KeyVoucher = "GioHang.Voucher";
    public const int SoLuongToiDa = 99;

    public static List<DongGioLuu> LayGio(this ISession s)
    {
        var json = s.GetString(KeyGio);
        if (string.IsNullOrEmpty(json)) return new();
        try { return JsonSerializer.Deserialize<List<DongGioLuu>>(json) ?? new(); }
        catch (JsonException) { return new(); }
    }

    public static void LuuGio(this ISession s, IEnumerable<DongGioLuu> gio)
        => s.SetString(KeyGio, JsonSerializer.Serialize(gio.ToList()));

    public static string? LayVoucher(this ISession s) => s.GetString(KeyVoucher);

    public static void LuuVoucher(this ISession s, string? ma)
    {
        if (string.IsNullOrWhiteSpace(ma)) s.Remove(KeyVoucher);
        else s.SetString(KeyVoucher, ma.Trim().ToUpperInvariant());
    }

    public static void XoaGio(this ISession s) { s.Remove(KeyGio); s.Remove(KeyVoucher); }

    public static int DemSoLuong(this ISession s) => s.LayGio().Sum(d => d.SoLuong);

    // TV3: tinh gio tu Session, dong bo lai Session (bo san pham ngung ban); voucher sai thi bo khoi Session
    public static KetQuaGioHang TinhTuSession(this GioHangService svc, ISession s)
    {
        var kq = svc.Tinh(s.LayGio().Select(d => (d.MaSP, d.SoLuong)), s.LayVoucher());
        s.LuuGio(kq.Dong.Select(d => new DongGioLuu(d.MaSP, d.SoLuong)));
        if (kq.LoiVoucher != null) s.LuuVoucher(null);
        return kq;
    }
}
