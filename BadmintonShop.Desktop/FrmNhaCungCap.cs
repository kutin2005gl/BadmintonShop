using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

/// <summary>Quan ly nha cung cap: tim kiem, them, sua, xoa (SV1 - ROADMAP 2.3).</summary>
public partial class FrmNhaCungCap : Form
{
    private readonly NhaCungCapRepository _repo = new();
    private int _maDangChon;   // 0 = dang o che do them moi

    // TV1: khoi tao form va nap danh sach nha cung cap
    public FrmNhaCungCap()
    {
        InitializeComponent();
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
        => MessageBox.Show("Có lỗi khi làm việc với CSDL:\n" + ex.Message, "Lỗi",
                           MessageBoxButtons.OK, MessageBoxIcon.Error);
}
