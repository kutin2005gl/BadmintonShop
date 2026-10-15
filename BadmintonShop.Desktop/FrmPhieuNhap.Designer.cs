namespace BadmintonShop.Desktop;

partial class FrmPhieuNhap
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
        pnlDau = new Panel(); lblNCC = new Label(); cboNCC = new ComboBox(); lblNgay = new Label(); dtpNgayNhap = new DateTimePicker();
        lblNguoiLap = new Label();
        grpDong = new GroupBox(); lblSP = new Label(); cboSanPham = new ComboBox(); lblSL = new Label(); numSoLuong = new NumericUpDown();
        lblGia = new Label(); numDonGia = new NumericUpDown(); btnThemDong = new Button(); btnXoaDong = new Button();
        dgvChiTiet = new DataGridView();
        pnlCuoi = new Panel(); lblTongTien = new Label(); btnLuu = new Button(); btnHuy = new Button();
        pnlDau.SuspendLayout(); grpDong.SuspendLayout(); pnlCuoi.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numSoLuong).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numDonGia).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dgvChiTiet).BeginInit();
        SuspendLayout();

        // pnlDau: thong tin phieu + nhap dong
        pnlDau.Dock = DockStyle.Top; pnlDau.Height = 165;
        pnlDau.Controls.AddRange(new Control[] { lblNCC, cboNCC, lblNgay, dtpNgayNhap, lblNguoiLap, grpDong });
        lblNCC.AutoSize = true; lblNCC.Location = new Point(12, 15); lblNCC.Text = "Nhà cung cấp (*)";
        cboNCC.Location = new Point(135, 12); cboNCC.Size = new Size(280, 28); cboNCC.DropDownStyle = ComboBoxStyle.DropDownList; cboNCC.TabIndex = 0;
        lblNgay.AutoSize = true; lblNgay.Location = new Point(430, 15); lblNgay.Text = "Ngày nhập";
        dtpNgayNhap.Location = new Point(515, 12); dtpNgayNhap.Size = new Size(185, 27); dtpNgayNhap.TabIndex = 1;
        dtpNgayNhap.Format = DateTimePickerFormat.Custom; dtpNgayNhap.CustomFormat = "dd/MM/yyyy HH:mm";
        lblNguoiLap.AutoSize = true; lblNguoiLap.Location = new Point(715, 15); lblNguoiLap.ForeColor = Color.DimGray;

        grpDong.Text = "Thêm sản phẩm vào phiếu"; grpDong.Location = new Point(12, 50); grpDong.Size = new Size(906, 105);
        grpDong.Controls.AddRange(new Control[] { lblSP, cboSanPham, lblSL, numSoLuong, lblGia, numDonGia, btnThemDong, btnXoaDong });
        lblSP.AutoSize = true; lblSP.Location = new Point(10, 32); lblSP.Text = "Sản phẩm";
        cboSanPham.Location = new Point(90, 29); cboSanPham.Size = new Size(420, 28); cboSanPham.TabIndex = 2;
        cboSanPham.DropDownStyle = ComboBoxStyle.DropDown;
        cboSanPham.AutoCompleteMode = AutoCompleteMode.SuggestAppend; cboSanPham.AutoCompleteSource = AutoCompleteSource.ListItems;
        cboSanPham.SelectionChangeCommitted += cboSanPham_SelectionChangeCommitted;
        lblSL.AutoSize = true; lblSL.Location = new Point(525, 32); lblSL.Text = "SL";
        numSoLuong.Location = new Point(555, 29); numSoLuong.Size = new Size(80, 27); numSoLuong.TabIndex = 3;
        numSoLuong.Minimum = 1; numSoLuong.Maximum = 100000; numSoLuong.Value = 1; numSoLuong.ThousandsSeparator = true;
        lblGia.AutoSize = true; lblGia.Location = new Point(650, 32); lblGia.Text = "Đơn giá";
        numDonGia.Location = new Point(715, 29); numDonGia.Size = new Size(175, 27); numDonGia.TabIndex = 4;
        numDonGia.Maximum = 1000000000; numDonGia.Increment = 1000; numDonGia.ThousandsSeparator = true;
        btnThemDong.Location = new Point(90, 64); btnThemDong.Size = new Size(150, 32); btnThemDong.Text = "Thêm vào phiếu"; btnThemDong.TabIndex = 5;
        btnThemDong.Click += btnThemDong_Click;
        btnXoaDong.Location = new Point(250, 64); btnXoaDong.Size = new Size(150, 32); btnXoaDong.Text = "Xóa dòng đang chọn"; btnXoaDong.TabIndex = 6;
        btnXoaDong.Click += btnXoaDong_Click;

        // dgvChiTiet
        dgvChiTiet.Dock = DockStyle.Fill; dgvChiTiet.AllowUserToAddRows = false; dgvChiTiet.AllowUserToDeleteRows = false;
        dgvChiTiet.MultiSelect = false; dgvChiTiet.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvChiTiet.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; dgvChiTiet.RowHeadersVisible = false;
        dgvChiTiet.BackgroundColor = SystemColors.Window; dgvChiTiet.TabIndex = 7;
        dgvChiTiet.CellValidating += dgvChiTiet_CellValidating;
        dgvChiTiet.CellEndEdit += dgvChiTiet_CellEndEdit;

        // pnlCuoi: tong tien + luu
        pnlCuoi.Dock = DockStyle.Bottom; pnlCuoi.Height = 60;
        pnlCuoi.Controls.AddRange(new Control[] { lblTongTien, btnLuu, btnHuy });
        lblTongTien.AutoSize = true; lblTongTien.Location = new Point(12, 18);
        lblTongTien.Font = new Font(Font.FontFamily, 12F, FontStyle.Bold); lblTongTien.ForeColor = Color.DarkRed;
        btnLuu.Anchor = AnchorStyles.Top | AnchorStyles.Right; btnLuu.Location = new Point(690, 12); btnLuu.Size = new Size(120, 36);
        btnLuu.Text = "Lưu phiếu"; btnLuu.TabIndex = 8; btnLuu.Click += btnLuu_Click;
        btnHuy.Anchor = AnchorStyles.Top | AnchorStyles.Right; btnHuy.Location = new Point(818, 12); btnHuy.Size = new Size(100, 36);
        btnHuy.Text = "Đóng"; btnHuy.TabIndex = 9; btnHuy.DialogResult = DialogResult.Cancel;

        // Form
        AutoScaleDimensions = new SizeF(8F, 20F); AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(930, 560); MinimumSize = new Size(946, 450);
        Controls.Add(dgvChiTiet); Controls.Add(pnlCuoi); Controls.Add(pnlDau);
        CancelButton = btnHuy;
        Name = "FrmPhieuNhap"; Text = "Lập phiếu nhập hàng"; StartPosition = FormStartPosition.CenterParent;
        FormClosing += FrmPhieuNhap_FormClosing;

        pnlDau.ResumeLayout(false); pnlDau.PerformLayout(); grpDong.ResumeLayout(false); grpDong.PerformLayout();
        pnlCuoi.ResumeLayout(false); pnlCuoi.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numSoLuong).EndInit();
        ((System.ComponentModel.ISupportInitialize)numDonGia).EndInit();
        ((System.ComponentModel.ISupportInitialize)dgvChiTiet).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlDau, pnlCuoi; private Label lblNCC, lblNgay, lblNguoiLap, lblSP, lblSL, lblGia, lblTongTien;
    private ComboBox cboNCC, cboSanPham; private DateTimePicker dtpNgayNhap;
    private GroupBox grpDong; private NumericUpDown numSoLuong, numDonGia;
    private Button btnThemDong, btnXoaDong, btnLuu, btnHuy; private DataGridView dgvChiTiet;
}
