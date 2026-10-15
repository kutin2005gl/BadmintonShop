namespace BadmintonShop.Desktop;

partial class FrmNhaCungCap
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
        pnlTim = new Panel(); lblTim = new Label(); txtTimKiem = new TextBox(); btnTim = new Button();
        dgvNCC = new DataGridView();
        pnlNhap = new Panel();
        lblTen = new Label(); txtTen = new TextBox();
        lblDiaChi = new Label(); txtDiaChi = new TextBox();
        lblSDT = new Label(); txtSDT = new TextBox();
        lblEmail = new Label(); txtEmail = new TextBox();
        btnThem = new Button(); btnSua = new Button(); btnXoa = new Button(); btnLamMoi = new Button();
        pnlTim.SuspendLayout(); ((System.ComponentModel.ISupportInitialize)dgvNCC).BeginInit();
        pnlNhap.SuspendLayout(); SuspendLayout();
        // pnlTim
        pnlTim.Dock = DockStyle.Top; pnlTim.Height = 50;
        pnlTim.Controls.AddRange(new Control[] { lblTim, txtTimKiem, btnTim });
        lblTim.AutoSize = true; lblTim.Location = new Point(15, 16); lblTim.Text = "Tìm (tên / SĐT)";
        txtTimKiem.Location = new Point(135, 13); txtTimKiem.Size = new Size(280, 27); txtTimKiem.TabIndex = 0;
        txtTimKiem.KeyDown += txtTimKiem_KeyDown;
        btnTim.Location = new Point(425, 11); btnTim.Size = new Size(90, 31); btnTim.Text = "Tìm"; btnTim.TabIndex = 1;
        btnTim.Click += btnTim_Click;
        // dgvNCC
        dgvNCC.Dock = DockStyle.Fill; dgvNCC.ReadOnly = true; dgvNCC.AllowUserToAddRows = false;
        dgvNCC.AllowUserToDeleteRows = false; dgvNCC.MultiSelect = false;
        dgvNCC.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvNCC.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvNCC.RowHeadersVisible = false; dgvNCC.BackgroundColor = SystemColors.Window;
        dgvNCC.SelectionChanged += dgvNCC_SelectionChanged;
        // pnlNhap
        pnlNhap.Dock = DockStyle.Bottom; pnlNhap.Height = 175;
        pnlNhap.Controls.AddRange(new Control[] { lblTen, txtTen, lblDiaChi, txtDiaChi, lblSDT, txtSDT, lblEmail, txtEmail, btnThem, btnSua, btnXoa, btnLamMoi });
        lblTen.AutoSize = true; lblTen.Location = new Point(15, 18); lblTen.Text = "Tên NCC (*)";
        txtTen.Location = new Point(110, 15); txtTen.Size = new Size(300, 27); txtTen.MaxLength = 150; txtTen.TabIndex = 2;
        lblSDT.AutoSize = true; lblSDT.Location = new Point(430, 18); lblSDT.Text = "SĐT";
        txtSDT.Location = new Point(480, 15); txtSDT.Size = new Size(180, 27); txtSDT.MaxLength = 11; txtSDT.TabIndex = 3;
        lblDiaChi.AutoSize = true; lblDiaChi.Location = new Point(15, 58); lblDiaChi.Text = "Địa chỉ";
        txtDiaChi.Location = new Point(110, 55); txtDiaChi.Size = new Size(300, 27); txtDiaChi.MaxLength = 250; txtDiaChi.TabIndex = 4;
        lblEmail.AutoSize = true; lblEmail.Location = new Point(430, 58); lblEmail.Text = "Email";
        txtEmail.Location = new Point(480, 55); txtEmail.Size = new Size(180, 27); txtEmail.MaxLength = 100; txtEmail.TabIndex = 5;
        btnThem.Location = new Point(110, 105); btnThem.Size = new Size(95, 34); btnThem.Text = "Thêm"; btnThem.TabIndex = 6; btnThem.Click += btnThem_Click;
        btnSua.Location = new Point(215, 105); btnSua.Size = new Size(95, 34); btnSua.Text = "Lưu sửa"; btnSua.TabIndex = 7; btnSua.Click += btnSua_Click;
        btnXoa.Location = new Point(320, 105); btnXoa.Size = new Size(95, 34); btnXoa.Text = "Xóa"; btnXoa.TabIndex = 8; btnXoa.Click += btnXoa_Click;
        btnLamMoi.Location = new Point(425, 105); btnLamMoi.Size = new Size(95, 34); btnLamMoi.Text = "Làm mới"; btnLamMoi.TabIndex = 9; btnLamMoi.Click += btnLamMoi_Click;
        // Form
        AutoScaleDimensions = new SizeF(8F, 20F); AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(690, 520); MinimumSize = new Size(706, 400);
        Controls.Add(dgvNCC); Controls.Add(pnlNhap); Controls.Add(pnlTim);
        Name = "FrmNhaCungCap"; Text = "Quản lý nhà cung cấp"; StartPosition = FormStartPosition.CenterParent;
        pnlTim.ResumeLayout(false); pnlTim.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvNCC).EndInit();
        pnlNhap.ResumeLayout(false); pnlNhap.PerformLayout(); ResumeLayout(false);
    }

    #endregion

    private Panel pnlTim; private Label lblTim; private TextBox txtTimKiem; private Button btnTim;
    private DataGridView dgvNCC; private Panel pnlNhap;
    private Label lblTen, lblDiaChi, lblSDT, lblEmail;
    private TextBox txtTen, txtDiaChi, txtSDT, txtEmail;
    private Button btnThem, btnSua, btnXoa, btnLamMoi;
}
