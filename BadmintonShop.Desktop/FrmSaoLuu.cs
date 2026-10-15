using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

/// <summary>Sao luu / phuc hoi CSDL (SV1 - ROADMAP 4.1). Chi ADMIN thay menu nay.</summary>
public partial class FrmSaoLuu : Form
{
    private readonly SaoLuuRepository _repo = new();

    // TV1: khoi tao form (chi giao dien, KHONG truy cap CSDL de VS Designer mo duoc)
    public FrmSaoLuu()
    {
        InitializeComponent();
    }

    // TV1: khi form hien len - kiem tra quyen (chi ADMIN), goi y thu muc sao luu mac dinh cua SQL Server
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode || !QuyenForm.KiemTraKhiMo(this, ChucNang.SaoLuuPhucHoi)) return;
        lblCSDL.Text = $"CSDL đang dùng: {_repo.TenCSDL}";
        txtThuMuc.Text = _repo.LayThuMucMacDinh() ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    // TV1: chon thu muc luu file .bak
    private void btnChonThuMuc_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog { Description = "Chọn thư mục lưu file sao lưu", SelectedPath = txtThuMuc.Text };
        if (dlg.ShowDialog(this) == DialogResult.OK) txtThuMuc.Text = dlg.SelectedPath;
    }

    // TV1: chon file .bak can phuc hoi
    private void btnChonFile_Click(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog { Title = "Chọn file sao lưu", Filter = "File sao lưu SQL Server (*.bak)|*.bak", InitialDirectory = txtThuMuc.Text };
        if (dlg.ShowDialog(this) == DialogResult.OK) txtFileBak.Text = dlg.FileName;
    }

    // TV1: sao luu CSDL ra thu muc da chon, chay nen de form khong bi treo
    private async void btnSaoLuu_Click(object? sender, EventArgs e)
    {
        var thuMuc = txtThuMuc.Text.Trim();
        if (!Directory.Exists(thuMuc)) { BaoLoi("Thư mục không tồn tại."); return; }
        DangXuLy(true, "Đang sao lưu...");
        try
        {
            var file = await Task.Run(() => _repo.SaoLuu(thuMuc));
            txtFileBak.Text = file;
            MessageBox.Show($"Sao lưu thành công:\n{file}", "Sao lưu", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            BaoLoi("Sao lưu thất bại:\n" + ex.Message +
                   "\n\nGợi ý: tài khoản chạy SQL Server cần quyền ghi vào thư mục này (thử thư mục mặc định của SQL Server).");
        }
        finally { DangXuLy(false, ""); }
    }

    // TV1: phuc hoi CSDL tu file .bak (ghi de toan bo du lieu), xong thi bat dang nhap lai
    private async void btnPhucHoi_Click(object? sender, EventArgs e)
    {
        var file = txtFileBak.Text.Trim();
        if (!File.Exists(file)) { BaoLoi("Vui lòng chọn file .bak hợp lệ."); return; }
        if (MessageBox.Show($"Phục hồi sẽ GHI ĐÈ toàn bộ dữ liệu hiện tại của CSDL \"{_repo.TenCSDL}\" bằng dữ liệu trong file:\n{file}\n\n" +
                            "Mọi người đang dùng hệ thống sẽ bị ngắt kết nối. Tiếp tục?", "Xác nhận phục hồi",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        DangXuLy(true, "Đang phục hồi...");
        try
        {
            await Task.Run(() => _repo.PhucHoi(file));
            MessageBox.Show("Phục hồi thành công. Vui lòng đăng nhập lại.", "Phục hồi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            (Owner as FrmMain)?.DangXuatBatBuoc();   // tai khoan / quyen co the da khac sau khi phuc hoi
            Close();
        }
        catch (Exception ex) { BaoLoi("Phục hồi thất bại, dữ liệu hiện tại được giữ nguyên:\n" + ex.Message); }
        finally { if (!IsDisposed) DangXuLy(false, ""); }
    }

    // TV1: khoa cac nut + doi con tro trong luc dang sao luu / phuc hoi
    private void DangXuLy(bool dang, string trangThai)
    {
        grpSaoLuu.Enabled = grpPhucHoi.Enabled = !dang;
        UseWaitCursor = dang;
        lblTrangThai.Text = trangThai;
    }

    // TV1: hien thong bao loi
    private void BaoLoi(string s) => MessageBox.Show(this, s, "Sao lưu - phục hồi", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
