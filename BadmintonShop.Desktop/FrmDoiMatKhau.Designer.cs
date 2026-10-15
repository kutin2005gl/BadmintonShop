namespace BadmintonShop.Desktop;

partial class FrmDoiMatKhau
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
        lblMatKhauCu = new Label();
        txtMatKhauCu = new TextBox();
        lblMatKhauMoi = new Label();
        txtMatKhauMoi = new TextBox();
        lblXacNhan = new Label();
        txtXacNhan = new TextBox();
        lblGoiY = new Label();
        btnLuu = new Button();
        btnHuy = new Button();
        SuspendLayout();
        //
        // lblMatKhauCu
        //
        lblMatKhauCu.AutoSize = true;
        lblMatKhauCu.Location = new Point(25, 28);
        lblMatKhauCu.Name = "lblMatKhauCu";
        lblMatKhauCu.Text = "Mật khẩu hiện tại";
        //
        // txtMatKhauCu
        //
        txtMatKhauCu.Location = new Point(175, 25);
        txtMatKhauCu.Name = "txtMatKhauCu";
        txtMatKhauCu.Size = new Size(200, 27);
        txtMatKhauCu.TabIndex = 0;
        txtMatKhauCu.UseSystemPasswordChar = true;
        //
        // lblMatKhauMoi
        //
        lblMatKhauMoi.AutoSize = true;
        lblMatKhauMoi.Location = new Point(25, 68);
        lblMatKhauMoi.Name = "lblMatKhauMoi";
        lblMatKhauMoi.Text = "Mật khẩu mới";
        //
        // txtMatKhauMoi
        //
        txtMatKhauMoi.Location = new Point(175, 65);
        txtMatKhauMoi.Name = "txtMatKhauMoi";
        txtMatKhauMoi.Size = new Size(200, 27);
        txtMatKhauMoi.TabIndex = 1;
        txtMatKhauMoi.UseSystemPasswordChar = true;
        //
        // lblXacNhan
        //
        lblXacNhan.AutoSize = true;
        lblXacNhan.Location = new Point(25, 108);
        lblXacNhan.Name = "lblXacNhan";
        lblXacNhan.Text = "Nhập lại mật khẩu mới";
        //
        // txtXacNhan
        //
        txtXacNhan.Location = new Point(175, 105);
        txtXacNhan.Name = "txtXacNhan";
        txtXacNhan.Size = new Size(200, 27);
        txtXacNhan.TabIndex = 2;
        txtXacNhan.UseSystemPasswordChar = true;
        //
        // lblGoiY
        //
        lblGoiY.ForeColor = Color.DimGray;
        lblGoiY.Location = new Point(25, 140);
        lblGoiY.Name = "lblGoiY";
        lblGoiY.Size = new Size(350, 40);
        lblGoiY.Text = "Tối thiểu 8 ký tự, có chữ hoa, chữ thường và chữ số.";
        //
        // btnLuu
        //
        btnLuu.Location = new Point(175, 185);
        btnLuu.Name = "btnLuu";
        btnLuu.Size = new Size(95, 34);
        btnLuu.TabIndex = 3;
        btnLuu.Text = "Lưu";
        btnLuu.Click += btnLuu_Click;
        //
        // btnHuy
        //
        btnHuy.DialogResult = DialogResult.Cancel;
        btnHuy.Location = new Point(280, 185);
        btnHuy.Name = "btnHuy";
        btnHuy.Size = new Size(95, 34);
        btnHuy.TabIndex = 4;
        btnHuy.Text = "Hủy";
        //
        // FrmDoiMatKhau
        //
        AcceptButton = btnLuu;
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnHuy;
        ClientSize = new Size(400, 240);
        Controls.Add(lblMatKhauCu);
        Controls.Add(txtMatKhauCu);
        Controls.Add(lblMatKhauMoi);
        Controls.Add(txtMatKhauMoi);
        Controls.Add(lblXacNhan);
        Controls.Add(txtXacNhan);
        Controls.Add(lblGoiY);
        Controls.Add(btnLuu);
        Controls.Add(btnHuy);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "FrmDoiMatKhau";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Đổi mật khẩu";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblMatKhauCu;
    private TextBox txtMatKhauCu;
    private Label lblMatKhauMoi;
    private TextBox txtMatKhauMoi;
    private Label lblXacNhan;
    private TextBox txtXacNhan;
    private Label lblGoiY;
    private Button btnLuu;
    private Button btnHuy;
}
