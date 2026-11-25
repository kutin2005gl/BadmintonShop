using System.Globalization;
using System.Runtime.CompilerServices;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

internal interface IUnsavedWork { bool HasUnsavedWork { get; } }

internal static class ShopTheme
{
    // TV1: giao diện của app; các thuộc tính màu (Green, Background...) đọc từ đây nên form không phải sửa.
    public static ThemeDef Current => ThemeCatalog.Giay;
    public static Color Green => Current.Accent;
    public static Color Background => Current.Bg;
    public static Color Ink => Current.Ink;
    public static Color Muted => Current.Muted;
    public static Color Line => Current.Line;
    public static Color Surface => Current.Surface;
    public static Color SurfaceAlt => Current.SurfaceAlt;
    public static Color Danger => Current.Danger;
    public static Color DangerBg => Current.DangerBg;
    public static Color Warn => Current.Warn;
    public static Color WarnBg => Current.WarnBg;
    public static Color AccentSoft => Current.AccentSoft;
    public static readonly Brush InkBrush = new SolidBrush(ThemeCatalog.Giay.Ink);
    public static readonly Brush MutedBrush = new SolidBrush(ThemeCatalog.Giay.Muted);
    public static readonly CultureInfo Vietnamese = CultureInfo.GetCultureInfo("vi-VN");
    private sealed class LoadState { public bool Busy; public bool Closed; public Label? Status; }
    private static readonly ConditionalWeakTable<Form, LoadState> States = new();
    public static bool IsBusy(Form form) => States.TryGetValue(form, out var state) && state.Busy;

    private static readonly Dictionary<string, Font> fonts = new();
    // TV1: phông dùng chung, cache để không rò GDI
    public static Font Face(string family, float size, FontStyle style = FontStyle.Regular)
    {
        var key = $"{family}|{size}|{(int)style}";
        if (!fonts.TryGetValue(key, out var f)) fonts[key] = f = new Font(family, size, style);
        return f;
    }
    public static Font TitleFace(float size) => Face(Current.TitleFont, size, FontStyle.Bold);

    // ---- Nhận vai trò màu của một control rồi gán lại theo giao diện ----
    private enum Role { None, Bg, Surface, SurfaceAlt, Line, Accent, AccentSoft, Ink, Muted, Danger, Warn, OnAccent, DangerBg, WarnBg }
    private static readonly Dictionary<int, Role> backRoles = BuildBackRoles(), foreRoles = BuildForeRoles();
    // TV1: bảng nhận vai trò màu nền (màu giao diện + màu cũ trong code)
    private static Dictionary<int, Role> BuildBackRoles()
    {
        var d = new Dictionary<int, Role> { [Color.White.ToArgb()] = Role.Surface, [Color.FromArgb(240, 240, 240).ToArgb()] = Role.Bg, [Color.FromArgb(248, 251, 249).ToArgb()] = Role.SurfaceAlt };
        var t = Current;
        d[t.Bg.ToArgb()] = Role.Bg; d[t.Surface.ToArgb()] = Role.Surface; d[t.SurfaceAlt.ToArgb()] = Role.SurfaceAlt; d[t.Line.ToArgb()] = Role.Line;
        d[t.Accent.ToArgb()] = Role.Accent; d[t.AccentSoft.ToArgb()] = Role.AccentSoft; d[t.DangerBg.ToArgb()] = Role.DangerBg; d[t.WarnBg.ToArgb()] = Role.WarnBg;
        return d;
    }
    // TV1: bảng nhận vai trò màu chữ (màu giao diện + màu cũ trong code)
    private static Dictionary<int, Role> BuildForeRoles()
    {
        var d = new Dictionary<int, Role>
        {
            [Color.Black.ToArgb()] = Role.Ink, [Color.DimGray.ToArgb()] = Role.Muted, [Color.Gray.ToArgb()] = Role.Muted,
            [Color.Firebrick.ToArgb()] = Role.Danger, [Color.DarkRed.ToArgb()] = Role.Danger, [Color.SeaGreen.ToArgb()] = Role.Accent,
            [Color.FromArgb(35, 100, 75).ToArgb()] = Role.Accent, [Color.White.ToArgb()] = Role.OnAccent, [Color.FromArgb(155, 96, 10).ToArgb()] = Role.Warn,
        };
        var t = Current;
        d[t.Ink.ToArgb()] = Role.Ink; d[t.Muted.ToArgb()] = Role.Muted; d[t.Accent.ToArgb()] = Role.Accent;
        d[t.Danger.ToArgb()] = Role.Danger; d[t.Warn.ToArgb()] = Role.Warn; d[t.OnAccent.ToArgb()] = Role.OnAccent;
        return d;
    }
    // TV1: đổi vai trò màu thành màu của giao diện
    private static Color Of(Role r) => r switch
    {
        Role.Bg => Current.Bg, Role.Surface => Current.Surface, Role.SurfaceAlt => Current.SurfaceAlt, Role.Line => Current.Line,
        Role.Accent => Current.Accent, Role.AccentSoft => Current.AccentSoft, Role.Ink => Current.Ink, Role.Muted => Current.Muted,
        Role.Danger => Current.Danger, Role.Warn => Current.Warn, Role.OnAccent => Current.OnAccent, Role.DangerBg => Current.DangerBg, Role.WarnBg => Current.WarnBg, _ => Color.Empty
    };

    // TV1: áp giao diện lên form/control và toàn bộ con; gọi lại được nhiều lần
    public static void Apply(Control root)
    {
        if (root is Form tracked) LifecycleDiagnostics.Track(tracked);
        if (root is Form f)
        {
            if (f.Font.Name != "Segoe UI" || f.Font.Size != 10) f.Font = new Font("Segoe UI", 10);
            f.BackColor = Current.Bg; f.ForeColor = Current.Ink;
            f.StartPosition = FormStartPosition.CenterParent;
            f.AutoScaleMode = AutoScaleMode.Font;
            ChuONhap.Gan(f);
        }
        else Paint(root);
        Walk(root);
    }
    // TV1: duyệt cây control để tô lại từng phần tử
    private static void Walk(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            Paint(c);
            if (c is DataGridView) continue;
            if (c.HasChildren) Walk(c);
            if (c is FlowLayoutPanel flow) CanDeuHangNut(flow);
        }
    }

    public const int CaoNut = 38, RongNutToiThieu = 112;
    // Thanh công cụ có nút: mọi nút cùng cỡ, ô nhập/nhãn cùng font và canh giữa theo chiều dọc với nút
    private static void CanDeuHangNut(FlowLayoutPanel flow)
    {
        if (!flow.Controls.OfType<Button>().Any()) return;
        foreach (Control c in flow.Controls)
        {
            if (c is Button b) { b.Margin = new Padding(4); b.Height = Math.Max(b.Height, CaoNut); continue; }
            bool field = c is TextBox { Multiline: false } or NumericUpDown or DateTimePicker or ComboBox;
            if (!field && c is not Label { AutoSize: true }) continue;
                        int h = c.PreferredSize.Height;
            c.Margin = new Padding(4, 4 + Math.Max(0, (CaoNut - h) / 2), 4, 4);
        }
    }
    // TV1: tô một control theo loại và vai trò màu hiện có
    private static void Paint(Control c)
    {
        // Mọi chữ tối thiểu cỡ 10 (giữ nguyên kiểu đậm/nghiêng và các tiêu đề cỡ lớn hơn)
        if (c is not DataGridView && c.Font.Size < 10)
            c.Font = new Font("Segoe UI", 10, c.Font.Style);
        switch (c)
        {
            case Button b: StyleButton(b); return;
            case DataGridView g: StyleGrid(g); return;
            case TextBoxBase t: t.BorderStyle = BorderStyle.FixedSingle; t.BackColor = Current.Field; t.ForeColor = Current.Ink; return;
            case ComboBox cb: StyleCombo(cb); return;
            case TabControl tab: StyleTabs(tab); break;
            case TabPage tp: tp.UseVisualStyleBackColor = false; tp.BackColor = Current.Surface; return;
            case NumericUpDown n: n.BackColor = Current.Field; n.ForeColor = Current.Ink; return;
            case DateTimePicker d: d.CalendarMonthBackground = Current.Field; d.CalendarForeColor = Current.Ink; d.CalendarTitleBackColor = Current.Accent; d.CalendarTitleForeColor = Current.OnAccent; return;
            case ListBox or ListView or TreeView: c.BackColor = Current.Field; c.ForeColor = Current.Ink; return;
            case BieuDoCot or BieuDoThuChi: c.BackColor = Current.Surface; c.Invalidate(); return;
        }
        var back = backRoles.GetValueOrDefault(c.BackColor.ToArgb());
        if (back != Role.None) c.BackColor = Of(back);
        var fore = foreRoles.GetValueOrDefault(c.ForeColor.ToArgb());
        if (fore == Role.OnAccent) { if (back == Role.Accent) c.ForeColor = Current.OnAccent; }
        else if (fore != Role.None) c.ForeColor = Of(fore);
    }

    private static readonly ConditionalWeakTable<ComboBox, object> combos = new();
    // TV1: ô chọn tự vẽ để hộp và danh sách thả xuống theo màu giao diện (ô nhập tay giữ kiểu hệ thống)
    private static void StyleCombo(ComboBox cb)
    {
        cb.FlatStyle = cb.DropDownStyle == ComboBoxStyle.DropDownList ? FlatStyle.Flat : FlatStyle.Standard; cb.BackColor = Current.Field; cb.ForeColor = Current.Ink;
        if (cb.DropDownStyle != ComboBoxStyle.DropDownList || combos.TryGetValue(cb, out _)) { cb.Invalidate(); return; }
        combos.Add(cb, new object());
        cb.DrawMode = DrawMode.OwnerDrawFixed;
        cb.DrawItem += (_, e) =>
        {
            if (e.Index < 0) return;
            var t = Current; bool hot = (e.State & DrawItemState.Selected) != 0 && (e.State & DrawItemState.ComboBoxEdit) == 0;
            using (var bg = new SolidBrush(hot ? t.AccentSoft : t.Field)) e.Graphics.FillRectangle(bg, e.Bounds);
            TextRenderer.DrawText(e.Graphics, cb.GetItemText(cb.Items[e.Index]), cb.Font, new Rectangle(e.Bounds.X + 2, e.Bounds.Y, e.Bounds.Width - 2, e.Bounds.Height), hot ? t.Accent : t.Ink,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        };
    }

    private static readonly ConditionalWeakTable<TabControl, object> tabbed = new();
    // TV1: tab tự vẽ theo màu giao diện
    private static void StyleTabs(TabControl tab)
    {
        if (tabbed.TryGetValue(tab, out _)) { tab.Invalidate(); return; }
        tabbed.Add(tab, new object());
        tab.DrawMode = TabDrawMode.OwnerDrawFixed; tab.Appearance = TabAppearance.FlatButtons;
        tab.DrawItem += (_, e) =>
        {
            var t = Current; bool sel = e.Index == tab.SelectedIndex;
            using (var bg = new SolidBrush(sel ? t.Surface : t.Bg)) e.Graphics.FillRectangle(bg, e.Bounds);
            TextRenderer.DrawText(e.Graphics, tab.TabPages[e.Index].Text, tab.Font, e.Bounds, sel ? t.Accent : t.Muted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            if (sel) { using var line = new SolidBrush(t.Accent); e.Graphics.FillRectangle(line, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, 3); }
        };
    }

    // TV1: kiểu nút theo ý nghĩa (chính / nguy hiểm / phụ) lấy theo chữ trên nút
    public static void StyleButton(Button b)
    {
        bool primary = b.Text.StartsWith("Thêm") || b.Text.StartsWith("Lưu") || b.Text.StartsWith("Thanh toán") || b.Text.StartsWith("Xem báo cáo") || b.Text == "Xác nhận";
        bool danger = b.Text.StartsWith("Xóa") || b.Text.StartsWith("Hủy") || b.Text.StartsWith("Ngừng");
        var t = Current;
        b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = primary ? 0 : 1;
        b.FlatAppearance.BorderColor = danger ? t.Danger : t.Line;
        b.BackColor = primary ? t.Accent : danger ? t.DangerBg : t.Surface;
        b.ForeColor = primary ? t.OnAccent : danger ? t.Danger : t.Accent; b.Cursor = Cursors.Hand;
        b.Font = new Font("Segoe UI", 10); b.Padding = new Padding(10, 5, 10, 5);
        b.AutoSize = true; b.AutoSizeMode = AutoSizeMode.GrowOnly;
        b.MinimumSize = new Size(b.Parent is FlowLayoutPanel || b.Parent == null ? RongNutToiThieu : 0, CaoNut);
        b.FlatAppearance.MouseOverBackColor = primary ? t.AccentHover : t.AccentSoft;
        b.FlatAppearance.MouseDownBackColor = primary ? t.AccentDown : t.Line;
        if (primary && vedNutChinh.Add(b)) b.Paint += VeChuNutChinhKhiTat;
    }

    private static readonly HashSet<Button> vedNutChinh = new();

    // Nút chính bị vô hiệu: WinForms tự vẽ chữ xám khó đọc trên nền đậm -> vẽ lại chữ trắng
    private static void VeChuNutChinhKhiTat(object? sender, PaintEventArgs e)
    {
        if (sender is not Button b || b.Enabled) return;
        var t = Current;
        using (var br = new SolidBrush(b.BackColor)) e.Graphics.FillRectangle(br, b.ClientRectangle);
        TextRenderer.DrawText(e.Graphics, b.Text, b.Font, b.ClientRectangle, t.OnAccent,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
    }
    // TV1: kiểu bảng dữ liệu; chỉnh tại chỗ nên không mất định dạng cột của form
    public static void StyleGrid(DataGridView g)
    {
        var t = Current;
        g.BorderStyle = BorderStyle.None; g.BackgroundColor = t.Surface;
        g.EnableHeadersVisualStyles = false; g.GridColor = t.Line;
        g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        var h = g.ColumnHeadersDefaultCellStyle;
        h.BackColor = t.HeadBg; h.ForeColor = t.HeadInk; h.SelectionBackColor = t.HeadBg; h.SelectionForeColor = t.HeadInk;
        h.WrapMode = DataGridViewTriState.True; h.Font = Face("Segoe UI", 10, FontStyle.Bold); h.Padding = new Padding(6);
        var d = g.DefaultCellStyle;
        d.BackColor = t.Surface; d.ForeColor = t.Ink; d.SelectionBackColor = t.AccentSoft; d.SelectionForeColor = t.Accent;
        d.Padding = new Padding(5); d.FormatProvider = Vietnamese; d.WrapMode = DataGridViewTriState.True;
        g.AlternatingRowsDefaultCellStyle.BackColor = t.SurfaceAlt;
        g.RowTemplate.Height = t.RowHeight; g.RowTemplate.MinimumHeight = t.RowHeight - 2;
        g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        g.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
    }
    public static Label Title(string text, int size = 17) => new() { Text = text, AutoSize = true, Font = TitleFace(size), ForeColor = Current.Accent, Margin = new Padding(4, 8, 4, 8) };

    // Use the destination control's thread, even if a caller has no WinForms SynchronizationContext.
    public static bool OnUi(Form form, Action action)
    {
        bool Ended()=>form.IsDisposed || form.Disposing || States.TryGetValue(form,out var state) && state.Closed;
        if(Ended() || !form.IsHandleCreated)return false;
        try
        {
            if(form.InvokeRequired)form.Invoke((Action)(()=> { if(!Ended())action(); }));
            else action();
            return !form.IsDisposed;
        }
        catch(InvalidOperationException ex) when(Ended() && !ex.Message.Contains("CreateHandle",StringComparison.OrdinalIgnoreCase))
        { LifecycleDiagnostics.Record(form.GetType().Name+" skipped callback after close");return false; }
    }

    // The repository runs off the UI thread. Inputs must be captured before calling this method.
    // Each screen serializes loads and remains mounted when hidden by navigation.
    public static async Task<bool> Load<T>(Form form, Func<T> query, Action<T> bind, Func<T, bool>? empty = null)
    {
        var state = States.GetOrCreateValue(form);
        if (state.Busy || state.Closed || form.IsDisposed || form.Disposing) return false;
        LifecycleDiagnostics.Record(form.GetType().Name+" load begin");
        state.Busy = true;
        if (state.Status == null)
        {
            LifecycleDiagnostics.Track(form);
            form.FormClosed+=(_,_)=>state.Closed=true;
            form.Disposed+=(_,_)=>state.Closed=true;
            state.Status = new Label { Dock = DockStyle.Bottom, Height = form.Font.Height + 14, Padding = new Padding(8, 4, 8, 4), BackColor = Background, ForeColor = Muted };
            form.Controls.Add(state.Status);
        }
        var controls = form.Controls.Cast<Control>().Where(c => c != state.Status).Select(c => (Control:c, c.Enabled)).ToArray();
        foreach (var item in controls) item.Control.Enabled = false;
        state.Status.ForeColor = Muted;
        state.Status.Text = "Đang tải dữ liệu…";
        try
        {
            var result = await Task.Run(query);
            if (form.IsDisposed) return false;
            return OnUi(form,()=> {
                bind(result);
                state.Status.Text = empty?.Invoke(result) == true ? "Chưa có dữ liệu phù hợp. Hãy thay đổi bộ lọc hoặc thêm dữ liệu." : $"Đã cập nhật lúc {DateTime.Now:HH:mm:ss}";
            });
        }
        catch (Exception ex)
        {
            OnUi(form,()=> { state.Status.ForeColor = Color.Firebrick; state.Status.Text = "Lỗi: " + LoiCsdl.ThongBao(ex.GetBaseException()); state.Status.AutoEllipsis = true; });
            return false;
        }
        finally
        {
            OnUi(form,()=> { foreach (var item in controls) if (!item.Control.IsDisposed) item.Control.Enabled = item.Enabled; });
            state.Busy = false;
            LifecycleDiagnostics.Record(form.GetType().Name+" load end closed="+state.Closed);
        }
    }
}
