using System.Globalization;
using System.Runtime.CompilerServices;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

internal interface IUnsavedWork { bool HasUnsavedWork { get; } }

internal static class ShopTheme
{
    public static readonly Color Green = Color.FromArgb(19, 83, 63);
    public static readonly Color Background = Color.FromArgb(244, 247, 246);
    public static readonly Color Ink = Color.FromArgb(32, 49, 44);
    public static readonly Color Muted = Color.FromArgb(100, 117, 110);
    public static readonly Color Line = Color.FromArgb(224, 233, 228);
    public static readonly CultureInfo Vietnamese = CultureInfo.GetCultureInfo("vi-VN");
    private sealed class LoadState { public bool Busy; public Label? Status; }
    private static readonly ConditionalWeakTable<Form, LoadState> States = new();
    public static bool IsBusy(Form form) => States.TryGetValue(form, out var state) && state.Busy;

    public static void Apply(Control root)
    {
        if (root is Form) root.Font = new Font("Segoe UI", 10);
        root.ForeColor = Ink;
        if (root is Form f)
        {
            f.BackColor = Background;
            f.StartPosition = FormStartPosition.CenterParent;
            f.AutoScaleMode = AutoScaleMode.Font;
        }
        foreach (Control c in root.Controls)
        {
            if (c is Button b) StyleButton(b);
            else if (c is DataGridView grid) StyleGrid(grid);
            else if (c is TextBoxBase t) { t.BorderStyle = BorderStyle.FixedSingle; t.BackColor = Color.White; }
            else if (c is ComboBox combo) { combo.FlatStyle = FlatStyle.Flat; combo.BackColor = Color.White; }
            if (c.HasChildren) ApplyChildren(c);
        }
    }
    private static void ApplyChildren(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            if (c is Button b) StyleButton(b);
            if (c is DataGridView g) { StyleGrid(g); continue; }
            if (c.HasChildren) ApplyChildren(c);
        }
    }
    public static void StyleButton(Button b)
    {
        bool primary = b.Text.StartsWith("Thêm") || b.Text.StartsWith("Lưu") || b.Text.StartsWith("Thanh toán") || b.Text.StartsWith("Xem báo cáo") || b.Text == "Xác nhận";
        bool danger = b.Text.StartsWith("Xóa") || b.Text.StartsWith("Hủy") || b.Text.StartsWith("Ngừng");
        b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = primary ? 0 : 1;
        b.FlatAppearance.BorderColor = Line;
        b.BackColor = primary ? Green : danger ? Color.FromArgb(255, 241, 239) : Color.White;
        b.ForeColor = primary ? Color.White : danger ? Color.Firebrick : Green; b.Cursor = Cursors.Hand;
        b.Padding = new Padding(10, 5, 10, 5); b.MinimumSize = new Size(0, 38);
        b.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(35, 112, 83) : Color.FromArgb(225, 240, 231);
        b.FlatAppearance.MouseDownBackColor = primary ? Color.FromArgb(10, 61, 44) : Line;
    }
    public static void StyleGrid(DataGridView g)
    {
        g.BorderStyle = BorderStyle.None; g.BackgroundColor = Color.White;
        g.EnableHeadersVisualStyles = false; g.GridColor = Line;
        g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        g.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Green, ForeColor = Color.White, SelectionBackColor = Green, SelectionForeColor = Color.White, WrapMode = DataGridViewTriState.True, Font = new Font("Segoe UI", 10, FontStyle.Bold), Padding = new Padding(6) };
        g.DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.White, ForeColor = Ink, SelectionBackColor = Color.FromArgb(219, 239, 228), SelectionForeColor = Green, Padding = new Padding(5), FormatProvider = Vietnamese };
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 251, 249);
        g.RowTemplate.Height = 42;
        g.RowTemplate.MinimumHeight = 40;
        g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        g.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        g.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
    }
    public static Label Title(string text, int size = 17) => new() { Text = text, AutoSize = true, Font = new Font("Segoe UI", size, FontStyle.Bold), ForeColor = Green, Margin = new Padding(4, 8, 4, 8) };

    // The repository runs off the UI thread. Inputs must be captured before calling this method.
    // Each screen serializes loads and remains mounted when hidden by navigation.
    public static async Task<bool> Load<T>(Form form, Func<T> query, Action<T> bind, Func<T, bool>? empty = null)
    {
        var state = States.GetOrCreateValue(form);
        if (state.Busy || form.IsDisposed) return false;
        state.Busy = true;
        if (state.Status == null)
        {
            state.Status = new Label { Dock = DockStyle.Bottom, Height = 34, Padding = new Padding(8), BackColor = Background, ForeColor = Muted };
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
            bind(result);
            state.Status.Text = empty?.Invoke(result) == true ? "Chưa có dữ liệu phù hợp. Hãy thay đổi bộ lọc hoặc thêm dữ liệu." : $"Đã cập nhật lúc {DateTime.Now:HH:mm:ss}";
            return true;
        }
        catch (Exception ex)
        {
            if (!form.IsDisposed) { state.Status.ForeColor = Color.Firebrick; state.Status.Text = "Lỗi: " + LoiCsdl.ThongBao(ex); state.Status.AutoEllipsis = true; }
            return false;
        }
        finally
        {
            state.Busy = false;
            if (!form.IsDisposed) foreach (var item in controls) if (!item.Control.IsDisposed) item.Control.Enabled = item.Enabled;
        }
    }
}
