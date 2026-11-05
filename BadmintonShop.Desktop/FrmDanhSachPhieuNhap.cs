using System.Data;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

/// <summary>Danh sach phieu nhap: loc theo ngay / NCC, xem chi tiet, mo form lap phieu moi (SV1 - ROADMAP 3.1).</summary>
public partial class FrmDanhSachPhieuNhap : Form
{
    private readonly PhieuNhapRepository _pnRepo = new();
    private bool binding;
    private readonly BoNhoPhien<int,DataTable> details=new(BoNhoPhien<int,int>.ThoiHanMacDinh);

    // TV1: khoi tao form (chi giao dien, KHONG truy cap CSDL de VS Designer mo duoc)
    public FrmDanhSachPhieuNhap()
    {
        InitializeComponent();
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
            cboNCC.DataSource = ds;
            cboNCC.DisplayMember = nameof(NhaCungCap.TenNCC);
            cboNCC.ValueMember = nameof(NhaCungCap.MaNCC);
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
            DinhDangTien(dgvPhieu, "TongTien", "Tổng tiền");

            var tong = dt.AsEnumerable().Sum(r => r.Field<decimal>("TongTien"));
            lblTongKet.Text = $"{dt.Rows.Count} phiếu – tổng giá trị nhập: {tong:N0} đ";

            foreach (DataGridViewRow r in dgvPhieu.Rows)
                if (chonMaPN != 0 && (int)r.Cells["MaPN"].Value == chonMaPN) { r.Selected = true; dgvPhieu.CurrentCell = r.Cells["MaPN"]; }
            dgvPhieu_SelectionChanged(this,EventArgs.Empty);
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: chon 1 phieu thi hien chi tiet cac san pham ben duoi
    private void dgvPhieu_SelectionChanged(object? sender, EventArgs e)
    {
        if(binding)return;
        if (dgvPhieu.CurrentRow?.DataBoundItem is not DataRowView r)
        { dgvChiTiet.DataSource=null;grpChiTiet.Text="Chi tiết phiếu nhập";return; }
        var maPN = (int)r["MaPN"];
        try
        {
            dgvChiTiet.DataSource = details.Lay(maPN,$"{DbHelper.ConnectionString}|{PhienDangNhap.MaTK}|{PhienDangNhap.VaiTro}|{DbHelper.DauHieuDuLieu}",()=>_pnRepo.LayChiTietHienThi(maPN));
            dgvChiTiet.Columns["MaSP"]!.HeaderText = "Mã SP";
            dgvChiTiet.Columns["TenSP"]!.HeaderText = "Sản phẩm";
            dgvChiTiet.Columns["TenSP"]!.FillWeight = 250;
            dgvChiTiet.Columns["DonViTinh"]!.HeaderText = "ĐVT";
            dgvChiTiet.Columns["SoLuong"]!.HeaderText = "Số lượng";
            DinhDangTien(dgvChiTiet, "DonGia", "Đơn giá nhập");
            DinhDangTien(dgvChiTiet, "ThanhTien", "Thành tiền");
            grpChiTiet.Text = $"Chi tiết phiếu nhập số {maPN} – {r["TenNCC"]}";
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: bam Loc thi nap lai danh sach
    private void btnLoc_Click(object? sender, EventArgs e) => NapDanhSach();

    // TV1: mo form lap phieu nhap moi, luu xong thi nap lai va chon phieu vua tao
    private void btnLapPhieu_Click(object? sender, EventArgs e)
    {
        using var frm = new FrmPhieuNhap();
        if (frm.ShowDialog(this) != DialogResult.OK) return;
        dtpDenNgay.Checked = false;   // phieu moi co the nam ngoai khoang dang loc
        NapDanhSach(frm.MaPNVuaLuu);
    }

    // TV1: dat tieu de + dinh dang so tien (N0, can phai) cho 1 cot
    private static void DinhDangTien(DataGridView g, string cot, string tieuDe)
    {
        g.Columns[cot]!.HeaderText = tieuDe;
        g.Columns[cot]!.DefaultCellStyle.Format = "N0";
        g.Columns[cot]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
    }

    // TV1: bao loi he thong (mat ket noi CSDL, ...)
    private static void BaoLoiHeThong(Exception ex)
        => MessageBox.Show(LoiCsdl.ThongBao(ex), "Lỗi",
                           MessageBoxButtons.OK, MessageBoxIcon.Error);
}
