namespace BadmintonShop.Desktop;

partial class FrmMain
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
        menuStrip = new MenuStrip();
        mnuHeThong = new ToolStripMenuItem();
        mnuDoiMatKhau = new ToolStripMenuItem();
        mnuSaoLuu = new ToolStripMenuItem();
        mnuSep1 = new ToolStripSeparator();
        mnuDangXuat = new ToolStripMenuItem();
        mnuThoat = new ToolStripMenuItem();
        mnuNhanSu = new ToolStripMenuItem();
        mnuNhanVien = new ToolStripMenuItem();
        mnuNhapHang = new ToolStripMenuItem();
        mnuNhaCungCap = new ToolStripMenuItem();
        mnuPhieuNhap = new ToolStripMenuItem();
        mnuSanPham = new ToolStripMenuItem();
        mnuDsSanPham = new ToolStripMenuItem();
        mnuDanhMuc = new ToolStripMenuItem();
        mnuKhuyenMai = new ToolStripMenuItem();
        mnuBanHang = new ToolStripMenuItem();
        mnuPOS = new ToolStripMenuItem();
        mnuDonHang = new ToolStripMenuItem();
        mnuBaoCao = new ToolStripMenuItem();
        mnuThongKe = new ToolStripMenuItem();
        mnuDuyetBinhLuan = new ToolStripMenuItem();
        statusStrip = new StatusStrip();
        lblNguoiDung = new ToolStripStatusLabel();
        lblVaiTro = new ToolStripStatusLabel();
        lblChao = new Label();
        menuStrip.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        //
        // menuStrip
        //
        menuStrip.ImageScalingSize = new Size(20, 20);
        menuStrip.Items.AddRange(new ToolStripItem[] { mnuHeThong, mnuNhanSu, mnuNhapHang, mnuSanPham, mnuBanHang, mnuBaoCao });
        menuStrip.Location = new Point(0, 0);
        menuStrip.Name = "menuStrip";
        menuStrip.Size = new Size(1000, 28);
        //
        // mnuHeThong
        //
        mnuHeThong.DropDownItems.AddRange(new ToolStripItem[] { mnuDoiMatKhau, mnuSaoLuu, mnuSep1, mnuDangXuat, mnuThoat });
        mnuHeThong.Name = "mnuHeThong";
        mnuHeThong.Text = "&Hệ thống";
        //
        // mnuDoiMatKhau
        //
        mnuDoiMatKhau.Name = "mnuDoiMatKhau";
        mnuDoiMatKhau.Text = "Đổi mật khẩu";
        mnuDoiMatKhau.Click += mnuDoiMatKhau_Click;
        //
        // mnuSaoLuu
        //
        mnuSaoLuu.Name = "mnuSaoLuu";
        mnuSaoLuu.Text = "Sao lưu - phục hồi CSDL";
        mnuSaoLuu.Click += mnuChuaCo_Click;
        //
        // mnuDangXuat
        //
        mnuDangXuat.Name = "mnuDangXuat";
        mnuDangXuat.Text = "Đăng xuất";
        mnuDangXuat.Click += mnuDangXuat_Click;
        //
        // mnuThoat
        //
        mnuThoat.Name = "mnuThoat";
        mnuThoat.Text = "Thoát";
        mnuThoat.Click += mnuThoat_Click;
        //
        // mnuNhanSu
        //
        mnuNhanSu.DropDownItems.AddRange(new ToolStripItem[] { mnuNhanVien });
        mnuNhanSu.Name = "mnuNhanSu";
        mnuNhanSu.Text = "&Nhân sự";
        //
        // mnuNhanVien
        //
        mnuNhanVien.Name = "mnuNhanVien";
        mnuNhanVien.Text = "Nhân viên && tài khoản";
        mnuNhanVien.Click += mnuChuaCo_Click;
        //
        // mnuNhapHang
        //
        mnuNhapHang.DropDownItems.AddRange(new ToolStripItem[] { mnuNhaCungCap, mnuPhieuNhap });
        mnuNhapHang.Name = "mnuNhapHang";
        mnuNhapHang.Text = "Nhập &hàng";
        //
        // mnuNhaCungCap
        //
        mnuNhaCungCap.Name = "mnuNhaCungCap";
        mnuNhaCungCap.Text = "Nhà cung cấp";
        mnuNhaCungCap.Click += mnuNhaCungCap_Click;
        //
        // mnuPhieuNhap
        //
        mnuPhieuNhap.Name = "mnuPhieuNhap";
        mnuPhieuNhap.Text = "Phiếu nhập";
        mnuPhieuNhap.Click += mnuChuaCo_Click;
        //
        // mnuSanPham
        //
        mnuSanPham.DropDownItems.AddRange(new ToolStripItem[] { mnuDsSanPham, mnuDanhMuc, mnuKhuyenMai });
        mnuSanPham.Name = "mnuSanPham";
        mnuSanPham.Text = "&Sản phẩm";
        //
        // mnuDsSanPham
        //
        mnuDsSanPham.Name = "mnuDsSanPham";
        mnuDsSanPham.Text = "Sản phẩm";
        mnuDsSanPham.Click += mnuChuaCo_Click;
        //
        // mnuDanhMuc
        //
        mnuDanhMuc.Name = "mnuDanhMuc";
        mnuDanhMuc.Text = "Danh mục";
        mnuDanhMuc.Click += mnuChuaCo_Click;
        //
        // mnuKhuyenMai
        //
        mnuKhuyenMai.Name = "mnuKhuyenMai";
        mnuKhuyenMai.Text = "Khuyến mãi";
        mnuKhuyenMai.Click += mnuChuaCo_Click;
        //
        // mnuBanHang
        //
        mnuBanHang.DropDownItems.AddRange(new ToolStripItem[] { mnuPOS, mnuDonHang });
        mnuBanHang.Name = "mnuBanHang";
        mnuBanHang.Text = "&Bán hàng";
        //
        // mnuPOS
        //
        mnuPOS.Name = "mnuPOS";
        mnuPOS.Text = "Bán hàng tại quầy (POS)";
        mnuPOS.Click += mnuChuaCo_Click;
        //
        // mnuDonHang
        //
        mnuDonHang.Name = "mnuDonHang";
        mnuDonHang.Text = "Đơn hàng";
        mnuDonHang.Click += mnuChuaCo_Click;
        //
        // mnuBaoCao
        //
        mnuBaoCao.DropDownItems.AddRange(new ToolStripItem[] { mnuThongKe, mnuDuyetBinhLuan });
        mnuBaoCao.Name = "mnuBaoCao";
        mnuBaoCao.Text = "Báo &cáo";
        //
        // mnuThongKe
        //
        mnuThongKe.Name = "mnuThongKe";
        mnuThongKe.Text = "Thống kê - báo cáo";
        mnuThongKe.Click += mnuChuaCo_Click;
        //
        // mnuDuyetBinhLuan
        //
        mnuDuyetBinhLuan.Name = "mnuDuyetBinhLuan";
        mnuDuyetBinhLuan.Text = "Duyệt bình luận";
        mnuDuyetBinhLuan.Click += mnuChuaCo_Click;
        //
        // statusStrip
        //
        statusStrip.ImageScalingSize = new Size(20, 20);
        statusStrip.Items.AddRange(new ToolStripItem[] { lblNguoiDung, lblVaiTro });
        statusStrip.Location = new Point(0, 574);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1000, 26);
        //
        // lblNguoiDung
        //
        lblNguoiDung.Name = "lblNguoiDung";
        lblNguoiDung.Text = "Người dùng:";
        //
        // lblVaiTro
        //
        lblVaiTro.Name = "lblVaiTro";
        lblVaiTro.Text = "Vai trò:";
        //
        // lblChao
        //
        lblChao.Dock = DockStyle.Fill;
        lblChao.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        lblChao.ForeColor = Color.SeaGreen;
        lblChao.Name = "lblChao";
        lblChao.Text = "BADMINTON SHOP";
        lblChao.TextAlign = ContentAlignment.MiddleCenter;
        //
        // FrmMain
        //
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1000, 600);
        Controls.Add(lblChao);
        Controls.Add(statusStrip);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        Name = "FrmMain";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "BadmintonShop - Quản lý cửa hàng";
        WindowState = FormWindowState.Maximized;
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private MenuStrip menuStrip;
    private ToolStripMenuItem mnuHeThong;
    private ToolStripMenuItem mnuDoiMatKhau;
    private ToolStripMenuItem mnuSaoLuu;
    private ToolStripSeparator mnuSep1;
    private ToolStripMenuItem mnuDangXuat;
    private ToolStripMenuItem mnuThoat;
    private ToolStripMenuItem mnuNhanSu;
    private ToolStripMenuItem mnuNhanVien;
    private ToolStripMenuItem mnuNhapHang;
    private ToolStripMenuItem mnuNhaCungCap;
    private ToolStripMenuItem mnuPhieuNhap;
    private ToolStripMenuItem mnuSanPham;
    private ToolStripMenuItem mnuDsSanPham;
    private ToolStripMenuItem mnuDanhMuc;
    private ToolStripMenuItem mnuKhuyenMai;
    private ToolStripMenuItem mnuBanHang;
    private ToolStripMenuItem mnuPOS;
    private ToolStripMenuItem mnuDonHang;
    private ToolStripMenuItem mnuBaoCao;
    private ToolStripMenuItem mnuThongKe;
    private ToolStripMenuItem mnuDuyetBinhLuan;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblNguoiDung;
    private ToolStripStatusLabel lblVaiTro;
    private Label lblChao;
}
