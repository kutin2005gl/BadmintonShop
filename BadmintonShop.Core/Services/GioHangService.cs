using Microsoft.Data.SqlClient;
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
        => TinhCore(dongTho,maVoucher,ngay??DateTime.Now,_sanPham.LayTheoMa,false);

    // SV4: same calculator and SV3 rules, using locked products and a transaction-bound promotion repository.
    internal static KetQuaGioHang TinhTrongGiaoDich(SqlConnection c,SqlTransaction t,IEnumerable<(int MaSP,int SoLuong)> rows,string? voucher,DateTime ngay)
    {
        var service=new GioHangService(new SanPhamRepository(),new TinhKhuyenMaiService(new KhuyenMaiRepository(c,t)));
        return service.TinhCore(rows,voucher,ngay,id=>service._sanPham.LayTheoMa(id,c,t),true);
    }
    private KetQuaGioHang TinhCore(IEnumerable<(int MaSP,int SoLuong)> dongTho,string? maVoucher,DateTime bayGio,Func<int,SanPham?> doc,bool strict)
    {
        maVoucher=string.IsNullOrWhiteSpace(maVoucher)?null:maVoucher.Trim().ToUpperInvariant();
        var kq = new KetQuaGioHang { VoucherYeuCau=maVoucher };

        foreach (var nhom in dongTho.Where(d => d.SoLuong > 0).GroupBy(d => d.MaSP).OrderBy(g=>g.Key))
        {
            var sp = doc(nhom.Key);
            if (sp == null || sp.TrangThai != "Đang bán")
            {
                if(strict)throw new InvalidOperationException($"Sản phẩm {nhom.Key} không còn bán. Hãy xem lại giỏ.");
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
                ThanhTien = TinhKhuyenMaiService.TinhThanhTien(sl,sp.DonGiaBan,pt),
            };
            if (!dong.DuHang && strict)throw new InvalidOperationException($"Sản phẩm {sp.MaSP} không đủ tồn kho. Hãy giảm số lượng.");
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
            kq.LoiVoucher = _km.LoiVoucher(maVoucher, bayGio);
            if (kq.LoiVoucher == null && kq.MaVoucher == null)
                kq.CanhBao.Add("Đang áp dụng ưu đãi theo giá trị đơn hàng vì giảm nhiều hơn mã giảm giá.");
        }
        return kq;
    }
}
