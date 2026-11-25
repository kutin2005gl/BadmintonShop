using System.Data;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

/// <summary>Danh sach phieu nhap: loc theo ngay / NCC, xem chi tiet, mo form lap phieu moi (SV1 - ROADMAP 3.1).
/// Bo cuc main-sub theo mau Don hang: danh sach phieu ben trai, chi tiet (the san pham) ben phai.</summary>
public class FrmDanhSachPhieuNhap : Form
{
    private readonly PhieuNhapRepository _pnRepo = new();
    private bool binding;
    private readonly BoNhoPhien<int,DataTable> details=new(BoNhoPhien<int,int>.ThoiHanMacDinh);

    private DateTimePicker dtpTuNgay = null!, dtpDenNgay = null!;
    private ComboBox cboNCC = null!;
    private DataGridView dgvPhieu = null!;
    private Label lblTongKet = null!, emptyState = null!, maPhieu = null!;
    private Panel detailBody = null!;
    private FlowLayoutPanel cards = null!;
    private readonly Dictionary<string, Label> values = new();

    // TV1: khoi tao form (chi giao dien, KHONG truy cap CSDL de VS Designer mo duoc)
    public FrmDanhSachPhieuNhap()
    {
        InitializeComponent();
    }

    // TV1: dung thanh loc, luoi phieu ben trai va panel chi tiet ben phai
    private void InitializeComponent()
    {
        Text = "Phiếu nhập hàng"; Size = new Size(1250, 800); MinimumSize = new Size(1000, 650); AutoScaleMode = AutoScaleMode.Font;
        dtpTuNgay = new DateTimePicker { Width = 160, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", ShowCheckBox = true };
        dtpDenNgay = new DateTimePicker { Width = 160, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", ShowCheckBox = true };
        cboNCC = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260, Margin = new Padding(4) };
        lblTongKet = Sv4Ui.Nhan("");
        var bar = Sv4Ui.ThanhCongCu();
        bar.Controls.AddRange(new Control[]
        {
            Sv4Ui.Nhan("Từ ngày"), dtpTuNgay, Sv4Ui.Nhan("Đến ngày"), dtpDenNgay, Sv4Ui.Nhan("NCC"), cboNCC,
            Sv4Ui.Nut("Lọc", () => NapDanhSach()), Sv4Ui.Nut("+ Lập phiếu nhập", LapPhieu), lblTongKet,
        });

        dgvPhieu = Sv4Ui.Luoi();
        dgvPhieu.CurrentCellChanged += (_, _) => ChiTiet();
        var split = KhungChiTiet.Chia(this, 0.65, 420, 360);
        split.Panel1.Controls.Add(dgvPhieu);
        XayChiTiet(split.Panel2);
        Controls.Add(split); Controls.Add(bar);
        ShopTheme.Apply(this);
        HienTrong("Chọn một phiếu nhập ở danh sách bên trái để xem chi tiết.");
    }

    // TV1: dung panel chi tiet ben phai: so phieu, thong tin phieu, danh sach the san pham nhap
    private void XayChiTiet(Control host)
    {
        var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(14, 12, 14, 12), BackColor = ShopTheme.Surface };
        emptyState = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = ShopTheme.Muted, Padding = new Padding(20) };
        detailBody = new Panel { Dock = DockStyle.Fill, Visible = false };
        var header = KhungChiTiet.TieuDe(out maPhieu, out var badge);
        badge.Visible = false;
        var info = KhungChiTiet.BangThongTin();
        foreach (var (key, label) in new[] { ("TenNCC", "Nhà cung cấp"), ("NgayNhap", "Ngày nhập"), ("NguoiLap", "Người lập"), ("TongTien", "Tổng giá trị") })
        {
            bool total = key == "TongTien";
            var value = new Label { AutoSize = true, Margin = new Padding(0, total ? 3 : 5, 0, total ? 3 : 5),
                Font = total ? new Font("Segoe UI", 13, FontStyle.Bold) : new Font("Segoe UI", 10), ForeColor = total ? ShopTheme.Green : ShopTheme.Ink };
            KhungChiTiet.Dong(info, label, value); values[key] = value;
        }
        info.SizeChanged += (_, _) => { foreach (var v in values.Values) v.MaximumSize = new Size(Math.Max(80, info.ClientSize.Width * 2 / 3), 0); };
        cards = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = ShopTheme.Surface, Padding = new Padding(0, 6, 0, 0) };
        cards.SizeChanged += (_, _) => XepThe();
        // Thứ tự thêm: Fill trước, các Top sau (Top thêm sau nằm cao hơn).
        detailBody.Controls.Add(cards); detailBody.Controls.Add(KhungChiTiet.Muc("Sản phẩm trong phiếu")); detailBody.Controls.Add(info); detailBody.Controls.Add(header);
        root.Controls.Add(detailBody); root.Controls.Add(emptyState);
        host.Controls.Add(root);
    }

    // TV1: khi form hien len - kiem tra quyen, mac dinh khong loc ngay, nap NCC + danh sach phieu
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode || !QuyenForm.KiemTraKhiMo(this, ChucNang.NhaCungCapPhieuNhap)) return;
        dtpTuNgay.Value = DateTime.Today.AddDays(-30);
        dtpDenNgay.Value = DateTime.Today;
        dtpTuNgay.Checked = false;   // bo tick = khong loc theo ngay -> lan dau thay het phieu
        dtpDenNgay.Checked = false;
        NapNhaCungCap();
        NapDanhSach();
    }

    // TV1: nap ComboBox NCC, dong dau "Tất cả" = khong loc
    private void NapNhaCungCap()
    {
        try
        {
            var ds = new NhaCungCapRepository().LayTatCa();
            ds.Insert(0, new NhaCungCap { MaNCC = 0, TenNCC = "(Tất cả)" });
            cboNCC.DisplayMember = nameof(NhaCungCap.TenNCC);
            cboNCC.ValueMember = nameof(NhaCungCap.MaNCC);
            cboNCC.DataSource = ds;
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: nap danh sach phieu nhap theo bo loc, chon lai phieu co ma = chonMaPN (neu co)
    private void NapDanhSach(int chonMaPN = 0)
    {
        DateTime? tu = dtpTuNgay.Checked ? dtpTuNgay.Value.Date : null;
        DateTime? den = dtpDenNgay.Checked ? dtpDenNgay.Value.Date : null;
        if (tu > den) { MessageBox.Show("\"Từ ngày\" phải trước hoặc bằng \"Đến ngày\".", "Phiếu nhập", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        int? maNCC = cboNCC.SelectedValue is int m && m > 0 ? m : null;
        try
        {
            var dt = _pnRepo.LayDanhSachHienThi(tu, den, maNCC);
            binding=true;
            try { dgvPhieu.DataSource = dt; }finally { binding=false; }
            dgvPhieu.Columns["MaPN"]!.HeaderText = "Số phiếu";
            dgvPhieu.Columns["NgayNhap"]!.HeaderText = "Ngày nhập";
            dgvPhieu.Columns["NgayNhap"]!.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            dgvPhieu.Columns["TenNCC"]!.HeaderText = "Nhà cung cấp";
            dgvPhieu.Columns["NguoiLap"]!.HeaderText = "Người lập";
            dgvPhieu.Columns["TongTien"]!.HeaderText = "Tổng tiền (đ)";
            KhungChiTiet.DoRong(dgvPhieu, ("MaPN", 60, 70), ("NgayNhap", 130, 150), ("TenNCC", 200, 180), ("NguoiLap", 140, 130), ("TongTien", 120, 130));

            var tong = dt.AsEnumerable().Sum(r => r.Field<decimal>("TongTien"));
            lblTongKet.Text = $"{dt.Rows.Count} phiếu – tổng giá trị nhập: {tong:N0} đ";

            foreach (DataGridViewRow r in dgvPhieu.Rows)
                if (chonMaPN != 0 && (int)r.Cells["MaPN"].Value == chonMaPN) { r.Selected = true; dgvPhieu.CurrentCell = r.Cells["MaPN"]; }
            ChiTiet();
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: chon 1 phieu thi hien thong tin va cac san pham nhap o panel ben phai
    private void ChiTiet()
    {
        if(binding)return;
        if (dgvPhieu.CurrentRow?.DataBoundItem is not DataRowView r)
        { HienTrong("Không có phiếu nhập trong khoảng lọc."); return; }
        var maPN = (int)r["MaPN"];
        try
        {
            var chiTiet = details.Lay(maPN,$"{DbHelper.ConnectionString}|{PhienDangNhap.MaTK}|{PhienDangNhap.VaiTro}|{DbHelper.DauHieuDuLieu}",()=>_pnRepo.LayChiTietHienThi(maPN));
            emptyState.Visible = false; detailBody.Visible = true;
            maPhieu.Text = $"Phiếu nhập {maPN}";
            values["TenNCC"].Text = $"{r["TenNCC"]}";
            values["NgayNhap"].Text = $"{r["NgayNhap"]:dd/MM/yyyy HH:mm}";
            values["NguoiLap"].Text = $"{r["NguoiLap"]}";
            values["TongTien"].Text = $"{r["TongTien"]:N0} đ";
            DungThe(chiTiet);
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: trang thai trong cua panel chi tiet (chua chon phieu / khong co phieu)
    private void HienTrong(string text)
    {
        emptyState.Text = text; emptyState.Visible = true; detailBody.Visible = false; XoaThe();
    }

    // TV1: bo cac the san pham cu
    private void XoaThe()
    {
        cards.SuspendLayout();
        foreach (Control c in cards.Controls.Cast<Control>().ToArray()) { cards.Controls.Remove(c); c.Dispose(); }
        cards.ResumeLayout();
    }

    // TV1: moi dong chi tiet thanh mot the: ten, ma · so luong × don gia nhap, thanh tien
    private void DungThe(DataTable chiTiet)
    {
        cards.SuspendLayout(); XoaThe();
        foreach (DataRow row in chiTiet.Rows)
        {
            var table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var name = new Label { Text = Convert.ToString(row["TenSP"]), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(0, 0, 0, 2) };
            var line = new Label { Text = $"Mã {row["MaSP"]}  ·  SL {row["SoLuong"]} {row["DonViTinh"]} × {Convert.ToDecimal(row["DonGia"]):#,##0} đ", AutoSize = true, ForeColor = ShopTheme.Muted, Font = new Font("Segoe UI", 9), Margin = new Padding(0, 0, 0, 4) };
            var money = new Label { Text = $"{Convert.ToDecimal(row["ThanhTien"]):#,##0} đ", AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = ShopTheme.Green, Anchor = AnchorStyles.Right, Margin = Padding.Empty };
            table.Controls.Add(name, 0, 0); table.Controls.Add(line, 0, 1); table.Controls.Add(money, 0, 2);
            var card = new Panel { Name = "TheSanPham", Tag = row, BorderStyle = BorderStyle.FixedSingle, BackColor = ShopTheme.SurfaceAlt, Padding = new Padding(10, 8, 10, 8), Margin = new Padding(0, 0, 0, 8) };
            card.Controls.Add(table); cards.Controls.Add(card);
        }
        cards.ResumeLayout(); XepThe();
    }

    // TV1: the luon rong bang vung cuon; ten san pham dai tu xuong dong
    private void XepThe()
    {
        if (cards.Controls.Count == 0) return;
        cards.SuspendLayout();
        cards.AutoScroll = false;
        int w = Math.Max(120, cards.ClientSize.Width - cards.Padding.Horizontal);
        foreach (var card in cards.Controls.OfType<Panel>())
        {
            var table = (TableLayoutPanel)card.Controls[0]; int inner = w - card.Padding.Horizontal - 2;
            foreach (var l in table.Controls.OfType<Label>()) l.MaximumSize = new Size(inner, 0);
            card.Width = w; table.PerformLayout(); card.Height = table.PreferredSize.Height + card.Padding.Vertical + 2;
        }
        cards.AutoScroll = true;
        cards.ResumeLayout();
    }

    // TV1: mo form lap phieu nhap moi, luu xong thi nap lai va chon phieu vua tao
    private void LapPhieu()
    {
        using var frm = new FrmPhieuNhap();
        if (frm.ShowDialog(this) != DialogResult.OK) return;
        dtpDenNgay.Checked = false;   // phieu moi co the nam ngoai khoang dang loc
        NapDanhSach(frm.MaPNVuaLuu);
    }

    // TV1: bao loi he thong (mat ket noi CSDL, ...)
    private static void BaoLoiHeThong(Exception ex)
        => MessageBox.Show(LoiCsdl.ThongBao(ex), "Lỗi",
                           MessageBoxButtons.OK, MessageBoxIcon.Error);
}
