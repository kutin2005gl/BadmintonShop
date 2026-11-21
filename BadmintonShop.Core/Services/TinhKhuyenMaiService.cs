using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;

namespace BadmintonShop.Core.Services;

/// <summary>
/// Tinh tien giam gia cho gio hang / hoa don - dung chung cho Web (SV3) va POS Desktop (SV4).
/// Quy uoc: % giam theo SP ghi vao ChiTietHoaDon.GiamGia; giam theo HD/voucher ghi vao HoaDon.TienGiamGia + MaCTKM.
/// Tien giam duoc lam tron ve dong (DECIMAL(18,0)) va khong vuot tong tien hang.
/// LUU Y: 'tongTienHang' la tong ThanhTien cac dong SAU KHI da giam theo san pham.
/// </summary>
/// <summary>Ket qua chon uu dai cap hoa don. VoucherThang = voucher duoc chon; ConUuDaiThua = con uu dai kia (thua) hop le.</summary>
public sealed record KetQuaGiamHoaDon(int? MaCTKM, decimal TienGiam, string? MaVoucher, decimal PhanTram,
    bool VoucherThang, bool ConUuDaiThua, decimal NguongTien = 0m);

public class TinhKhuyenMaiService
{
    private readonly KhuyenMaiRepository _repo;

    public TinhKhuyenMaiService() : this(new KhuyenMaiRepository()) { }
    public TinhKhuyenMaiService(KhuyenMaiRepository repo) => _repo = repo;

    // SV4: shared validation message, without changing the existing SV3 discount APIs.
    public string? LoiVoucher(string maVoucher, DateTime ngay) => _repo.LoiVoucher(maVoucher, ngay);

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

    // Quy tac: giam theo SP luon ap dung truoc (tongTienHang da la tien SAU giam SP). O cap hoa don chi chon MOT trong hai:
    // voucher khach nhap hoac bac giam theo nguong tien; cai nao giam NHIEU hon (so tien thuc te sau lam tron) thang,
    // bang nhau thi uu tien voucher. Hai loai khong cong don. Tra ve (null, 0) neu khong co uu dai nao ap duoc.
    public KetQuaGiamHoaDon TinhGiamHoaDonChiTiet(decimal tongTienHang, DateTime ngay, string? maVoucher = null)
    {
        var bac = _repo.LayBacHoaDon(tongTienHang, ngay);
        var theoHD = bac == null ? 0m : TienGiamTu(tongTienHang, bac.PhanTramGG);

        KMTheoVoucher? vc = string.IsNullOrWhiteSpace(maVoucher) ? null : _repo.LayVoucherHopLe(maVoucher, ngay);
        var theoVC = vc == null ? 0m : TienGiamTu(tongTienHang, vc.PhanTramGG);

        if (vc != null && theoVC >= theoHD)
            return new KetQuaGiamHoaDon(vc.MaCTKM, theoVC, vc.MaVoucher.Trim().ToUpperInvariant(), vc.PhanTramGG, true, bac != null);
        if (bac != null)
            return new KetQuaGiamHoaDon(bac.MaCTKM, theoHD, null, bac.PhanTramGG, false, vc != null, bac.TongTienToiThieu);
        return new KetQuaGiamHoaDon(null, 0m, null, 0m, false, false);
    }

    // Giu chu ky cu cho cac noi goi don gian
    public (int? MaCTKM, decimal TienGiam, string? MaVoucherDung) TinhGiamHoaDon(
        decimal tongTienHang, DateTime ngay, string? maVoucher = null)
    {
        var kq = TinhGiamHoaDonChiTiet(tongTienHang, ngay, maVoucher);
        return (kq.MaCTKM, kq.TienGiam, kq.MaVoucher);
    }

    // SV4: round once per invoice line, exactly like the persisted SQL computed ThanhTien column.
    public static decimal TinhThanhTien(int soLuong,decimal donGia,decimal phanTram)
        => LamTronDong(soLuong*donGia*(1-phanTram/100m));

    // TV3: tien giam = tong * %/100, lam tron dong, khong vuot tong
    private static decimal TienGiamTu(decimal tongTienHang, decimal phanTram)
        => Math.Min(tongTienHang, LamTronDong(tongTienHang * phanTram / 100m));

    private static decimal LamTronDong(decimal x) => Math.Round(x, 0, MidpointRounding.AwayFromZero);
}
