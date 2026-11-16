namespace BadmintonShop.Core.Services;

// SV4: CSDL chỉ lưu khóa tương đối products/...; hai ứng dụng cấu hình cùng MediaRoot.
public static class KhoAnhSanPham
{
    public static string ThuMucGoc => Path.GetFullPath(Environment.GetEnvironmentVariable("BadmintonShop__MediaRoot")
        ?? TimThuMucMediaDuAn() ?? Path.Combine(AppContext.BaseDirectory, "media"));

    // Khong dat bien moi truong: di nguoc len tu thu muc chay de tim "media/products" da co anh
    // (thu muc media o goc du an) -> Web va Desktop tu dung chung, khong can cau hinh.
    private static string? TimThuMucMediaDuAn()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            var products = Path.Combine(dir.FullName, "media", "products");
            if (Directory.Exists(products) && Directory.EnumerateFiles(products).Any())
                return Path.Combine(dir.FullName, "media");
        }
        return null;
    }

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
