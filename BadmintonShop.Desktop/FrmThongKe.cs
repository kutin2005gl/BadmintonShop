using System.Data;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public class FrmThongKe : Form
{
    private readonly DateTimePicker tu = TaoChonThang();
    private readonly DateTimePicker den = TaoChonThang();
    private readonly NumericUpDown nam = new() { Minimum = 1753, Maximum = 9998, Width = 90, Value = DateTime.Today.Year, AccessibleName = "Năm thống kê" };
    private readonly System.Windows.Forms.Timer henNapNam = new() { Interval = 400 };
    private readonly ComboBox loai = Sv4Ui.Chon("Tất cả loại bán", "Online", "Tại quầy");
    private readonly Label tong = new() { Dock = DockStyle.Top, Height = 34, Padding = new Padding(14, 8, 0, 0), ForeColor = ShopTheme.Muted };
    private readonly TabControl tabs = new() { Dock = DockStyle.Fill };
    // Thanh chọn báo cáo dạng "chip" thay cho dải tab: tự xuống hàng, mỗi chip rộng vừa chữ
    private readonly FlowLayoutPanel chips = new() { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Padding = new Padding(10, 6, 10, 6) };
    private readonly Label tongThu = ShopTheme.Title("—", 20);
    private readonly Label tongChi = ShopTheme.Title("—", 20);
    private int soThang;
    private KetQuaBaoCao? ketQua;
    private const ChucNang Quyen=ChucNang.ThongKeBinhLuan;
    private readonly BoNhoPhien<(DateTime, DateTime, string?, bool), KetQuaBaoCao> boNho = new(BoNhoPhien<int, int>.ThoiHanMacDinh);
    public FrmThongKe() { InitializeComponent(); }
    // TV1: khoi tao giao dien thong ke va hen nap khi doi nam
    private void InitializeComponent()
    {
        Text = "Thống kê — báo cáo";
        Size = new Size(1200, 820);
        MinimumSize = new Size(780, 540);
        AutoScaleMode = AutoScaleMode.Font;
        tu.AccessibleName = "Từ tháng";
        den.AccessibleName = "Đến tháng";
        tu.Value = new DateTime(DateTime.Today.Year, 1, 1);
        tu.Enabled = den.Enabled = false;
        loai.AccessibleName = "Kênh bán hàng chi tiết";

        var ky = Sv4Ui.ThanhCongCu();
        ky.MinimumSize = Size.Empty;
        ky.Controls.AddRange([TaoLuaChon("Theo quý", 0), nam, TaoLuaChon("3 tháng", 3),
            TaoLuaChon("6 tháng", 6), TaoLuaChon("9 tháng", 9), TaoLuaChon("12 tháng", 12),
            TaoLuaChon("Từ tháng", -1), tu, Sv4Ui.Nhan("đến tháng"), den]);
        var bar = Sv4Ui.ThanhCongCu();
        bar.Controls.AddRange([Sv4Ui.Nhan("Kênh bán hàng chi tiết"), loai,
            Sv4Ui.Nut("Xem báo cáo", () => Chay(Nap)),
            Sv4Ui.Nut("Làm mới", () => Chay(() => { boNho.Xoa(); Nap(); })),
            Sv4Ui.Nut("Xuất Excel", () => Chay(() => Xuat(false))),
            Sv4Ui.Nut("Xuất PDF", () => Chay(() => Xuat(true)))]);
        var cards = new TableLayoutPanel { Dock = DockStyle.Top, Height = 96, ColumnCount = 2, Padding = new Padding(8) };
        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        cards.Controls.Add(TaoThe("Tổng thu", tongThu), 0, 0);
        cards.Controls.Add(TaoThe("Tổng chi", tongChi), 1, 0);
        Controls.Add(tabs);
        Controls.Add(chips);
        Controls.Add(tong);
        Controls.Add(cards);
        Controls.Add(bar);
        Controls.Add(ky);
        nam.ValueChanged += (_, _) =>
        {
            henNapNam.Stop();
            henNapNam.Start();
        };
        henNapNam.Tick += (_, _) =>
        {
            henNapNam.Stop();
            Chay(Nap);
        };
        loai.SelectedIndexChanged += (_, _) => Chay(Nap);
        ShopTheme.Apply(this);
    }

    // TV1: tao o chon thang va nam
    private static DateTimePicker TaoChonThang() => new()
    {
        Format = DateTimePickerFormat.Custom, CustomFormat = "MM/yyyy", ShowUpDown = true,
        Width = 100, MinDate = new DateTime(1753, 1, 1), MaxDate = new DateTime(9998, 12, 31)
    };

    // TV1: tao nut chon ky thong ke
    private RadioButton TaoLuaChon(string ten, int thang)
    {
        var nut = new RadioButton { Text = ten, AutoSize = true, Checked = thang == 0, Margin = new Padding(3, 7, 3, 3) };
        nut.CheckedChanged += (_, _) =>
        {
            if (!nut.Checked) return;
            henNapNam.Stop();
            soThang = thang;
            nam.Enabled = thang == 0;
            tu.Enabled = den.Enabled = thang < 0;
            if (thang >= 0) Chay(Nap);
            else tu.Focus();
        };
        return nut;
    }

    // TV1: tao the hien thi so tien tong hop
    private static Panel TaoThe(string ten, Label giaTri)
    {
        var the = new Panel { Dock = DockStyle.Fill, BackColor = ShopTheme.Surface, Padding = new Padding(12, 8, 12, 4), Margin = new Padding(4) };
        giaTri.Dock = DockStyle.Fill;
        the.Controls.Add(giaTri);
        the.Controls.Add(new Label { Text = ten, Dock = DockStyle.Top, Height = 24, ForeColor = ShopTheme.Muted });
        return the;
    }
    protected override void OnLoad(EventArgs e)
    { base.OnLoad(e);if(DesignMode || !QuyenForm.KiemTraKhiMo(this,Quyen))return;Chay(Nap); }

    // TV1: huy bo hen nap nam khi giai phong form
    protected override void Dispose(bool disposing)
    {
        if (disposing) henNapNam.Dispose();
        base.Dispose(disposing);
    }

    private void Chay(Action a)=>Sv4Ui.Chay(this,Quyen,a);
    // SV4: nạp thành công mới thay kết quả, bảng và biểu đồ cùng dùng snapshot này.
    // TV1: nap bao cao theo ky va kenh ban da chon
    private async void Nap()
    {
        henNapNam.Stop();
        var homNay = DateTime.Today;
        var dauThang = new DateTime(homNay.Year, homNay.Month, 1);
        var theoQuy = soThang == 0;
        var from = theoQuy ? new DateTime((int)nam.Value, 1, 1)
            : soThang > 0 ? dauThang.AddMonths(1 - soThang) : new DateTime(tu.Value.Year, tu.Value.Month, 1);
        var thangCuoi = theoQuy ? from.AddMonths(11)
            : soThang > 0 ? dauThang : new DateTime(den.Value.Year, den.Value.Month, 1);
        if (from > thangCuoi)
        {
            MessageBox.Show(this, "Tháng bắt đầu phải trước hoặc bằng tháng kết thúc.", "Kỳ thống kê", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var to = thangCuoi.AddMonths(1).AddDays(-1);
        var saleType = Sv4Ui.Loc(loai);
        var scope=$"{DbHelper.ConnectionString}|{PhienDangNhap.MaTK}|{PhienDangNhap.VaiTro}|{DbHelper.DauHieuDuLieu}";
        await ShopTheme.Load(this, () => boNho.Lay((from, to, saleType, theoQuy), scope,
            () => BaoCaoService.TaoTheoKy(new BaoCaoRepository().LayDuLieuBaoCao(from, to, null), from, to, saleType, theoQuy)), HienThi,
            kq => kq.Bang[BaoCaoService.BangThuChi].AsEnumerable().All(r => r.Field<decimal>("Thu") == 0 && r.Field<decimal>("Chi") == 0)
                && kq.Bang["Trạng thái đơn"].AsEnumerable().All(r => r.Field<int>("Số đơn") == 0));
    }

    // TV1: hien thi so tong hop, bang va bieu do
    private void HienThi(KetQuaBaoCao kq)
    {
        var thuChi = kq.Bang[BaoCaoService.BangThuChi].AsEnumerable();
        tongThu.Text = $"{thuChi.Sum(r => r.Field<decimal>("Thu")):N0} đ";
        tongChi.Text = $"{thuChi.Sum(r => r.Field<decimal>("Chi")):N0} đ";
        int soDon = kq.Bang["Trạng thái đơn"].AsEnumerable().Sum(r => r.Field<int>("Số đơn"));
        long daBan = kq.Bang["Bán chạy"].AsEnumerable().Sum(r => r.Field<long>("Số lượng"));
        tong.Text = $"{kq.TuNgay:dd/MM/yyyy} – {kq.DenNgay:dd/MM/yyyy} | {kq.LoaiBan}: {soDon:N0} đơn, {daBan:N0} sản phẩm đã bán";

        var tabDangXem = tabs.SelectedTab?.Text;
        ketQua = kq;
        while (tabs.TabPages.Count > 0)
        {
            var old = tabs.TabPages[0];
            tabs.TabPages.RemoveAt(0);
            old.Dispose();
        }
        foreach (var (ten, dt) in kq.Bang)
        {
            var page = new TabPage(ten);
            var grid = Sv4Ui.Luoi();
            grid.DataBindingComplete += (_, _) => CanGiuaSo(grid);
            if (ten == BaoCaoService.BangThuChi)
            {
                grid.DataBindingComplete += (_, _) =>
                {
                    grid.Columns["Kỳ"].MinimumWidth = 75;
                    grid.Columns["Thu"].MinimumWidth = 92;
                    grid.Columns["Chi"].MinimumWidth = 92;
                };
                grid.DataSource = dt;
                var chia = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = ShopTheme.Surface, Padding = new Padding(8) };
                chia.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43));
                chia.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57));
                chia.Controls.Add(grid, 0, 0);
                chia.Controls.Add(new BieuDoThuChi(dt), 1, 0);
                page.Controls.Add(chia);
            }
            else if (ten == "Bán chạy")
            {
                var top = dt.Clone();
                foreach (var row in dt.AsEnumerable().Take(10)) top.ImportRow(row);
                grid.DataSource = top;
                var bar = Sv4Ui.ThanhCongCu();
                bar.Controls.Add(Sv4Ui.Nhan("10 sản phẩm bán chạy nhất"));
                var xem = Sv4Ui.Nut("Xem tất cả", () => Chay(() => XemTatCa(kq, dt)));
                ShopTheme.StyleButton(xem);
                bar.Controls.Add(xem);
                page.Controls.Add(grid);
                page.Controls.Add(bar);
            }
            else
            {
                // Bảng bên trái, biểu đồ bên phải (giống Thu–chi toàn cửa hàng)
                var chia = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = ShopTheme.Surface, Padding = new Padding(8) };
                chia.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                chia.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                var chart = new BieuDoCot { DuLieu = dt, Dock = DockStyle.Fill };
                if (ten.StartsWith("Theo sản phẩm", StringComparison.Ordinal))
                {
                    chart.DuLieu = QuyCuaDong(dt, null);
                    grid.CurrentCellChanged += (_, _) => { chart.DuLieu = QuyCuaDong(dt, grid); chart.Invalidate(); };
                }
                grid.DataSource = dt;
                grid.Dock = DockStyle.Fill;
                chia.Controls.Add(grid, 0, 0);
                chia.Controls.Add(chart, 1, 0);
                page.Controls.Add(chia);
            }
            tabs.TabPages.Add(page);
        }
        // Ẩn dải tab gốc; chọn báo cáo bằng các chip ở trên
        tabs.Appearance = TabAppearance.FlatButtons; tabs.SizeMode = TabSizeMode.Fixed; tabs.Multiline = false; tabs.ItemSize = new Size(0, 1);
        DungChip();
        tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().FirstOrDefault(p => p.Text == tabDangXem) ?? tabs.TabPages[0];
        ToChip();
    }

    // Cột số (tiền, số lượng, số đơn, mã) canh giữa cùng tiêu đề; chạy sau định dạng chung (vốn canh phải số)
    private static void CanGiuaSo(DataGridView grid)
    {
        foreach (DataGridViewColumn c in grid.Columns)
        {
            var t = Nullable.GetUnderlyingType(c.ValueType ?? typeof(string)) ?? c.ValueType;
            if (t != typeof(decimal) && t != typeof(int) && t != typeof(long) && t != typeof(double)) continue;
            c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
    }

    // Tạo lại các chip theo danh sách báo cáo hiện có
    private void DungChip()
    {
        chips.SuspendLayout();
        while (chips.Controls.Count > 0) { var c = chips.Controls[0]; chips.Controls.RemoveAt(0); c.Dispose(); }
        chips.BackColor = ShopTheme.Background;
        foreach (TabPage page in tabs.TabPages)
        {
            var b = new Button { Text = page.Text, Tag = page, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlatStyle = FlatStyle.Flat,
                Padding = new Padding(10, 3, 10, 3), Margin = new Padding(0, 0, 8, 8), MinimumSize = new Size(0, 34), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
            b.Click += (_, _) => { tabs.SelectedTab = page; ToChip(); };
            chips.Controls.Add(b);
        }
        chips.ResumeLayout();
        if (!chipDaGan) { chipDaGan = true; tabs.SelectedIndexChanged += (_, _) => ToChip(); }
    }
    private bool chipDaGan;

    // Chip đang chọn: nền xanh đậm chữ trắng; còn lại: nền sáng viền mảnh
    private void ToChip()
    {
        var t = ShopTheme.Current;
        foreach (Button b in chips.Controls)
        {
            bool chon = ReferenceEquals(b.Tag, tabs.SelectedTab);
            b.BackColor = chon ? t.Accent : t.Surface;
            b.ForeColor = chon ? t.OnAccent : t.Ink;
            b.Font = new Font(b.Font, chon ? FontStyle.Bold : FontStyle.Regular);
            b.FlatAppearance.BorderSize = chon ? 0 : 1;
            b.FlatAppearance.BorderColor = t.Line;
            b.FlatAppearance.MouseOverBackColor = chon ? t.AccentHover : t.AccentSoft;
            b.FlatAppearance.MouseDownBackColor = chon ? t.AccentDown : t.Line;
        }
    }

    // TV1: mo danh sach day du san pham da ban trong ky
    private void XemTatCa(KetQuaBaoCao kq, DataTable dt)
    {
        using var form = new Form { Text = "Tất cả sản phẩm đã bán", Size = new Size(760, 570), MinimumSize = new Size(520, 360) };
        var grid = Sv4Ui.Luoi();
        grid.DataBindingComplete += (_, _) => CanGiuaSo(grid);
        grid.DataSource = dt;
        form.Controls.Add(grid);
        form.Controls.Add(new Label
        {
            Text = $"{kq.TuNgay:dd/MM/yyyy} – {kq.DenNgay:dd/MM/yyyy} | {kq.LoaiBan} | {dt.Rows.Count:N0} mặt hàng",
            Dock = DockStyle.Top, Height = 42, Padding = new Padding(12, 10, 0, 0)
        });
        ShopTheme.Apply(form);
        form.ShowDialog(this);
    }
    // CO-06: bảng "Quý — doanh thu" của dòng đang chọn (chưa chọn thì lấy dòng đầu); không có dòng thì bảng rỗng.
    public static DataTable QuyCuaDong(DataTable nguon,DataGridView? grid,DataRow? dong=null)
    {
        var row=dong ?? (grid?.CurrentRow?.DataBoundItem as DataRowView)?.Row ?? nguon.Rows.Cast<DataRow>().FirstOrDefault();
        var kq=new DataTable();kq.Columns.Add("Quý",typeof(string));
        kq.Columns.Add(row==null?"Doanh thu":$"Doanh thu — {Convert.ToString(row["Sản phẩm"])}",typeof(decimal));
        if(row!=null)for(int q=1;q<=4;q++)kq.Rows.Add($"Quý {q}",row.IsNull($"Q{q}")?0m:row.Field<decimal>($"Q{q}"));
        return kq;
    }
    private void Xuat(bool pdf)
    {
        var kq=ketQua??throw new InvalidOperationException("Hãy xem báo cáo trước.");
        using var d=new SaveFileDialog { Filter=pdf?"PDF|*.pdf":"Excel|*.xlsx",FileName=$"BaoCao_{kq.TuNgay:yyyyMMdd}_{kq.DenNgay:yyyyMMdd}."+(pdf?"pdf":"xlsx") };
        if(d.ShowDialog(this)!=DialogResult.OK)return;
        GhiBaoCao(d.FileName,kq,pdf);
    }
    private async void GhiBaoCao(string path,KetQuaBaoCao kq,bool pdf)
    {
        var success=await ShopTheme.Load(this,()=> { if(pdf)XuatTaiLieu.BaoCaoPdf(path,kq);else XuatTaiLieu.Excel(path,kq);return true; },_=>{});
        if(success)MessageBox.Show(this,"Đã xuất kết quả đang hiển thị, gồm bộ lọc và quy tắc thống kê.");
    }
}
