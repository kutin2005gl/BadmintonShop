using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

/// <summary>Quan ly nhan vien (tab 1) va tai khoan dang nhap cua nhan vien (tab 2) - SV1, ROADMAP 2.4. Chi ADMIN.</summary>
public partial class FrmNhanVien : Form
{
    private static readonly string[] ChucVuGoiY = { "Quản lý", "Nhân viên bán hàng", "Kho" };
    private static readonly string[] DanhSachVaiTro = { VaiTro.Admin, VaiTro.QuanLy, VaiTro.NhanVien };

    private readonly NhanVienRepository _nvRepo = new();
    private readonly TaiKhoanRepository _tkRepo = new();
    private int _maNVDangChon;   // 0 = che do them moi
    private int _maTKDangChon;   // 0 = chua chon tai khoan

    // TV1: khoi tao form, nap goi y chuc vu / vai tro va du lieu 2 tab
    public FrmNhanVien()
    {
        InitializeComponent();
        cboChucVu.Items.AddRange(ChucVuGoiY);
        cboVaiTroCap.Items.AddRange(DanhSachVaiTro);
        cboVaiTroDoi.Items.AddRange(DanhSachVaiTro);
        cboVaiTroCap.SelectedIndex = 2;
        NapLuoiNhanVien();
        LamMoiNhanVien();
        NapLuoiTaiKhoan();
        NapNhanVienChuaCoTaiKhoan();
    }

    // ===================== TAB NHAN VIEN =====================

    // TV1: nap lai luoi nhan vien theo tu khoa tim kiem
    private void NapLuoiNhanVien()
    {
        try
        {
            dgvNV.DataSource = _nvRepo.LayTatCa(txtTimKiem.Text.Trim());
            DatTieuDe(dgvNV, ("MaNV", "Mã"), ("HoTen", "Họ tên"), ("SDT", "SĐT"), ("Email", "Email"),
                      ("DiaChi", "Địa chỉ"), ("ChucVu", "Chức vụ"), ("Luong", "Lương"), ("NgayVaoLam", "Ngày vào làm"));
            dgvNV.Columns["Luong"]!.DefaultCellStyle.Format = "N0";
            dgvNV.Columns["NgayVaoLam"]!.DefaultCellStyle.Format = "dd/MM/yyyy";
            dgvNV.ClearSelection();
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: xoa trang o nhap, quay ve che do them nhan vien moi
    private void LamMoiNhanVien()
    {
        _maNVDangChon = 0;
        txtHoTen.Clear(); txtSDT.Clear(); txtEmail.Clear(); txtDiaChi.Clear(); txtLuong.Clear();
        cboChucVu.Text = "";
        dtpNgayVaoLam.Value = DateTime.Today;
        btnThemNV.Enabled = true;
        btnSuaNV.Enabled = btnXoaNV.Enabled = false;
        dgvNV.ClearSelection();
        txtHoTen.Focus();
    }

    // TV1: chon 1 dong nhan vien thi do du lieu xuong o nhap
    private void dgvNV_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvNV.CurrentRow?.DataBoundItem is not NhanVien nv || !dgvNV.CurrentRow.Selected) return;
        _maNVDangChon = nv.MaNV;
        txtHoTen.Text = nv.HoTen;
        txtSDT.Text = nv.SDT ?? "";
        txtEmail.Text = nv.Email ?? "";
        txtDiaChi.Text = nv.DiaChi ?? "";
        cboChucVu.Text = nv.ChucVu;
        txtLuong.Text = nv.Luong?.ToString("0", CultureInfo.InvariantCulture) ?? "";
        dtpNgayVaoLam.Value = nv.NgayVaoLam;
        btnThemNV.Enabled = false;
        btnSuaNV.Enabled = btnXoaNV.Enabled = true;
    }

    // TV1: tim nhan vien theo ten / SDT / email
    private void btnTim_Click(object? sender, EventArgs e) { NapLuoiNhanVien(); LamMoiNhanVien(); }

    // TV1: Enter trong o tim kiem = bam Tim
    private void txtTimKiem_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;
        e.SuppressKeyPress = true;
        btnTim_Click(sender, e);
    }

    // TV1: doc o nhap va kiem tra, tra ve null (kem thong bao) neu khong hop le
    private NhanVien? DocVaKiemTraNhanVien()
    {
        var ten = txtHoTen.Text.Trim();
        var sdt = txtSDT.Text.Trim();
        var email = txtEmail.Text.Trim();
        var chucVu = cboChucVu.Text.Trim();
        decimal? luong = null;

        if (ten.Length == 0) { BaoLoi("Vui lòng nhập họ tên nhân viên.", txtHoTen); return null; }
        if (sdt.Length > 0 && !KiemTraDuLieu.LaSoDienThoai(sdt)) { BaoLoi("Số điện thoại chỉ gồm chữ số, dài 10–11 ký tự.", txtSDT); return null; }
        if (email.Length > 0 && !KiemTraDuLieu.LaEmail(email)) { BaoLoi("Email không hợp lệ.", txtEmail); return null; }
        if (chucVu.Length == 0) { BaoLoi("Vui lòng nhập hoặc chọn chức vụ.", cboChucVu); return null; }
        if (txtLuong.Text.Trim().Length > 0)
        {
            if (!decimal.TryParse(txtLuong.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var l) || l < 0)
            { BaoLoi("Lương phải là số không âm (chỉ nhập chữ số).", txtLuong); return null; }
            luong = l;
        }
        if (dtpNgayVaoLam.Value.Date > DateTime.Today) { BaoLoi("Ngày vào làm không được ở tương lai.", dtpNgayVaoLam); return null; }

        return new NhanVien
        {
            MaNV = _maNVDangChon, HoTen = ten, SDT = sdt, Email = email,
            DiaChi = txtDiaChi.Text.Trim(), ChucVu = chucVu, Luong = luong,
            NgayVaoLam = dtpNgayVaoLam.Value.Date,
        };
    }

    // TV1: them nhan vien moi
    private void btnThemNV_Click(object? sender, EventArgs e)
    {
        var nv = DocVaKiemTraNhanVien();
        if (nv == null) return;
        try
        {
            _nvRepo.Them(nv);
            NapLuoiNhanVien(); LamMoiNhanVien(); NapNhanVienChuaCoTaiKhoan();
            ThongBao("Đã thêm nhân viên.");
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: luu thay doi nhan vien dang chon
    private void btnSuaNV_Click(object? sender, EventArgs e)
    {
        var nv = DocVaKiemTraNhanVien();
        if (nv == null) return;
        try
        {
            _nvRepo.Sua(nv);
            NapLuoiNhanVien(); LamMoiNhanVien(); NapLuoiTaiKhoan(); NapNhanVienChuaCoTaiKhoan();
            ThongBao("Đã cập nhật nhân viên.");
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: xoa nhan vien dang chon, chan neu da co tai khoan / phieu nhap / hoa don
    private void btnXoaNV_Click(object? sender, EventArgs e)
    {
        if (_maNVDangChon == 0) return;
        try
        {
            if (!_nvRepo.CoTheXoa(_maNVDangChon))
            {
                MessageBox.Show("Không thể xóa: nhân viên này đã có tài khoản, phiếu nhập hoặc hóa đơn.\n" +
                                "Hãy khóa tài khoản thay vì xóa nhân viên.",
                                "Nhân viên", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show($"Xóa nhân viên \"{txtHoTen.Text}\"?", "Xác nhận xóa",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _nvRepo.Xoa(_maNVDangChon);
            NapLuoiNhanVien(); LamMoiNhanVien(); NapNhanVienChuaCoTaiKhoan();
            ThongBao("Đã xóa nhân viên.");
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: bo chon, quay ve che do them moi
    private void btnLamMoiNV_Click(object? sender, EventArgs e) => LamMoiNhanVien();

    // ===================== TAB TAI KHOAN =====================

    // TV1: nap lai luoi tai khoan (kem ho ten, chuc vu nhan vien)
    private void NapLuoiTaiKhoan()
    {
        try
        {
            dgvTK.DataSource = _tkRepo.LayDanhSachHienThi();
            DatTieuDe(dgvTK, ("MaTK", "Mã TK"), ("TenDangNhap", "Tên đăng nhập"), ("VaiTro", "Vai trò"),
                      ("TrangThai", "Hoạt động"), ("HoTen", "Nhân viên"), ("ChucVu", "Chức vụ"));
            dgvTK.Columns["MaNV"]!.Visible = false;
            dgvTK.ClearSelection();
            _maTKDangChon = 0;
            CapNhatNutTaiKhoan();
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: nap ComboBox cac nhan vien chua co tai khoan
    private void NapNhanVienChuaCoTaiKhoan()
    {
        try
        {
            cboNhanVien.DataSource = _nvRepo.LayChuaCoTaiKhoan()
                .Select(n => new { n.MaNV, Ten = $"{n.HoTen} - {n.ChucVu}" }).ToList();
            cboNhanVien.DisplayMember = "Ten";
            cboNhanVien.ValueMember = "MaNV";
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: chon 1 tai khoan tren luoi thi bat/tat cac nut thao tac
    private void dgvTK_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvTK.CurrentRow?.DataBoundItem is not DataRowView r || !dgvTK.CurrentRow.Selected)
        {
            _maTKDangChon = 0;
        }
        else
        {
            _maTKDangChon = (int)r["MaTK"];
            cboVaiTroDoi.Text = (string)r["VaiTro"];
            btnKhoaMo.Text = (bool)r["TrangThai"] ? "Khóa tài khoản" : "Mở khóa";
        }
        CapNhatNutTaiKhoan();
    }

    // TV1: bat/tat nut theo viec co chon tai khoan hay khong
    private void CapNhatNutTaiKhoan()
        => btnDoiVaiTro.Enabled = btnKhoaMo.Enabled = btnResetMK.Enabled = cboVaiTroDoi.Enabled = _maTKDangChon != 0;

    // TV1: cap tai khoan moi cho nhan vien dang chon trong ComboBox
    private void btnCapTK_Click(object? sender, EventArgs e)
    {
        if (cboNhanVien.SelectedValue is not int maNV) { ThongBaoLoi("Không còn nhân viên nào chưa có tài khoản."); return; }
        var ten = txtTenDN.Text.Trim();
        var mk = txtMatKhau.Text;

        if (ten.Length < 3 || ten.Length > 50 || ten.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '_' && c != '.'))
        { BaoLoi("Tên đăng nhập dài 3–50 ký tự, chỉ gồm chữ không dấu, số, dấu _ hoặc dấu chấm.", txtTenDN); return; }
        if (!MatKhauHelper.LaMatKhauManh(mk))
        { BaoLoi("Mật khẩu chưa đủ mạnh: tối thiểu 8 ký tự, có chữ hoa, chữ thường và chữ số.", txtMatKhau); return; }
        if (cboVaiTroCap.SelectedItem is not string vaiTro) return;

        try
        {
            if (_tkRepo.TenDangNhapDaTonTai(ten)) { BaoLoi("Tên đăng nhập đã được dùng.", txtTenDN); return; }
            if (_tkRepo.LayTheoMaNV(maNV) != null) { ThongBaoLoi("Nhân viên này đã có tài khoản."); return; }
            _tkRepo.Them(new TaiKhoan { TenDangNhap = ten, VaiTro = vaiTro, MaNV = maNV, TrangThai = true }, mk);
            txtTenDN.Clear(); txtMatKhau.Clear();
            NapLuoiTaiKhoan(); NapNhanVienChuaCoTaiKhoan();
            ThongBao($"Đã cấp tài khoản \"{ten}\" ({vaiTro}).");
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: doi vai tro tai khoan dang chon (khong cho ADMIN ha quyen chinh minh)
    private void btnDoiVaiTro_Click(object? sender, EventArgs e)
    {
        if (_maTKDangChon == 0 || cboVaiTroDoi.SelectedItem is not string vaiTro) return;
        if (_maTKDangChon == PhienDangNhap.MaTK && vaiTro != PhienDangNhap.VaiTro)
        {
            ThongBaoLoi("Bạn không thể tự đổi vai trò của chính mình.");
            return;
        }
        try
        {
            _tkRepo.DoiVaiTro(_maTKDangChon, vaiTro);
            NapLuoiTaiKhoan();
            ThongBao("Đã đổi vai trò. Người dùng cần đăng nhập lại để quyền mới có hiệu lực.");
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: khoa / mo khoa tai khoan dang chon (khong cho ADMIN tu khoa chinh minh)
    private void btnKhoaMo_Click(object? sender, EventArgs e)
    {
        if (dgvTK.CurrentRow?.DataBoundItem is not DataRowView r) return;
        var dangHoatDong = (bool)r["TrangThai"];
        if (dangHoatDong && _maTKDangChon == PhienDangNhap.MaTK)
        {
            ThongBaoLoi("Bạn không thể tự khóa tài khoản của chính mình.");
            return;
        }
        try
        {
            _tkRepo.DoiTrangThai(_maTKDangChon, !dangHoatDong);
            NapLuoiTaiKhoan();
            ThongBao(dangHoatDong ? "Đã khóa tài khoản." : "Đã mở khóa tài khoản.");
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: reset mat khau tai khoan dang chon bang mat khau ngau nhien, hien 1 lan cho ADMIN
    private void btnResetMK_Click(object? sender, EventArgs e)
    {
        if (dgvTK.CurrentRow?.DataBoundItem is not DataRowView r) return;
        var ten = (string)r["TenDangNhap"];
        if (MessageBox.Show($"Đặt lại mật khẩu cho \"{ten}\"?", "Xác nhận",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            var mkMoi = TaoMatKhauNgauNhien();
            _tkRepo.DatMatKhau(_maTKDangChon, mkMoi);
            MessageBox.Show($"Mật khẩu mới của \"{ten}\":\n\n{mkMoi}\n\nHãy gửi cho nhân viên và nhắc đổi mật khẩu sau khi đăng nhập.",
                            "Đã đặt lại mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: sinh mat khau 10 ky tu co chu hoa, chu thuong va chu so (thoa LaMatKhauManh)
    private static string TaoMatKhauNgauNhien()
    {
        const string hoa = "ABCDEFGHJKLMNPQRSTUVWXYZ", thuong = "abcdefghijkmnpqrstuvwxyz", so = "23456789";
        var tatCa = hoa + thuong + so;
        var kyTu = new List<char> { hoa[RandomNumberGenerator.GetInt32(hoa.Length)],
                                    thuong[RandomNumberGenerator.GetInt32(thuong.Length)],
                                    so[RandomNumberGenerator.GetInt32(so.Length)] };
        while (kyTu.Count < 10) kyTu.Add(tatCa[RandomNumberGenerator.GetInt32(tatCa.Length)]);
        return new string(kyTu.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
    }

    // ===================== DUNG CHUNG =====================

    // TV1: dat tieu de tieng Viet cho cac cot cua luoi
    private static void DatTieuDe(DataGridView g, params (string Cot, string TieuDe)[] ds)
    {
        foreach (var (cot, tieuDe) in ds) g.Columns[cot]!.HeaderText = tieuDe;
    }

    // TV1: hien thong bao thanh cong
    private static void ThongBao(string s)
        => MessageBox.Show(s, "Nhân viên & tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Information);

    // TV1: hien thong bao khong thuc hien duoc
    private static void ThongBaoLoi(string s)
        => MessageBox.Show(s, "Nhân viên & tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    // TV1: bao loi nhap lieu va dua con tro ve o can sua
    private static void BaoLoi(string s, Control o)
    {
        ThongBaoLoi(s);
        o.Focus();
    }

    // TV1: bao loi he thong (mat ket noi CSDL, ...)
    private static void BaoLoiHeThong(Exception ex)
        => MessageBox.Show("Có lỗi khi làm việc với CSDL:\n" + ex.Message, "Lỗi",
                           MessageBoxButtons.OK, MessageBoxIcon.Error);
}
