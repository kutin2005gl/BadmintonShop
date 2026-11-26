using System.Data;
using System.Drawing.Drawing2D;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public class FrmTongQuan : Form
{
    private static readonly int[] SoNgayChon = [7, 14, 30];
    // Chieu cao toi thieu (96 dpi) cua: loi chao, the so lieu, bieu do, danh sach
    private static readonly int[] CaoDong = [48, 118, 292, 294];
    private readonly Panel cuon = new() { Dock = DockStyle.Fill, AutoScroll = true };
    private readonly TableLayoutPanel body = new() { ColumnCount = 1, RowCount = 4 };
    private readonly ComboBox kyXem = Sv4Ui.Chon("7 ngày gần nhất", "14 ngày gần nhất", "30 ngày gần nhất");
    private readonly TheSoLieu theDoanhThu = new("", "Doanh thu hôm nay"), theCho = new("", "Đơn chờ xác nhận"),
        theDangBan = new("", "Sản phẩm đang bán"), theTon = new("", "Tồn thấp / Hết hàng");
    private readonly BieuDoDoanhThu bieuDo = new();
    private readonly BieuDoTrangThai trangThai = new();
    private readonly DataGridView orders = Sv4Ui.Luoi();
    private readonly Panel dsTon = new() { Dock = DockStyle.Fill, AutoScroll = true };
    private readonly List<Image> anhDangDung = new();
    private DataTable? sapHet;

    // TV1: dung bo cuc trang tong quan: loi chao, bon the so lieu, hai bieu do, hai danh sach
    public FrmTongQuan()
    {
        Text = "Tổng quan"; AutoScaleMode = AutoScaleMode.Font;
        body.Padding = new Padding(S(18), S(4), S(18), S(8));
        foreach (var cao in CaoDong) body.RowStyles.Add(new RowStyle(SizeType.Absolute, S(cao)));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var the = Hang(25, 25, 25, 25);
        TheSoLieu[] cacThe = [theDoanhThu, theCho, theDangBan, theTon];
        for (int i = 0; i < cacThe.Length; i++) Dat(the, cacThe[i], i);

        var giua = Hang(65, 35); Dat(giua, bieuDo, 0); Dat(giua, trangThai, 1);

        var duoi = Hang(57, 43);
        Dat(duoi, Muc("", "Đơn hàng gần đây", orders, PhienDangNhap.CoQuyen(ChucNang.DonHangPOS), m => m.MoTrangDonHang()), 0);
        Dat(duoi, Muc("", $"Cần bổ sung (tồn ≤ {QuyTacBanHang.NguongSapHet})", dsTon, PhienDangNhap.CoQuyen(ChucNang.SanPhamDanhMucKhuyenMai), m => m.MoTrangSanPham()), 1);

        body.Controls.Add(DauTrang(), 0, 0); body.Controls.Add(the, 0, 1); body.Controls.Add(giua, 0, 2); body.Controls.Add(duoi, 0, 3);
        body.Dock = DockStyle.Top; cuon.Controls.Add(body); Controls.Add(cuon);
        cuon.Resize += (_, _) => BoTri();
        ShopTheme.Apply(this);
    }

    // TV1: doi so do thiet ke (96 dpi) sang diem anh man hinh
    private int S(int v) => LogicalToDeviceUnits(v);

    // TV1: cua so cao thi chia phan du cho bieu do va danh sach, cua so thap thi cuon doc
    private void BoTri()
    {
        int toiThieu = CaoDong.Sum(S) + body.Padding.Vertical, du = Math.Max(0, cuon.Height - toiThieu);
        body.RowStyles[2].Height = S(CaoDong[2]) + du / 2; body.RowStyles[3].Height = S(CaoDong[3]) + du - du / 2;
        body.Height = toiThieu + du;
    }

    // TV1: dong dau trang gom loi chao, o chon ky va nut lam moi
    private Control DauTrang()
    {
        var dau = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = Padding.Empty };
        dau.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); dau.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); dau.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var chao = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true, UseMnemonic = false,
            Font = ShopTheme.Face("Segoe UI", 12), ForeColor = ShopTheme.Ink, Text = $"Chào mừng trở lại, {PhienDangNhap.HoTen}!" };
        kyXem.Width = S(170); kyXem.Anchor = AnchorStyles.Right; kyXem.AccessibleName = "Khoảng thời gian";
        kyXem.SelectionChangeCommitted += (_, _) => Tai();
        var lamMoi = Sv4Ui.Nut("↻  Làm mới", Tai); lamMoi.Anchor = AnchorStyles.Right;
        dau.Controls.Add(chao, 0, 0); dau.Controls.Add(kyXem, 1, 0); dau.Controls.Add(lamMoi, 2, 0);
        return dau;
    }

    // TV1: tao mot hang nhieu cot chia theo phan tram
    private static TableLayoutPanel Hang(params int[] phanTram)
    {
        var hang = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = phanTram.Length, RowCount = 1, Margin = Padding.Empty };
        foreach (var p in phanTram) hang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, p));
        hang.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        return hang;
    }

    // TV1: dat mot the vao cot cua hang voi khoang cach deu
    private void Dat(TableLayoutPanel hang, Control c, int cot)
    {
        c.Dock = DockStyle.Fill; c.Margin = new Padding(S(6)); hang.Controls.Add(c, cot, 0);
    }

    // TV1: the danh sach co bieu tuong, tieu de va lien ket Xem tat ca khi du quyen
    private TheKhung Muc(string glyph, string tieuDe, Control noiDung, bool coLienKet, Action<FrmMain> xemTatCa)
    {
        var khung = new TheKhung { Padding = new Padding(S(12), S(8), S(12), S(10)) };
        var dau = new TableLayoutPanel { Dock = DockStyle.Top, Height = S(42), ColumnCount = 3, RowCount = 1, BackColor = ShopTheme.Surface };
        dau.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, S(34))); dau.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); dau.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        dau.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        dau.Controls.Add(new Label { Text = glyph, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, UseCompatibleTextRendering = false,
            Font = ShopTheme.Face("Segoe MDL2 Assets", 15), ForeColor = ShopTheme.Green, Margin = Padding.Empty }, 0, 0);
        dau.Controls.Add(new Label { Text = tieuDe, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true,
            Font = ShopTheme.Face("Segoe UI", 12, FontStyle.Bold), ForeColor = ShopTheme.Green }, 1, 0);
        if (coLienKet)
        {
            var lienKet = new LinkLabel { Text = "Xem tất cả →", AutoSize = true, Anchor = AnchorStyles.Right, LinkBehavior = LinkBehavior.HoverUnderline,
                LinkColor = ShopTheme.Ink, ActiveLinkColor = ShopTheme.Current.Highlight, VisitedLinkColor = ShopTheme.Ink, AccessibleName = "Xem tất cả " + tieuDe };
            lienKet.LinkClicked += (_, _) => { if (KhungChinh is { } main) xemTatCa(main); };
            dau.Controls.Add(lienKet, 2, 0);
        }
        noiDung.Dock = DockStyle.Fill; khung.Controls.Add(noiDung); khung.Controls.Add(dau);
        return khung;
    }

    private FrmMain? KhungChinh => Parent?.FindForm() as FrmMain;

    // TV1: tai du lieu lan dau khi trang hien len
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode) return;
        KieuBangDon(); BoTri(); Tai();
    }

    // TV1: bang don gan day gon hon bang chuan: dong thap, khong ke soc, trang thai ve thanh nhan mau
    private void KieuBangDon()
    {
        orders.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        orders.RowTemplate.MinimumHeight = S(20); orders.RowTemplate.Height = S(36);
        orders.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
        orders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        orders.ColumnHeadersHeight = S(38);
        orders.DefaultCellStyle.SelectionBackColor = ShopTheme.AccentSoft;
        orders.AlternatingRowsDefaultCellStyle.BackColor = ShopTheme.Surface;
        orders.ScrollBars = ScrollBars.Vertical; orders.AllowUserToResizeRows = false;
        orders.CellPainting += VeNhanTrangThai;
        orders.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) KhungChinh?.MoTrangDonHang(); };
    }

    // TV1: ve o trang thai thanh nhan bo goc mau theo trang thai
    private void VeNhanTrangThai(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics == null || orders.Columns[e.ColumnIndex].Name != "TrangThai") return;
        var text = Convert.ToString(e.Value) ?? "";
        var (chu, nen) = text switch
        {
            "Chờ xác nhận" => (ShopTheme.Warn, ShopTheme.WarnBg),
            "Đã hủy" => (ShopTheme.Danger, ShopTheme.DangerBg),
            "Hoàn thành" => (MauTongQuan.Tang, Ve.Tron(ShopTheme.Surface, MauTongQuan.Tang, 0.14f)),
            _ => (ShopTheme.Green, ShopTheme.AccentSoft)
        };
        e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.SelectionBackground | DataGridViewPaintParts.Border);
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        var font = ShopTheme.Face("Segoe UI", 9);
        int rong = Math.Min(e.CellBounds.Width - S(8), Ve.Rong(g, text, font) + S(18)), cao = S(25);
        var o = new Rectangle(e.CellBounds.X + S(6), e.CellBounds.Y + (e.CellBounds.Height - cao) / 2, rong, cao);
        Ve.Hop(g, o, S(8), nen);
        Ve.Chu(g, text, font, o, chu, TextFormatFlags.HorizontalCenter);
        e.Handled = true;
    }

    // TV1: tai so lieu theo ky dang chon, khong chan giao dien
    private async void Tai()
    {
        if (!PhienDangNhap.CoQuyen(ChucNang.DonHangPOS)) return;
        bool report = PhienDangNhap.CoQuyen(ChucNang.ThongKeBinhLuan), product = PhienDangNhap.CoQuyen(ChucNang.SanPhamDanhMucKhuyenMai);
        var today = DateTime.Today; int soNgay = SoNgayChon[Math.Max(0, kyXem.SelectedIndex)];
        await ShopTheme.Load(this, () => TongQuanService.Tai(today, report, product, soNgay), data =>
        {
            GanThe(data, report, product);
            bieuDo.Gan(data.TheoNgay, data.TongKy, data.TongKyTruoc, report);
            trangThai.Gan(data.TrangThaiDon, soNgay);
            GanDonGanDay(data.DonGanDay);
            sapHet = product ? data.SapHet : null; VeDanhSachTon(product);
        });
    }

    // TV1: dien bon the so lieu va dong so sanh
    private void GanThe(TongQuanSnapshot d, bool report, bool product)
    {
        var mo = ShopTheme.Muted;
        if (!report) theDoanhThu.Gan("Theo quyền", "Cần quyền thống kê", mo);
        else
        {
            var (ghiChu, mau) = d.DoanhThuHomQua == 0 ? ("Hôm qua chưa có doanh thu", mo)
                : d.DoanhThu >= d.DoanhThuHomQua ? ($"↑ {(d.DoanhThu - d.DoanhThuHomQua) / d.DoanhThuHomQua * 100:0.#}% so với hôm qua", MauTongQuan.Tang)
                : ($"↓ {(d.DoanhThuHomQua - d.DoanhThu) / d.DoanhThuHomQua * 100:0.#}% so với hôm qua", ShopTheme.Danger);
            theDoanhThu.Gan($"{d.DoanhThu:N0} đ", ghiChu, mau, d.TheoNgay.Select(x => x.DoanhThu), ShopTheme.Green);
        }
        theCho.Gan(d.ChoXacNhan.ToString("N0"), d.ChoMoiHomNay > 0 ? $"↑ +{d.ChoMoiHomNay} đơn mới hôm nay" : "Hôm nay không có đơn mới",
            d.ChoMoiHomNay > 0 ? ShopTheme.Current.Highlight : mo, d.TheoNgay.Select(x => (decimal)x.DonCho), ShopTheme.Current.Highlight);
        if (!product) { theDangBan.Gan("Theo quyền", "Cần quyền sản phẩm", mo); theTon.Gan("Theo quyền", "Cần quyền sản phẩm", mo); return; }
        theDangBan.Gan(d.DangBan.ToString("N0"), $"Trên tổng {d.TongSanPham:N0} sản phẩm", mo);
        int het = d.SapHet.AsEnumerable().Count(r => r.Field<int>("SoLuongTon") == 0), sap = d.SapHet.Rows.Count - het;
        theTon.Gan(d.SapHet.Rows.Count.ToString("N0"), d.SapHet.Rows.Count == 0 ? "Không có cảnh báo" : $"{het} hết hàng • {sap} sắp hết",
            d.SapHet.Rows.Count == 0 ? mo : ShopTheme.Danger);
    }

    // TV1: gan bang don gan day, chi giu bon cot can xem
    private void GanDonGanDay(DataTable don)
    {
        orders.DataSource = don;
        foreach (DataGridViewColumn c in orders.Columns)
        {
            c.Visible = c.Name is "MaHD" or "KhachHang" or "TongTien" or "TrangThai";
            c.DefaultCellStyle.WrapMode = DataGridViewTriState.False; c.SortMode = DataGridViewColumnSortMode.NotSortable;
            if (!c.Visible) continue;
            (c.HeaderText, c.FillWeight, c.MinimumWidth) = c.Name switch
            {
                "MaHD" => ("Mã đơn", 55f, S(70)), "KhachHang" => ("Khách hàng", 150f, S(110)),
                "TongTien" => ("Tổng tiền", 95f, S(105)), _ => ("Trạng thái", 100f, S(122))
            };
            if (c.Name == "MaHD") { c.DefaultCellStyle.Format = "'#'0"; c.DefaultCellStyle.Font = ShopTheme.Face("Segoe UI", 10, FontStyle.Bold); }
        }
        orders.ClearSelection();
    }

    // TV1: dung lai danh sach san pham sap het, moi dong co anh, ton kho va nut nhap hang
    private void VeDanhSachTon(bool coQuyen)
    {
        dsTon.SuspendLayout();
        while (dsTon.Controls.Count > 0) dsTon.Controls[0].Dispose();
        foreach (var anh in anhDangDung) anh.Dispose();
        anhDangDung.Clear();
        if (!coQuyen || sapHet == null || sapHet.Rows.Count == 0)
        {
            dsTon.Controls.Add(new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = ShopTheme.Muted,
                Text = coQuyen ? "Không có sản phẩm cần bổ sung." : "Danh sách chỉ hiển thị cho tài khoản có quyền sản phẩm." });
        }
        else
        {
            bool nhap = PhienDangNhap.CoQuyen(ChucNang.NhaCungCapPhieuNhap);
            // Dock Top xep nguoc thu tu them, nen them tu dong cuoi; gioi han so dong de trang nhe
            foreach (var row in sapHet.AsEnumerable().Take(20).Reverse()) dsTon.Controls.Add(DongTon(row, nhap));
        }
        dsTon.ResumeLayout();
    }

    // TV1: mot dong san pham sap het
    private Control DongTon(DataRow row, bool nhap)
    {
        int ma = row.Field<int>("MaSP"), ton = row.Field<int>("SoLuongTon");
        string ten = row.Field<string>("TenSP")!;
        var dong = new TableLayoutPanel { Dock = DockStyle.Top, Height = S(76), ColumnCount = 3, RowCount = 2, BackColor = ShopTheme.Surface, Padding = new Padding(0, S(6), 0, S(7)) };
        dong.Paint += (_, e) => { using var but = new Pen(ShopTheme.Line); e.Graphics.DrawLine(but, 0, dong.Height - 1, dong.Width, dong.Height - 1); };
        dong.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, S(74))); dong.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); dong.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        dong.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); dong.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        var hinh = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(S(2), 0, S(10), 0) };
        hinh.Image = TaiAnh(row.Field<string?>("HinhAnh"));
        dong.Controls.Add(hinh, 0, 0); dong.SetRowSpan(hinh, 2);
        dong.Controls.Add(new Label { Text = ten, Dock = DockStyle.Fill, AutoEllipsis = true, UseMnemonic = false, TextAlign = ContentAlignment.BottomLeft,
            Font = ShopTheme.Face("Segoe UI", 10, FontStyle.Bold), ForeColor = ShopTheme.Ink }, 1, 0);
        dong.Controls.Add(new Label { Text = ton == 0 ? "Hết hàng" : $"Tồn kho: {ton}", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopLeft, ForeColor = ShopTheme.Danger }, 1, 1);
        if (nhap)
        {
            var nut = Sv4Ui.Nut("＋  Nhập hàng", () => NhapHang(ma)); nut.Anchor = AnchorStyles.Right; nut.AccessibleName = "Nhập hàng " + ten;
            dong.Controls.Add(nut, 2, 0); dong.SetRowSpan(nut, 2);
        }
        return dong;
    }

    // TV1: doc anh san pham, thieu hoac hong thi de trong
    private Image? TaiAnh(string? khoa)
    {
        try
        {
            var path = KhoAnhSanPham.DuongDanDayDu(khoa);
            if (path == null || !File.Exists(path)) return null;
            var anh = AnhSanPham.Doc(path); anhDangDung.Add(anh); return anh;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException) { return null; }
    }

    // TV1: mo phieu nhap moi chon san san pham, luu xong thi nap lai so lieu
    private void NhapHang(int maSP)
    {
        if (!QuyenForm.DuQuyen(ChucNang.NhaCungCapPhieuNhap)) return;
        // Mo giong nut Lap phieu o trang Phieu nhap; ap theme vao form Designer nay se lam lech bo cuc
        using var frm = new FrmPhieuNhap { MaSPChonSan = maSP };
        if (frm.ShowDialog(this) == DialogResult.OK) Tai();
    }

    // TV1: giai phong anh da nap khi dong trang
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) { foreach (var anh in anhDangDung) anh.Dispose(); anhDangDung.Clear(); }
    }
}
