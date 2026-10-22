using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;

namespace BadmintonShop.Core.Services;

/// <summary>
/// Tinh gio hang (SV3). Gio chi luu (MaSP, SoLuong) trong Session; gia, ton kho, khuyen mai luon doc LAI tu CSDL
/// moi lan hien thi / dat hang, nen khong bao gio dung gia cu.
/// Controller nen ghi lai Session tu kq.Dong (san pham da ngung ban se bi loai khoi gio).
/// </summary>
public class GioHangService
{
    private readonly SanPhamRepository _sanPham;
    private readonly TinhKhuyenMaiService _km;

    public GioHangService() : this(new SanPhamRepository(), new TinhKhuyenMaiService()) { }
    public GioHangService(SanPhamRepository sanPham, TinhKhuyenMaiService km)
    {
        _sanPham = sanPham;
        _km = km;
    }

    // TV3: tinh toan bo gio hang tu danh sach (MaSP, SoLuong) + ma voucher tuy chon
    public KetQuaGioHang Tinh(IEnumerable<(int MaSP, int SoLuong)> dongTho, string? maVoucher = null, DateTime? ngay = null)
    {
        var bayGio = ngay ?? DateTime.Now;
        var kq = new KetQuaGioHang();

        foreach (var nhom in dongTho.Where(d => d.SoLuong > 0).GroupBy(d => d.MaSP))
        {
            var sp = _sanPham.LayTheoMa(nhom.Key);
            if (sp == null || sp.TrangThai != "Đang bán")
            {
                kq.CanhBao.Add($"Sản phẩm mã {nhom.Key} không còn bán nên đã được bỏ khỏi giỏ.");
                continue;
            }

            var sl = nhom.Sum(x => x.SoLuong);
            var pt = _km.LayPhanTramGiamSanPham(sp.MaSP, bayGio);
            var dong = new DongGioHang
            {
                MaSP = sp.MaSP,
                TenSP = sp.TenSP,
                HinhAnh = sp.HinhAnh,
                SoLuong = sl,
                TonKho = sp.SoLuongTon,
                DonGia = sp.DonGiaBan,
                PhanTramGiam = pt,
                DonGiaSauGiam = _km.TinhGiaSauGiam(sp.DonGiaBan, pt),
                ThanhTien = Math.Round(sl * sp.DonGiaBan * (1 - pt / 100m), 0, MidpointRounding.AwayFromZero),
            };
            if (!dong.DuHang)
                kq.CanhBao.Add($"“{sp.TenSP}” chỉ còn {sp.SoLuongTon} sản phẩm, vui lòng giảm số lượng.");
            kq.Dong.Add(dong);
        }

        kq.TongTienHang = kq.Dong.Sum(d => d.ThanhTien);

        var giam = _km.TinhGiamHoaDon(kq.TongTienHang, bayGio, maVoucher);
        kq.MaCTKM = giam.MaCTKM;
        kq.TienGiamHoaDon = giam.TienGiam;
        kq.MaVoucher = giam.MaVoucherDung;

        if (!string.IsNullOrWhiteSpace(maVoucher))
        {
            if (_km.TinhGiamTheoVoucher(maVoucher, kq.TongTienHang, bayGio) == null)
                kq.LoiVoucher = "Mã giảm giá không hợp lệ, đã hết hạn hoặc hết lượt dùng.";
            else if (kq.MaVoucher == null)
                kq.CanhBao.Add("Đang áp dụng ưu đãi theo giá trị đơn hàng vì giảm nhiều hơn mã giảm giá.");
        }
        return kq;
    }
}