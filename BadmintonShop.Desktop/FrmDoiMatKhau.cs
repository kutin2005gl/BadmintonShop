using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

/// <summary>Doi mat khau cua nguoi dang dang nhap (SV1 - ROADMAP 2.1).</summary>
public partial class FrmDoiMatKhau : Form
{
    private readonly TaiKhoanRepository _taiKhoanRepo = new();

    // TV1: khoi tao form doi mat khau
    public FrmDoiMatKhau()
    {
        InitializeComponent();
    }

    // TV1: kiem tra du lieu nhap roi luu mat khau moi (da bam BCrypt)
    private void btnLuu_Click(object? sender, EventArgs e)
    {
        var cu = txtMatKhauCu.Text;
        var moi = txtMatKhauMoi.Text;

        if (cu.Length == 0) { BaoLoi("Vui lòng nhập mật khẩu hiện tại.", txtMatKhauCu); return; }
        if (!MatKhauHelper.LaMatKhauManh(moi)) { BaoLoi("Mật khẩu mới chưa đủ mạnh: tối thiểu 8 ký tự, có chữ hoa, chữ thường và chữ số.", txtMatKhauMoi); return; }
        if (moi != txtXacNhan.Text) { BaoLoi("Nhập lại mật khẩu mới không khớp.", txtXacNhan); return; }
        if (moi == cu) { BaoLoi("Mật khẩu mới phải khác mật khẩu hiện tại.", txtMatKhauMoi); return; }

        if (!_taiKhoanRepo.DoiMatKhau(PhienDangNhap.MaTK, cu, moi))
        {
            BaoLoi("Mật khẩu hiện tại không đúng.", txtMatKhauCu);
            return;
        }
        MessageBox.Show("Đổi mật khẩu thành công.", "Đổi mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
        Close();
    }

    // TV1: hien thong bao loi va dua con tro ve o can sua
    private static void BaoLoi(string thongBao, TextBox oCanSua)
    {
        MessageBox.Show(thongBao, "Đổi mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        oCanSua.Focus();
        oCanSua.SelectAll();
    }
}
