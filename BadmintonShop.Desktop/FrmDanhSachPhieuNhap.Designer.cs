namespace BadmintonShop.Desktop;

partial class FrmDanhSachPhieuNhap
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
        pnlLoc = new Panel(); lblTu = new Label(); dtpTuNgay = new DateTimePicker(); lblDen = new Label(); dtpDenNgay = new DateTimePicker();
        lblNCC = new Label(); cboNCC = new ComboBox(); btnLoc = new Button(); btnLapPhieu = new Button();
        split = new SplitContainer(); dgvPhieu = new DataGridView(); lblTongKet = new Label();
        grpChiTiet = new GroupBox(); dgvChiTiet = new DataGridView();
        pnlLoc.SuspendLayout(); ((System.ComponentModel.ISupportInitialize)split).BeginInit();
        split.Panel1.SuspendLayout(); split.Panel2.SuspendLayout(); split.SuspendLayout(); grpChiTiet.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvPhieu).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dgvChiTiet).BeginInit();
        SuspendLayout();

        // pnlLoc
        pnlLoc.Dock = DockStyle.Top; pnlLoc.Height = 52;
        pnlLoc.Controls.AddRange(new Control[] { lblTu, dtpTuNgay, lblDen, dtpDenNgay, lblNCC, cboNCC, btnLoc, btnLapPhieu });
        lblTu.AutoSize = true; lblTu.Location = new Point(12, 16); lblTu.Text = "Từ ngày";
        dtpTuNgay.Location = new Point(80, 13); dtpTuNgay.Size = new Size(150, 27); dtpTuNgay.Format = DateTimePickerFormat.Short;
        dtpTuNgay.ShowCheckBox = true; dtpTuNgay.TabIndex = 0;
        lblDen.AutoSize = true; lblDen.Location = new Point(245, 16); lblDen.Text = "Đến ngày";
        dtpDenNgay.Location = new Point(320, 13); dtpDenNgay.Size = new Size(150, 27); dtpDenNgay.Format = DateTimePickerFormat.Short;
        dtpDenNgay.ShowCheckBox = true; dtpDenNgay.TabIndex = 1;
        lblNCC.AutoSize = true; lblNCC.Location = new Point(485, 16); lblNCC.Text = "NCC";
        cboNCC.Location = new Point(525, 13); cboNCC.Size = new Size(220, 28); cboNCC.DropDownStyle = ComboBoxStyle.DropDownList; cboNCC.TabIndex = 2;
        btnLoc.Location = new Point(755, 11); btnLoc.Size = new Size(80, 31); btnLoc.Text = "Lọc"; btnLoc.TabIndex = 3; btnLoc.Click += btnLoc_Click;
        btnLapPhieu.Anchor = AnchorStyles.Top | AnchorStyles.Right; btnLapPhieu.Location = new Point(850, 11); btnLapPhieu.Size = new Size(150, 31);
        btnLapPhieu.Text = "+ Lập phiếu nhập"; btnLapPhieu.TabIndex = 4; btnLapPhieu.Click += btnLapPhieu_Click;

        // split: tren = danh sach phieu, duoi = chi tiet
        split.Dock = DockStyle.Fill; split.Orientation = Orientation.Horizontal; split.SplitterDistance = 270;
        split.Panel1.Controls.Add(dgvPhieu); split.Panel1.Controls.Add(lblTongKet);
        split.Panel2.Controls.Add(grpChiTiet);
        CauHinhLuoi(dgvPhieu); dgvPhieu.CurrentCellChanged += dgvPhieu_SelectionChanged;
        lblTongKet.Dock = DockStyle.Bottom; lblTongKet.Height = 26; lblTongKet.TextAlign = ContentAlignment.MiddleRight;
        lblTongKet.Font = new Font(Font.FontFamily, 10F, FontStyle.Bold); lblTongKet.Padding = new Padding(0, 0, 10, 0);
        grpChiTiet.Dock = DockStyle.Fill; grpChiTiet.Text = "Chi tiết phiếu nhập"; grpChiTiet.Controls.Add(dgvChiTiet);
        CauHinhLuoi(dgvChiTiet);

        // Form
        AutoScaleDimensions = new SizeF(8F, 20F); AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1012, 600); MinimumSize = new Size(1028, 450);
        Controls.Add(split); Controls.Add(pnlLoc);
        Name = "FrmDanhSachPhieuNhap"; Text = "Phiếu nhập hàng"; StartPosition = FormStartPosition.CenterParent;

        pnlLoc.ResumeLayout(false); pnlLoc.PerformLayout(); grpChiTiet.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvPhieu).EndInit();
        ((System.ComponentModel.ISupportInitialize)dgvChiTiet).EndInit();
        split.Panel1.ResumeLayout(false); split.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)split).EndInit(); split.ResumeLayout(false);
        ResumeLayout(false);
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

    private Panel pnlLoc; private Label lblTu, lblDen, lblNCC, lblTongKet;
    private DateTimePicker dtpTuNgay, dtpDenNgay; private ComboBox cboNCC; private Button btnLoc, btnLapPhieu;
    private SplitContainer split; private DataGridView dgvPhieu, dgvChiTiet; private GroupBox grpChiTiet;
}
