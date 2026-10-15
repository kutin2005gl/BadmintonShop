using System.Net.Mail;
using System.Text.RegularExpressions;

namespace BadmintonShop.Core.Services;

/// <summary>Cac ham kiem tra dinh dang nhap lieu dung chung cho Desktop/Web (SV1).</summary>
public static class KiemTraDuLieu
{
    // TV1: SDT hop le = toan chu so, dai 10-11 ky tu
    public static bool LaSoDienThoai(string? s)
        => !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"^\d{10,11}$");

    // TV1: email hop le theo MailAddress va phai co dau cham trong ten mien
    public static bool LaEmail(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        try
        {
            var m = new MailAddress(s);
            return m.Address == s && m.Host.Contains('.');
        }
        catch (FormatException) { return false; }
    }
}
