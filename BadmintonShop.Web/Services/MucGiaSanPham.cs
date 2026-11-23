using System.Globalization;

namespace BadmintonShop.Web.Services;

// TV1: một khoảng giá [Tu, Den) để lọc sản phẩm; Den = null là không giới hạn trên
public sealed record MucGia(decimal Tu, decimal? Den)
{
    // Khóa dùng trên URL: "200000-500000", "-200000" (dưới), "4000000-" (từ trở lên)
    public string Khoa => $"{(Tu > 0 ? ((long)Tu).ToString(CultureInfo.InvariantCulture) : "")}-{(Den.HasValue ? ((long)Den.Value).ToString(CultureInfo.InvariantCulture) : "")}";

    public string Nhan => Tu <= 0 && Den.HasValue ? $"Dưới {Tien(Den.Value)}"
        : !Den.HasValue ? $"Từ {Tien(Tu)} trở lên"
        : $"{Tien(Tu)} – {Tien(Den.Value)}";

    // TV1: kiểm tra giá thuộc khoảng [Tu, Den)
    public bool ChuaGia(decimal gia) => gia >= Tu && (!Den.HasValue || gia < Den.Value);

    static string Tien(decimal v) => ((long)v).ToString("#,##0", CultureInfo.InvariantCulture).Replace(',', '.') + "đ";

    // TV1: đọc khóa từ URL; sai định dạng hoặc Tu >= Den thì trả null (coi như không lọc giá)
    public static MucGia? DocKhoa(string? khoa)
    {
        if (string.IsNullOrWhiteSpace(khoa)) return null;
        var p = khoa.Split('-');
        if (p.Length != 2) return null;
        decimal tu = 0; decimal? den = null;
        if (p[0] != "" && !decimal.TryParse(p[0], NumberStyles.None, CultureInfo.InvariantCulture, out tu)) return null;
        if (p[1] != "")
        {
            if (!decimal.TryParse(p[1], NumberStyles.None, CultureInfo.InvariantCulture, out var d)) return null;
            den = d;
        }
        if (tu == 0 && den == null) return null;
        if (den.HasValue && tu >= den.Value) return null;
        return new MucGia(tu, den);
    }

    // TV1: sinh 4-6 khoảng giá từ giá thực tế: lấy phân vị, làm tròn 1 chữ số có nghĩa, bỏ mốc làm khoảng rỗng.
    // Khoảng liên tiếp [mốc_i, mốc_i+1) nên không chồng lấn, không sót; khoảng cuối không giới hạn.
    public static List<MucGia> Tao(IEnumerable<decimal> giaBan, int soKhoangMongMuon = 5)
    {
        var gia = giaBan.Where(g => g >= 0).OrderBy(g => g).ToList();
        if (gia.Count < 2 || gia[0] == gia[^1]) return new List<MucGia>();

        var mocUngVien = new SortedSet<decimal>();
        for (int i = 1; i < soKhoangMongMuon; i++)
        {
            var phanVi = gia[Math.Min(gia.Count - 1, gia.Count * i / soKhoangMongMuon)];
            var moc = LamTron(phanVi);
            if (moc > gia[0] && moc <= gia[^1]) mocUngVien.Add(moc);
        }

        var moc2 = new List<decimal>();
        decimal truoc = 0;
        foreach (var m in mocUngVien)
        {
            if (gia.Any(g => g >= truoc && g < m)) { moc2.Add(m); truoc = m; }
        }
        if (moc2.Count > 0 && !gia.Any(g => g >= moc2[^1])) moc2.RemoveAt(moc2.Count - 1);
        if (moc2.Count == 0) return new List<MucGia>();

        var ds = new List<MucGia>();
        decimal tu = 0;
        foreach (var m in moc2) { ds.Add(new MucGia(tu, m)); tu = m; }
        ds.Add(new MucGia(tu, null));
        return ds;
    }

    // TV1: làm tròn về 1 chữ số có nghĩa (245.000 -> 200.000, 3.590.000 -> 4.000.000)
    static decimal LamTron(decimal x)
    {
        if (x <= 0) return 0;
        var bac = (decimal)Math.Pow(10, Math.Floor(Math.Log10((double)x)));
        return Math.Round(x / bac, 0, MidpointRounding.AwayFromZero) * bac;
    }
}
