namespace BadmintonShop.Desktop;

// TV1: các mảnh dựng màn hình main-sub theo mẫu Đơn hàng: danh sách bên trái, panel chi tiết bên phải.
internal static class KhungChiTiet
{
    // TV1: thanh chia trái/phải; tỉ lệ mặc định đặt khi form hiện lên, người dùng vẫn kéo được
    public static SplitContainer Chia(Form form, double tiLeTrai, int toiThieuTrai, int toiThieuPhai)
    {
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterWidth = 6, BackColor = ShopTheme.Line };
        split.Panel1.BackColor = ShopTheme.Surface; split.Panel2.BackColor = ShopTheme.Surface;
        form.Shown += (_, _) =>
        {
            // MinSize chỉ đặt sau khi SplitContainer có kích thước thật, nếu không WinForms ném lỗi.
            try
            {
                int min1 = Math.Min(toiThieuTrai, split.Width / 2), min2 = Math.Min(toiThieuPhai, split.Width / 2 - split.SplitterWidth);
                split.Panel1MinSize = min1; split.Panel2MinSize = min2;
                split.SplitterDistance = Math.Clamp((int)(split.Width * tiLeTrai), min1, Math.Max(min1, split.Width - min2 - split.SplitterWidth));
            }
            catch (InvalidOperationException) { }
        };
        return split;
    }

    // TV1: lưới danh sách ít cột: lấp đầy panel trái, chia độ rộng theo trọng số từng cột
    public static void DoRong(DataGridView grid, params (string Cot, int TrongSo, int ToiThieu)[] ds)
    {
        foreach (var (cot, trongSo, toiThieu) in ds)
            if (grid.Columns.Contains(cot)) { var c = grid.Columns[cot]!; c.MinimumWidth = toiThieu; c.FillWeight = trongSo; }
    }

    // TV1: dòng đầu panel chi tiết: mã đang chọn bên trái, badge trạng thái bên phải
    public static TableLayoutPanel TieuDe(out Label ma, out Label badge)
    {
        ma = new Label { AutoSize = true, Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = ShopTheme.Green, Margin = new Padding(0, 2, 8, 2) };
        badge = new Label { AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), Padding = new Padding(10, 4, 10, 4), Anchor = AnchorStyles.Right, Margin = Padding.Empty };
        var header = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(0, 0, 0, 6) };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.Controls.Add(ma, 0, 0); header.Controls.Add(badge, 1, 0);
        return header;
    }

    // TV1: đặt chữ và màu badge: bình thường / cần chú ý / đã ngừng; chữ rỗng thì ẩn
    public static void DatBadge(Label badge, string text, bool canhBao = false, bool ngung = false)
    {
        badge.Text = text; badge.Visible = text.Length > 0;
        var (back, fore) = ngung ? (ShopTheme.DangerBg, ShopTheme.Danger) : canhBao ? (ShopTheme.WarnBg, ShopTheme.Warn) : (ShopTheme.AccentSoft, ShopTheme.Green);
        badge.BackColor = back; badge.ForeColor = fore;
    }

    // TV1: bảng hai cột nhãn / giá trị (hoặc nhãn / ô nhập) của panel chi tiết
    public static TableLayoutPanel BangThongTin()
    {
        var t = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(0, 4, 0, 10) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return t;
    }

    // TV1: thêm một dòng nhãn + điều khiển vào bảng thông tin
    public static void Dong(TableLayoutPanel bang, string nhan, Control giaTri)
    {
        var name = new Label { Text = nhan, AutoSize = true, ForeColor = ShopTheme.Muted, Font = new Font("Segoe UI", 9), Anchor = AnchorStyles.Left, Margin = new Padding(0, 5, 12, 5) };
        int row = bang.RowCount++; bang.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        if (giaTri is Label) giaTri.Anchor = AnchorStyles.Left;
        else { giaTri.Anchor = AnchorStyles.Left | AnchorStyles.Right; giaTri.Margin = new Padding(0, 4, 2, 4); }
        bang.Controls.Add(name, 0, row); bang.Controls.Add(giaTri, 1, row);
    }

    // TV1: tiêu đề một mục trong panel chi tiết
    public static Label Muc(string text) => new()
    {
        Dock = DockStyle.Top, Text = text, AutoSize = false, Height = 34, TextAlign = ContentAlignment.BottomLeft,
        Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = ShopTheme.Green,
    };

    // TV1: hàng nút trong panel chi tiết, tự xuống dòng khi hẹp
    public static FlowLayoutPanel HangNut(params Control[] nut)
    {
        var p = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Padding = new Padding(0, 4, 0, 4) };
        p.Controls.AddRange(nut);
        return p;
    }
}
