namespace BadmintonShop.Desktop;

partial class FrmDangNhap
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    // Bo cuc va mau sac duoc dung trong FrmDangNhap.DungGiaoDien() theo ShopTheme (giong khung FrmMain).
    private void InitializeComponent()
    {
        lblTieuDe = new Label();
        lblTenDangNhap = new Label();
        txtTenDangNhap = new TextBox();
        lblMatKhau = new Label();
        txtMatKhau = new TextBox();
        chkHienMatKhau = new CheckBox();
        btnDangNhap = new Button();
        btnThoat = new Button();
        SuspendLayout();
        //
        // lblTieuDe
        //
        lblTieuDe.Name = "lblTieuDe";
        lblTieuDe.Text = "Đăng nhập";
        //
        // lblTenDangNhap
        //
        lblTenDangNhap.Name = "lblTenDangNhap";
        lblTenDangNhap.Text = "Tên đăng nhập";
        //
        // txtTenDangNhap
        //
        txtTenDangNhap.MaxLength = 50;
        txtTenDangNhap.Name = "txtTenDangNhap";
        txtTenDangNhap.TabIndex = 0;
        //
        // lblMatKhau
        //
        lblMatKhau.Name = "lblMatKhau";
        lblMatKhau.Text = "Mật khẩu";
        //
        // txtMatKhau
        //
        txtMatKhau.MaxLength = 100;
        txtMatKhau.Name = "txtMatKhau";
        txtMatKhau.TabIndex = 1;
        txtMatKhau.UseSystemPasswordChar = true;
        //
        // chkHienMatKhau
        //
        chkHienMatKhau.AutoSize = true;
        chkHienMatKhau.Name = "chkHienMatKhau";
        chkHienMatKhau.TabIndex = 2;
        chkHienMatKhau.Text = "Hiện mật khẩu";
        chkHienMatKhau.CheckedChanged += chkHienMatKhau_CheckedChanged;
        //
        // btnDangNhap
        //
        btnDangNhap.Name = "btnDangNhap";
        btnDangNhap.TabIndex = 3;
        btnDangNhap.Text = "Đăng nhập";
        btnDangNhap.UseVisualStyleBackColor = false;
        btnDangNhap.Click += btnDangNhap_Click;
        //
        // btnThoat
        //
        btnThoat.DialogResult = DialogResult.Cancel;
        btnThoat.Name = "btnThoat";
        btnThoat.TabIndex = 4;
        btnThoat.Text = "Thoát";
        //
        // FrmDangNhap
        //
        AcceptButton = btnDangNhap;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        CancelButton = btnThoat;
        ClientSize = new Size(780, 480);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "FrmDangNhap";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Đăng nhập hệ thống - Badminton Shop";
        ResumeLayout(false);
    }

    #endregion

    private Label lblTieuDe;
    private Label lblTenDangNhap;
    private TextBox txtTenDangNhap;
    private Label lblMatKhau;
    private TextBox txtMatKhau;
    private CheckBox chkHienMatKhau;
    private Button btnDangNhap;
    private Button btnThoat;
}
