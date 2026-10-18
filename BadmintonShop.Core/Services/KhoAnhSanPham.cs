namespace BadmintonShop.Core.Services;

// SV4: CSDL chỉ lưu khóa tương đối products/...; hai ứng dụng cấu hình cùng MediaRoot.
public static class KhoAnhSanPham
{
    public static string ThuMucGoc => Path.GetFullPath(Environment.GetEnvironmentVariable("BadmintonShop__MediaRoot")
        ?? Path.Combine(AppContext.BaseDirectory, "media"));

    // SV4: từ chối URL, ổ đĩa, traversal và đường dẫn không đúng vùng ảnh sản phẩm.
    public static string? DuongDanDayDu(string? khoa)
    {
        if (string.IsNullOrWhiteSpace(khoa)) return null;
        if (khoa.Length > 255 || !khoa.StartsWith("products/", StringComparison.Ordinal) ||
            khoa.Contains('\\') || khoa.Contains(':') || khoa.Split('/').Any(p => p is "" or "." or ".."))
            throw new ArgumentException("Ảnh phải dùng đường dẫn tương đối products/ten-file.");
        var root = ThuMucGoc + Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(Path.Combine(root, khoa));
        if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Đường dẫn ảnh không hợp lệ.");
        return path;
    }
}
