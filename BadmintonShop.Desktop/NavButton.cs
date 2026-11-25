using System.Drawing.Drawing2D;

namespace BadmintonShop.Desktop;

// TV1: nút điều hướng tự vẽ, một hàng gồm icon và chữ.
internal sealed class NavButton : Control
{
    private readonly string glyph, caption;
    private bool hover, active;
    // TV1: tạo nút điều hướng với icon và chữ
    public NavButton(string glyph, string caption)
    {
        this.glyph = glyph; this.caption = caption; Text = caption;
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.StandardClick, true);
        TabStop = true; Cursor = Cursors.Hand; Margin = new Padding(2);
        AccessibleName = caption; AccessibleRole = AccessibleRole.PushButton;
    }
    public bool Active { get => active; set { active = value; Invalidate(); } }
    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
    // TV1: con lăn trên nút cuộn danh sách điều hướng chứa nó
    protected override void OnMouseWheel(MouseEventArgs e) { if (Parent is ScrollableControl sc) sc.AutoScrollPosition = new Point(0, -sc.AutoScrollPosition.Y - e.Delta); base.OnMouseWheel(e); }
    protected override bool IsInputKey(Keys k) => k is Keys.Enter or Keys.Space || base.IsInputKey(k);
    // TV1: Enter hoặc Space kích hoạt nút khi dùng bàn phím
    protected override void OnKeyDown(KeyEventArgs e) { if (e.KeyCode is Keys.Enter or Keys.Space) { OnClick(EventArgs.Empty); e.Handled = true; } base.OnKeyDown(e); }

    // MDL2 là phông biểu tượng thật của Windows; thiếu phông thì chỉ còn chữ.
    private static readonly Font IconFont = new("Segoe MDL2 Assets", 12.5f);

    // TV1: vẽ nút: nền khi đang chọn hoặc rê chuột, icon, chữ và chấm đánh dấu
    protected override void OnPaint(PaintEventArgs e)
    {
        var t = ShopTheme.Current; var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias; g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.Clear(t.NavBg);
        using var path = Round(new Rectangle(2, 2, Width - 5, Height - 5), 8);
        Color ink = active ? t.NavActiveInk : t.NavInk, sub = active ? t.NavActiveInk : t.NavMuted;
        if (active || hover) { using var b = new SolidBrush(active ? t.NavActive : t.NavHover); g.FillPath(b, path); }
        var flags = TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;
        TextRenderer.DrawText(g, glyph, IconFont, new Rectangle(10, 0, 28, Height), sub, flags | TextFormatFlags.HorizontalCenter);
        TextRenderer.DrawText(g, caption, ShopTheme.Face("Segoe UI", 10, active ? FontStyle.Bold : FontStyle.Regular), new Rectangle(44, 0, Width - 50, Height), ink, flags);
        if (active) { using var dot = new SolidBrush(t.Highlight); g.FillEllipse(dot, Width - 18, Height / 2 - 3, 6, 6); }
        if (Focused && ShowFocusCues) { using var pen = new Pen(t.Highlight, 2); g.DrawPath(pen, path); }
    }
    // TV1: tạo hình chữ nhật bo góc cho nền nút
    private static GraphicsPath Round(Rectangle r, int d)
    {
        var p = new GraphicsPath(); int a = d * 2;
        p.AddArc(r.X, r.Y, a, a, 180, 90); p.AddArc(r.Right - a, r.Y, a, a, 270, 90);
        p.AddArc(r.Right - a, r.Bottom - a, a, a, 0, 90); p.AddArc(r.X, r.Bottom - a, a, a, 90, 90); p.CloseFigure(); return p;
    }
}
