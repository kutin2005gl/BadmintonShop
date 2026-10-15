using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

/// <summary>
/// Form chinh Desktop - menu an/hien theo vai tro (SV1 lam khung + phan quyen, SV4 hoan thien giao dien).
/// Them form moi: doi "Click += mnuChuaCo_Click" cua menu tuong ung thanh handler mo form cua minh.
/// </summary>
public partial class FrmMain : Form
{
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

    // TV1: mo form con dang hop thoai, form da mo thi chi dua len truoc
    private void MoForm<T>() where T : Form, new()
    {
        var daMo = OwnedForms.OfType<T>().FirstOrDefault();
        if (daMo != null) { daMo.Activate(); return; }
        var frm = new T { Owner = this, StartPosition = FormStartPosition.CenterParent };
        frm.FormClosed += (_, _) => frm.Dispose();
        frm.Show(this);
    }

    // TV1: mo form doi mat khau cua chinh minh
    private void mnuDoiMatKhau_Click(object? sender, EventArgs e)
    {
        using var frm = new FrmDoiMatKhau();
        frm.ShowDialog(this);
    }

    // TV1: dang xuat - xoa phien va quay ve man hinh dang nhap
    private void mnuDangXuat_Click(object? sender, EventArgs e)
    {
        if (MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Đăng xuất",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
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
