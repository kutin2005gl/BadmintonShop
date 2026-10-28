
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace BadmintonShop.Web.Services;

public class OtpService
{
    private const int ThoiHanPhut = 5;
    private const int SoLanSaiToiDa = 5;

    private readonly ConcurrentDictionary<string, OtpInfo> _danhSachOtp = new();

    private class OtpInfo
    {
        public string MaOtp { get; set; } = string.Empty;

        public DateTime HetHan { get; set; }

        public int SoLanSai { get; set; }
    }

    // Tao OTP 6 chu so cho email
    public string TaoOtp(string email)
    {
        email = ChuanHoaEmail(email);

        string otp = RandomNumberGenerator
            .GetInt32(0, 1000000)
            .ToString("D6");

        _danhSachOtp[email] = new OtpInfo
        {
            MaOtp = otp,
            HetHan = DateTime.UtcNow.AddMinutes(ThoiHanPhut),
            SoLanSai = 0
        };

        return otp;
    }

    // Kiem tra OTP, chi su dung duoc mot lan
    public bool XacThucOtp(string email, string otp)
    {
        email = ChuanHoaEmail(email);

        if (!_danhSachOtp.TryGetValue(email, out var thongTin))
            return false;

        lock (thongTin)
        {
            if (DateTime.UtcNow > thongTin.HetHan ||
                thongTin.SoLanSai >= SoLanSaiToiDa)
            {
                _danhSachOtp.TryRemove(
                    new KeyValuePair<string, OtpInfo>(
                        email, thongTin));

                return false;
            }

            if (thongTin.MaOtp != otp)
            {
                thongTin.SoLanSai++;

                return false;
            }

            // OTP dung: xoa de khong the dung lai
            _danhSachOtp.TryRemove(
                new KeyValuePair<string, OtpInfo>(
                    email, thongTin));

            return true;
        }
    }

    private static string ChuanHoaEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
