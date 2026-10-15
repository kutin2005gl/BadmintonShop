using System.ComponentModel;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Desktop;

/// <summary>
/// Lap phieu nhap hang (SV1 - ROADMAP 3.1): chon NCC, them dong san pham, tu tinh tong tien,
/// luu PhieuNhap + ChiTietPhieuNhap trong 1 transaction. Ton kho + TongTien do trigger CSDL cap nhat.
/// </summary>
public partial class FrmPhieuNhap : Form
{
    /// <summary>1 dong tren luoi chi tiet (chua luu CSDL).</summary>
    private sealed class DongNhap
    {
        public int MaSP { get; init; }
        public string TenSP { get; init; } = "";
        public string DonViTinh { get; init; } = "";
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }

    private readonly PhieuNhapRepository _pnRepo = new();
    private readonly BindingList<DongNhap> _dong = new();
    private List<SanPham> _sanPham = new();

    /// <summary>Ma phieu vua luu (0 = chua luu) - FrmDanhSachPhieuNhap dung de chon lai dong moi.</summary>
    public int MaPNVuaLuu { get; private set; }

    // TV1: khoi tao form, nguoi lap = nhan vien dang dang nhap, nap NCC + san pham
    public FrmPhieuNhap()
    {
        InitializeComponent();
        lblNguoiLap.Text = $"Người lập: {PhienDangNhap.HoTen}";
        dtpNgayNhap.Value = DateTime.Now;
        dgvChiTiet.DataSource = _dong;
        DinhDangLuoi();
        NapDanhMucChon();
        CapNhatTongTien();
    }

    // TV1: nap ComboBox nha cung cap va san pham
    private void NapDanhMucChon()
    {
        try
        {
            cboNCC.DataSource = new NhaCungCapRepository().LayTatCa();
            cboNCC.DisplayMember = nameof(NhaCungCap.TenNCC);
            cboNCC.ValueMember = nameof(NhaCungCap.MaNCC);
            cboNCC.SelectedIndex = -1;

            _sanPham = new SanPhamRepository().LayTatCa();
            cboSanPham.DataSource = _sanPham.Select(s => new { s.MaSP, Ten = $"{s.TenSP} ({s.DonViTinh}) – tồn {s.SoLuongTon}" }).ToList();
            cboSanPham.DisplayMember = "Ten";
            cboSanPham.ValueMember = "MaSP";
            cboSanPham.SelectedIndex = -1;
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: dat tieu de, dinh dang so tien cho luoi chi tiet
    private void DinhDangLuoi()
    {
        dgvChiTiet.Columns[nameof(DongNhap.MaSP)]!.HeaderText = "Mã SP";
        dgvChiTiet.Columns[nameof(DongNhap.TenSP)]!.HeaderText = "Sản phẩm";
        dgvChiTiet.Columns[nameof(DongNhap.DonViTinh)]!.HeaderText = "ĐVT";
        dgvChiTiet.Columns[nameof(DongNhap.SoLuong)]!.HeaderText = "Số lượng";
        dgvChiTiet.Columns[nameof(DongNhap.DonGia)]!.HeaderText = "Đơn giá nhập";
        dgvChiTiet.Columns[nameof(DongNhap.ThanhTien)]!.HeaderText = "Thành tiền";
        dgvChiTiet.Columns[nameof(DongNhap.TenSP)]!.FillWeight = 250;
        foreach (DataGridViewColumn c in dgvChiTiet.Columns)   // chi cho sua truc tiep so luong + don gia
            c.ReadOnly = c.DataPropertyName is not (nameof(DongNhap.SoLuong) or nameof(DongNhap.DonGia));
        foreach (var c in new[] { nameof(DongNhap.DonGia), nameof(DongNhap.ThanhTien) })
        {
            dgvChiTiet.Columns[c]!.DefaultCellStyle.Format = "N0";
            dgvChiTiet.Columns[c]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
    }

    // TV1: chon san pham thi goi y don gia nhap lan gan nhat
    private void cboSanPham_SelectionChangeCommitted(object? sender, EventArgs e)
    {
        if (cboSanPham.SelectedValue is not int maSP) return;
        var dangCo = _dong.FirstOrDefault(d => d.MaSP == maSP);
        if (dangCo != null) { numDonGia.Value = dangCo.DonGia; return; }
        try
        {
            var gia = _pnRepo.LayDonGiaNhapGanNhat(maSP);
            if (gia.HasValue) numDonGia.Value = Math.Min(gia.Value, numDonGia.Maximum);
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: them dong san pham vao phieu; san pham da co thi gop (cong so luong, lay don gia moi)
    private void btnThemDong_Click(object? sender, EventArgs e)
    {
        if (cboSanPham.SelectedValue is not int maSP) { BaoLoi("Vui lòng chọn sản phẩm.", cboSanPham); return; }
        var soLuong = (int)numSoLuong.Value;
        var donGia = numDonGia.Value;
        if (soLuong <= 0) { BaoLoi("Số lượng phải lớn hơn 0.", numSoLuong); return; }

        var dangCo = _dong.FirstOrDefault(d => d.MaSP == maSP);
        if (dangCo != null)
        {
            dangCo.SoLuong += soLuong;
            dangCo.DonGia = donGia;
            _dong.ResetItem(_dong.IndexOf(dangCo));
        }
        else
        {
            var sp = _sanPham.First(s => s.MaSP == maSP);
            _dong.Add(new DongNhap { MaSP = sp.MaSP, TenSP = sp.TenSP, DonViTinh = sp.DonViTinh, SoLuong = soLuong, DonGia = donGia });
        }
        CapNhatTongTien();
        cboSanPham.SelectedIndex = -1;
        numSoLuong.Value = 1;
        numDonGia.Value = 0;
        cboSanPham.Focus();
    }

    // TV1: xoa dong dang chon khoi phieu
    private void btnXoaDong_Click(object? sender, EventArgs e)
    {
        if (dgvChiTiet.CurrentRow?.DataBoundItem is not DongNhap d) return;
        _dong.Remove(d);
        CapNhatTongTien();
    }

    // TV1: sua truc tiep so luong / don gia tren luoi thi kiem tra hop le va tinh lai tong
    private void dgvChiTiet_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
    {
        var cot = dgvChiTiet.Columns[e.ColumnIndex].DataPropertyName;
        var s = e.FormattedValue?.ToString()?.Replace(",", "").Replace(".", "").Trim() ?? "";
        if (cot == nameof(DongNhap.SoLuong) && (!int.TryParse(s, out var sl) || sl <= 0))
        { e.Cancel = true; dgvChiTiet.Rows[e.RowIndex].ErrorText = "Số lượng phải là số nguyên > 0"; }
        else if (cot == nameof(DongNhap.DonGia) && (!decimal.TryParse(s, out var g) || g < 0))
        { e.Cancel = true; dgvChiTiet.Rows[e.RowIndex].ErrorText = "Đơn giá phải là số ≥ 0"; }
        else dgvChiTiet.Rows[e.RowIndex].ErrorText = "";
    }

    // TV1: sau khi sua o tren luoi thi ve lai thanh tien va tong tien
    private void dgvChiTiet_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        _dong.ResetItem(e.RowIndex);
        CapNhatTongTien();
    }

    // TV1: tinh lai tong tien phieu tren form
    private void CapNhatTongTien()
    {
        lblTongTien.Text = $"Tổng tiền: {_dong.Sum(d => d.ThanhTien):N0} đ   ({_dong.Count} sản phẩm)";
        btnLuu.Enabled = _dong.Count > 0;
    }

    // TV1: kiem tra va luu phieu nhap (1 transaction), trigger tu tang ton kho + tinh TongTien
    private void btnLuu_Click(object? sender, EventArgs e)
    {
        if (!dgvChiTiet.EndEdit()) return;
        if (cboNCC.SelectedValue is not int maNCC) { BaoLoi("Vui lòng chọn nhà cung cấp.", cboNCC); return; }
        if (_dong.Count == 0) { BaoLoi("Phiếu nhập phải có ít nhất 1 sản phẩm.", cboSanPham); return; }
        if (dtpNgayNhap.Value > DateTime.Now.AddMinutes(5)) { BaoLoi("Ngày nhập không được ở tương lai.", dtpNgayNhap); return; }

        var tong = _dong.Sum(d => d.ThanhTien);
        if (MessageBox.Show($"Lưu phiếu nhập từ \"{cboNCC.Text}\" gồm {_dong.Count} sản phẩm, tổng {tong:N0} đ?",
                            "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            var pn = new PhieuNhap { NgayNhap = dtpNgayNhap.Value, MaNCC = maNCC, MaNV = PhienDangNhap.MaNV };
            var chiTiet = _dong.Select(d => new ChiTietPhieuNhap { MaSP = d.MaSP, SoLuong = d.SoLuong, DonGia = d.DonGia }).ToList();
            MaPNVuaLuu = _pnRepo.Them(pn, chiTiet);
            MessageBox.Show($"Đã lưu phiếu nhập số {MaPNVuaLuu}. Tồn kho đã được cập nhật.", "Phiếu nhập",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { BaoLoiHeThong(ex); }
    }

    // TV1: dong form, hoi lai neu phieu dang co du lieu chua luu
    private void FrmPhieuNhap_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (DialogResult == DialogResult.OK || _dong.Count == 0) return;
        if (MessageBox.Show("Phiếu nhập chưa được lưu. Đóng và bỏ phiếu này?", "Xác nhận",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) e.Cancel = true;
    }

    // TV1: bao loi nhap lieu va dua con tro ve o can sua
    private static void BaoLoi(string s, Control o)
    {
        MessageBox.Show(s, "Phiếu nhập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        o.Focus();
    }

    // TV1: bao loi he thong (mat ket noi CSDL, vi pham rang buoc...)
    private static void BaoLoiHeThong(Exception ex)
    {
        var s = ex is SqlException ? "Không lưu được phiếu nhập, dữ liệu chưa thay đổi.\n" + ex.Message
                                   : "Có lỗi khi làm việc với CSDL:\n" + ex.Message;
        MessageBox.Show(s, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
