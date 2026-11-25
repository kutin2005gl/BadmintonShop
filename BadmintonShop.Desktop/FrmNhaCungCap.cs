using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

/// <summary>Quan ly nha cung cap: tim kiem, them, sua, xoa (SV1 - ROADMAP 2.3).</summary>
public partial class FrmNhaCungCap : Form
{
    private readonly NhaCungCapRepository _repo = new();
    private int _maDangChon;   // 0 = dang o che do them moi
    private Label nhanMa = null!;

    // TV1: khoi tao form (chi giao dien, KHONG truy cap CSDL de VS Designer mo duoc)
    public FrmNhaCungCap()
    {
        InitializeComponent();
        DungBoCuc();
    }

    // Dựng bố cục main-sub giống Phiếu nhập: thanh tìm kiếm trên, danh sách trái, form nhập phải
    private void DungBoCuc()
    {
        var bar = Sv4Ui.ThanhCongCu();
        lblTim.AutoSize = true; lblTim.Margin = new Padding(5, 9, 5, 3);
        txtTimKiem.Width = 280; txtTimKiem.Margin = new Padding(4, 6, 4, 4);
        btnTim.AutoSize = true; btnTim.Margin = new Padding(4); ShopTheme.StyleButton(btnTim);
        bar.Controls.AddRange(new Control[] { lblTim, txtTimKiem, btnTim });

        dgvNCC.Dock = DockStyle.Fill;
        ShopTheme.StyleGrid(dgvNCC);
        var split = KhungChiTiet.Chia(this, 0.6, 420, 360);
        split.Panel1.Controls.Add(dgvNCC);

        var info = KhungChiTiet.LuoiO(
            ("Tên nhà cung cấp (*)", txtTen, true),
            ("Số điện thoại", txtSDT, false),
            ("Email", txtEmail, false),
            ("Địa chỉ", txtDiaChi, true));
        foreach (var b in new[] { btnThem, btnSua, btnXoa, btnLamMoi })
        { b.AutoSize = true; b.Margin = new Padding(0, 0, 8, 6); ShopTheme.StyleButton(b); }
        var root = KhungChiTiet.NenChiTiet();
        split.Panel2.BackColor = ShopTheme.SurfaceAlt;
        var header = KhungChiTiet.TieuDe(out nhanMa, out var badge);
        badge.Visible = false;
        // Thứ tự thêm: Top thêm sau nằm cao hơn
        root.Controls.Add(KhungChiTiet.HangNut(btnThem, btnSua, btnXoa, btnLamMoi));
        root.Controls.Add(KhungChiTiet.Cach(12));
        root.Controls.Add(KhungChiTiet.The("Thông tin nhà cung cấp", info));
        root.Controls.Add(header);
        split.Panel2.Controls.Add(root);

        Controls.Add(split); Controls.Add(bar);
        ShopTheme.Apply(this);
    }

    // TV1: khi form hien len - kiem tra quyen roi nap danh sach nha cung cap
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode || !QuyenForm.KiemTraKhiMo(this, ChucNang.NhaCungCapPhieuNhap)) return;
        NapLuoi();
        LamMoiForm();
    }

    // TV1: nap lai luoi theo tu khoa tim kiem
    private void NapLuoi()
    {
        try
        {
            dgvNCC.DataSource = _repo.LayTatCa(txtTimKiem.Text.Trim());
            dgvNCC.Columns["MaNCC"]!.HeaderText = "Mã";
            dgvNCC.Columns["TenNCC"]!.HeaderText = "Tên nhà cung cấp";
            dgvNCC.Columns["DiaChi"]!.HeaderText = "Địa chỉ";
            dgvNCC.Columns["SDT"]!.HeaderText = "Số điện thoại";
            dgvNCC.Columns["Email"]!.HeaderText = "Email";
            dgvNCC.ClearSelection();
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: xoa trang o nhap, chuyen ve che do them moi
    private void LamMoiForm()
    {
        _maDangChon = 0;
        nhanMa.Text = "Nhà cung cấp mới";
        txtTen.Clear(); txtDiaChi.Clear(); txtSDT.Clear(); txtEmail.Clear();
        btnThem.Enabled = true;
        btnSua.Enabled = btnXoa.Enabled = false;
        dgvNCC.ClearSelection();
        txtTen.Focus();
    }

    // TV1: chon 1 dong tren luoi thi do du lieu xuong o nhap
    private void dgvNCC_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvNCC.CurrentRow?.DataBoundItem is not NhaCungCap n || !dgvNCC.CurrentRow.Selected) return;
        _maDangChon = n.MaNCC;
        nhanMa.Text = $"Nhà cung cấp {n.MaNCC}";
        txtTen.Text = n.TenNCC;
        txtDiaChi.Text = n.DiaChi ?? "";
        txtSDT.Text = n.SDT ?? "";
        txtEmail.Text = n.Email ?? "";
        btnThem.Enabled = false;
        btnSua.Enabled = btnXoa.Enabled = true;
    }

    // TV1: tim kiem theo ten / SDT
    private void btnTim_Click(object? sender, EventArgs e) { NapLuoi(); LamMoiForm(); }

    // TV1: Enter trong o tim kiem = bam Tim
    private void txtTimKiem_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;
        e.SuppressKeyPress = true;
        btnTim_Click(sender, e);
    }

    // TV1: doc o nhap va kiem tra, tra ve null (kem thong bao) neu khong hop le
    private NhaCungCap? DocVaKiemTra()
    {
        var ten = txtTen.Text.Trim();
        var sdt = txtSDT.Text.Trim();
        var email = txtEmail.Text.Trim();

        if (ten.Length == 0) { BaoLoi("Vui lòng nhập tên nhà cung cấp.", txtTen); return null; }
        if (sdt.Length > 0 && !KiemTraDuLieu.LaSoDienThoai(sdt)) { BaoLoi("Số điện thoại chỉ gồm chữ số, dài 10–11 ký tự.", txtSDT); return null; }
        if (email.Length > 0 && !KiemTraDuLieu.LaEmail(email)) { BaoLoi("Email không hợp lệ.", txtEmail); return null; }

        return new NhaCungCap
        {
            MaNCC = _maDangChon,
            TenNCC = ten,
            DiaChi = txtDiaChi.Text.Trim(),
            SDT = sdt,
            Email = email,
        };
    }

    // TV1: them nha cung cap moi
    private void btnThem_Click(object? sender, EventArgs e)
    {
        var ncc = DocVaKiemTra();
        if (ncc == null) return;
        try
        {
            _repo.Them(ncc);
            NapLuoi(); LamMoiForm();
            ThongBao("Đã thêm nhà cung cấp.");
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: luu thay doi nha cung cap dang chon
    private void btnSua_Click(object? sender, EventArgs e)
    {
        var ncc = DocVaKiemTra();
        if (ncc == null) return;
        try
        {
            _repo.Sua(ncc);
            NapLuoi(); LamMoiForm();
            ThongBao("Đã cập nhật nhà cung cấp.");
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: xoa nha cung cap dang chon, chan neu da co phieu nhap
    private void btnXoa_Click(object? sender, EventArgs e)
    {
        if (_maDangChon == 0) return;
        try
        {
            if (!_repo.CoTheXoa(_maDangChon))
            {
                MessageBox.Show("Không thể xóa: nhà cung cấp này đã có phiếu nhập hàng.\nBạn chỉ có thể sửa thông tin.",
                                "Nhà cung cấp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show($"Xóa nhà cung cấp \"{txtTen.Text}\"?", "Xác nhận xóa",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _repo.Xoa(_maDangChon);
            NapLuoi(); LamMoiForm();
            ThongBao("Đã xóa nhà cung cấp.");
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: bo chon, quay ve che do them moi
    private void btnLamMoi_Click(object? sender, EventArgs e) => LamMoiForm();

    // TV1: hien thong bao thanh cong
    private static void ThongBao(string s)
        => MessageBox.Show(s, "Nhà cung cấp", MessageBoxButtons.OK, MessageBoxIcon.Information);

    // TV1: bao loi nhap lieu va dua con tro ve o can sua
    private static void BaoLoi(string s, TextBox o)
    {
        MessageBox.Show(s, "Nhà cung cấp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        o.Focus();
    }

    // TV1: bao loi he thong (mat ket noi CSDL, ...)
    private static void BaoLoiHeThong(Exception ex)
        => MessageBox.Show(LoiCsdl.ThongBao(ex), "Lỗi",
                           MessageBoxButtons.OK, MessageBoxIcon.Error);
}
