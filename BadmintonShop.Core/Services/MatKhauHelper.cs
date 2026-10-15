namespace BadmintonShop.Core.Services;

/// <summary>Bam va kiem tra mat khau bang BCrypt (goi BCrypt.Net-Next).</summary>
public static class MatKhauHelper
{
    private const int DoKho = 11; // work factor

    public static string BamMatKhau(string matKhauTho)
        => BCrypt.Net.BCrypt.HashPassword(matKhauTho, DoKho);

    public static bool KiemTra(string matKhauTho, string chuoiBam)
    {
        if (string.IsNullOrEmpty(chuoiBam) || !chuoiBam.StartsWith("$2")) return false;
        try { return BCrypt.Net.BCrypt.Verify(matKhauTho, chuoiBam); }
        catch (BCrypt.Net.SaltParseException) { return false; }
    }

    /// <summary>Mat khau manh: >= 8 ky tu, co chu hoa, chu thuong, chu so.</summary>
    public static bool LaMatKhauManh(string matKhau)
        => matKhau.Length >= 8
           && matKhau.Any(char.IsUpper)
           && matKhau.Any(char.IsLower)
           && matKhau.Any(char.IsDigit);
}
