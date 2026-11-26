namespace BadmintonShop.Desktop;

// Giao dien dung bang code (bo cuc co gian theo cua so va muc phong to man hinh), khong sua bang VS Designer.
partial class FrmPhieuNhap
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            foreach (var anh in _anhNho.Values) anh?.Dispose();
            _anhNho.Clear();
        }
        base.Dispose(disposing);
    }

    // TV1: doi so do thiet ke (96 dpi) sang diem anh man hinh
    private int S(int v) => LogicalToDeviceUnits(v);

    // TV1: dung toan bo control cua phieu nhap: tieu de, o nhap, nut, bang chi tiet va chan phieu
    private void InitializeComponent()
    {
        cboNCC = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, TabIndex = 0, AccessibleName = "Nhà cung cấp" };
        dtpNgayNhap = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", TabIndex = 1, AccessibleName = "Ngày nhập" };
        cboSanPham = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown, AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            AutoCompleteSource = AutoCompleteSource.ListItems, TabIndex = 2, AccessibleName = "Sản phẩm" };
        numSoLuong = new NumericUpDown { Minimum = 1, Maximum = 100000, Value = 1, ThousandsSeparator = true, TextAlign = HorizontalAlignment.Center,
            BorderStyle = BorderStyle.None, TabIndex = 3, AccessibleName = "Số lượng" };
        numDonGia = new NumericUpDown { Maximum = 1000000000, Increment = 1000, ThousandsSeparator = true, BorderStyle = BorderStyle.None, TabIndex = 4, AccessibleName = "Đơn giá nhập" };
        btnThemDong = new NutBieuTuong { Text = "Thêm vào phiếu", Glyph = "", Chinh = true, TabIndex = 5 };
        btnXoaDong = new NutBieuTuong { Text = "Xóa dòng đã chọn", Glyph = "", TabIndex = 6 };
        dgvChiTiet = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false,
            AutoGenerateColumns = false, ScrollBars = ScrollBars.Vertical, TabIndex = 7 };
        btnLuu = new NutBieuTuong { Text = "Lưu phiếu", Glyph = "", TabIndex = 8 };
        btnHuy = new NutBieuTuong { Text = "Đóng", Glyph = "", Chinh = true, TabIndex = 9, DialogResult = DialogResult.Cancel };
        lblNguoiLap = new Label { AutoSize = true, Anchor = AnchorStyles.Right, ForeColor = ShopTheme.Muted, Margin = Padding.Empty };
        lblTongTien = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Margin = Padding.Empty,
            Font = ShopTheme.Face("Segoe UI", 14, FontStyle.Bold), ForeColor = ShopTheme.Ink };
        lblSoDong = new Label { AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = ShopTheme.Muted, Margin = new Padding(0, 0, S(12), 0) };

        var goc = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(S(20), S(10), S(20), S(10)) };
        goc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        foreach (var cao in new[] { 52, 68, 68, 52, 0, 60 })
            goc.RowStyles.Add(cao == 0 ? new RowStyle(SizeType.Percent, 100) : new RowStyle(SizeType.Absolute, S(cao)));

        // Tieu de va nguoi lap
        var dau = Hang((SizeType.Absolute, S(40)), (SizeType.Percent, 100), (SizeType.AutoSize, 0));
        dau.Controls.Add(new Label { Text = "", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Margin = Padding.Empty,
            Font = ShopTheme.Face("Segoe MDL2 Assets", 18), ForeColor = ShopTheme.Green }, 0, 0);
        dau.Controls.Add(new Label { Text = "Lập phiếu nhập hàng", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Margin = Padding.Empty,
            Font = ShopTheme.TitleFace(15), ForeColor = ShopTheme.Green }, 1, 0);
        dau.Controls.Add(lblNguoiLap, 2, 0);

        var hang1 = Hang((SizeType.Percent, 50), (SizeType.Percent, 50));
        hang1.Controls.Add(Truong("Nhà cung cấp", new KhungNhap(cboNCC, ""), S(12)), 0, 0);
        hang1.Controls.Add(Truong("Ngày nhập", new KhungNhap(dtpNgayNhap, ""), 0), 1, 0);

        // So luong co nut tru / cong hai ben
        var soLuong = Hang((SizeType.Absolute, S(38)), (SizeType.Percent, 100), (SizeType.Absolute, S(38)));
        var tru = new NutBieuTuong { Text = "−", Dock = DockStyle.Fill, Margin = Padding.Empty, TabStop = false, AccessibleName = "Giảm số lượng" };
        var cong = new NutBieuTuong { Text = "+", Dock = DockStyle.Fill, Margin = Padding.Empty, TabStop = false, AccessibleName = "Tăng số lượng" };
        tru.Click += (_, _) => numSoLuong.Value = Math.Max(numSoLuong.Minimum, numSoLuong.Value - 1);
        cong.Click += (_, _) => numSoLuong.Value = Math.Min(numSoLuong.Maximum, numSoLuong.Value + 1);
        soLuong.Controls.Add(tru, 0, 0); soLuong.Controls.Add(new KhungNhap(numSoLuong) { Dock = DockStyle.Fill, Margin = new Padding(S(4), 0, S(4), 0) }, 1, 0); soLuong.Controls.Add(cong, 2, 0);

        var hang2 = Hang((SizeType.Percent, 50), (SizeType.Percent, 20), (SizeType.Percent, 30));
        hang2.Controls.Add(Truong("Sản phẩm", new KhungNhap(cboSanPham, ""), S(12)), 0, 0);
        hang2.Controls.Add(Truong("Số lượng", soLuong, S(12)), 1, 0);
        hang2.Controls.Add(Truong("Đơn giá nhập", new KhungNhap(numDonGia, "", "đ"), 0), 2, 0);

        var nut = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = Padding.Empty, Padding = new Padding(0, S(6), 0, 0) };
        nut.Controls.Add(btnThemDong); nut.Controls.Add(btnXoaDong);

        // Bang nam trong khung vien manh
        var khungBang = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1), Margin = new Padding(0, S(2), 0, 0) };
        khungBang.Paint += (_, e) => { using var but = new Pen(ShopTheme.Line); e.Graphics.DrawRectangle(but, 0, 0, khungBang.Width - 1, khungBang.Height - 1); };
        khungBang.Resize += (_, _) => khungBang.Invalidate();
        khungBang.Controls.Add(dgvChiTiet);

        var cuoi = Hang((SizeType.AutoSize, 0), (SizeType.AutoSize, 0), (SizeType.Percent, 100), (SizeType.Absolute, S(140)), (SizeType.Absolute, S(120)));
        lblTongTien.Dock = DockStyle.None; lblTongTien.AutoSize = true; lblTongTien.Anchor = AnchorStyles.Left; lblTongTien.Margin = new Padding(0, 0, S(10), 0);
        foreach (var b in new[] { btnLuu, btnHuy }) { b.Anchor = AnchorStyles.Left | AnchorStyles.Right; b.Margin = new Padding(S(10), 0, 0, 0); }
        cuoi.Controls.Add(lblTongTien, 0, 0); cuoi.Controls.Add(lblSoDong, 1, 0); cuoi.Controls.Add(btnLuu, 3, 0); cuoi.Controls.Add(btnHuy, 4, 0);

        goc.Controls.Add(dau, 0, 0); goc.Controls.Add(hang1, 0, 1); goc.Controls.Add(hang2, 0, 2);
        goc.Controls.Add(nut, 0, 3); goc.Controls.Add(khungBang, 0, 4); goc.Controls.Add(cuoi, 0, 5);
        Controls.Add(goc);

        Name = "FrmPhieuNhap"; Text = "Lập phiếu nhập hàng";
        ClientSize = new Size(S(920), S(600)); MinimumSize = new Size(S(820), S(540));
        CancelButton = btnHuy;
        ShopTheme.Apply(this);

        // Ap theme xong moi chinh rieng, neu khong kich thuoc va kieu bi theme ghi de
        foreach (var (b, rong) in new[] { (btnThemDong, 170), (btnXoaDong, 185), (btnLuu, 130), (btnHuy, 110) })
        { b.AutoSize = false; b.MinimumSize = Size.Empty; b.Size = new Size(S(rong), S(38)); }
        foreach (var b in new[] { btnThemDong, btnXoaDong }) b.Margin = new Padding(0, 0, S(10), 0);
        foreach (var b in new[] { tru, cong }) { b.AutoSize = false; b.MinimumSize = Size.Empty; }
        cboSanPham.FlatStyle = FlatStyle.Flat;
        foreach (var so in new[] { numSoLuong, numDonGia }) { so.BorderStyle = BorderStyle.None; so.Controls[0].Visible = false; }

        btnThemDong.Click += btnThemDong_Click;
        btnXoaDong.Click += btnXoaDong_Click;
        btnLuu.Click += btnLuu_Click;
        cboSanPham.SelectionChangeCommitted += cboSanPham_SelectionChangeCommitted;
        dgvChiTiet.CellValidating += dgvChiTiet_CellValidating;
        dgvChiTiet.CellEndEdit += dgvChiTiet_CellEndEdit;
        FormClosing += FrmPhieuNhap_FormClosing;
    }

    // TV1: tao mot hang nhieu cot khong le
    private static TableLayoutPanel Hang(params (SizeType Kieu, float Co)[] cot)
    {
        var hang = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = cot.Length, RowCount = 1, Margin = Padding.Empty };
        foreach (var (kieu, co) in cot) hang.ColumnStyles.Add(new ColumnStyle(kieu, co));
        hang.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        return hang;
    }

    // TV1: mot truong nhap gom nhan co dau sao bat buoc va o nhap ben duoi
    private Control Truong(string nhan, Control o, int lePhai)
    {
        var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(0, 0, lePhai, 0) };
        p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        p.RowStyles.Add(new RowStyle(SizeType.Absolute, S(24))); p.RowStyles.Add(new RowStyle(SizeType.Absolute, S(38)));
        p.Controls.Add(new Label { Text = nhan + " *", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Margin = Padding.Empty, ForeColor = ShopTheme.Ink,
            Font = ShopTheme.Face("Segoe UI", 10, FontStyle.Bold) }, 0, 0);
        o.Dock = DockStyle.Fill; o.Margin = Padding.Empty; p.Controls.Add(o, 0, 1);
        return p;
    }

    private Label lblNguoiLap, lblTongTien, lblSoDong;
    private ComboBox cboNCC, cboSanPham; private DateTimePicker dtpNgayNhap;
    private NumericUpDown numSoLuong, numDonGia;
    private NutBieuTuong btnThemDong, btnXoaDong, btnLuu, btnHuy; private DataGridView dgvChiTiet;
}
