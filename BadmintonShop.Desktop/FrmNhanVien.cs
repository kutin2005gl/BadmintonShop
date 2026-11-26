using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

/// <summary>Quan ly nhan vien va tai khoan dang nhap cua nhan vien - SV1, ROADMAP 2.4. Chi ADMIN.
/// Bo cuc main-sub theo mau Don hang: danh sach nhan vien ben trai; ben phai la ho so va tai khoan cua nguoi dang chon.</summary>
public class FrmNhanVien : Form
{
    private static readonly string[] ChucVuGoiY = { "Quản lý", "Nhân viên bán hàng", "Kho" };
    private static readonly string[] DanhSachVaiTro = { VaiTro.Admin, VaiTro.QuanLy, VaiTro.NhanVien };

    private readonly NhanVienRepository _nvRepo = new();
    private readonly TaiKhoanRepository _tkRepo = new();
    private int _maNVDangChon;   // 0 = che do them moi
    private int _maTKDangChon;   // 0 = nhan vien dang chon chua co tai khoan
    private Dictionary<int, DataRow> _taiKhoan = new();   // MaNV -> tai khoan

    private DataGridView dgvNV = null!;
    private TextBox txtTimKiem = null!, txtHoTen = null!, txtSDT = null!, txtEmail = null!, txtDiaChi = null!, txtLuong = null!, txtTenDN = null!, txtMatKhau = null!;
    private ComboBox cboChucVu = null!, cboVaiTroCap = null!, cboVaiTroDoi = null!;
    private DateTimePicker dtpNgayVaoLam = null!, dtpNgaySinh = null!;
    private bool coNgaySinh, dangDatNgaySinh;

    // Đặt ngày sinh: null = chưa có (ô trống), có ngày = hiện dd/MM/yyyy
    private void DatNgaySinh(DateTime? ngay)
    {
        dangDatNgaySinh = true;
        try
        {
            coNgaySinh = ngay.HasValue;
            dtpNgaySinh.Value = ngay ?? new DateTime(2000, 1, 1);
            dtpNgaySinh.CustomFormat = coNgaySinh ? "dd/MM/yyyy" : " ";
        }
        finally { dangDatNgaySinh = false; }
    }
    private Button btnThemNV = null!, btnSuaNV = null!, btnXoaNV = null!, btnKhoaMo = null!;
    private Label maNV = null!, badge = null!, lblChuaLuu = null!, lblTenTK = null!;
    private Panel panelRoot = null!, tkDaCo = null!, tkCapMoi = null!;

    // TV1: khoi tao form (chi giao dien, KHONG truy cap CSDL de VS Designer mo duoc)
    public FrmNhanVien()
    {
        InitializeComponent();
    }

    // TV1: dung thanh tim kiem, luoi nhan vien ben trai va panel chi tiet ben phai
    private void InitializeComponent()
    {
        Text = "Nhân viên && tài khoản"; Size = new Size(1250, 800); MinimumSize = new Size(1000, 650); AutoScaleMode = AutoScaleMode.Font;
        txtTimKiem = new TextBox { Width = 280, PlaceholderText = "Tên / SĐT / email" };
        txtTimKiem.KeyDown += txtTimKiem_KeyDown;
        var bar = Sv4Ui.ThanhCongCu();
        bar.Controls.AddRange(new Control[]
        {
            Sv4Ui.Nhan("Tìm"), txtTimKiem, Sv4Ui.Nut("Tìm", Tim), Sv4Ui.Nut("+ Nhân viên mới", LamMoiNhanVien),
        });

        dgvNV = Sv4Ui.Luoi();
        dgvNV.SelectionChanged += dgvNV_SelectionChanged;
        dgvNV.CellFormatting += dgvNV_CellFormatting;
        var split = KhungChiTiet.Chia(this, 0.60, 420, 460);
        split.Panel1.Controls.Add(dgvNV);
        XayChiTiet(split.Panel2);
        Controls.Add(split); Controls.Add(bar);
        ShopTheme.Apply(this);
    }

    // TV1: panel ben phai: ho so nhan vien (main) va tai khoan dang nhap (sub)
    private void XayChiTiet(Control host)
    {
        panelRoot = KhungChiTiet.NenChiTiet(); panelRoot.AutoScroll = true;
        host.BackColor = ShopTheme.SurfaceAlt;
        var header = KhungChiTiet.TieuDe(out maNV, out badge);

        txtHoTen = new TextBox { MaxLength = 100 };
        txtSDT = new TextBox { MaxLength = 11 };
        txtEmail = new TextBox { MaxLength = 100 };
        txtDiaChi = new TextBox { MaxLength = 255 };
        cboChucVu = new ComboBox { MaxLength = 50 };
        cboChucVu.Items.AddRange(ChucVuGoiY);
        txtLuong = new TextBox();
        // Ngày sinh không bắt buộc: không dùng dấu tích; chưa có thì ô để trống, chọn ngày thì hiện, phím Delete xóa về trống
        dtpNgaySinh = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = " " };
        dtpNgaySinh.ValueChanged += (_, _) => { if (!dangDatNgaySinh) DatNgaySinh(dtpNgaySinh.Value); };
        dtpNgaySinh.CloseUp += (_, _) => DatNgaySinh(dtpNgaySinh.Value);
        dtpNgaySinh.KeyDown += (_, e) => { if (e.KeyCode is Keys.Delete or Keys.Back) { DatNgaySinh(null); e.SuppressKeyPress = true; } };
        dtpNgayVaoLam = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };
        var hoSo = KhungChiTiet.LuoiO(
            ("Họ tên (*)", txtHoTen, true),
            ("Điện thoại", txtSDT, false), ("Email", txtEmail, false),
            ("Địa chỉ", txtDiaChi, true),
            ("Chức vụ (*)", cboChucVu, false), ("Lương (đ)", txtLuong, false),
            ("Ngày sinh", dtpNgaySinh, false), ("Ngày vào làm", dtpNgayVaoLam, false));

        btnThemNV = Sv4Ui.Nut("Thêm nhân viên", ThemNhanVien);
        btnSuaNV = Sv4Ui.Nut("Lưu sửa", SuaNhanVien);
        btnXoaNV = Sv4Ui.Nut("Xóa", XoaNhanVien);
        var nutHoSo = KhungChiTiet.HangNut(btnThemNV, btnSuaNV, btnXoaNV);

        // sub: chua luu nhan vien / da co tai khoan / cap tai khoan moi
        lblChuaLuu = new Label { Dock = DockStyle.Top, AutoSize = true, ForeColor = ShopTheme.Muted, Padding = new Padding(0, 2, 0, 10), Text = "Lưu nhân viên trước rồi cấp tài khoản." };

        lblTenTK = new Label { AutoSize = true, Margin = new Padding(0, 5, 0, 5), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        cboVaiTroDoi = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        cboVaiTroDoi.Items.AddRange(DanhSachVaiTro);
        var daCo = KhungChiTiet.LuoiO(("Tên đăng nhập", lblTenTK, false), ("Vai trò", cboVaiTroDoi, false));
        btnKhoaMo = Sv4Ui.Nut("Khóa tài khoản", KhoaMo);
        var nutTK = KhungChiTiet.HangNut(Sv4Ui.Nut("Đổi vai trò", DoiVaiTro), btnKhoaMo, Sv4Ui.Nut("Đặt lại mật khẩu", ResetMatKhau));
        tkDaCo = new Panel { Dock = DockStyle.Top, AutoSize = true, BackColor = ShopTheme.Surface };
        tkDaCo.Controls.Add(nutTK); tkDaCo.Controls.Add(daCo);

        txtTenDN = new TextBox { MaxLength = 50 };
        txtMatKhau = new TextBox { UseSystemPasswordChar = true };
        cboVaiTroCap = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        cboVaiTroCap.Items.AddRange(DanhSachVaiTro);
        cboVaiTroCap.SelectedIndex = 2;
        var capMoi = KhungChiTiet.LuoiO(("Tên đăng nhập", txtTenDN, false), ("Mật khẩu", txtMatKhau, false), ("Vai trò", cboVaiTroCap, false));
        var nutCap = KhungChiTiet.HangNut(Sv4Ui.Nut("Cấp tài khoản", CapTaiKhoan));
        tkCapMoi = new Panel { Dock = DockStyle.Top, AutoSize = true, BackColor = ShopTheme.Surface };
        tkCapMoi.Controls.Add(nutCap); tkCapMoi.Controls.Add(capMoi);

        // Mọi mục đều Dock Top: thêm sau nằm cao hơn.
        panelRoot.Controls.Add(KhungChiTiet.The("Tài khoản đăng nhập", lblChuaLuu, tkDaCo, tkCapMoi));
        panelRoot.Controls.Add(KhungChiTiet.Cach(16));
        panelRoot.Controls.Add(nutHoSo);
        panelRoot.Controls.Add(KhungChiTiet.Cach(12));
        panelRoot.Controls.Add(KhungChiTiet.The("Hồ sơ nhân viên", hoSo));
        panelRoot.Controls.Add(header);
        host.Controls.Add(panelRoot);
    }

    // TV1: khi form hien len - kiem tra quyen (chi ADMIN) roi nap danh sach
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode || !QuyenForm.KiemTraKhiMo(this, ChucNang.NhanVienTaiKhoan)) return;
        NapLuoiNhanVien();
        LamMoiNhanVien();
    }

    // ===================== NHAN VIEN =====================

    // TV1: nap lai tai khoan va luoi nhan vien theo tu khoa tim kiem
    private void NapLuoiNhanVien()
    {
        try
        {
            _taiKhoan = _tkRepo.LayDanhSachHienThi().AsEnumerable().ToDictionary(r => r.Field<int>("MaNV"));
            dgvNV.DataSource = _nvRepo.LayTatCa(txtTimKiem.Text.Trim());
            DatTieuDe(dgvNV, ("MaNV", "Mã"), ("HoTen", "Họ tên"), ("SDT", "SĐT"), ("ChucVu", "Chức vụ"));
            // cac cot chi tiet da hien o panel ben phai
            foreach (var cot in new[] { "Email", "DiaChi", "Luong", "NgaySinh", "NgayVaoLam" }) dgvNV.Columns[cot]!.Visible = false;
            foreach (var (cot, tieuDe) in new[] { ("TenDangNhap", "Tài khoản"), ("VaiTro", "Vai trò") })
                if (!dgvNV.Columns.Contains(cot))
                    dgvNV.Columns.Add(new DataGridViewTextBoxColumn { Name = cot, HeaderText = tieuDe, ReadOnly = true });
            dgvNV.Columns["TenDangNhap"]!.DisplayIndex = dgvNV.Columns.Count - 2;
            dgvNV.Columns["VaiTro"]!.DisplayIndex = dgvNV.Columns.Count - 1;
            KhungChiTiet.DoRong(dgvNV, ("MaNV", 40, 55), ("HoTen", 160, 150), ("SDT", 100, 120), ("ChucVu", 150, 140), ("TenDangNhap", 100, 110), ("VaiTro", 100, 110));
            dgvNV.ClearSelection();
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: dien cot tai khoan / vai tro cua tung nhan vien tren luoi
    private void dgvNV_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
        var cot = dgvNV.Columns[e.ColumnIndex].Name;
        if (cot is not ("TenDangNhap" or "VaiTro") || dgvNV.Rows[e.RowIndex].DataBoundItem is not NhanVien nv) return;
        e.Value = _taiKhoan.TryGetValue(nv.MaNV, out var tk) ? tk[cot] : "—";
        e.FormattingApplied = true;
    }

    // TV1: xoa trang o nhap, quay ve che do them nhan vien moi
    private void LamMoiNhanVien()
    {
        _maNVDangChon = 0;
        txtHoTen.Clear(); txtSDT.Clear(); txtEmail.Clear(); txtDiaChi.Clear(); txtLuong.Clear();
        cboChucVu.Text = "";
        dtpNgayVaoLam.Value = DateTime.Today;
        DatNgaySinh(null);
        maNV.Text = "Nhân viên mới";
        btnThemNV.Visible = true;
        btnSuaNV.Visible = btnXoaNV.Visible = false;
        dgvNV.ClearSelection();
        HienTaiKhoan();
        txtHoTen.Focus();
    }

    // TV1: chon 1 dong nhan vien thi do du lieu sang panel ben phai
    private void dgvNV_SelectionChanged(object? sender, EventArgs e)
    {
        // doc dong dang duoc chon: CurrentRow co the chua doi kip khi su kien nay ban ra
        if (dgvNV.SelectedRows.Count != 1 || dgvNV.SelectedRows[0].DataBoundItem is not NhanVien nv) return;
        _maNVDangChon = nv.MaNV;
        txtHoTen.Text = nv.HoTen;
        txtSDT.Text = nv.SDT ?? "";
        txtEmail.Text = nv.Email ?? "";
        txtDiaChi.Text = nv.DiaChi ?? "";
        cboChucVu.Text = nv.ChucVu;
        txtLuong.Text = nv.Luong?.ToString("0", CultureInfo.InvariantCulture) ?? "";
        dtpNgayVaoLam.Value = nv.NgayVaoLam;
        DatNgaySinh(nv.NgaySinh);
        maNV.Text = $"Nhân viên {nv.MaNV}";
        btnThemNV.Visible = false;
        btnSuaNV.Visible = btnXoaNV.Visible = true;
        HienTaiKhoan();
    }

    // TV1: chon lai nhan vien theo ma sau khi nap lai luoi
    private void ChonNhanVien(int ma)
    {
        foreach (DataGridViewRow r in dgvNV.Rows)
            if (r.DataBoundItem is NhanVien nv && nv.MaNV == ma)
            { dgvNV.CurrentCell = r.Cells["MaNV"]; r.Selected = true; dgvNV_SelectionChanged(this, EventArgs.Empty); return; }
        LamMoiNhanVien();
    }

    // TV1: tim nhan vien theo ten / SDT / email
    private void Tim() { NapLuoiNhanVien(); LamMoiNhanVien(); }

    // TV1: Enter trong o tim kiem = bam Tim
    private void txtTimKiem_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;
        e.SuppressKeyPress = true;
        Tim();
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

        DateTime? ngaySinh = coNgaySinh ? dtpNgaySinh.Value.Date : null;
        if (ngaySinh.HasValue)
        {
            if (ngaySinh.Value > DateTime.Today.AddYears(-16)) { BaoLoi("Nhân viên phải đủ 16 tuổi.", dtpNgaySinh); return null; }
            if (ngaySinh.Value > dtpNgayVaoLam.Value.Date) { BaoLoi("Ngày sinh phải trước ngày vào làm.", dtpNgaySinh); return null; }
        }

        return new NhanVien
        {
            MaNV = _maNVDangChon, HoTen = ten, SDT = sdt, Email = email,
            DiaChi = txtDiaChi.Text.Trim(), ChucVu = chucVu, Luong = luong, NgaySinh = ngaySinh,
            NgayVaoLam = dtpNgayVaoLam.Value.Date,
        };
    }

    // TV1: them nhan vien moi
    private void ThemNhanVien()
    {
        var nv = DocVaKiemTraNhanVien();
        if (nv == null) return;
        try
        {
            var ma = _nvRepo.Them(nv);
            NapLuoiNhanVien(); ChonNhanVien(ma);
            ThongBao("Đã thêm nhân viên.");
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: luu thay doi nhan vien dang chon
    private void SuaNhanVien()
    {
        var nv = DocVaKiemTraNhanVien();
        if (nv == null) return;
        try
        {
            _nvRepo.Sua(nv);
            NapLuoiNhanVien(); ChonNhanVien(nv.MaNV);
            ThongBao("Đã cập nhật nhân viên.");
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: xoa nhan vien dang chon, chan neu da co tai khoan / phieu nhap / hoa don
    private void XoaNhanVien()
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
            NapLuoiNhanVien(); LamMoiNhanVien();
            ThongBao("Đã xóa nhân viên.");
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // ===================== TAI KHOAN CUA NHAN VIEN DANG CHON =====================

    // TV1: hien dung phan tai khoan: chua luu / da co (doi quyen, khoa, reset) / cap moi; badge theo trang thai
    private void HienTaiKhoan()
    {
        DataRow? tk = null;
        if (_maNVDangChon != 0) _taiKhoan.TryGetValue(_maNVDangChon, out tk);
        _maTKDangChon = tk == null ? 0 : (int)tk["MaTK"];
        panelRoot.SuspendLayout();
        lblChuaLuu.Visible = _maNVDangChon == 0;
        tkDaCo.Visible = tk != null;
        tkCapMoi.Visible = _maNVDangChon != 0 && tk == null;
        txtTenDN.Clear(); txtMatKhau.Clear();
        if (tk != null)
        {
            var hoatDong = (bool)tk["TrangThai"];
            lblTenTK.Text = (string)tk["TenDangNhap"];
            cboVaiTroDoi.SelectedItem = (string)tk["VaiTro"];
            btnKhoaMo.Text = hoatDong ? "Khóa tài khoản" : "Mở khóa";
            KhungChiTiet.DatBadge(badge, hoatDong ? "Đang hoạt động" : "Đã khóa", ngung: !hoatDong);
        }
        else KhungChiTiet.DatBadge(badge, _maNVDangChon == 0 ? "" : "Chưa có tài khoản", canhBao: true);
        panelRoot.ResumeLayout(true);
    }

    // TV1: cap tai khoan moi cho nhan vien dang chon
    private void CapTaiKhoan()
    {
        var maNhanVien = _maNVDangChon;
        if (maNhanVien == 0) return;
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
            if (_tkRepo.LayTheoMaNV(maNhanVien) != null) { ThongBaoLoi("Nhân viên này đã có tài khoản."); return; }
            _tkRepo.Them(new TaiKhoan { TenDangNhap = ten, VaiTro = vaiTro, MaNV = maNhanVien, TrangThai = true }, mk);
            NapLuoiNhanVien(); ChonNhanVien(maNhanVien);
            ThongBao($"Đã cấp tài khoản \"{ten}\" ({vaiTro}).");
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: doi vai tro tai khoan dang chon (khong cho ADMIN ha quyen chinh minh)
    private void DoiVaiTro()
    {
        if (_maTKDangChon == 0 || cboVaiTroDoi.SelectedItem is not string vaiTro) return;
        if (_maTKDangChon == PhienDangNhap.MaTK && vaiTro != PhienDangNhap.VaiTro)
        {
            ThongBaoLoi("Bạn không thể tự đổi vai trò của chính mình.");
            return;
        }
        try
        {
            var ma = _maNVDangChon;
            _tkRepo.DoiVaiTro(_maTKDangChon, vaiTro);
            NapLuoiNhanVien(); ChonNhanVien(ma);
            ThongBao("Đã đổi vai trò. Người dùng cần đăng nhập lại để quyền mới có hiệu lực.");
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: khoa / mo khoa tai khoan dang chon (khong cho ADMIN tu khoa chinh minh)
    private void KhoaMo()
    {
        if (!_taiKhoan.TryGetValue(_maNVDangChon, out var tk)) return;
        var dangHoatDong = (bool)tk["TrangThai"];
        if (dangHoatDong && _maTKDangChon == PhienDangNhap.MaTK)
        {
            ThongBaoLoi("Bạn không thể tự khóa tài khoản của chính mình.");
            return;
        }
        try
        {
            var ma = _maNVDangChon;
            _tkRepo.DoiTrangThai(_maTKDangChon, !dangHoatDong);
            NapLuoiNhanVien(); ChonNhanVien(ma);
            ThongBao(dangHoatDong ? "Đã khóa tài khoản." : "Đã mở khóa tài khoản.");
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: reset mat khau tai khoan dang chon bang mat khau ngau nhien, hien 1 lan cho ADMIN
    private void ResetMatKhau()
    {
        if (!_taiKhoan.TryGetValue(_maNVDangChon, out var tk)) return;
        var ten = (string)tk["TenDangNhap"];
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
        => MessageBox.Show(LoiCsdl.ThongBao(ex), "Lỗi",
                           MessageBoxButtons.OK, MessageBoxIcon.Error);
}
