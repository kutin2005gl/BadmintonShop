using System.Drawing.Drawing2D;

namespace BadmintonShop.Desktop;

// TV1: khung o nhap bo goc: bieu tuong ben trai, control nhap o giua (cat vien mac dinh cua Windows), hau to ben phai
internal sealed class KhungNhap : Panel
{
    private readonly Control o;
    private readonly string glyph, hauTo;
    private readonly Panel cat = new() { BackColor = ShopTheme.Current.Field };

    // TV1: tao khung cho mot control nhap
    public KhungNhap(Control o, string glyph = "", string hauTo = "")
    {
        this.o = o; this.glyph = glyph; this.hauTo = hauTo;
        DoubleBuffered = true; ResizeRedraw = true; BackColor = ShopTheme.Current.Field;
        cat.Controls.Add(o); Controls.Add(cat);
        o.Enter += (_, _) => Invalidate(); o.Leave += (_, _) => Invalidate();
        o.SizeChanged += (_, _) => PerformLayout();
    }

    private int S(int v) => LogicalToDeviceUnits(v);

    // TV1: can control nhap vao giua khung, de vien cua no nam ngoai vung nhin thay
    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        int trai = S(glyph == "" ? 8 : 38), phai = S(hauTo == "" ? 6 : 28), vien = o is TextBoxBase ? 0 : 2;
        int rong = Math.Max(10, Width - trai - phai), cao = Math.Max(10, Math.Min(Height - 4, o.Height - vien * 2));
        cat.SetBounds(trai, (Height - cao) / 2, rong, cao);
        // O so: noi rong them de phan nut mui ten (da an) nam ngoai vung nhin thay
        int du = o is UpDownBase ? SystemInformation.VerticalScrollBarWidth + S(6) : 0;
        var moi = new Rectangle(-vien, -vien, rong + vien * 2 + du, o.Height);
        if (o.Bounds != moi) o.Bounds = moi;
    }

    // TV1: ve nen bo goc, vien (dam hon khi dang nhap), bieu tuong va hau to
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var g = e.Graphics; var t = ShopTheme.Current; g.Clear(t.Bg); g.SmoothingMode = SmoothingMode.AntiAlias;
        Ve.Hop(g, new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), S(7), t.Field, o.ContainsFocus ? t.Accent : Ve.Tron(t.Line, t.Muted, 0.18f));
        if (glyph != "") Ve.BieuTuong(g, glyph, 12, new Rectangle(S(6), 0, S(30), Height), t.Accent);
        if (hauTo != "") Ve.Chu(g, hauTo, ShopTheme.Face("Segoe UI", 10), new Rectangle(Width - S(28), 0, S(20), Height), t.Muted, TextFormatFlags.Right);
    }
}

// TV1: nut bo goc co bieu tuong va chu; Chinh = nen muc, nguoc lai nen kem vien manh
internal sealed class NutBieuTuong : Button
{
    private bool re;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string Glyph { get; set; } = "";
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool Chinh { get; set; }

    // TV1: bat tu ve de co goc bo tron
    public NutBieuTuong()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Cursor = Cursors.Hand;
    }

    // TV1: doi mau nen khi re chuot vao
    protected override void OnMouseEnter(EventArgs e) { re = true; Invalidate(); base.OnMouseEnter(e); }

    // TV1: tra mau nen khi chuot roi nut
    protected override void OnMouseLeave(EventArgs e) { re = false; Invalidate(); base.OnMouseLeave(e); }

    // TV1: ve nen, bieu tuong va chu can giua; nut tat thi mau nhat
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; var t = ShopTheme.Current; g.Clear(Parent?.BackColor ?? t.Bg); g.SmoothingMode = SmoothingMode.AntiAlias;
        Color nen = Chinh ? (re ? t.AccentHover : t.Accent) : (re ? t.AccentSoft : t.Surface);
        Color chu = Chinh ? t.OnAccent : t.Accent;
        if (!Enabled) { nen = Chinh ? Ve.Tron(nen, t.Bg, 0.55f) : t.Bg; chu = Chinh ? t.OnAccent : Ve.Tron(t.Muted, t.Bg, 0.35f); }
        int bo = LogicalToDeviceUnits(7);
        Ve.Hop(g, new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), bo, nen, Chinh ? null : Ve.Tron(t.Line, t.Muted, 0.18f));
        var font = ShopTheme.Face("Segoe UI", 10);
        int rongIcon = Glyph == "" ? 0 : LogicalToDeviceUnits(26), rongChu = Math.Min(Ve.Rong(g, Text, font), Width - rongIcon - 4);
        int x = (Width - rongIcon - rongChu) / 2;
        if (Glyph != "") Ve.BieuTuong(g, Glyph, 11, new Rectangle(x, 0, rongIcon - LogicalToDeviceUnits(4), Height), chu);
        Ve.Chu(g, Text, font, new Rectangle(x + rongIcon, 0, rongChu + 2, Height), chu);
        if (Focused && ShowFocusCues) { using var but = new Pen(t.Highlight, 2); using var p = Ve.BoGoc(new RectangleF(2, 2, Width - 5, Height - 5), bo); g.DrawPath(but, p); }
    }
}
