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
        public bool Chon { get; set; }
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
    private readonly Dictionary<int, Image?> _anhNho = new();
    private const string CotXoa = "Xoa";

    /// <summary>Ma phieu vua luu (0 = chua luu) - FrmDanhSachPhieuNhap dung de chon lai dong moi.</summary>
    public int MaPNVuaLuu { get; private set; }

    /// <summary>Ma san pham chon san trong o san pham khi mo form (0 = khong chon).</summary>
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int MaSPChonSan { get; set; }

    // TV1: khoi tao form (chi giao dien, KHONG truy cap CSDL)
    public FrmPhieuNhap()
    {
        InitializeComponent();
        TaoCot();
        GanGoiY();
    }

    // TV1: khi form hien len - kiem tra quyen, nguoi lap = nhan vien dang dang nhap, nap NCC + san pham
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode || !QuyenForm.KiemTraKhiMo(this, ChucNang.NhaCungCapPhieuNhap)) return;
        lblNguoiLap.Text = $"Người lập: {PhienDangNhap.HoTen}";
        dtpNgayNhap.Value = DateTime.Now;
        DinhDangLuoi();
        dgvChiTiet.DataSource = _dong;
        NapDanhMucChon();
        if (MaSPChonSan > 0) { cboSanPham.SelectedValue = MaSPChonSan; cboSanPham_SelectionChangeCommitted(cboSanPham, EventArgs.Empty); }
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

    // TV1: khai bao cac cot cua bang chi tiet: o chon, thong tin dong va nut xoa
    private void TaoCot()
    {
        dgvChiTiet.Columns.Add(new DataGridViewCheckBoxColumn { Name = nameof(DongNhap.Chon), DataPropertyName = nameof(DongNhap.Chon), HeaderText = "", FillWeight = 28, MinimumWidth = S(44) });
        foreach (var (ten, tieuDe, rong, toiThieu) in new[]
        {
            (nameof(DongNhap.MaSP), "Mã SP", 55, 64), (nameof(DongNhap.TenSP), "Sản phẩm", 210, 160), (nameof(DongNhap.DonViTinh), "ĐVT", 50, 56),
            (nameof(DongNhap.SoLuong), "Số lượng", 70, 84), (nameof(DongNhap.DonGia), "Đơn giá nhập", 95, 112), (nameof(DongNhap.ThanhTien), "Thành tiền", 100, 112),
        })
            dgvChiTiet.Columns.Add(new DataGridViewTextBoxColumn { Name = ten, DataPropertyName = ten, HeaderText = tieuDe, FillWeight = rong, MinimumWidth = S(toiThieu) });
        dgvChiTiet.Columns.Add(new DataGridViewTextBoxColumn { Name = CotXoa, HeaderText = "", FillWeight = 28, MinimumWidth = S(44) });
    }

    // TV1: chinh kieu bang (dong gon, can le, ve anh san pham va thung rac) va gan thao tac chuot
    private void DinhDangLuoi()
    {
        var t = ShopTheme.Current;
        dgvChiTiet.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None; dgvChiTiet.AllowUserToResizeRows = false;
        dgvChiTiet.RowTemplate.MinimumHeight = S(30); dgvChiTiet.RowTemplate.Height = S(46);
        dgvChiTiet.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing; dgvChiTiet.ColumnHeadersHeight = S(40);
        dgvChiTiet.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
        dgvChiTiet.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        dgvChiTiet.AlternatingRowsDefaultCellStyle.BackColor = t.Surface;
        foreach (DataGridViewColumn c in dgvChiTiet.Columns)   // chi cho tich chon va sua truc tiep so luong + don gia
        {
            c.SortMode = DataGridViewColumnSortMode.NotSortable;
            c.ReadOnly = c.Name is not (nameof(DongNhap.Chon) or nameof(DongNhap.SoLuong) or nameof(DongNhap.DonGia));
        }
        foreach (var c in new[] { nameof(DongNhap.DonGia), nameof(DongNhap.ThanhTien) })
        {
            dgvChiTiet.Columns[c]!.DefaultCellStyle.Format = "#,##0 'đ'";
            dgvChiTiet.Columns[c]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvChiTiet.Columns[c]!.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        foreach (var c in new[] { nameof(DongNhap.DonViTinh), nameof(DongNhap.SoLuong) })
        {
            dgvChiTiet.Columns[c]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvChiTiet.Columns[c]!.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        dgvChiTiet.Columns[nameof(DongNhap.ThanhTien)]!.DefaultCellStyle.Font = ShopTheme.Face("Segoe UI", 10, FontStyle.Bold);
        // Tich o chon thi ghi ngay vao dong, khong cho roi o
        dgvChiTiet.CurrentCellDirtyStateChanged += (_, _) => { if (dgvChiTiet.CurrentCell is DataGridViewCheckBoxCell) dgvChiTiet.CommitEdit(DataGridViewDataErrorContexts.Commit); };
        dgvChiTiet.CellPainting += VeO;
        dgvChiTiet.CellClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || dgvChiTiet.Columns[e.ColumnIndex].Name != CotXoa || !dgvChiTiet.EndEdit()) return;
            _dong.RemoveAt(e.RowIndex); CapNhatTongTien();
        };
        dgvChiTiet.CellMouseEnter += (_, e) => dgvChiTiet.Cursor = e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvChiTiet.Columns[e.ColumnIndex].Name == CotXoa ? Cursors.Hand : Cursors.Default;
        dgvChiTiet.Paint += (_, e) =>
        {
            if (dgvChiTiet.Rows.Count > 0) return;
            TextRenderer.DrawText(e.Graphics, "Chưa có sản phẩm. Chọn sản phẩm rồi bấm Thêm vào phiếu.", dgvChiTiet.Font,
                new Rectangle(0, dgvChiTiet.ColumnHeadersHeight, dgvChiTiet.Width, dgvChiTiet.Height - dgvChiTiet.ColumnHeadersHeight), ShopTheme.Muted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        };
    }

    // TV1: ve anh nho canh ten san pham va bieu tuong thung rac o cot xoa
    private void VeO(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics == null) return;
        var cot = dgvChiTiet.Columns[e.ColumnIndex].Name;
        if (cot != CotXoa && cot != nameof(DongNhap.TenSP)) return;
        e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.SelectionBackground | DataGridViewPaintParts.Border);
        if (cot == CotXoa) Ve.BieuTuong(e.Graphics, "\uE74D", 12, e.CellBounds, ShopTheme.Muted);
        else if (dgvChiTiet.Rows[e.RowIndex].DataBoundItem is DongNhap d)
        {
            int co = e.CellBounds.Height - S(10), x = e.CellBounds.X + S(6);
            var khung = new Rectangle(x, e.CellBounds.Y + S(5), co, co);
            e.Graphics.FillRectangle(Brushes.White, khung);
            if (AnhNho(d.MaSP) is { } anh)
            {
                float tiLe = Math.Min((float)co / anh.Width, (float)co / anh.Height);
                e.Graphics.DrawImage(anh, khung.X + (co - anh.Width * tiLe) / 2, khung.Y + (co - anh.Height * tiLe) / 2, anh.Width * tiLe, anh.Height * tiLe);
            }
            using (var vien = new Pen(ShopTheme.Line)) e.Graphics.DrawRectangle(vien, khung);
            var chon = (e.State & DataGridViewElementStates.Selected) != 0;
            Ve.Chu(e.Graphics, d.TenSP, dgvChiTiet.Font, new Rectangle(khung.Right + S(8), e.CellBounds.Y, e.CellBounds.Right - khung.Right - S(12), e.CellBounds.Height),
                chon ? ShopTheme.Green : ShopTheme.Ink);
        }
        e.Handled = true;
    }

    // TV1: doc anh san pham mot lan roi giu lai; thieu hoac hong anh thi tra ve null
    private Image? AnhNho(int maSP)
    {
        if (_anhNho.TryGetValue(maSP, out var anh)) return anh;
        try
        {
            var path = KhoAnhSanPham.DuongDanDayDu(_sanPham.FirstOrDefault(s => s.MaSP == maSP)?.HinhAnh);
            if (path != null && File.Exists(path)) anh = AnhSanPham.Doc(path);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException) { anh = null; }
        return _anhNho[maSP] = anh;
    }

    // TV1: chu goi y khi chua chon nha cung cap / san pham
    private void GanGoiY()
    {
        cboNCC.DrawItem += (_, e) =>
        {
            if (e.Index >= 0) return;
            using (var nen = new SolidBrush(ShopTheme.Current.Field)) e.Graphics.FillRectangle(nen, e.Bounds);
            Ve.Chu(e.Graphics, "Chọn nhà cung cấp", cboNCC.Font, e.Bounds, ShopTheme.Muted);
        };
        cboSanPham.HandleCreated += (_, _) => SendMessage(cboSanPham.Handle, 0x1703 /* CB_SETCUEBANNER */, IntPtr.Zero, "Gõ hoặc chọn sản phẩm");
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr h, int msg, IntPtr w, string l);

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
        if (donGia <= 0) { BaoLoi("Đơn giá nhập phải lớn hơn 0.", numDonGia); return; }

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
        // O chon gan du lieu phai dat -1 hai lan moi trong, neu khong no tu nhay sang san pham dau danh sach
        cboSanPham.SelectedIndex = -1; cboSanPham.SelectedIndex = -1;
        numSoLuong.Value = 1;
        numDonGia.Value = 0;
        cboSanPham.Focus();
    }

    // TV1: xoa cac dong da tich chon; khong tich dong nao thi xoa dong dang chon
    private void btnXoaDong_Click(object? sender, EventArgs e)
    {
        if (!dgvChiTiet.EndEdit()) return;
        var xoa = _dong.Where(d => d.Chon).ToList();
        if (xoa.Count == 0 && dgvChiTiet.CurrentRow?.DataBoundItem is DongNhap dangChon) xoa.Add(dangChon);
        foreach (var d in xoa) _dong.Remove(d);
        CapNhatTongTien();
    }

    // TV1: sua truc tiep so luong / don gia tren luoi thi kiem tra hop le va tinh lai tong
    private void dgvChiTiet_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
    {
        var cot = dgvChiTiet.Columns[e.ColumnIndex].DataPropertyName;
        var s = e.FormattedValue?.ToString()?.Replace(",", "").Replace(".", "").Trim() ?? "";
        if (cot == nameof(DongNhap.SoLuong) && (!int.TryParse(s, out var sl) || sl <= 0))
        { e.Cancel = true; dgvChiTiet.Rows[e.RowIndex].ErrorText = "Số lượng phải là số nguyên > 0"; }
        else if (cot == nameof(DongNhap.DonGia) && (!decimal.TryParse(s, out var g) || g <= 0))
        { e.Cancel = true; dgvChiTiet.Rows[e.RowIndex].ErrorText = "Đơn giá phải là số > 0"; }
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
        lblTongTien.Text = $"Tổng tiền: {_dong.Sum(d => d.ThanhTien):N0} đ";
        lblSoDong.Text = $"({_dong.Count} sản phẩm)";
        btnLuu.Enabled = btnXoaDong.Enabled = _dong.Count > 0;
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
        var s = ex is SqlException ? "Không lưu được phiếu nhập, dữ liệu chưa thay đổi.\n" + LoiCsdl.ThongBao(ex)
                                   : LoiCsdl.ThongBao(ex);
        MessageBox.Show(s, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
