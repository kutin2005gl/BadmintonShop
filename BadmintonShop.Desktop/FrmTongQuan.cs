using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public class FrmTongQuan : Form
{
    private readonly Label[] values = new Label[4];
    private readonly DataGridView orders = Sv4Ui.Luoi(), low = Sv4Ui.Luoi();
    private readonly BieuDoCot chart = new();
    public FrmTongQuan()
    {
        Text = "Tổng quan"; AutoScaleMode = AutoScaleMode.Font;
        var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(16), AutoScroll = true };
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 44)); body.RowStyles.Add(new RowStyle(SizeType.Percent, 56));
        var cards = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
        string[] names = ["DOANH THU HÔM NAY", "ĐƠN CHỜ XÁC NHẬN", "SẢN PHẨM ĐANG BÁN", "TỒN THẤP / HẾT HÀNG"];
        for (int i = 0; i < 4; i++)
        {
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12), Margin = new Padding(4) };
            values[i] = new Label { Text = "—", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = ShopTheme.Green, AutoEllipsis = true };
            card.Controls.Add(values[i]); card.Controls.Add(new Label { Text = names[i], Dock = DockStyle.Top, Height = 30, ForeColor = ShopTheme.Muted, AutoEllipsis = true }); cards.Controls.Add(card, i, 0);
        }
        var info = Sv4Ui.ThanhCongCu(70);
        info.Controls.Add(Sv4Ui.Nut("↻  Làm mới", Tai));
        info.Controls.Add(Sv4Ui.Nhan("Hôm nay: doanh thu đơn Hoàn thành theo ngày lập.\nChờ xác nhận: toàn thời gian đến hôm nay. Tồn thấp: đang bán, tồn 0–5."));
        var lists = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        lists.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58)); lists.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        lists.Controls.Add(Section("Đơn gần đây • 30 ngày gần nhất", orders), 0, 0);
        lists.Controls.Add(Section("Sản phẩm cần bổ sung • tồn ≤ 5", low), 1, 0);
        body.Controls.Add(cards, 0, 0); body.Controls.Add(info, 0, 1);
        body.Controls.Add(Section("Doanh thu • 7 ngày gần nhất (VNĐ)", chart), 0, 2); body.Controls.Add(lists, 0, 3);
        Controls.Add(body); ShopTheme.Apply(this);
    }
    private static Panel Section(string title, Control content)
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10), Margin = new Padding(4) };
        p.Controls.Add(content); p.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 36, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = ShopTheme.Green }); return p;
    }
    protected override void OnLoad(EventArgs e) { base.OnLoad(e); if (!DesignMode) Tai(); }
    private async void Tai()
    {
        if (!PhienDangNhap.CoQuyen(ChucNang.DonHangPOS)) return;
        bool report = PhienDangNhap.CoQuyen(ChucNang.ThongKeBinhLuan), product = PhienDangNhap.CoQuyen(ChucNang.SanPhamDanhMucKhuyenMai);
        var today = DateTime.Today;
        await ShopTheme.Load(this, () => TongQuanService.Tai(today, report, product), data =>
        {
            values[0].Text = report ? $"{data.DoanhThu:N0} đ" : "Theo quyền";
            values[1].Text = data.ChoXacNhan.ToString("N0"); values[2].Text = product ? data.DangBan.ToString("N0") : "Theo quyền";
            values[3].Text = product ? data.SapHet.Rows.Count.ToString("N0") : "Theo quyền";
            chart.DuLieu = data.DoanhThuNgay; chart.Invalidate();
            orders.DataSource = data.DonGanDay; low.DataSource = data.SapHet;
            foreach (DataGridViewColumn c in orders.Columns) c.Visible = c.Name is "MaHD" or "KhachHang" or "TongTien" or "TrangThai";
            foreach (DataGridViewColumn c in low.Columns) c.Visible = c.Name is "TenSP" or "SoLuongTon" or "CanhBao";
        });
    }
}
