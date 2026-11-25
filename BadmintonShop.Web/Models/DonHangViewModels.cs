using System.Data;
using BadmintonShop.Core.Models;

namespace BadmintonShop.Web.Models;

public class DatHangViewModel
{
    public KetQuaGioHang Gio { get; set; } = new();
    public string HoTen { get; set; } = "";
    public string SDT { get; set; } = "";
    public bool DaDangNhap { get; set; }
    public string DiaChi { get; set; } = "";
    public string GhiChu { get; set; } = "";
    public string HinhThucTT { get; set; } = "COD";
    public string? Loi { get; set; }
    public string TinhThanh { get; set; } = "";
    public string PhuongXa { get; set; } = "";
    /// <summary>Thuong hieu + ten danh muc theo MaSP, de hien duoi ten san pham trong cot tom tat.</summary>
    public Dictionary<int, (string? ThuongHieu, string TenDanhMuc)> ThongTinSP { get; } = new();
}

/// <summary>Man hinh thanh toan online gia lap: so lieu giam gia lay tu hoa don va chi tiet DA LUU (khong tinh lai).</summary>
public class ThanhToanOnlineViewModel
{
    public HoaDon Don { get; set; } = new();
    public decimal TienHangTruocGiam { get; set; }
    public decimal GiamSanPham { get; set; }
    public decimal GiamHoaDon { get; set; }
    public string? MaVoucher { get; set; }       // ma voucher thang (HoaDon_Voucher); null = giam theo nguong hoac khong giam hoa don
    public decimal TongGiam => GiamSanPham + GiamHoaDon;
}

public class ChiTietDonViewModel
{
    public HoaDon Don { get; set; } = new();
    public DataTable Dong { get; set; } = new();
}

/// <summary>Form tra cuu don cho khach chua dang nhap (ma don + SDT).</summary>
public class TraCuuDonViewModel
{
    public string MaDon { get; set; } = "";
    public string SDT { get; set; } = "";
    public string? Loi { get; set; }
}
