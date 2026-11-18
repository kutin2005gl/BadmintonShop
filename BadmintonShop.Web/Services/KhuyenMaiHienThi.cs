using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Web.Services;

// SV2: tra % giam theo san pham de HIEN THI (cache 30 giay). Gia tinh tien that van do GioHangService/TinhKhuyenMaiService.
public static class KhuyenMaiHienThi
{
    private static readonly object Khoa = new();
    private static readonly TinhKhuyenMaiService Tinh = new();
    private static Dictionary<int, decimal> bang = new();
    private static DateTime hetHan = DateTime.MinValue;

    public static decimal LayPhanTram(int maSP)
    {
        var bayGio = DateTime.Now;
        lock (Khoa)
        {
            if (bayGio >= hetHan)
            {
                bang = new KhuyenMaiRepository().LayPhanTramTatCaSanPham(bayGio);
                hetHan = bayGio.AddSeconds(30);
            }
            return bang.TryGetValue(maSP, out var pt) ? pt : 0m;
        }
    }

    public static decimal GiaSauGiam(decimal donGia, decimal phanTram)
        => phanTram > 0 ? Tinh.TinhGiaSauGiam(donGia, phanTram) : donGia;
}