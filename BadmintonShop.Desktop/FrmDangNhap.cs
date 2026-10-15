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
        InitializeComponent();
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
