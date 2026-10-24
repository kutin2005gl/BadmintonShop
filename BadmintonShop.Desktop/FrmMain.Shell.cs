using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public partial class FrmMain
{
    private readonly Dictionary<Type, Form> pages = new();
    private readonly Dictionary<Type, Button> navigation = new();
    private Panel workspace = null!;
    private Label screenTitle = null!;
    private Form? currentPage;
    private bool daXacNhanDong;

    private void TaoKhungMoi()
    {
        Controls.Remove(lblChao); Controls.Remove(menuStrip); Controls.Remove(statusStrip);
        MainMenuStrip = null;
        Font = new Font("Segoe UI", 10); BackColor = ShopTheme.Background;
        MinimumSize = new Size(900, 600);
        workspace = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = ShopTheme.Background };
        workspace.Resize += (_, _) => BoTriTrang();
        var header = new TableLayoutPanel { Dock = DockStyle.Top, Height = 76, BackColor = Color.White, ColumnCount = 3, Padding = new Padding(20, 8, 12, 8) };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        screenTitle = ShopTheme.Title("Tổng quan", 18); screenTitle.Dock = DockStyle.Fill; screenTitle.AutoSize = false; screenTitle.TextAlign = ContentAlignment.MiddleLeft;
        var user = new Label { AutoSize = true, Anchor = AnchorStyles.Right, Text = $"{PhienDangNhap.HoTen}\n{PhanQuyen.TenVaiTro(PhienDangNhap.VaiTro)}", ForeColor = ShopTheme.Muted, Margin = new Padding(8) };
        var logout = Sv4Ui.Nut("Đăng xuất", () => mnuDangXuat_Click(this, EventArgs.Empty)); logout.Anchor = AnchorStyles.Right;
        header.Controls.Add(screenTitle, 0, 0); header.Controls.Add(user, 1, 0); header.Controls.Add(logout, 2, 0);
        var side = new Panel { Dock = DockStyle.Left, Width = 210, BackColor = ShopTheme.Green };
        var links = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(8) };
        var brand = new Label { Text = "BADMINTON\nSHOP  /  QUẢN LÝ", Dock = DockStyle.Top, Height = 90, ForeColor = Color.White, Padding = new Padding(18, 18, 8, 8), Font = new Font("Segoe UI", 14, FontStyle.Bold) };
        void Group(string text) => links.Controls.Add(new Label { Text = text, ForeColor = Color.FromArgb(165, 204, 183), AutoSize = true, Margin = new Padding(12, 18, 0, 8), Font = new Font("Segoe UI", 9, FontStyle.Bold) });
        void Link(string text, Action action, Type? type = null)
        {
            var button = new Button { Text = "  " + text, Width = 174, Height = 40, TextAlign = ContentAlignment.MiddleLeft, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = ShopTheme.Green, Cursor = Cursors.Hand, Margin = new Padding(2) };
            button.FlatAppearance.BorderSize = 0; button.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 110, 82);
            button.Click += (_, _) => action(); links.Controls.Add(button); if (type != null) navigation[type] = button;
        }
        Group("CỬA HÀNG"); Link("◈   Tổng quan", () => HienTrang(typeof(FrmTongQuan), () => new FrmTongQuan()), typeof(FrmTongQuan));
        if (PhienDangNhap.CoQuyen(ChucNang.SanPhamDanhMucKhuyenMai))
        {
            Link("▦   Sản phẩm", () => mnuDsSanPham_Click(this, EventArgs.Empty), typeof(FrmSanPham));
            Link("▤   Danh mục", () => mnuDanhMuc_Click(this, EventArgs.Empty), typeof(FrmDanhMuc));
            Link("◇   Khuyến mãi", () => mnuKhuyenMai.PerformClick());
        }
        if (PhienDangNhap.CoQuyen(ChucNang.DonHangPOS))
        {
            Group("BÁN HÀNG"); Link("▤   Đơn hàng", () => mnuDonHang_Click(this, EventArgs.Empty), typeof(FrmDonHang));
            Link("▣   Bán tại quầy", () => mnuPOS_Click(this, EventArgs.Empty), typeof(FrmPOS));
        }
        if (PhienDangNhap.CoQuyen(ChucNang.NhaCungCapPhieuNhap))
        {
            Group("NHẬP HÀNG"); Link("▥   Phiếu nhập", () => mnuPhieuNhap_Click(this, EventArgs.Empty), typeof(FrmDanhSachPhieuNhap));
            Link("◇   Nhà cung cấp", () => mnuNhaCungCap_Click(this, EventArgs.Empty), typeof(FrmNhaCungCap));
        }
        if (PhienDangNhap.CoQuyen(ChucNang.ThongKeBinhLuan))
        {
            Group("BÁO CÁO"); Link("▥   Thống kê", () => mnuThongKe_Click(this, EventArgs.Empty), typeof(FrmThongKe));
            Link("▤   Duyệt bình luận", () => mnuDuyetBinhLuan.PerformClick());
        }
        Group("HỆ THỐNG");
        if (PhienDangNhap.CoQuyen(ChucNang.NhanVienTaiKhoan)) Link("◇   Nhân viên, tài khoản", () => mnuNhanVien_Click(this, EventArgs.Empty), typeof(FrmNhanVien));
        Link("◇   Đổi mật khẩu", () => mnuDoiMatKhau_Click(this, EventArgs.Empty));
        if (PhienDangNhap.CoQuyen(ChucNang.SaoLuuPhucHoi)) Link("▤   Sao lưu, phục hồi", () => mnuSaoLuu_Click(this, EventArgs.Empty));
        Link("↪   Thoát", Close);
        side.Controls.Add(links); side.Controls.Add(brand);
        Controls.Add(workspace); Controls.Add(header); Controls.Add(side);
        Shown += (_, _) => HienTrang(typeof(FrmTongQuan), () => new FrmTongQuan());
        FormClosing += (_, e) => { if (!daXacNhanDong && !ChoPhepDong()) e.Cancel = true; };
    }
    private bool ChoPhepDong()
    {
        if (pages.Values.Any(ShopTheme.IsBusy)) { MessageBox.Show(this, "Có tác vụ đang chạy. Vui lòng chờ hoàn tất trước khi đóng hoặc đăng xuất."); return false; }
        return !pages.Values.OfType<IUnsavedWork>().Any(x => x.HasUnsavedWork) || Sv4Ui.XacNhan(this, "Có dữ liệu chưa lưu hoặc giỏ hàng chưa thanh toán. Đóng ứng dụng và bỏ dữ liệu này?");
    }
    private void HienTrang(Type type, Func<Form> create)
    {
        if (!pages.TryGetValue(type, out var page) || page.IsDisposed)
        {
            page = create(); pages[type] = page;
            ShopTheme.Apply(page); page.TopLevel = false; page.FormBorderStyle = FormBorderStyle.None;
            page.AutoScroll = true; workspace.Controls.Add(page);
        }
        currentPage?.Hide(); currentPage = page;
        screenTitle.Text = type == typeof(FrmPOS) ? "Bán tại quầy" : page.Text;
        screenTitle.AutoEllipsis = true;
        foreach (var (key, button) in navigation) button.BackColor = key == type ? Color.FromArgb(49, 122, 90) : ShopTheme.Green;
        BoTriTrang(); page.Show(); page.BringToFront();
    }
    private void BoTriTrang()
    {
        if (currentPage == null) return;
        workspace.AutoScrollPosition = Point.Empty;
        var min = currentPage.MinimumSize;
        currentPage.Bounds = new Rectangle(0, 0, Math.Max(workspace.ClientSize.Width - 2, Math.Max(780, min.Width)), Math.Max(workspace.ClientSize.Height - 2, Math.Max(540, min.Height)));
    }
}
