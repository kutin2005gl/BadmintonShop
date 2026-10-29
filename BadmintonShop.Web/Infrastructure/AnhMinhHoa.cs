namespace BadmintonShop.Web.Infrastructure;

/// <summary>Anh minh hoa theo danh muc cho giao dien SmashPro (anh san pham that chua duoc Web phuc vu).</summary>
public static class AnhMinhHoa
{
    private const string Hau = "?auto=format&fit=crop&q=80&w=800";
    private const string Goc = "https://images.unsplash.com/";

    public const string Hero = Goc + "photo-1599474924187-334a4ae5bd3c?auto=format&fit=crop&q=80&w=2000";
    public const string BannerGiay = Goc + "photo-1542291026-7eec264c27ff?auto=format&fit=crop&q=80&w=1000";
    public const string BannerVot = Goc + "photo-1626224583764-f87db24ac4ea?auto=format&fit=crop&q=80&w=1000";

    // Giao dien: chon anh minh hoa theo ten danh muc (vot / giay / ao quan / tui / phu kien)
    public static string TheoDanhMuc(string? tenDanhMuc)
    {
        var t = (tenDanhMuc ?? "").ToLowerInvariant();
        if (t.Contains("vợt")) return Goc + "photo-1626224583764-f87db24ac4ea" + Hau;
        if (t.Contains("giày")) return Goc + "photo-1608231387042-66d1773070a5" + Hau;
        if (t.Contains("áo") || t.Contains("quần")) return Goc + "photo-1581655353564-df123a1eb820" + Hau;
        if (t.Contains("túi") || t.Contains("balo")) return Goc + "photo-1553062407-98eeb64c6a62" + Hau;
        return Goc + "photo-1517436073-3b1b1130e9d6" + Hau;
    }
}
