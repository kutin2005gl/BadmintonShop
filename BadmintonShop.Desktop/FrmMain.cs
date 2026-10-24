using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

/// <summary>
/// Form chinh Desktop - menu an/hien theo vai tro (SV1 lam khung + phan quyen, SV4 hoan thien giao dien).
/// Them form moi: doi "Click += mnuChuaCo_Click" cua menu tuong ung thanh handler mo form cua minh.
/// </summary>
public partial class FrmMain : Form
{
    // SV4: tích hợp đúng nhóm quyền; form con kiểm tra lại trong OnLoad và khi thao tác.
    private void mnuDsSanPham_Click(object? s,EventArgs e)=>MoForm<FrmSanPham>(ChucNang.SanPhamDanhMucKhuyenMai);
    private void mnuDanhMuc_Click(object? s,EventArgs e)=>MoForm<FrmDanhMuc>(ChucNang.SanPhamDanhMucKhuyenMai);
    private void mnuPOS_Click(object? s,EventArgs e)=>MoForm<FrmPOS>(ChucNang.DonHangPOS);
    private void mnuDonHang_Click(object? s,EventArgs e)=>MoForm<FrmDonHang>(ChucNang.DonHangPOS);
    private void mnuThongKe_Click(object? s,EventArgs e)=>MoForm<FrmThongKe>(ChucNang.ThongKeBinhLuan);
    /// <summary>true = nguoi dung bam Dang xuat -> Program.cs hien lai FrmDangNhap.</summary>
    public bool YeuCauDangXuat { get; private set; }

    // TV1: khoi tao form chinh, hien thong tin nguoi dung va ap dung phan quyen menu
    public FrmMain()
    {
        InitializeComponent();
        lblNguoiDung.Text = $"Người dùng: {PhienDangNhap.HoTen} ({PhienDangNhap.TenDangNhap})";
        lblVaiTro.Text = $"   |   Vai trò: {PhanQuyen.TenVaiTro(PhienDangNhap.VaiTro)}";
        lblChao.Text = $"BADMINTON SHOP\nXin chào, {PhienDangNhap.HoTen}";
        ApDungPhanQuyen();
        TaoKhungMoi();
    }

    // TV1: an/hien menu theo bang phan quyen (ADMIN / QUANLY / NHANVIEN)
    private void ApDungPhanQuyen()
    {
        mnuNhanSu.Visible   = PhienDangNhap.CoQuyen(ChucNang.NhanVienTaiKhoan);
        mnuNhapHang.Visible = PhienDangNhap.CoQuyen(ChucNang.NhaCungCapPhieuNhap);
        mnuSanPham.Visible  = PhienDangNhap.CoQuyen(ChucNang.SanPhamDanhMucKhuyenMai);
        mnuBanHang.Visible  = PhienDangNhap.CoQuyen(ChucNang.DonHangPOS);
        mnuBaoCao.Visible   = PhienDangNhap.CoQuyen(ChucNang.ThongKeBinhLuan);
        mnuSaoLuu.Visible   = PhienDangNhap.CoQuyen(ChucNang.SaoLuuPhucHoi);
    }

    // TV1: mo form con (khong chan form chinh) neu du quyen, form da mo thi chi dua len truoc
    private void MoForm<T>(ChucNang chucNang) where T : Form, new()
    {
        if (!QuyenForm.DuQuyen(chucNang)) return;
        HienTrang(typeof(T), () => new T());
    }

    // TV1: mo form quan ly nhan vien va tai khoan (chi ADMIN thay menu)
    private void mnuNhanVien_Click(object? sender, EventArgs e) => MoForm<FrmNhanVien>(ChucNang.NhanVienTaiKhoan);

    // TV1: mo form quan ly nha cung cap
    private void mnuNhaCungCap_Click(object? sender, EventArgs e) => MoForm<FrmNhaCungCap>(ChucNang.NhaCungCapPhieuNhap);

    // TV1: mo form danh sach phieu nhap (tu day lap phieu moi)
    private void mnuPhieuNhap_Click(object? sender, EventArgs e) => MoForm<FrmDanhSachPhieuNhap>(ChucNang.NhaCungCapPhieuNhap);

    // TV1: mo form doi mat khau cua chinh minh
    private void mnuDoiMatKhau_Click(object? sender, EventArgs e)
    {
        using var frm = new FrmDoiMatKhau();
        ShopTheme.Apply(frm);
        frm.ShowDialog(this);
    }

    // TV1: dang xuat - xoa phien va quay ve man hinh dang nhap
    private void mnuDangXuat_Click(object? sender, EventArgs e)
    {
        if (MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Đăng xuất",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        if (!ChoPhepDong()) return;
        daXacNhanDong = true;
        YeuCauDangXuat = true;
        PhienDangNhap.DangXuat();
        Close();
    }

    // TV1: mo form sao luu - phuc hoi CSDL (chi ADMIN thay menu)
    private void mnuSaoLuu_Click(object? sender, EventArgs e)
    {
        if (!QuyenForm.DuQuyen(ChucNang.SaoLuuPhucHoi)) return;
        using var dialog = new FrmSaoLuu(); ShopTheme.Apply(dialog); dialog.ShowDialog(this);
    }

    // TV1: dang xuat khong hoi lai (vd. sau khi phuc hoi CSDL, tai khoan co the da thay doi)
    public void DangXuatBatBuoc()
    {
        daXacNhanDong = true;
        YeuCauDangXuat = true;
        PhienDangNhap.DangXuat();
        Close();
    }

    // TV1: thoat ung dung
    private void mnuThoat_Click(object? sender, EventArgs e) => Close();

    // TV1: thong bao tam cho menu chua co form (cac SV thay bang handler mo form that)
    private void mnuChuaCo_Click(object? sender, EventArgs e)
    {
        var ten = (sender as ToolStripItem)?.Text?.Replace("&&", "&") ?? "";
        MessageBox.Show($"Chức năng \"{ten}\" đang được phát triển.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
