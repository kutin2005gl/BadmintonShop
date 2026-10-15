namespace BadmintonShop.Desktop;

partial class FrmNhanVien
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        tabs = new TabControl(); tabNV = new TabPage(); tabTK = new TabPage();
        // --- tab nhan vien
        pnlTim = new Panel(); lblTim = new Label(); txtTimKiem = new TextBox(); btnTim = new Button();
        dgvNV = new DataGridView(); pnlNhapNV = new Panel();
        lblHoTen = new Label(); txtHoTen = new TextBox(); lblSDT = new Label(); txtSDT = new TextBox();
        lblEmail = new Label(); txtEmail = new TextBox(); lblDiaChi = new Label(); txtDiaChi = new TextBox();
        lblChucVu = new Label(); cboChucVu = new ComboBox(); lblLuong = new Label(); txtLuong = new TextBox();
        lblNgay = new Label(); dtpNgayVaoLam = new DateTimePicker();
        btnThemNV = new Button(); btnSuaNV = new Button(); btnXoaNV = new Button(); btnLamMoiNV = new Button();
        // --- tab tai khoan
        dgvTK = new DataGridView(); pnlTK = new Panel();
        grpCap = new GroupBox(); lblNV = new Label(); cboNhanVien = new ComboBox();
        lblTenDN = new Label(); txtTenDN = new TextBox(); lblMK = new Label(); txtMatKhau = new TextBox();
        lblVTCap = new Label(); cboVaiTroCap = new ComboBox(); btnCapTK = new Button();
        grpSua = new GroupBox(); lblVTDoi = new Label(); cboVaiTroDoi = new ComboBox();
        btnDoiVaiTro = new Button(); btnKhoaMo = new Button(); btnResetMK = new Button();

        tabs.SuspendLayout(); tabNV.SuspendLayout(); tabTK.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvNV).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dgvTK).BeginInit();
        pnlTim.SuspendLayout(); pnlNhapNV.SuspendLayout(); pnlTK.SuspendLayout();
        grpCap.SuspendLayout(); grpSua.SuspendLayout(); SuspendLayout();

        // tabs
        tabs.Dock = DockStyle.Fill; tabs.Controls.Add(tabNV); tabs.Controls.Add(tabTK);
        tabNV.Text = "Nhân viên"; tabNV.Padding = new Padding(3);
        tabNV.Controls.Add(dgvNV); tabNV.Controls.Add(pnlNhapNV); tabNV.Controls.Add(pnlTim);
        tabTK.Text = "Tài khoản đăng nhập"; tabTK.Padding = new Padding(3);
        tabTK.Controls.Add(dgvTK); tabTK.Controls.Add(pnlTK);

        // ---- tab nhan vien: tim kiem
        pnlTim.Dock = DockStyle.Top; pnlTim.Height = 48;
        pnlTim.Controls.AddRange(new Control[] { lblTim, txtTimKiem, btnTim });
        lblTim.AutoSize = true; lblTim.Location = new Point(10, 15); lblTim.Text = "Tìm (tên / SĐT / email)";
        txtTimKiem.Location = new Point(175, 12); txtTimKiem.Size = new Size(260, 27); txtTimKiem.TabIndex = 0;
        txtTimKiem.KeyDown += txtTimKiem_KeyDown;
        btnTim.Location = new Point(445, 10); btnTim.Size = new Size(90, 31); btnTim.Text = "Tìm"; btnTim.TabIndex = 1;
        btnTim.Click += btnTim_Click;

        // ---- tab nhan vien: luoi
        CauHinhLuoi(dgvNV);
        dgvNV.SelectionChanged += dgvNV_SelectionChanged;

        // ---- tab nhan vien: o nhap
        pnlNhapNV.Dock = DockStyle.Bottom; pnlNhapNV.Height = 200;
        pnlNhapNV.Controls.AddRange(new Control[] { lblHoTen, txtHoTen, lblSDT, txtSDT, lblEmail, txtEmail,
            lblDiaChi, txtDiaChi, lblChucVu, cboChucVu, lblLuong, txtLuong, lblNgay, dtpNgayVaoLam,
            btnThemNV, btnSuaNV, btnXoaNV, btnLamMoiNV });
        lblHoTen.AutoSize = true; lblHoTen.Location = new Point(10, 18); lblHoTen.Text = "Họ tên (*)";
        txtHoTen.Location = new Point(100, 15); txtHoTen.Size = new Size(240, 27); txtHoTen.MaxLength = 100; txtHoTen.TabIndex = 2;
        lblSDT.AutoSize = true; lblSDT.Location = new Point(360, 18); lblSDT.Text = "SĐT";
        txtSDT.Location = new Point(410, 15); txtSDT.Size = new Size(150, 27); txtSDT.MaxLength = 11; txtSDT.TabIndex = 3;
        lblEmail.AutoSize = true; lblEmail.Location = new Point(10, 58); lblEmail.Text = "Email";
        txtEmail.Location = new Point(100, 55); txtEmail.Size = new Size(240, 27); txtEmail.MaxLength = 100; txtEmail.TabIndex = 4;
        lblDiaChi.AutoSize = true; lblDiaChi.Location = new Point(360, 58); lblDiaChi.Text = "Địa chỉ";
        txtDiaChi.Location = new Point(410, 55); txtDiaChi.Size = new Size(250, 27); txtDiaChi.MaxLength = 255; txtDiaChi.TabIndex = 5;
        lblChucVu.AutoSize = true; lblChucVu.Location = new Point(10, 98); lblChucVu.Text = "Chức vụ (*)";
        cboChucVu.Location = new Point(100, 95); cboChucVu.Size = new Size(240, 28); cboChucVu.MaxLength = 50; cboChucVu.TabIndex = 6;
        lblLuong.AutoSize = true; lblLuong.Location = new Point(360, 98); lblLuong.Text = "Lương";
        txtLuong.Location = new Point(410, 95); txtLuong.Size = new Size(110, 27); txtLuong.TabIndex = 7;
        lblNgay.AutoSize = true; lblNgay.Location = new Point(535, 98); lblNgay.Text = "Vào làm";
        dtpNgayVaoLam.Location = new Point(605, 95); dtpNgayVaoLam.Size = new Size(130, 27);
        dtpNgayVaoLam.Format = DateTimePickerFormat.Short; dtpNgayVaoLam.TabIndex = 8;
        btnThemNV.Location = new Point(100, 145); btnThemNV.Size = new Size(95, 34); btnThemNV.Text = "Thêm"; btnThemNV.TabIndex = 9; btnThemNV.Click += btnThemNV_Click;
        btnSuaNV.Location = new Point(205, 145); btnSuaNV.Size = new Size(95, 34); btnSuaNV.Text = "Lưu sửa"; btnSuaNV.TabIndex = 10; btnSuaNV.Click += btnSuaNV_Click;
        btnXoaNV.Location = new Point(310, 145); btnXoaNV.Size = new Size(95, 34); btnXoaNV.Text = "Xóa"; btnXoaNV.TabIndex = 11; btnXoaNV.Click += btnXoaNV_Click;
        btnLamMoiNV.Location = new Point(415, 145); btnLamMoiNV.Size = new Size(95, 34); btnLamMoiNV.Text = "Làm mới"; btnLamMoiNV.TabIndex = 12; btnLamMoiNV.Click += btnLamMoiNV_Click;

        // ---- tab tai khoan
        CauHinhLuoi(dgvTK);
        dgvTK.SelectionChanged += dgvTK_SelectionChanged;
        pnlTK.Dock = DockStyle.Bottom; pnlTK.Height = 190;
        pnlTK.Controls.Add(grpCap); pnlTK.Controls.Add(grpSua);

        grpCap.Text = "Cấp tài khoản mới"; grpCap.Location = new Point(5, 5); grpCap.Size = new Size(430, 175);
        grpCap.Controls.AddRange(new Control[] { lblNV, cboNhanVien, lblTenDN, txtTenDN, lblMK, txtMatKhau, lblVTCap, cboVaiTroCap, btnCapTK });
        lblNV.AutoSize = true; lblNV.Location = new Point(10, 30); lblNV.Text = "Nhân viên";
        cboNhanVien.Location = new Point(110, 27); cboNhanVien.Size = new Size(305, 28); cboNhanVien.DropDownStyle = ComboBoxStyle.DropDownList; cboNhanVien.TabIndex = 0;
        lblTenDN.AutoSize = true; lblTenDN.Location = new Point(10, 65); lblTenDN.Text = "Tên đăng nhập";
        txtTenDN.Location = new Point(110, 62); txtTenDN.Size = new Size(140, 27); txtTenDN.MaxLength = 50; txtTenDN.TabIndex = 1;
        lblMK.AutoSize = true; lblMK.Location = new Point(10, 100); lblMK.Text = "Mật khẩu";
        txtMatKhau.Location = new Point(110, 97); txtMatKhau.Size = new Size(140, 27); txtMatKhau.UseSystemPasswordChar = true; txtMatKhau.TabIndex = 2;
        lblVTCap.AutoSize = true; lblVTCap.Location = new Point(262, 100); lblVTCap.Text = "Vai trò";
        cboVaiTroCap.Location = new Point(320, 97); cboVaiTroCap.Size = new Size(95, 28); cboVaiTroCap.DropDownStyle = ComboBoxStyle.DropDownList; cboVaiTroCap.TabIndex = 3;
        btnCapTK.Location = new Point(110, 133); btnCapTK.Size = new Size(140, 32); btnCapTK.Text = "Cấp tài khoản"; btnCapTK.TabIndex = 4; btnCapTK.Click += btnCapTK_Click;

        grpSua.Text = "Tài khoản đang chọn"; grpSua.Location = new Point(445, 5); grpSua.Size = new Size(260, 175);
        grpSua.Controls.AddRange(new Control[] { lblVTDoi, cboVaiTroDoi, btnDoiVaiTro, btnKhoaMo, btnResetMK });
        lblVTDoi.AutoSize = true; lblVTDoi.Location = new Point(10, 30); lblVTDoi.Text = "Vai trò";
        cboVaiTroDoi.Location = new Point(70, 27); cboVaiTroDoi.Size = new Size(95, 28); cboVaiTroDoi.DropDownStyle = ComboBoxStyle.DropDownList; cboVaiTroDoi.TabIndex = 5;
        btnDoiVaiTro.Location = new Point(172, 25); btnDoiVaiTro.Size = new Size(80, 31); btnDoiVaiTro.Text = "Đổi"; btnDoiVaiTro.TabIndex = 6; btnDoiVaiTro.Click += btnDoiVaiTro_Click;
        btnKhoaMo.Location = new Point(10, 70); btnKhoaMo.Size = new Size(240, 32); btnKhoaMo.Text = "Khóa tài khoản"; btnKhoaMo.TabIndex = 7; btnKhoaMo.Click += btnKhoaMo_Click;
        btnResetMK.Location = new Point(10, 112); btnResetMK.Size = new Size(240, 32); btnResetMK.Text = "Đặt lại mật khẩu"; btnResetMK.TabIndex = 8; btnResetMK.Click += btnResetMK_Click;

        // Form
        AutoScaleDimensions = new SizeF(8F, 20F); AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(760, 600); MinimumSize = new Size(776, 520);
        Controls.Add(tabs);
        Name = "FrmNhanVien"; Text = "Nhân viên && tài khoản"; StartPosition = FormStartPosition.CenterParent;

        grpCap.ResumeLayout(false); grpCap.PerformLayout(); grpSua.ResumeLayout(false); grpSua.PerformLayout();
        pnlTim.ResumeLayout(false); pnlTim.PerformLayout(); pnlNhapNV.ResumeLayout(false); pnlNhapNV.PerformLayout();
        pnlTK.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvNV).EndInit();
        ((System.ComponentModel.ISupportInitialize)dgvTK).EndInit();
        tabNV.ResumeLayout(false); tabTK.ResumeLayout(false); tabs.ResumeLayout(false); ResumeLayout(false);
    }

    // cau hinh chung cho 2 luoi: chi doc, chon ca dong
    private static void CauHinhLuoi(DataGridView g)
    {
        g.Dock = DockStyle.Fill; g.ReadOnly = true; g.AllowUserToAddRows = false; g.AllowUserToDeleteRows = false;
        g.MultiSelect = false; g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; g.RowHeadersVisible = false;
        g.BackgroundColor = SystemColors.Window;
    }

    #endregion

    private TabControl tabs; private TabPage tabNV, tabTK;
    private Panel pnlTim, pnlNhapNV, pnlTK; private Label lblTim; private TextBox txtTimKiem; private Button btnTim;
    private DataGridView dgvNV, dgvTK;
    private Label lblHoTen, lblSDT, lblEmail, lblDiaChi, lblChucVu, lblLuong, lblNgay;
    private TextBox txtHoTen, txtSDT, txtEmail, txtDiaChi, txtLuong;
    private ComboBox cboChucVu; private DateTimePicker dtpNgayVaoLam;
    private Button btnThemNV, btnSuaNV, btnXoaNV, btnLamMoiNV;
    private GroupBox grpCap, grpSua;
    private Label lblNV, lblTenDN, lblMK, lblVTCap, lblVTDoi;
    private ComboBox cboNhanVien, cboVaiTroCap, cboVaiTroDoi; private TextBox txtTenDN, txtMatKhau;
    private Button btnCapTK, btnDoiVaiTro, btnKhoaMo, btnResetMK;
}
