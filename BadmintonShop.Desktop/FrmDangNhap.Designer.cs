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
        lblTieuDe.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTieuDe.ForeColor = Color.SeaGreen;
        lblTieuDe.Location = new Point(20, 15);
        lblTieuDe.Name = "lblTieuDe";
        lblTieuDe.Size = new Size(340, 40);
        lblTieuDe.Text = "BADMINTON SHOP";
        lblTieuDe.TextAlign = ContentAlignment.MiddleCenter;
        //
        // lblTenDangNhap
        //
        lblTenDangNhap.AutoSize = true;
        lblTenDangNhap.Location = new Point(30, 78);
        lblTenDangNhap.Name = "lblTenDangNhap";
        lblTenDangNhap.Text = "Tên đăng nhập";
        //
        // txtTenDangNhap
        //
        txtTenDangNhap.Location = new Point(140, 75);
        txtTenDangNhap.MaxLength = 50;
        txtTenDangNhap.Name = "txtTenDangNhap";
        txtTenDangNhap.Size = new Size(210, 27);
        txtTenDangNhap.TabIndex = 0;
        //
        // lblMatKhau
        //
        lblMatKhau.AutoSize = true;
        lblMatKhau.Location = new Point(30, 118);
        lblMatKhau.Name = "lblMatKhau";
        lblMatKhau.Text = "Mật khẩu";
        //
        // txtMatKhau
        //
        txtMatKhau.Location = new Point(140, 115);
        txtMatKhau.MaxLength = 100;
        txtMatKhau.Name = "txtMatKhau";
        txtMatKhau.Size = new Size(210, 27);
        txtMatKhau.TabIndex = 1;
        txtMatKhau.UseSystemPasswordChar = true;
        //
        // chkHienMatKhau
        //
        chkHienMatKhau.AutoSize = true;
        chkHienMatKhau.Location = new Point(140, 150);
        chkHienMatKhau.Name = "chkHienMatKhau";
        chkHienMatKhau.TabIndex = 2;
        chkHienMatKhau.Text = "Hiện mật khẩu";
        chkHienMatKhau.CheckedChanged += chkHienMatKhau_CheckedChanged;
        //
        // btnDangNhap
        //
        btnDangNhap.BackColor = Color.SeaGreen;
        btnDangNhap.ForeColor = Color.White;
        btnDangNhap.Location = new Point(140, 190);
        btnDangNhap.Name = "btnDangNhap";
        btnDangNhap.Size = new Size(110, 34);
        btnDangNhap.TabIndex = 3;
        btnDangNhap.Text = "Đăng nhập";
        btnDangNhap.UseVisualStyleBackColor = false;
        btnDangNhap.Click += btnDangNhap_Click;
        //
        // btnThoat
        //
        btnThoat.DialogResult = DialogResult.Cancel;
        btnThoat.Location = new Point(260, 190);
        btnThoat.Name = "btnThoat";
        btnThoat.Size = new Size(90, 34);
        btnThoat.TabIndex = 4;
        btnThoat.Text = "Thoát";
        //
        // FrmDangNhap
        //
        AcceptButton = btnDangNhap;
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnThoat;
        ClientSize = new Size(380, 245);
        Controls.Add(lblTieuDe);
        Controls.Add(lblTenDangNhap);
        Controls.Add(txtTenDangNhap);
        Controls.Add(lblMatKhau);
        Controls.Add(txtMatKhau);
        Controls.Add(chkHienMatKhau);
        Controls.Add(btnDangNhap);
        Controls.Add(btnThoat);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "FrmDangNhap";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Đăng nhập hệ thống";
        ResumeLayout(false);
        PerformLayout();
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
