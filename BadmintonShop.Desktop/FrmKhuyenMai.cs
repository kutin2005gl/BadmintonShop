using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

/// <summary>
/// Quan ly chuong trinh khuyen mai (SV3): theo san pham / theo hoa don (nhieu bac) / voucher (co sinh ma hang loat).
/// Bang tren: danh sach; phan giua: thong tin chung; bang duoi: chi tiet theo loai (sua truc tiep, Delete de xoa dong).
/// </summary>
public class FrmKhuyenMai : Form, IUnsavedWork
{
    private const ChucNang Quyen = ChucNang.SanPhamDanhMucKhuyenMai;

    private sealed record LoaiItem(string Ma, string Ten)
    {
        public override string ToString() => Ten;
    }

    private static readonly LoaiItem[] CacLoai =
    [
        new("SANPHAM", "Giảm theo sản phẩm"),
        new("HOADON", "Giảm theo hóa đơn"),
        new("VOUCHER", "Voucher"),
    ];

    private readonly KhuyenMaiQuanLyRepository _repo = new();

    private DataGridView luoi = null!, chiTiet = null!;
    private TextBox ten = null!;
    private ComboBox loai = null!;
    private DateTimePicker tuNgay = null!, denNgay = null!;
    private CheckBox bat = null!;
    private Label nhanMa = null!, nhanChiTiet = null!;
    private Button btnSinh = null!;

    private int ma;                                   // 0 = dang them moi
    private string loaiHienTai = "SANPHAM";
    private DataTable bang = KhuyenMaiQuanLyRepository.TaoBangChiTiet("SANPHAM");
    private DataTable sanPham = new();
    private string original = "";
    private bool dangNap;                             // true khi code dang tu dat gia tri cho form

    public bool HasUnsavedWork => Chup() != original;

    // TV3: chi dung giao dien trong constructor (khong truy cap CSDL) de VS Designer / mo form khong loi
    public FrmKhuyenMai() { InitializeComponent(); }

    private void InitializeComponent()
    {
        Text = "Chương trình khuyến mãi";
        Size = new Size(1100, 760);
        MinimumSize = new Size(900, 600);
        AutoScaleMode = AutoScaleMode.Font;

        luoi = Sv4Ui.Luoi();
        luoi.Dock = DockStyle.Top;
        luoi.Height = 220;

        chiTiet = Sv4Ui.Luoi();
        chiTiet.AutoGenerateColumns = false;
        chiTiet.ReadOnly = false;
        chiTiet.AllowUserToAddRows = true;
        chiTiet.AllowUserToDeleteRows = true;
        chiTiet.DataError += (_, e) =>
        {
            e.ThrowException = false;
            MessageBox.Show(this, "Giá trị không hợp lệ. Hãy nhập số, hoặc chọn sản phẩm từ danh sách.",
                "Dữ liệu chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        };

        ten = new TextBox { Width = 280, MaxLength = 150 };
        loai = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 190, Margin = new Padding(4) };
        loai.Items.AddRange(CacLoai);
        loai.SelectedIndex = 0;
        tuNgay = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110 };
        denNgay = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110 };
        bat = new CheckBox { Text = "Đang áp dụng", AutoSize = true, Checked = true, Margin = new Padding(8, 8, 8, 3) };
        nhanMa = Sv4Ui.Nhan("Chương trình mới");

        var edit = Sv4Ui.ThanhCongCu(120);
        edit.Controls.AddRange(new Control[]
        {
            nhanMa, Sv4Ui.Nhan("Tên"), ten, Sv4Ui.Nhan("Loại"), loai,
            Sv4Ui.Nhan("Từ ngày"), tuNgay, Sv4Ui.Nhan("Đến ngày"), denNgay, bat,
            Sv4Ui.Nut("Mới", () => { if (!HasUnsavedWork || Sv4Ui.XacNhan(this, "Bỏ thay đổi để tạo chương trình mới?")) Moi(); }),
            Sv4Ui.Nut("Lưu", () => Chay(Luu)),
            Sv4Ui.Nut("Bật/Tắt", () => Chay(BatTat)),
            Sv4Ui.Nut("Xóa", () => Chay(Xoa)),
            Sv4Ui.Nut("Tải lại", () => Chay(Nap)),
        });

        nhanChiTiet = Sv4Ui.Nhan("");
        btnSinh = Sv4Ui.Nut("Sinh voucher hàng loạt", () => Chay(SinhVoucher));
        var thanhChiTiet = Sv4Ui.ThanhCongCu(58);
        thanhChiTiet.Controls.AddRange(new Control[] { nhanChiTiet, btnSinh });

        // thu tu Add: Fill truoc, Top sau cung nam tren cung
        Controls.Add(chiTiet);
        Controls.Add(thanhChiTiet);
        Controls.Add(edit);
        Controls.Add(luoi);
        ShopTheme.Apply(this);

        loai.SelectedIndexChanged += (_, _) =>
        {
            if (!dangNap && ma == 0 && loai.SelectedItem is LoaiItem li) ThietLapChiTiet(li.Ma);
        };
        FormClosing += (_, e) =>
        {
            if (HasUnsavedWork && !Sv4Ui.XacNhan(this, "Bỏ thay đổi chương trình chưa lưu?")) e.Cancel = true;
        };
        luoi.RowValidating += (_, e) =>
        {
            if (!dangNap && HasUnsavedWork && !Sv4Ui.XacNhan(this, "Bỏ thay đổi trước khi chọn chương trình khác?")) e.Cancel = true;
        };
        luoi.SelectionChanged += (_, _) =>
        {
            if (dangNap) return;
            if (luoi.CurrentRow is { Selected: true } row && row.Cells["MaCTKM"].Value is int id) Chay(() => NapChiTiet(id));
        };
    }

    // TV3: khi form hien len - kiem tra quyen roi nap du lieu
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode || !QuyenForm.KiemTraKhiMo(this, Quyen)) return;
        Chay(() =>
        {
            sanPham = new SanPhamRepository().LayDanhSachHienThi();
            Nap();
        });
    }

    private void Chay(Action a) => Sv4Ui.Chay(this, Quyen, a);

    // TV3: nap danh sach (chay nen), roi chuyen sang che do them moi
    private async void Nap()
    {
        if (HasUnsavedWork && !Sv4Ui.XacNhan(this, "Tải lại và bỏ thay đổi chưa lưu?")) return;
        await ShopTheme.Load(this, () => _repo.LayDanhSach(), dt =>
        {
            dangNap = true;
            try { luoi.DataSource = dt; DatTieuDe(); }
            finally { dangNap = false; }
            Moi();
        }, dt => dt.Rows.Count == 0);
    }

    private void DatTieuDe()
    {
        void Tieu(string cot, string tieuDe) { if (luoi.Columns.Contains(cot)) luoi.Columns[cot]!.HeaderText = tieuDe; }
        Tieu("MaCTKM", "Mã");
        Tieu("TenCTKM", "Tên chương trình");
        Tieu("Loai", "Loại");
        Tieu("TuNgay", "Từ ngày");
        Tieu("DenNgay", "Đến ngày");
        Tieu("SoMuc", "Số mục chi tiết");
    }

    // TV3: chon 1 dong trong danh sach -> hien thong tin + chi tiet de sua
    private void NapChiTiet(int id)
    {
        var k = _repo.LayCTKM(id) ?? throw new InvalidOperationException("Chương trình không còn tồn tại. Hãy tải lại.");
        var du = _repo.LayChiTiet(id, k.LoaiKM);
        dangNap = true;
        try
        {
            ma = k.MaCTKM;
            nhanMa.Text = $"Mã: {ma}";
            ten.Text = k.TenCTKM;
            loai.SelectedItem = CacLoai.First(x => x.Ma == k.LoaiKM);
            loai.Enabled = false;                       // khong doi loai chuong trinh da tao
            tuNgay.Value = k.NgayBatDau.Date;
            denNgay.Value = k.NgayKetThuc.Date;
            bat.Checked = k.TrangThai;
            ThietLapChiTiet(k.LoaiKM, du);
        }
        finally { dangNap = false; }
        original = Chup();
    }

    private void Moi()
    {
        dangNap = true;
        try
        {
            ma = 0;
            nhanMa.Text = "Chương trình mới";
            ten.Clear();
            loai.Enabled = true;
            loai.SelectedIndex = 0;
            tuNgay.Value = DateTime.Today;
            denNgay.Value = DateTime.Today.AddDays(30);
            bat.Checked = true;
            ThietLapChiTiet("SANPHAM");
            luoi.ClearSelection();
        }
        finally { dangNap = false; }
        original = Chup();
        ten.Focus();
    }

    // TV3: dung cot cho bang chi tiet theo loai
    private void ThietLapChiTiet(string maLoai, DataTable? du = null)
    {
        loaiHienTai = maLoai;
        bang = du ?? KhuyenMaiQuanLyRepository.TaoBangChiTiet(maLoai);
        chiTiet.DataSource = null;
        chiTiet.Columns.Clear();

        switch (maLoai)
        {
            case "SANPHAM":
                chiTiet.Columns.Add(new DataGridViewComboBoxColumn
                {
                    Name = "SanPham",
                    DataPropertyName = "MaSP",
                    HeaderText = "Sản phẩm",
                    FillWeight = 300,
                    DataSource = sanPham,
                    DisplayMember = "TenSP",
                    ValueMember = "MaSP",
                    FlatStyle = FlatStyle.Flat,
                });
                chiTiet.Columns.Add(Cot("GiamGia", "PhanTramGG", "Giảm (%)", typeof(decimal), "0.##"));
                nhanChiTiet.Text = "Sản phẩm áp dụng và % giảm. Thêm ở dòng trống cuối bảng, xóa dòng bằng phím Delete.";
                break;
            case "HOADON":
                chiTiet.Columns.Add(Cot("TongTienToiThieu", "TongTienToiThieu", "Từ tổng tiền hàng (đ)", typeof(decimal), "#,##0"));
                chiTiet.Columns.Add(Cot("GiamGia", "PhanTramGG", "Giảm (%)", typeof(decimal), "0.##"));
                nhanChiTiet.Text = "Mỗi dòng là một bậc: tổng tiền hàng đạt mốc thì được giảm %. Hệ thống áp dụng bậc cao nhất đạt được.";
                break;
            default:
                chiTiet.Columns.Add(Cot("MaVoucher", "MaVoucher", "Mã voucher", typeof(string)));
                chiTiet.Columns.Add(Cot("GiamGia", "PhanTramGG", "Giảm (%)", typeof(decimal), "0.##"));
                chiTiet.Columns.Add(Cot("SoLuot", "SoLuong", "Số lượt dùng", typeof(int)));
                chiTiet.Columns.Add(Cot("DaDung", "DaDung", "Đã dùng", typeof(int), null, chiDoc: true));
                nhanChiTiet.Text = "Mỗi dòng là một mã. Mã đã có người dùng thì không xóa được và số lượt không được nhỏ hơn số đã dùng.";
                break;
        }
        btnSinh.Visible = maLoai == "VOUCHER";
        chiTiet.DataSource = bang;
    }

    private static DataGridViewTextBoxColumn Cot(string tenCot, string thuocTinh, string tieuDe, Type kieu,
                                                 string? dinhDang = null, bool chiDoc = false)
    {
        var c = new DataGridViewTextBoxColumn
        {
            Name = tenCot,
            DataPropertyName = thuocTinh,
            HeaderText = tieuDe,
            ValueType = kieu,
            ReadOnly = chiDoc,
        };
        if (dinhDang != null) c.DefaultCellStyle.Format = dinhDang;
        return c;
    }

    // TV3: anh chup trang thai form de biet co thay doi chua luu hay khong
    private string Chup()
    {
        var dong = bang.Rows.Cast<DataRow>()
            .Where(r => r.RowState != DataRowState.Deleted)
            .Select(r => string.Join(",", r.ItemArray.Select(v => Convert.ToString(v, CultureInfo.InvariantCulture))));
        return $"{ten.Text}|{loaiHienTai}|{tuNgay.Value:yyyyMMdd}|{denNgay.Value:yyyyMMdd}|{bat.Checked}|{string.Join(";", dong)}";
    }

    private void KetThucSua()
    {
        chiTiet.EndEdit();
        BindingContext[bang].EndCurrentEdit();
    }

    private void Luu()
    {
        KetThucSua();
        var k = new CTKM
        {
            MaCTKM = ma,
            TenCTKM = ten.Text,
            NgayBatDau = tuNgay.Value.Date,
            NgayKetThuc = denNgay.Value.Date,
            LoaiKM = ((LoaiItem)loai.SelectedItem!).Ma,
            TrangThai = bat.Checked,
        };
        KhuyenMaiQuanLyRepository.KiemTra(k, bang);            // bao loi ngay tren giao dien
        if (!Sv4Ui.XacNhan(this, "Lưu chương trình khuyến mãi này?")) return;
        var du = bang;
        Ghi(() => _repo.Luu(k, du));
    }

    private void BatTat()
    {
        if (ma == 0) throw new ArgumentException("Hãy chọn một chương trình trong danh sách.");
        if (HasUnsavedWork) throw new ArgumentException("Hãy lưu hoặc bỏ thay đổi trước khi đổi trạng thái.");
        var id = ma;
        var moi = !bat.Checked;
        if (!Sv4Ui.XacNhan(this, moi ? "Bật áp dụng chương trình này?" : "Ngừng áp dụng chương trình này?")) return;
        Ghi(() => _repo.DatTrangThai(id, moi));
    }

    private void Xoa()
    {
        if (ma == 0) throw new ArgumentException("Hãy chọn một chương trình trong danh sách.");
        if (!Sv4Ui.XacNhan(this, $"Xóa chương trình “{ten.Text}” cùng toàn bộ chi tiết?")) return;
        var id = ma;
        Ghi(() => _repo.Xoa(id));
    }

    // TV3: chay thao tac ghi o nen, thanh cong thi tai lai danh sach
    private async void Ghi(Action action)
    {
        var ok = await ShopTheme.Load(this, () => { action(); return true; }, _ => { original = Chup(); });
        if (ok) Nap();
    }

    // TV3: sinh N ma ngau nhien (tien to + 6 ky tu, bo ky tu de nham) vao bang chi tiet; chi luu khi bam "Lưu"
    private void SinhVoucher()
    {
        if (loaiHienTai != "VOUCHER") return;

        using var f = new Form
        {
            Text = "Sinh voucher hàng loạt",
            Width = 460,
            Height = 330,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
        };
        var soMa = new NumericUpDown { Minimum = 1, Maximum = 500, Value = 10 };
        var tienTo = new TextBox { MaxLength = 10, Text = "SALE" };
        var phanTram = new NumericUpDown { DecimalPlaces = 2, Minimum = 0.01m, Maximum = 100, Value = 10 };
        var luot = new NumericUpDown { Minimum = 1, Maximum = 100000, Value = 1 };
        var p = Sv4Ui.BangNhap();
        Sv4Ui.Truong(p, "Số mã cần sinh", soMa);
        Sv4Ui.Truong(p, "Tiền tố (A-Z, 0-9)", tienTo);
        Sv4Ui.Truong(p, "Giảm (%)", phanTram);
        Sv4Ui.Truong(p, "Số lượt dùng mỗi mã", luot);
        var ok = new Button { Text = "Sinh mã", Dock = DockStyle.Bottom, Height = 40, DialogResult = DialogResult.OK };
        f.Controls.Add(p);
        f.Controls.Add(ok);
        f.AcceptButton = ok;
        ShopTheme.Apply(f);
        if (f.ShowDialog(this) != DialogResult.OK) return;

        var tt = tienTo.Text.Trim().ToUpperInvariant();
        if (!Regex.IsMatch(tt, "^[A-Z0-9]{0,10}$"))
            throw new ArgumentException("Tiền tố chỉ gồm chữ A-Z và số, tối đa 10 ký tự.");

        KetThucSua();
        var dangCo = bang.Rows.Cast<DataRow>()
            .Where(r => r.RowState != DataRowState.Deleted && !r.IsNull("MaVoucher"))
            .Select(r => Convert.ToString(r["MaVoucher"])!.Trim().ToUpperInvariant())
            .ToHashSet();

        const string kyTu = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var so = (int)soMa.Value;
        for (var i = 0; i < so; i++)
        {
            string code;
            do
            {
                var s = new char[6];
                for (var j = 0; j < s.Length; j++) s[j] = kyTu[RandomNumberGenerator.GetInt32(kyTu.Length)];
                code = tt + (tt.Length > 0 ? "-" : "") + new string(s);
            } while (!dangCo.Add(code));

            var r = bang.NewRow();
            r["MaVoucher"] = code;
            r["PhanTramGG"] = phanTram.Value;
            r["SoLuong"] = (int)luot.Value;
            r["DaDung"] = 0;
            bang.Rows.Add(r);
        }
        MessageBox.Show(this, $"Đã thêm {so} mã vào bảng. Bấm “Lưu” để ghi vào cơ sở dữ liệu.", "Sinh voucher",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}