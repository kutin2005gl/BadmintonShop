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
        lblTim = new Label(); txtTimKiem = new TextBox(); btnTim = new Button();
        dgvNCC = new DataGridView();
        txtTen = new TextBox(); txtDiaChi = new TextBox(); txtSDT = new TextBox(); txtEmail = new TextBox();
        btnThem = new Button(); btnSua = new Button(); btnXoa = new Button(); btnLamMoi = new Button();
        // Bố cục main-sub (danh sách trái, form nhập phải) được dựng trong DungBoCuc() của FrmNhaCungCap.cs
        txtTimKiem.TabIndex = 0; txtTimKiem.KeyDown += txtTimKiem_KeyDown;
        btnTim.Text = "Tìm"; btnTim.TabIndex = 1; btnTim.Click += btnTim_Click;
        lblTim.Text = "Tìm (tên / SĐT)";
        dgvNCC.ReadOnly = true; dgvNCC.AllowUserToAddRows = false; dgvNCC.AllowUserToDeleteRows = false; dgvNCC.MultiSelect = false;
        dgvNCC.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvNCC.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvNCC.RowHeadersVisible = false; dgvNCC.BackgroundColor = SystemColors.Window;
        dgvNCC.SelectionChanged += dgvNCC_SelectionChanged;
        txtTen.MaxLength = 150; txtTen.TabIndex = 2;
        txtSDT.MaxLength = 11; txtSDT.TabIndex = 3;
        txtDiaChi.MaxLength = 250; txtDiaChi.TabIndex = 4;
        txtEmail.MaxLength = 100; txtEmail.TabIndex = 5;
        btnThem.Text = "Thêm"; btnThem.TabIndex = 6; btnThem.Click += btnThem_Click;
        btnSua.Text = "Lưu sửa"; btnSua.TabIndex = 7; btnSua.Click += btnSua_Click;
        btnXoa.Text = "Xóa"; btnXoa.TabIndex = 8; btnXoa.Click += btnXoa_Click;
        btnLamMoi.Text = "Làm mới"; btnLamMoi.TabIndex = 9; btnLamMoi.Click += btnLamMoi_Click;
        // Form
        AutoScaleDimensions = new SizeF(8F, 20F); AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 600); MinimumSize = new Size(900, 450);
        Name = "FrmNhaCungCap"; Text = "Quản lý nhà cung cấp"; StartPosition = FormStartPosition.CenterParent;
        ((System.ComponentModel.ISupportInitialize)dgvNCC).EndInit();
    }

    #endregion

    private Label lblTim; private TextBox txtTimKiem; private Button btnTim;
    private DataGridView dgvNCC;
    private TextBox txtTen, txtDiaChi, txtSDT, txtEmail;
    private Button btnThem, btnSua, btnXoa, btnLamMoi;
}
