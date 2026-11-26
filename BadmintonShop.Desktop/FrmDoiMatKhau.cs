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
        DungBoCuc();
    }

    // Nhãn nằm trên ô nhập (không đặt cạnh nhau theo tọa độ cố định nữa) để nhãn dài như
    // "Nhập lại mật khẩu mới" không đè lên ô, và dòng gợi ý tự xuống hàng thay vì bị cắt.
    private void DungBoCuc()
    {
        const int rong = 360;
        var bang = new TableLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Location = new Point(0, 0), Padding = new Padding(24, 18, 24, 16) };
        bang.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, rong));
        Controls.Clear();
        foreach (var (nhan, o) in new[] { (lblMatKhauCu, txtMatKhauCu), (lblMatKhauMoi, txtMatKhauMoi), (lblXacNhan, txtXacNhan) })
        {
            nhan.AutoSize = true; nhan.Margin = new Padding(0, 0, 0, 4);
            o.Dock = DockStyle.Fill; o.Margin = new Padding(0, 0, 0, 12);
            bang.Controls.Add(nhan); bang.Controls.Add(o);
        }
        lblGoiY.AutoSize = true; lblGoiY.MaximumSize = new Size(rong, 0); lblGoiY.Margin = new Padding(0, 0, 0, 16);
        bang.Controls.Add(lblGoiY);
        var nut = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, Margin = Padding.Empty, WrapContents = false };
        btnHuy.Margin = new Padding(8, 0, 0, 0); btnLuu.Margin = Padding.Empty;
        nut.Controls.Add(btnHuy); nut.Controls.Add(btnLuu);
        bang.Controls.Add(nut);
        Controls.Add(bang);
        AutoSize = true; AutoSizeMode = AutoSizeMode.GrowAndShrink;
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
