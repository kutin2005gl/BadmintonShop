using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

/// <summary>
/// Man hinh dang nhap nhan vien (SV1 - ROADMAP 2.1).
/// DialogResult.OK = dang nhap thanh cong, thong tin nguoi dung nam trong PhienDangNhap.
/// </summary>
public partial class FrmDangNhap : Form
{
    private readonly TaiKhoanRepository _taiKhoanRepo = new();

    // TV1: khoi tao form dang nhap
    public FrmDangNhap()
    {
        LifecycleDiagnostics.Track(this);
        InitializeComponent();
        DungGiaoDien();
    }

    // TV1: dung giao dien dang nhap cung bo mau voi khung chinh (thanh ben trai + the noi dung)
    private void DungGiaoDien()
    {
        var t = ShopTheme.Current;
        Font = new Font("Segoe UI", 10);
        BackColor = t.Bg; ForeColor = t.Ink;

        // ---- Cot thuong hieu ben trai, giong thanh dieu huong cua FrmMain ----
        var side = new Panel { Dock = DockStyle.Left, Width = 290, BackColor = t.NavBg, Padding = new Padding(28, 30, 24, 22) };
        side.Paint += (_, e) => { using var p = new Pen(t.Line); e.Graphics.DrawLine(p, side.Width - 1, 0, side.Width - 1, side.Height); };
        var brand = new Label { Dock = DockStyle.Top, Height = 96, BackColor = t.NavBg, ForeColor = t.NavInk, Font = ShopTheme.TitleFace(22), Text = "BADMINTON\nSHOP" };
        var slogan = new Label { Dock = DockStyle.Top, Height = 46, BackColor = t.NavBg, ForeColor = t.NavMuted, Text = "Hệ thống quản lý cửa hàng\ncầu lông dành cho nhân viên" };
        var group = new Label { Dock = DockStyle.Top, Height = 40, BackColor = t.NavBg, ForeColor = t.NavMuted, TextAlign = ContentAlignment.BottomLeft,
            Font = ShopTheme.Face("Segoe UI", 8.5f, FontStyle.Bold), Text = "CHỨC NĂNG" };
        var features = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 170, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = t.NavBg, Padding = new Padding(0, 6, 0, 0) };
        // Ma icon Segoe MDL2 Assets giong thanh dieu huong: Cart, Shop, Package, Chart
        foreach (var (glyph, text) in new[] { ("", "Bán tại quầy, đơn hàng"), ("", "Sản phẩm, khuyến mãi"), ("", "Nhập hàng, nhà cung cấp"), ("", "Thống kê, báo cáo") })
            features.Controls.Add(DongChucNang(glyph, text, t));
        var footer = new Label { Dock = DockStyle.Bottom, Height = 24, BackColor = t.NavBg, ForeColor = t.NavMuted, Font = ShopTheme.Face("Segoe UI", 8.5f),
            TextAlign = ContentAlignment.BottomLeft, Text = $"© {DateTime.Now.Year} Badminton Shop" };
        side.Controls.Add(features); side.Controls.Add(group); side.Controls.Add(slogan); side.Controls.Add(brand); side.Controls.Add(footer);

        // ---- Vung noi dung ben phai: the dang nhap nam giua, giong cac trang ben trong ----
        var content = new Panel { Dock = DockStyle.Fill, BackColor = t.Bg };
        var card = new Panel { Size = new Size(380, 384), BackColor = t.Surface, Padding = new Padding(32, 26, 32, 24) };
        card.Paint += (_, e) => { using var p = new Pen(t.Line); e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1); };
        var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, BackColor = t.Surface };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        lblTieuDe.AutoSize = true; lblTieuDe.Font = ShopTheme.TitleFace(20); lblTieuDe.ForeColor = t.Accent; lblTieuDe.Margin = new Padding(0, 0, 0, 2);
        var phu = new Label { AutoSize = true, ForeColor = t.Muted, Text = "Nhập tài khoản nhân viên để tiếp tục.", Margin = new Padding(0, 0, 0, 18) };
        foreach (var lbl in new[] { lblTenDangNhap, lblMatKhau })
        { lbl.AutoSize = true; lbl.ForeColor = t.Ink; lbl.Font = ShopTheme.Face("Segoe UI", 9.5f, FontStyle.Bold); lbl.Margin = new Padding(0, 0, 0, 4); }
        foreach (var txt in new[] { txtTenDangNhap, txtMatKhau })
        { txt.Font = ShopTheme.Face("Segoe UI", 11); txt.Dock = DockStyle.Fill; txt.Margin = new Padding(0, 0, 0, 14); }
        chkHienMatKhau.ForeColor = t.Muted; chkHienMatKhau.Margin = new Padding(0, 0, 0, 16); chkHienMatKhau.Cursor = Cursors.Hand;

        var buttons = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true, BackColor = t.Surface, Margin = Padding.Empty };
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62)); buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        btnDangNhap.Anchor = AnchorStyles.Left | AnchorStyles.Right; btnDangNhap.Margin = new Padding(0, 0, 6, 0); btnDangNhap.Height = 40;
        btnThoat.Anchor = AnchorStyles.Left | AnchorStyles.Right; btnThoat.Margin = new Padding(6, 0, 0, 0); btnThoat.Height = 40;
        buttons.Controls.Add(btnDangNhap, 0, 0); buttons.Controls.Add(btnThoat, 1, 0);

        foreach (var c in new Control[] { lblTieuDe, phu, lblTenDangNhap, txtTenDangNhap, lblMatKhau, txtMatKhau, chkHienMatKhau, buttons })
        { form.RowStyles.Add(new RowStyle(SizeType.AutoSize)); form.Controls.Add(c, 0, form.RowCount++); }
        form.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); form.RowCount++; // hang dem: khoang trong thua khong keo gian nut
        card.Controls.Add(form);
        content.Controls.Add(card);
        content.Resize += (_, _) => card.Location = new Point(Math.Max(0, (content.ClientSize.Width - card.Width) / 2), Math.Max(0, (content.ClientSize.Height - card.Height) / 2));

        Controls.Add(content); Controls.Add(side);

        // Ap theme cho o nhap/nut nhu cac form khac, roi chinh lai nut chinh va vi tri cua so
        ShopTheme.Apply(this);
        // Kich thuoc o tren tinh theo 96 DPI; de WinForms tu nhan theo DPI man hinh (app chay PerMonitorV2)
        AutoScaleMode = AutoScaleMode.Dpi; AutoScaleDimensions = new SizeF(96F, 96F);
        StartPosition = FormStartPosition.CenterScreen;
        btnDangNhap.BackColor = t.Accent; btnDangNhap.ForeColor = t.OnAccent; btnDangNhap.FlatAppearance.BorderSize = 0;
        btnDangNhap.FlatAppearance.MouseOverBackColor = t.AccentHover; btnDangNhap.FlatAppearance.MouseDownBackColor = t.AccentDown;
        btnDangNhap.Font = ShopTheme.Face("Segoe UI", 10, FontStyle.Bold);
        chkHienMatKhau.BackColor = t.Surface;
        ActiveControl = txtTenDangNhap;
    }

    // TV1: mot dong icon + chu o cot trai, cung kieu voi nut dieu huong
    private static Control DongChucNang(string glyph, string text, ThemeDef t)
    {
        var row = new Panel { Width = 236, Height = 36, BackColor = t.NavBg, Margin = new Padding(0, 0, 0, 2) };
        row.Paint += (_, e) =>
        {
            var flags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
            int icon = row.LogicalToDeviceUnits(28), gap = row.LogicalToDeviceUnits(38);
            TextRenderer.DrawText(e.Graphics, glyph, ShopTheme.Face("Segoe MDL2 Assets", 12.5f), new Rectangle(0, 0, icon, row.Height), t.NavMuted, flags | TextFormatFlags.HorizontalCenter);
            TextRenderer.DrawText(e.Graphics, text, ShopTheme.Face("Segoe UI", 10), new Rectangle(gap, 0, row.Width - gap, row.Height), t.NavInk, flags);
        };
        return row;
    }

    // TV1: xu ly nut Dang nhap (Enter cung goi vi AcceptButton = btnDangNhap)
    private void btnDangNhap_Click(object? sender, EventArgs e)
    {
        var tenDN = txtTenDangNhap.Text.Trim();
        var matKhau = txtMatKhau.Text;
        if (tenDN.Length == 0) { BaoLoi("Vui lòng nhập tên đăng nhập.", txtTenDangNhap); return; }
        if (matKhau.Length == 0) { BaoLoi("Vui lòng nhập mật khẩu.", txtMatKhau); return; }

        if (!DbHelper.ThuKetNoi(out var loi))
        {
            MessageBox.Show("Không kết nối được cơ sở dữ liệu.\nKiểm tra SQL Server và chuỗi kết nối trong App.config.\n\nChi tiết: " + loi,
                            "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Cursor = Cursors.WaitCursor;
        KetQuaDangNhap ketQua;
        try { ketQua = _taiKhoanRepo.DangNhap(tenDN, matKhau); }
        catch (Exception ex)
        {
            MessageBox.Show(LoiCsdl.ThongBao(ex), "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        finally { Cursor = Cursors.Default; }

        switch (ketQua)
        {
            case KetQuaDangNhap.ThanhCong:
                DialogResult = DialogResult.OK;
                Close();
                break;
            case KetQuaDangNhap.BiKhoa:
                BaoLoi("Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.", txtTenDangNhap);
                break;
            default:
                BaoLoi("Sai tên đăng nhập hoặc mật khẩu.", txtMatKhau);
                break;
        }
    }

    // TV1: bat/tat hien ky tu mat khau
    private void chkHienMatKhau_CheckedChanged(object? sender, EventArgs e)
        => txtMatKhau.UseSystemPasswordChar = !chkHienMatKhau.Checked;

    // TV1: hien thong bao loi va dua con tro ve o can sua
    private void BaoLoi(string thongBao, TextBox oCanSua)
    {
        MessageBox.Show(thongBao, "Đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        if (oCanSua == txtMatKhau) txtMatKhau.Clear();
        oCanSua.Focus();
        oCanSua.SelectAll();
    }
}
