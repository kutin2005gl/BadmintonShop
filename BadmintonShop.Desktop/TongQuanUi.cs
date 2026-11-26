using System.Drawing.Drawing2D;
using System.Drawing.Text;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

// TV1: ham ve dung chung cho cac the va bieu do cua trang tong quan (mau lay tu ShopTheme)
internal static class Ve
{
    private const TextFormatFlags Chung = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;

    // TV1: tron hai mau theo ti le
    public static Color Tron(Color a, Color b, float veB) =>
        Color.FromArgb((int)(a.R + (b.R - a.R) * veB), (int)(a.G + (b.G - a.G) * veB), (int)(a.B + (b.B - a.B) * veB));

    // TV1: tao duong bao chu nhat bo goc
    public static GraphicsPath BoGoc(RectangleF r, float banKinh)
    {
        var p = new GraphicsPath(); float d = Math.Max(1, Math.Min(banKinh * 2, Math.Min(r.Width, r.Height)));
        p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90); p.CloseFigure();
        return p;
    }

    // TV1: to nen mot hinh bo goc, co the kem vien
    public static void Hop(Graphics g, RectangleF r, float banKinh, Color nen, Color? vien = null)
    {
        if (r.Width <= 0 || r.Height <= 0) return;
        using var path = BoGoc(r, banKinh); using var b = new SolidBrush(nen); g.FillPath(b, path);
        if (vien is { } mau) { using var pen = new Pen(mau); g.DrawPath(pen, path); }
    }

    // TV1: ve mot dong chu, cat bang dau ba cham khi thieu cho
    public static void Chu(Graphics g, string text, Font font, Rectangle r, Color color, TextFormatFlags canh = TextFormatFlags.Left) =>
        TextRenderer.DrawText(g, text, font, r, color, Chung | TextFormatFlags.EndEllipsis | canh);

    // TV1: do be rong mot dong chu
    public static int Rong(Graphics g, string text, Font font) => TextRenderer.MeasureText(g, text, font, Size.Empty, Chung).Width;

    // TV1: ve bieu tuong Segoe MDL2 can giua trong mot o
    public static void BieuTuong(Graphics g, string glyph, float co, Rectangle r, Color color) =>
        TextRenderer.DrawText(g, glyph, ShopTheme.Face("Segoe MDL2 Assets", co), r, color, Chung | TextFormatFlags.HorizontalCenter);
}

// TV1: khung the nen sang, bo goc, vien manh; noi dung con nam ben trong phan dem
internal class TheKhung : Panel
{
    // TV1: khoi tao the voi nen cua trang o ngoai goc bo
    public TheKhung()
    {
        DoubleBuffered = true; ResizeRedraw = true; BackColor = ShopTheme.Surface;
    }

    // TV1: doi so do thiet ke (96 dpi) sang diem anh man hinh
    protected int S(int v) => LogicalToDeviceUnits(v);

    // TV1: ve nen bo goc va vien cua the
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var g = e.Graphics; g.Clear(ShopTheme.Background); g.SmoothingMode = SmoothingMode.AntiAlias;
        Ve.Hop(g, new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), S(12), ShopTheme.Surface, ShopTheme.Line);
    }
}

// TV1: the so lieu gom bieu tuong, ten chi so, gia tri, dong so sanh va duong xu huong
internal sealed class TheSoLieu : TheKhung
{
    private readonly string glyph, ten;
    private string giaTri = "—", ghiChu = "";
    private Color mauGhiChu;
    private Color mauXuHuong;
    private decimal[] xuHuong = [];

    // TV1: tao the voi bieu tuong va ten chi so
    public TheSoLieu(string glyph, string ten)
    {
        this.glyph = glyph; this.ten = ten; mauGhiChu = ShopTheme.Muted; AccessibleName = ten;
    }

    // TV1: gan so lieu moi cho the va ve lai
    public void Gan(string giaTri, string ghiChu, Color mauGhiChu, IEnumerable<decimal>? xuHuong = null, Color mauXuHuong = default)
    {
        this.giaTri = giaTri; this.ghiChu = ghiChu; this.mauGhiChu = mauGhiChu;
        this.xuHuong = xuHuong?.ToArray() ?? []; this.mauXuHuong = mauXuHuong;
        AccessibleDescription = $"{giaTri}. {ghiChu}"; Invalidate();
    }

    // TV1: ve noi dung the
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        int o = S(50), trai = S(14);
        var khungIcon = new Rectangle(trai, S(18), o, o);
        Ve.Hop(g, khungIcon, S(14), ShopTheme.Current.NavBg);
        Ve.BieuTuong(g, glyph, 17, khungIcon, ShopTheme.Green);
        int x = khungIcon.Right + S(14), w = Width - x - S(10);

        // Duong xu huong ve truoc, nam o goc duoi phai de chu de len tren khong bi che
        if (xuHuong.Length > 1 && xuHuong.Max() > 0)
        {
            var vung = new RectangleF(Width - S(76), S(52), S(62), S(28));
            decimal max = xuHuong.Max();
            var diem = xuHuong.Select((v, i) => new PointF(vung.X + vung.Width * i / (xuHuong.Length - 1), vung.Bottom - (float)(v / max) * vung.Height)).ToArray();
            using var nen = new SolidBrush(Color.FromArgb(34, mauXuHuong));
            g.FillPolygon(nen, [new PointF(vung.X, vung.Bottom), .. diem, new PointF(vung.Right, vung.Bottom)]);
            using var but = new Pen(Color.FromArgb(150, mauXuHuong), S(2)) { LineJoin = LineJoin.Round };
            g.DrawLines(but, diem);
        }

        Ve.Chu(g, ten, ShopTheme.Face("Segoe UI", 10), new Rectangle(x, S(14), w, S(22)), ShopTheme.Muted);
        Ve.Chu(g, giaTri, ShopTheme.Face("Segoe UI", giaTri.Length > 13 ? 15 : 19, FontStyle.Bold), new Rectangle(x, S(34), w, S(40)), ShopTheme.Ink);
        Ve.Chu(g, ghiChu, ShopTheme.Face("Segoe UI", 9), new Rectangle(trai, S(78), Width - trai - S(10), S(22)), mauGhiChu);
    }
}

// TV1: bieu do cot doanh thu theo ngay tren nen sang, re chuot de xem tung ngay
internal sealed class BieuDoDoanhThu : TheKhung
{
    private IReadOnlyList<DiemNgay> duLieu = [];
    private decimal tong, tongTruoc;
    private string tieuDe = "Doanh thu", thongBao = "Đang tải dữ liệu…";
    private int dangChi = -1;
    private readonly List<RectangleF> oCot = new();

    // TV1: gan chuoi doanh thu va tong ky de ve
    public void Gan(IReadOnlyList<DiemNgay> duLieu, decimal tong, decimal tongTruoc, bool coQuyen)
    {
        this.duLieu = coQuyen ? duLieu : []; this.tong = tong; this.tongTruoc = tongTruoc; dangChi = -1;
        tieuDe = $"Doanh thu {duLieu.Count} ngày gần nhất (VNĐ)";
        thongBao = coQuyen ? "" : "Doanh thu chỉ hiển thị cho tài khoản có quyền thống kê.";
        AccessibleName = coQuyen ? $"{tieuDe}: {tong:N0} đồng" : thongBao; Invalidate();
    }

    // TV1: tim cot dang duoc re chuot de hien so tien
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        int moi = oCot.FindIndex(r => e.X >= r.Left && e.X < r.Right && e.Y >= r.Top);
        if (moi != dangChi) { dangChi = moi; Invalidate(); }
    }

    // TV1: bo chon cot khi chuot roi bieu do
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); dangChi = -1; Invalidate(); }

    // TV1: chon buoc chia truc tung tron so
    private static decimal BuocTron(decimal max)
    {
        double tho = (double)Math.Max(max, 4) / 4, bac = Math.Pow(10, Math.Floor(Math.Log10(tho)));
        double he = tho / bac;
        return (decimal)(bac * (he <= 1 ? 1 : he <= 2 ? 2 : he <= 2.5 ? 2.5 : he <= 5 ? 5 : 10));
    }

    // TV1: rut gon so tien tren truc tung
    private static string RutGon(decimal v) =>
        v >= 1_000_000_000 ? $"{v / 1_000_000_000:0.##} tỷ" : v >= 1_000_000 ? $"{v / 1_000_000:0.##}M" : v >= 1000 ? $"{v / 1000:0.##}K" : $"{v:0}";

    // TV1: ve tieu de, tong ky, truc, cac cot va chu thich
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; var t = ShopTheme.Current; g.SmoothingMode = SmoothingMode.AntiAlias;
        int dai = S(66);
        int phaiRong = thongBao == "" ? S(210) : 0;
        bool coDuLieu = thongBao == "" && duLieu.Count > 0;
        Ve.Chu(g, tieuDe, ShopTheme.Face("Segoe UI", 12, FontStyle.Bold), new Rectangle(S(16), S(10), Width - S(28) - phaiRong, S(26)), t.Ink);
        oCot.Clear();
        if (thongBao != "" || duLieu.Count == 0)
        {
            Ve.Chu(g, thongBao == "" ? "Chưa có dữ liệu." : thongBao, Font, new Rectangle(S(16), dai, Width - S(32), Height - dai), ShopTheme.Muted, TextFormatFlags.HorizontalCenter);
            return;
        }
        Ve.Chu(g, "Tổng doanh thu", ShopTheme.Face("Segoe UI", 9), new Rectangle(Width - phaiRong - S(16), S(10), phaiRong, S(20)), ShopTheme.Muted, TextFormatFlags.Right);
        Ve.Chu(g, $"{tong:N0} đ", ShopTheme.Face("Segoe UI", 15, FontStyle.Bold), new Rectangle(Width - phaiRong - S(16), S(30), phaiRong, S(28)), t.Ink, TextFormatFlags.Right);

        // Nhan so voi ky truoc; ky truoc bang 0 thi khong tinh duoc phan tram
        string soSanh = tongTruoc == 0 ? "Kỳ trước chưa có doanh thu"
            : $"{(tong >= tongTruoc ? "↑ +" : "↓ ")}{(tong - tongTruoc) / tongTruoc * 100:0.#}% so với kỳ trước";
        var fontNho = ShopTheme.Face("Segoe UI", 9, FontStyle.Bold);
        var mau = tongTruoc == 0 ? ShopTheme.Muted : tong >= tongTruoc ? MauTongQuan.Tang : ShopTheme.Danger;
        // Nhan nam duoi tieu de de khong bi chu thich cot che
        var nhan = new Rectangle(S(16), S(40), Math.Min(Ve.Rong(g, soSanh, fontNho) + S(18), Width - S(28) - phaiRong), S(22));
        Ve.Hop(g, nhan, S(10), Ve.Tron(ShopTheme.Surface, mau, 0.1f), Ve.Tron(ShopTheme.Surface, mau, 0.3f));
        Ve.Chu(g, soSanh, fontNho, nhan, mau, TextFormatFlags.HorizontalCenter);

        var truc = ShopTheme.Face("Segoe UI", 9);
        float traiVe = S(58), phaiVe = Width - S(20), trenVe = dai + S(50), duoiVe = Height - S(34);
        if (duoiVe - trenVe < S(40) || phaiVe - traiVe < S(60)) return;
        decimal buoc = BuocTron(duLieu.Max(x => x.DoanhThu));
        decimal tran = buoc * Math.Max(1, Math.Ceiling(duLieu.Max(x => x.DoanhThu) / buoc));
        using (var ke = new Pen(Ve.Tron(ShopTheme.Surface, ShopTheme.Line, 0.8f)))
            for (decimal v = 0; v <= tran; v += buoc)
            {
                float y = duoiVe - (float)(v / tran) * (duoiVe - trenVe);
                g.DrawLine(ke, traiVe, y, phaiVe, y);
                Ve.Chu(g, RutGon(v), truc, new Rectangle(0, (int)y - S(10), (int)traiVe - S(8), S(20)), ShopTheme.Muted, TextFormatFlags.Right);
            }

        float o = (phaiVe - traiVe) / duLieu.Count, rong = Math.Min(S(52), o * 0.62f);
        int cach = Math.Max(1, (int)Math.Ceiling(S(44) / o));
        int caoNhat = duLieu.Select((x, i) => (x.DoanhThu, i)).MaxBy(x => x.DoanhThu).i;
        int noiBat = dangChi >= 0 && dangChi < duLieu.Count ? dangChi : duLieu[caoNhat].DoanhThu > 0 ? caoNhat : -1;
        RectangleF cotNoiBat = default;
        for (int i = 0; i < duLieu.Count; i++)
        {
            float giua = traiVe + o * (i + 0.5f), cao = (float)(duLieu[i].DoanhThu / tran) * (duoiVe - trenVe);
            var cot = new RectangleF(giua - rong / 2, duoiVe - cao, rong, cao);
            oCot.Add(new RectangleF(giua - o / 2, trenVe, o, duoiVe - trenVe));
            if (cao >= 1)
            {
                var mauCot = i == noiBat ? t.Accent : Ve.Tron(ShopTheme.Surface, t.Accent, 0.28f);
                float r = Math.Min(S(5), Math.Min(rong / 2, cao));
                Ve.Hop(g, cot, r, mauCot);
                using var chan = new SolidBrush(mauCot); g.FillRectangle(chan, cot.X, cot.Bottom - r, cot.Width, r);
            }
            if (i == noiBat) cotNoiBat = cot;
            // Nhan ngay dem nguoc tu hom nay de ngay cuoi ky luon co nhan
            if ((duLieu.Count - 1 - i) % cach == 0)
                Ve.Chu(g, duLieu[i].Ngay.ToString("dd/MM"), truc, new Rectangle((int)(giua - S(30)), (int)duoiVe + S(6), S(60), S(20)), ShopTheme.Muted, TextFormatFlags.HorizontalCenter);
        }
        if (noiBat < 0) return;

        string ngay = duLieu[noiBat].Ngay.ToString("dd/MM"), tien = $"{duLieu[noiBat].DoanhThu:N0} đ";
        int rongChu = Math.Max(Ve.Rong(g, tien, fontNho), Ve.Rong(g, ngay, truc)) + S(20), caoHop = S(44);
        float hx = Math.Clamp(cotNoiBat.X + cotNoiBat.Width / 2 - rongChu / 2f, S(4), Width - rongChu - S(4));
        float hy = Math.Max(dai + S(4), cotNoiBat.Top - caoHop - S(8));
        var hop = new Rectangle((int)hx, (int)hy, rongChu, caoHop);
        Ve.Hop(g, hop, S(8), ShopTheme.Ink);
        Ve.Chu(g, ngay, truc, new Rectangle(hop.X + S(10), hop.Y + S(3), rongChu - S(20), S(19)), ShopTheme.Surface);
        Ve.Chu(g, tien, fontNho, new Rectangle(hop.X + S(10), hop.Y + S(22), rongChu - S(20), S(19)), ShopTheme.Surface);
    }
}

// TV1: mau rieng cua trang tong quan, chon cho hop nen giay va muc navy
internal static class MauTongQuan
{
    public static readonly Color Tang = Color.FromArgb(38, 112, 70);
    public static readonly Color ChoXacNhan = Color.FromArgb(214, 148, 44);
    public static readonly Color DangXuLy = Color.FromArgb(112, 140, 196);
}

// TV1: bieu do vanh khuyen so don theo trang thai kem chu giai
internal sealed class BieuDoTrangThai : TheKhung
{
    private (string Ten, int So, Color Mau)[] phan = [];
    private string ghiChu = "";

    // TV1: gop trang thai don thanh bon nhom va ve lai
    public void Gan(IReadOnlyDictionary<string, int> soDon, int soNgay)
    {
        phan =
        [
            ("Chờ xác nhận", soDon.GetValueOrDefault("Chờ xác nhận"), MauTongQuan.ChoXacNhan),
            ("Đang xử lý", soDon.GetValueOrDefault("Đã xác nhận") + soDon.GetValueOrDefault("Đang giao"), MauTongQuan.DangXuLy),
            ("Hoàn thành", soDon.GetValueOrDefault("Hoàn thành"), ShopTheme.Green),
            ("Đã hủy", soDon.GetValueOrDefault("Đã hủy"), ShopTheme.Current.Highlight),
        ];
        ghiChu = $"Đơn lập trong {soNgay} ngày gần nhất";
        AccessibleName = "Trạng thái đơn hàng: " + string.Join(", ", phan.Select(p => $"{p.Ten} {p.So}")); Invalidate();
    }

    // TV1: ve tieu de, vanh khuyen, tong so don va chu giai
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        Ve.BieuTuong(g, "", 15, new Rectangle(S(12), S(12), S(34), S(30)), ShopTheme.Green);
        Ve.Chu(g, "Trạng thái đơn hàng", ShopTheme.Face("Segoe UI", 12, FontStyle.Bold), new Rectangle(S(50), S(12), Width - S(62), S(30)), ShopTheme.Green);
        Ve.Chu(g, ghiChu, ShopTheme.Face("Segoe UI", 9), new Rectangle(S(50), S(42), Width - S(62), S(20)), ShopTheme.Muted);
        if (phan.Length == 0) return;

        int tong = phan.Sum(p => p.So), tren = S(72), caoVung = Height - tren - S(14);
        // The hep thi chu giai nam duoi vanh khuyen, the rong thi nam ben phai
        bool doc = Width < S(350);
        int dk = Math.Max(S(96), doc ? Math.Min(Width - S(40), caoVung - S(86)) : Math.Min(caoVung - S(8), Width - S(236)));
        var vong = doc ? new RectangleF((Width - dk) / 2f, tren, dk, dk) : new RectangleF(S(20), tren + (caoVung - dk) / 2f, dk, dk);
        float day = dk * 0.17f;
        var tam = RectangleF.Inflate(vong, -day / 2, -day / 2);
        using (var nen = new Pen(ShopTheme.Current.NavBg, day)) g.DrawEllipse(nen, tam);
        float goc = -90;
        foreach (var p in phan.Where(p => p.So > 0))
        {
            float quet = 360f * p.So / tong;
            using var but = new Pen(p.Mau, day); g.DrawArc(but, tam, goc, quet);
            goc += quet;
        }
        // Khe trang giua cac phan de de phan biet
        if (phan.Count(p => p.So > 0) > 1)
        {
            using var khe = new Pen(ShopTheme.Surface, S(2)); float a = -90;
            float cx = vong.X + dk / 2f, cy = vong.Y + dk / 2f;
            foreach (var p in phan.Where(p => p.So > 0))
            {
                double rad = a * Math.PI / 180; float cos = (float)Math.Cos(rad), sin = (float)Math.Sin(rad);
                g.DrawLine(khe, cx + cos * (dk / 2f - day - 1), cy + sin * (dk / 2f - day - 1), cx + cos * (dk / 2f + 1), cy + sin * (dk / 2f + 1));
                a += 360f * p.So / tong;
            }
        }
        var o = Rectangle.Round(vong);
        // Vanh nho thi chu giua vanh cung nho lai de khong de len nhau
        bool nho = dk < S(150);
        int caoSo = S(nho ? 26 : 36), caoChu = S(nho ? 18 : 22), dinh = o.Y + (o.Height - caoSo - caoChu) / 2;
        Ve.Chu(g, tong.ToString("N0"), ShopTheme.Face("Segoe UI", nho ? 14 : 20, FontStyle.Bold), new Rectangle(o.X, dinh, o.Width, caoSo), ShopTheme.Ink, TextFormatFlags.HorizontalCenter);
        Ve.Chu(g, nho ? "đơn" : "đơn hàng", ShopTheme.Face("Segoe UI", nho ? 9 : 10), new Rectangle(o.X, dinh + caoSo, o.Width, caoChu), ShopTheme.Muted, TextFormatFlags.HorizontalCenter);

        int lx = doc ? S(20) : (int)vong.Right + S(22), rongCg = Width - lx - S(16), dong = doc ? S(20) : S(34);
        int ly = doc ? (int)vong.Bottom + S(6) : tren + (caoVung - dong * phan.Length) / 2;
        var chu = ShopTheme.Face("Segoe UI", 10); var dam = ShopTheme.Face("Segoe UI", 10, FontStyle.Bold);
        foreach (var p in phan)
        {
            using (var cham = new SolidBrush(p.Mau)) g.FillEllipse(cham, lx, ly + dong / 2 - S(5), S(11), S(11));
            string phanTram = $"({(tong == 0 ? 0 : p.So * 100.0 / tong):0}%)";
            int rongPt = S(48), rongSo = S(36);
            Ve.Chu(g, p.Ten, chu, new Rectangle(lx + S(18), ly, rongCg - S(18) - rongPt - rongSo, dong), ShopTheme.Ink);
            Ve.Chu(g, p.So.ToString("N0"), dam, new Rectangle(lx + rongCg - rongPt - rongSo, ly, rongSo, dong), ShopTheme.Ink, TextFormatFlags.Right);
            Ve.Chu(g, phanTram, chu, new Rectangle(lx + rongCg - rongPt, ly, rongPt, dong), ShopTheme.Muted, TextFormatFlags.Right);
            ly += dong;
        }
    }
}
