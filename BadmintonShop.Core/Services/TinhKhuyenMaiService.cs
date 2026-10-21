using BadmintonShop.Core.DataAccess;

namespace BadmintonShop.Core.Services;

/// <summary>
/// Tinh tien giam gia cho gio hang / hoa don - dung chung cho Web (SV3) va POS Desktop (SV4).
/// Quy uoc: % giam theo SP ghi vao ChiTietHoaDon.GiamGia; giam theo HD/voucher ghi vao HoaDon.TienGiamGia + MaCTKM.
/// Tien giam duoc lam tron ve dong (DECIMAL(18,0)) va khong vuot tong tien hang.
/// LUU Y: 'tongTienHang' la tong ThanhTien cac dong SAU KHI da giam theo san pham.
/// </summary>
public class TinhKhuyenMaiService
{
    private readonly KhuyenMaiRepository _repo;

    public TinhKhuyenMaiService() : this(new KhuyenMaiRepository()) { }
    public TinhKhuyenMaiService(KhuyenMaiRepository repo) => _repo = repo;

    // TV3: tra ve % giam dang ap dung cho 1 san pham vao ngay 'ngay', khong co tra ve 0
    public decimal LayPhanTramGiamSanPham(int maSP, DateTime ngay)
        => _repo.LayPhanTramSanPham(maSP, ngay);

    // TV3: don gia sau khi giam theo san pham (hien thi tren trang san pham / gio hang)
    public decimal TinhGiaSauGiam(decimal donGia, decimal phanTram)
        => LamTronDong(donGia * (1 - phanTram / 100m));

    // TV3: tra ve (MaCTKM, so tien giam) theo bac tong tien hoa don cao nhat dat duoc; khong dat bac nao tra ve (null, 0)
    public (int? MaCTKM, decimal TienGiam) TinhGiamTheoHoaDon(decimal tongTienHang, DateTime ngay)
    {
        var bac = _repo.LayBacHoaDon(tongTienHang, ngay);
        return bac == null ? (null, 0m) : (bac.MaCTKM, TienGiamTu(tongTienHang, bac.PhanTramGG));
    }

    // TV3: kiem tra ma voucher va tra ve (MaCTKM, so tien giam); voucher sai/het han/het luot tra ve null
    public (int MaCTKM, decimal TienGiam)? TinhGiamTheoVoucher(string maVoucher, decimal tongTienHang, DateTime ngay)
    {
        if (string.IsNullOrWhiteSpace(maVoucher)) return null;
        var vc = _repo.LayVoucherHopLe(maVoucher, ngay);
        return vc == null ? null : (vc.MaCTKM, TienGiamTu(tongTienHang, vc.PhanTramGG));
    }

    // TV3: chon 1 khuyen mai cap hoa don cho don hang (HoaDon chi luu 1 MaCTKM): lay cai giam NHIEU hon giua
    //      bac theo hoa don va voucher (neu co nhap). Tra ve (null, 0) neu khong co khuyen mai nao ap duoc.
    public (int? MaCTKM, decimal TienGiam, string? MaVoucherDung) TinhGiamHoaDon(
        decimal tongTienHang, DateTime ngay, string? maVoucher = null)
    {
        var theoHD = TinhGiamTheoHoaDon(tongTienHang, ngay);
        var theoVC = TinhGiamTheoVoucher(maVoucher ?? "", tongTienHang, ngay);

        if (theoVC is { } vc && vc.TienGiam >= theoHD.TienGiam)
            return (vc.MaCTKM, vc.TienGiam, maVoucher!.Trim().ToUpperInvariant());
        return (theoHD.MaCTKM, theoHD.TienGiam, null);
    }

    // TV3: tien giam = tong * %/100, lam tron dong, khong vuot tong
    private static decimal TienGiamTu(decimal tongTienHang, decimal phanTram)
        => Math.Min(tongTienHang, LamTronDong(tongTienHang * phanTram / 100m));

    private static decimal LamTronDong(decimal x) => Math.Round(x, 0, MidpointRounding.AwayFromZero);
}