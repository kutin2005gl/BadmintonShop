namespace BadmintonShop.Desktop;

partial class FrmSaoLuu
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
        lblCSDL = new Label(); lblTrangThai = new Label();
        grpSaoLuu = new GroupBox(); lblThuMuc = new Label(); txtThuMuc = new TextBox(); btnChonThuMuc = new Button(); btnSaoLuu = new Button();
        grpPhucHoi = new GroupBox(); lblFile = new Label(); txtFileBak = new TextBox(); btnChonFile = new Button(); btnPhucHoi = new Button(); lblCanhBao = new Label();
        grpSaoLuu.SuspendLayout(); grpPhucHoi.SuspendLayout(); SuspendLayout();

        lblCSDL.AutoSize = true; lblCSDL.Location = new Point(15, 12); lblCSDL.Font = new Font(Font.FontFamily, 10F, FontStyle.Bold);

        // grpSaoLuu
        grpSaoLuu.Text = "Sao lưu"; grpSaoLuu.Location = new Point(12, 42); grpSaoLuu.Size = new Size(606, 115);
        grpSaoLuu.Controls.AddRange(new Control[] { lblThuMuc, txtThuMuc, btnChonThuMuc, btnSaoLuu });
        lblThuMuc.AutoSize = true; lblThuMuc.Location = new Point(12, 34); lblThuMuc.Text = "Thư mục lưu";
        txtThuMuc.Location = new Point(110, 31); txtThuMuc.Size = new Size(390, 27); txtThuMuc.TabIndex = 0;
        btnChonThuMuc.Location = new Point(508, 29); btnChonThuMuc.Size = new Size(85, 31); btnChonThuMuc.Text = "Chọn..."; btnChonThuMuc.TabIndex = 1;
        btnChonThuMuc.Click += btnChonThuMuc_Click;
        btnSaoLuu.Location = new Point(110, 68); btnSaoLuu.Size = new Size(150, 34); btnSaoLuu.Text = "Sao lưu ngay"; btnSaoLuu.TabIndex = 2;
        btnSaoLuu.Click += btnSaoLuu_Click;

        // grpPhucHoi
        grpPhucHoi.Text = "Phục hồi"; grpPhucHoi.Location = new Point(12, 167); grpPhucHoi.Size = new Size(606, 140);
        grpPhucHoi.Controls.AddRange(new Control[] { lblFile, txtFileBak, btnChonFile, lblCanhBao, btnPhucHoi });
        lblFile.AutoSize = true; lblFile.Location = new Point(12, 34); lblFile.Text = "File .bak";
        txtFileBak.Location = new Point(110, 31); txtFileBak.Size = new Size(390, 27); txtFileBak.TabIndex = 3;
        btnChonFile.Location = new Point(508, 29); btnChonFile.Size = new Size(85, 31); btnChonFile.Text = "Chọn..."; btnChonFile.TabIndex = 4;
        btnChonFile.Click += btnChonFile_Click;
        lblCanhBao.AutoSize = true; lblCanhBao.Location = new Point(110, 64); lblCanhBao.ForeColor = Color.Firebrick;
        lblCanhBao.Text = "Phục hồi sẽ ghi đè toàn bộ dữ liệu hiện tại.";
        btnPhucHoi.Location = new Point(110, 92); btnPhucHoi.Size = new Size(150, 34); btnPhucHoi.Text = "Phục hồi"; btnPhucHoi.TabIndex = 5;
        btnPhucHoi.Click += btnPhucHoi_Click;

        lblTrangThai.AutoSize = true; lblTrangThai.Location = new Point(15, 318); lblTrangThai.ForeColor = Color.DimGray;

        // Form
        AutoScaleDimensions = new SizeF(8F, 20F); AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(630, 348); FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
        Controls.AddRange(new Control[] { lblCSDL, grpSaoLuu, grpPhucHoi, lblTrangThai });
        Name = "FrmSaoLuu"; Text = "Sao lưu - phục hồi CSDL"; StartPosition = FormStartPosition.CenterParent;
        grpSaoLuu.ResumeLayout(false); grpSaoLuu.PerformLayout(); grpPhucHoi.ResumeLayout(false); grpPhucHoi.PerformLayout();
        ResumeLayout(false); PerformLayout();
    }

    #endregion

    private Label lblCSDL, lblTrangThai, lblThuMuc, lblFile, lblCanhBao;
    private GroupBox grpSaoLuu, grpPhucHoi; private TextBox txtThuMuc, txtFileBak;
    private Button btnChonThuMuc, btnSaoLuu, btnChonFile, btnPhucHoi;
}
