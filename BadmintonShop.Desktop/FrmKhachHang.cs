using System.Data;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

/// <summary>Danh sach khach hang: tim theo ten/SDT/email, xem so don + tong mua, khoa/mo khoa tai khoan.</summary>
public class FrmKhachHang : Form
{
    private const ChucNang Quyen = ChucNang.DonHangPOS;
    private readonly KhachHangRepository _repo = new();
    private DataGridView luoi = null!;
    private TextBox tim = null!;
    private Label tong = null!;

    public FrmKhachHang()
    {
        Text = "Danh sách khách hàng";
        Size = new Size(1100, 700);
        AutoScaleMode = AutoScaleMode.Font;

        luoi = Sv4Ui.Luoi();
        var bar = Sv4Ui.ThanhCongCu(58);
        tim = new TextBox { Width = 260, Margin = new Padding(4, 7, 4, 3), PlaceholderText = "Tên, SĐT hoặc email" };
        tim.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Nap(); } };
        tong = new Label { AutoSize = true, Margin = new Padding(12, 9, 5, 3) };
        bar.Controls.AddRange(new Control[]
        {
            Sv4Ui.Nhan("Tìm"), tim,
            Sv4Ui.Nut("Tìm", Nap),
            Sv4Ui.Nut("Tải lại", () => { tim.Clear(); Nap(); }),
            Sv4Ui.Nut("Khóa / Mở khóa", DoiTrangThai),
            tong,
        });
        Controls.Add(luoi);
        Controls.Add(bar);
        luoi.BringToFront();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode || !QuyenForm.KiemTraKhiMo(this, Quyen)) return;
        Nap();
    }

    private void Nap()
        => Sv4Ui.Chay(this, Quyen, () =>
        {
            var dt = _repo.LayDanhSach(tim.Text);
            luoi.DataSource = dt;
            void Cot(string ten, string tieuDe, string? dinhDang = null)
            {
                var c = luoi.Columns[ten]!;
                c.HeaderText = tieuDe;
                if (dinhDang != null) c.DefaultCellStyle.Format = dinhDang;
            }
            Cot("MaKH", "Mã"); Cot("HoTen", "Họ tên"); Cot("SDT", "SĐT"); Cot("Email", "Email");
            Cot("DiaChi", "Địa chỉ"); Cot("NgayDangKy", "Ngày đăng ký", "dd/MM/yyyy");
            Cot("TrangThai", "Hoạt động"); Cot("SoDon", "Số đơn"); Cot("TongMua", "Tổng mua (đ)", "N0");
            luoi.ClearSelection();
            tong.Text = $"{dt.Rows.Count} khách hàng";
        });

    private void DoiTrangThai()
        => Sv4Ui.Chay(this, Quyen, () =>
        {
            if (Sv4Ui.MaDong(luoi, "MaKH") is not int ma)
            {
                MessageBox.Show(this, "Chọn một khách hàng trước.", "Thông báo");
                return;
            }
            bool hienTai = (bool)luoi.CurrentRow!.Cells["TrangThai"].Value;
            string ten = luoi.CurrentRow.Cells["HoTen"].Value?.ToString() ?? "";
            if (!Sv4Ui.XacNhan(this, (hienTai ? "Khóa" : "Mở khóa") + $" tài khoản của {ten}?")) return;
            _repo.DatTrangThai(ma, !hienTai);
            Nap();
        });
}
