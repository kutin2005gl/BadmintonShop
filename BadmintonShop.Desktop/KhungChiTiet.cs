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
        var name = new Label { Text = nhan, AutoSize = true, ForeColor = ShopTheme.Muted, Font = new Font("Segoe UI", 10), Anchor = AnchorStyles.Left, Margin = new Padding(0, 5, 12, 5) };
        int row = bang.RowCount++; bang.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        if (giaTri is Label) giaTri.Anchor = AnchorStyles.Left;
        else { giaTri.Anchor = AnchorStyles.Left | AnchorStyles.Right; giaTri.Margin = new Padding(0, 4, 2, 4); giaTri.Font = new Font("Segoe UI", 10); }
        bang.Controls.Add(name, 0, row); bang.Controls.Add(giaTri, 1, row);
    }

    // Nền kem của panel chi tiết (giống mẫu Đơn hàng / Phiếu nhập: nền kem, nội dung nằm trong thẻ sáng có viền)
    public static Panel NenChiTiet() => new() { Dock = DockStyle.Fill, Padding = new Padding(14, 12, 14, 12), BackColor = ShopTheme.SurfaceAlt };

    // Thẻ có viền bao quanh một nhóm nội dung (bảng thông tin, hàng nút...); các phần tử bên trong Dock Top
    public static Panel The(params Control[] noiDung)
    {
        var the = new Panel { Dock = DockStyle.Top, AutoSize = true, BackColor = ShopTheme.Surface, Padding = new Padding(12, 8, 12, 8) };
        the.Paint += (_, e) => { using var p = new Pen(ShopTheme.Line); e.Graphics.DrawRectangle(p, 0, 0, the.Width - 1, the.Height - 1); };
        the.Resize += (_, _) => the.Invalidate();
        // Dock Top: thêm sau nằm cao hơn, nên thêm ngược thứ tự
        for (int i = noiDung.Length - 1; i >= 0; i--) { noiDung[i].Dock = DockStyle.Top; the.Controls.Add(noiDung[i]); }
        return the;
    }

    // Thẻ có tiêu đề: tên nhóm + đường kẻ mảnh, rồi nội dung; nhiều thẻ xếp chồng tạo thành form gọn gàng
    public static Panel The(string tieuDe, params Control[] noiDung)
    {
        var tde = new Label { Text = tieuDe, AutoSize = false, Height = 30, TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = ShopTheme.Green };
        var ke = new Panel { Height = 1, BackColor = ShopTheme.Line, Margin = Padding.Empty };
        var dem = new Panel { Height = 10, BackColor = ShopTheme.Surface };
        var ds = new List<Control> { tde, ke, dem }; ds.AddRange(noiDung);
        var the = The(ds.ToArray());
        the.Padding = new Padding(16, 10, 16, 6);
        return the;
    }

    // Lưới ô nhập: nhãn nằm trên ô, hai ô một hàng; ô "rộng" chiếm cả hàng.
    // Nhãn và ô nằm thẳng trong một bảng (không lồng bảng tự co giãn, tránh vòng lặp dàn trang).
    public static TableLayoutPanel LuoiO(params (string Nhan, Control O, bool Rong)[] ds)
    {
        var t = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, BackColor = ShopTheme.Surface, Margin = Padding.Empty };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        // gom thành các hàng: mỗi hàng 1 ô rộng hoặc tối đa 2 ô hẹp
        var hang = new List<List<(string Nhan, Control O, bool Rong)>>();
        foreach (var o in ds)
        {
            if (o.Rong) { hang.Add(new() { o }); continue; }
            if (hang.Count == 0 || hang[^1].Count == 2 || hang[^1][0].Rong) hang.Add(new());
            hang[^1].Add(o);
        }
        int row = 0;
        foreach (var h in hang)
        {
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize)); t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            for (int col = 0; col < h.Count; col++)
            {
                var (nhan, o, rong) = h[col];
                int trai = col == 0 ? 0 : 8, phai = col == 0 && !rong ? 8 : 0;
                var lb = new Label { Text = nhan, AutoSize = true, ForeColor = ShopTheme.Muted, Anchor = AnchorStyles.Left, Margin = new Padding(trai, 0, phai, 4) };
                o.Margin = new Padding(trai, 0, phai, 14);
                o.Anchor = o is Label ? AnchorStyles.Left : AnchorStyles.Left | AnchorStyles.Right;
                t.Controls.Add(lb, col, row); t.Controls.Add(o, col, row + 1);
                if (rong) { t.SetColumnSpan(lb, 2); t.SetColumnSpan(o, 2); }
            }
            row += 2;
        }
        t.RowCount = row;
        return t;
    }

    // Khoảng cách giữa các thẻ
    public static Control Cach(int cao = 10) => new Panel { Dock = DockStyle.Top, Height = cao, BackColor = ShopTheme.SurfaceAlt };

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
