using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public partial class FrmMain
{
    private sealed record NavItem(string Group, string Glyph, string Text, Action Action, Type? Page);

    private readonly Dictionary<Type, Form> pages = new();
    private readonly Dictionary<Type, NavButton> navigation = new();
    private readonly List<NavItem> navItems = new();
    private Panel workspace = null!;
    private Label screenTitle = null!;
    private Form? currentPage;
    private Type? currentType;
    private bool daXacNhanDong;

    // TV1: dựng danh sách mục điều hướng theo quyền, rồi dựng khung
    private void TaoKhungMoi()
    {
        Controls.Remove(lblChao); Controls.Remove(menuStrip); Controls.Remove(statusStrip);
        MainMenuStrip = null;
        Font = new Font("Segoe UI", 10);
        MinimumSize = new Size(900, 600);
        workspace = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        workspace.Resize += (_, _) => BoTriTrang();
        Controls.Add(workspace);

        void Add(string group, string glyph, string text, Action action, Type? type = null) => navItems.Add(new NavItem(group, glyph, text, action, type));
        // Mã icon: Segoe MDL2 Assets (Home, Shop, Bullets, Tag, Cart, Document, People, Download, Package, Chart, Star, Contact, Lock, Restore, Power)
        Add("CỬA HÀNG", "", "Tổng quan", () => HienTrang(typeof(FrmTongQuan), () => new FrmTongQuan()), typeof(FrmTongQuan));
        if (PhienDangNhap.CoQuyen(ChucNang.SanPhamDanhMucKhuyenMai))
        {
            Add("CỬA HÀNG", "", "Sản phẩm", () => mnuDsSanPham_Click(this, EventArgs.Empty), typeof(FrmSanPham));
            Add("CỬA HÀNG", "", "Danh mục", () => mnuDanhMuc_Click(this, EventArgs.Empty), typeof(FrmDanhMuc));
            Add("CỬA HÀNG", "", "Khuyến mãi", () => mnuKhuyenMai.PerformClick(), typeof(FrmKhuyenMai));
        }
        if (PhienDangNhap.CoQuyen(ChucNang.DonHangPOS))
        {
            Add("BÁN HÀNG", "", "Đơn hàng", () => mnuDonHang_Click(this, EventArgs.Empty), typeof(FrmDonHang));
            Add("BÁN HÀNG", "", "Bán tại quầy", () => mnuPOS_Click(this, EventArgs.Empty), typeof(FrmPOS));
            Add("BÁN HÀNG", "", "Khách hàng", () => MoForm<FrmKhachHang>(ChucNang.DonHangPOS), typeof(FrmKhachHang));
        }
        if (PhienDangNhap.CoQuyen(ChucNang.NhaCungCapPhieuNhap))
        {
            Add("NHẬP HÀNG", "", "Phiếu nhập", () => mnuPhieuNhap_Click(this, EventArgs.Empty), typeof(FrmDanhSachPhieuNhap));
            Add("NHẬP HÀNG", "", "Nhà cung cấp", () => mnuNhaCungCap_Click(this, EventArgs.Empty), typeof(FrmNhaCungCap));
        }
        if (PhienDangNhap.CoQuyen(ChucNang.ThongKeBinhLuan))
        {
            Add("BÁO CÁO", "", "Thống kê", () => mnuThongKe_Click(this, EventArgs.Empty), typeof(FrmThongKe));
            // SV2: Quản lý đánh giá sản phẩm
            Add("BÁO CÁO", "", "Quản lý đánh giá", () => mnuQuanLyDanhGia_Click(this, EventArgs.Empty), typeof(FrmQuanLyDanhGia));
        }
        if (PhienDangNhap.CoQuyen(ChucNang.NhanVienTaiKhoan)) Add("HỆ THỐNG", "", "Nhân viên, tài khoản", () => mnuNhanVien_Click(this, EventArgs.Empty), typeof(FrmNhanVien));
        Add("HỆ THỐNG", "", "Đổi mật khẩu", () => mnuDoiMatKhau_Click(this, EventArgs.Empty));
        if (PhienDangNhap.CoQuyen(ChucNang.SaoLuuPhucHoi)) Add("HỆ THỐNG", "", "Sao lưu, phục hồi", () => mnuSaoLuu_Click(this, EventArgs.Empty));
        Add("HỆ THỐNG", "", "Thoát", Close);

        Shown += (_, _) => HienTrang(typeof(FrmTongQuan), () => new FrmTongQuan());
        FormClosing += (_, e) => { if (!daXacNhanDong && !ChoPhepDong()) e.Cancel = true; };
        DungKhung();
    }

    // TV1: mo trang don hang tu lien ket tren trang tong quan
    internal void MoTrangDonHang() => mnuDonHang_Click(this, EventArgs.Empty);

    // TV1: mo trang san pham tu lien ket tren trang tong quan
    internal void MoTrangSanPham() => mnuDsSanPham_Click(this, EventArgs.Empty);

    // TV1: dựng khung chính: thanh điều hướng bên trái và thanh trên
    private void DungKhung()
    {
        var t = ShopTheme.Current;
        BackColor = t.Bg; ForeColor = t.Ink; workspace.BackColor = t.Bg;

        // thanh trên: tiêu đề màn hình, người dùng, đăng xuất
        var header = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = t.TopBg };
        header.Paint += (_, e) => { using var p = new Pen(t.Line); e.Graphics.DrawLine(p, 0, header.Height - 1, header.Width, header.Height - 1); };
        var bar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, BackColor = t.TopBg, Padding = new Padding(24, 6, 14, 6) };
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); bar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); bar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var sub = Mix(t.TopInk, t.TopBg, 0.38f);
        screenTitle = new Label { AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true, BackColor = t.TopBg,
            Font = ShopTheme.TitleFace(18), ForeColor = t.Accent, Text = "Tổng quan" };
        var user = new Label { AutoSize = true, Anchor = AnchorStyles.Right, Text = $"{PhienDangNhap.HoTen}\n{PhanQuyen.TenVaiTro(PhienDangNhap.VaiTro)}", ForeColor = sub, BackColor = t.TopBg, TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(8, 0, 12, 0) };
        var logout = Sv4Ui.Nut("Đăng xuất", () => mnuDangXuat_Click(this, EventArgs.Empty)); logout.Anchor = AnchorStyles.Right;
        bar.Controls.Add(screenTitle, 0, 0); bar.Controls.Add(user, 1, 0); bar.Controls.Add(logout, 2, 0);
        header.Controls.Add(bar);

        DungThanhBen(header, t);
        Controls.SetChildIndex(workspace, 0);
    }

    // TV1: thanh điều hướng dọc bên trái
    private void DungThanhBen(Panel header, ThemeDef t)
    {
        var side = new Panel { Dock = DockStyle.Left, Width = 232, BackColor = t.NavBg };
        side.Paint += (_, e) => { using var p = new Pen(t.Line); e.Graphics.DrawLine(p, side.Width - 1, 0, side.Width - 1, side.Height); };
        var clip = new Panel { Dock = DockStyle.Fill, BackColor = t.NavBg };
        var links = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = t.NavBg, Padding = new Padding(8, 0, 8, 8) };
        // Thanh cuộn nằm ngoài vùng nhìn thấy (bị cắt); cuộn bằng con lăn chuột, nên nút không bị che khi cửa sổ thấp
        void SizeLinks() => links.SetBounds(0, 0, clip.Width + SystemInformation.VerticalScrollBarWidth + 2, clip.Height);
        clip.Resize += (_, _) => SizeLinks(); clip.Controls.Add(links); SizeLinks();
        links.MouseEnter += (_, _) => links.Focus();
        var brand = new ChuThuongHieu { Dock = DockStyle.Top };
        string? last = null;
        foreach (var item in navItems)
        {
            if (item.Group != last)
            {
                last = item.Group;
                links.Controls.Add(new Label { Text = item.Group, AutoSize = true, ForeColor = t.NavMuted, BackColor = t.NavBg, Margin = new Padding(14, 12, 0, 4), Font = ShopTheme.Face("Segoe UI", 8.5f, FontStyle.Bold) });
            }
            var b = new NavButton(item.Glyph, item.Text) { Width = 196, Height = 38 };
            b.Click += (_, _) => item.Action();
            links.Controls.Add(b); if (item.Page != null) navigation[item.Page] = b;
        }
        // Co chiều cao nút theo chiều cao cửa sổ để thanh điều hướng không phải cuộn (tối thiểu 34px)
        void Fit()
        {
            var buttons = links.Controls.OfType<NavButton>().ToList(); if (buttons.Count == 0) return;
            int fixedPart = links.Controls.Cast<Control>().Where(c => c is not NavButton).Sum(c => c.Height + c.Margin.Vertical) + links.Padding.Vertical;
            int per = (links.ClientSize.Height - fixedPart) / buttons.Count - buttons[0].Margin.Vertical;
            foreach (var b in buttons) b.Height = Math.Clamp(per, 34, 38);
        }
        links.Resize += (_, _) => Fit();
        side.Controls.Add(clip); side.Controls.Add(brand);
        Controls.Add(header); Controls.Add(side);
    }

    // TV1: tô sáng mục điều hướng của trang đang xem
    private void MarkActive()
    {
        foreach (var (page, button) in navigation) button.Active = page == currentType;
    }

    // TV1: trộn hai màu theo tỉ lệ, dùng cho chữ phụ trên thanh trên
    private static Color Mix(Color a, Color b, float toB) => Color.FromArgb((int)(a.R + (b.R - a.R) * toB), (int)(a.G + (b.G - a.G) * toB), (int)(a.B + (b.B - a.B) * toB));

    private bool ChoPhepDong()
    {
        if (pages.Values.Any(ShopTheme.IsBusy)) { MessageBox.Show(this, "Có tác vụ đang chạy. Vui lòng chờ hoàn tất trước khi đóng hoặc đăng xuất."); return false; }
        return !pages.Values.OfType<IUnsavedWork>().Any(x => x.HasUnsavedWork) || Sv4Ui.XacNhan(this, "Có dữ liệu chưa lưu hoặc giỏ hàng chưa thanh toán. Đóng ứng dụng và bỏ dữ liệu này?");
    }
    private void HienTrang(Type type, Func<Form> create)
    {
        if(IsDisposed || Disposing)return;
        LifecycleDiagnostics.Record("NAV begin "+type.Name);
        if (!pages.TryGetValue(type, out var page) || page.IsDisposed)
        {
            page = create(); pages[type] = page;
            var ownedPage=page;
            void RemoveClosed(object? sender,EventArgs args)
            {
                if(pages.TryGetValue(type,out var cached) && ReferenceEquals(cached,ownedPage))pages.Remove(type);
                if(ReferenceEquals(currentPage,ownedPage))currentPage=null;
            }
            page.FormClosed+=RemoveClosed;page.Disposed+=RemoveClosed;
            ShopTheme.Apply(page); page.TopLevel = false; page.FormBorderStyle = FormBorderStyle.None;
            page.AutoScroll = true; workspace.Controls.Add(page);
        }
        if(currentPage is { IsDisposed:false,Disposing:false })currentPage.Hide(); currentPage = page; currentType = type;
        screenTitle.Text = type == typeof(FrmPOS) ? "Bán tại quầy" : page.Text;
        screenTitle.AutoEllipsis = true;
        MarkActive();
        BoTriTrang(); page.Show(); page.BringToFront();
        LifecycleDiagnostics.Record("NAV shown "+type.Name);
    }
    private void BoTriTrang()
    {
        if (IsDisposed || Disposing || currentPage == null || currentPage.IsDisposed || currentPage.Disposing) return;
        workspace.AutoScrollPosition = Point.Empty;
        var min = currentPage.MinimumSize;
        // Tinh theo kich thuoc ca vung (khong theo ClientSize) de thanh cuon cu khong bi ket lai khi thu nho cua so
        int canRong = Math.Max(780, min.Width), canCao = Math.Max(540, min.Height);
        int truDoc = canCao > workspace.Height - 2 ? SystemInformation.VerticalScrollBarWidth : 0;
        int truNgang = canRong > workspace.Width - 2 ? SystemInformation.HorizontalScrollBarHeight : 0;
        currentPage.Bounds = new Rectangle(0, 0, Math.Max(workspace.Width - 2 - truDoc, canRong), Math.Max(workspace.Height - 2 - truNgang, canCao));
        workspace.PerformLayout();
    }
}
