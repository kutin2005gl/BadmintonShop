using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BadmintonShop.Core.Models;

namespace BadmintonShop.Core.Services;

// SV4: immutable pricing identity, independent of current database prices when retrying a committed sale.
public static class XacNhanGia
{
    public static string DauVanTay(KetQuaGioHang gio)
    {
        string N(decimal n)=>n.ToString("0.############################",CultureInfo.InvariantCulture);
        var lines=gio.Dong.OrderBy(d=>d.MaSP).Select(d=>new { d.MaSP,d.SoLuong,DonGia=N(d.DonGia),GiamGia=N(d.PhanTramGiam),ThanhTien=N(d.ThanhTien) }).ToArray();
        var voucher=(gio.VoucherYeuCau??gio.MaVoucher??"").Trim().ToUpperInvariant();
        return Hash(JsonSerializer.Serialize(new { Lines=lines,Voucher=voucher,SelectedVoucher=gio.MaVoucher,gio.MaCTKM,AfterProductDiscount=N(gio.TongTienHang),InvoiceDiscount=N(gio.TienGiamHoaDon) }));
    }
    public static string DauVanTayPOS(int employee,decimal cash,KetQuaGioHang gio)
        =>Hash($"{employee}|{cash.ToString("0",CultureInfo.InvariantCulture)}|{DauVanTay(gio)}");
    private static string Hash(string text)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    public static void KiemTra(KetQuaGioHang daXacNhan,KetQuaGioHang hienTai)
    {
        if(hienTai.LoiVoucher!=null || DauVanTay(daXacNhan)!=DauVanTay(hienTai))
            throw new GiaThanhToanThayDoiException(hienTai);
    }
}

public sealed class GiaThanhToanThayDoiException : InvalidOperationException
{
    public KetQuaGioHang GioMoi { get; }
    public GiaThanhToanThayDoiException(KetQuaGioHang gio)
        :base(gio.LoiVoucher??"Giá hoặc khuyến mãi đã thay đổi. Hãy kiểm tra lại tổng tiền và xác nhận lại trước khi thanh toán.")=>GioMoi=gio;
}
