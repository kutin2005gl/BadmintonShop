using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BadmintonShop.Desktop;

// Ô nhập do Windows tự vẽ chữ (TextBox, ô ngày, ô số, ô chọn gõ được) nhận font qua HFONT. Trên một số máy
// HFONT này được tạo theo DPI khác với DPI của cửa sổ, nên chữ trong ô nhỏ hơn nhãn và bảng (vẽ bởi app).
// Ở đây đo cỡ chữ thật của HFONT, so với cỡ đúng theo DPI của control, rồi phóng font bù lại.
// Máy hiển thị đúng thì tỉ lệ = 1 và không đổi gì.
internal static class ChuONhap
{
    private static readonly ConditionalWeakTable<Form, object> daGan = new();
    private static readonly ConditionalWeakTable<Control, Font> fontGoc = new();

    public static void Gan(Form form)
    {
        if (!daGan.TryAdd(form, new object())) return;
        void canLai(object? s, EventArgs e) { if (form.Visible) CanTatCa(form); }
        form.Shown += canLai; form.VisibleChanged += canLai; form.DpiChanged += canLai;
    }

    private static void CanTatCa(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            if (c is TextBoxBase or DateTimePicker or NumericUpDown || c is ComboBox { DrawMode: DrawMode.Normal }) Can(c);
            if (c.HasChildren && c is not DataGridView) CanTatCa(c);
        }
    }

    public static void Can(Control c)
    {
        if (c is TextBox { Multiline: false } tb) ChuGoiYWindows(tb);
        if (!fontGoc.TryGetValue(c, out var goc)) { goc = c.Font; fontGoc.Add(c, goc); }
        float tiLe = TiLeThieu(goc, c.DeviceDpi);
        float coMoi = tiLe > 1.05f ? goc.SizeInPoints * tiLe : goc.SizeInPoints;
        if (Math.Abs(c.Font.SizeInPoints - coMoi) > 0.1f) c.Font = new Font(goc.FontFamily, coMoi, goc.Style);
    }

    // Chữ gợi ý (PlaceholderText) do WinForms tự vẽ theo font của ô, nên to hơn chữ gõ khi font đã được phóng.
    // Chuyển sang gợi ý của chính Windows (EM_SETCUEBANNER) để vẽ cùng cỡ với chữ gõ vào.
    private static readonly ConditionalWeakTable<TextBox, string> goiY = new();
    private static void ChuGoiYWindows(TextBox tb)
    {
        if (!goiY.TryGetValue(tb, out var text))
        {
            if (string.IsNullOrEmpty(tb.PlaceholderText)) return;
            text = tb.PlaceholderText; goiY.Add(tb, text);
            tb.PlaceholderText = "";
            tb.HandleCreated += (_, _) => SendMessage(tb.Handle, EM_SETCUEBANNER, (IntPtr)1, text);
        }
        if (tb.IsHandleCreated) SendMessage(tb.Handle, EM_SETCUEBANNER, (IntPtr)1, text);
    }

    private const int EM_SETCUEBANNER = 0x1501;
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr SendMessage(IntPtr h, int msg, IntPtr w, string l);

    // Cỡ chữ đúng (pixel) theo DPI của control chia cho cỡ chữ của HFONT Windows sẽ dùng.
    private static float TiLeThieu(Font f, int dpi)
    {
        IntPtr h = f.ToHfont();
        try
        {
            var buf = new byte[92];
            if (GetObject(h, buf.Length, buf) == 0) return 1f;
            int cao = Math.Abs(BitConverter.ToInt32(buf, 0));
            if (cao == 0) return 1f;
            float dung = f.SizeInPoints * dpi / 72f;
            return dung / cao;
        }
        finally { DeleteObject(h); }
    }

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)] private static extern int GetObject(IntPtr h, int c, byte[] buf);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr h);
}
