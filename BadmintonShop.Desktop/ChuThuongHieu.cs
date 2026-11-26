namespace BadmintonShop.Desktop;

// TV1: ten cua hang tren dau thanh ben: BADMINTON mau muc, SHOP mau cam gian chu kem vach nhan
internal sealed class ChuThuongHieu : Control
{
    private const TextFormatFlags Co = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;
    private static Font Dong => ShopTheme.TitleFace(15);

    // TV1: tao khoi ten cua hang, chieu cao tinh theo phong chu de khong bi cat khi phong to man hinh
    public ChuThuongHieu()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
        Text = AccessibleName = "BADMINTON SHOP"; BackColor = ShopTheme.Current.NavBg;
        Height = LogicalToDeviceUnits(22) + Dong.Height * 2;
    }

    // TV1: ve hai dong ten cua hang va vach cam ben canh chu SHOP
    protected override void OnPaint(PaintEventArgs e)
    {
        var t = ShopTheme.Current; var g = e.Graphics; g.Clear(t.NavBg);
        int x = LogicalToDeviceUnits(22), y = LogicalToDeviceUnits(14), cao = Dong.Height;
        TextRenderer.DrawText(g, "BADMINTON", Dong, new Point(x, y), t.Accent, Co);
        // Ve tung chu de gian khoang cach vi WinForms khong co san thuoc tinh gian chu
        int cx = x;
        foreach (var chu in "SHOP")
        {
            var s = chu.ToString();
            TextRenderer.DrawText(g, s, Dong, new Point(cx, y + cao), t.Highlight, Co);
            cx += TextRenderer.MeasureText(g, s, Dong, Size.Empty, Co).Width + LogicalToDeviceUnits(4);
        }
        int phai = x + TextRenderer.MeasureText(g, "BADMINTON", Dong, Size.Empty, Co).Width;
        int trai = cx + LogicalToDeviceUnits(6), day = LogicalToDeviceUnits(3);
        if (phai <= trai) return;
        using var but = new SolidBrush(t.Highlight);
        g.FillRectangle(but, trai, y + cao + (cao - day) / 2, phai - trai, day);
    }
}
