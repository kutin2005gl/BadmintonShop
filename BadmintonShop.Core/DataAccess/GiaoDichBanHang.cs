using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

// SV4: shared write invariants for Web/POS; SQL triggers remain the only stock/total writers.
internal static class GiaoDichBanHang
{
    internal static void KiemTraGio(KetQuaGioHang gio)
    {
        if(gio.Dong.Count==0 || gio.Dong.Any(d=>d.MaSP<=0 || d.SoLuong<=0) || gio.Dong.GroupBy(d=>d.MaSP).Any(g=>g.Count()>1))
            throw new ArgumentException("Giỏ hàng trống, số lượng không hợp lệ hoặc sản phẩm trùng.");
        foreach(var d in gio.Dong)QuyTacBanHang.KiemTraTien(d.DonGia);
        QuyTacBanHang.KiemTraTien(gio.TongThanhToan);
    }
    internal static KetQuaGioHang TinhLai(SqlConnection c,SqlTransaction t,KetQuaGioHang gio,DateTime ngay)
    {
        var fresh=GioHangService.TinhTrongGiaoDich(c,t,gio.Dong.Select(d=>(d.MaSP,d.SoLuong)),gio.VoucherYeuCau??gio.MaVoucher,ngay);
        XacNhanGia.KiemTra(gio,fresh);
        return fresh;
    }
    internal static void DungVoucher(SqlConnection c,SqlTransaction t,KetQuaGioHang gio,DateTime ngay)
    {
        if(gio.MaVoucher!=null && !new KhuyenMaiRepository(c,t).DungVoucher(c,t,gio.MaVoucher,ngay,gio.MaCTKM))
            throw new InvalidOperationException("Voucher vừa hết hiệu lực hoặc hết lượt. Hãy áp dụng lại mã.");
    }
    internal static void GhiChiTiet(SqlConnection c,SqlTransaction t,int ma,KetQuaGioHang gio)
    {
        foreach(var d in gio.Dong.OrderBy(d=>d.MaSP))
        {
            using var cmd=new SqlCommand("INSERT ChiTietHoaDon(MaHD,MaSP,SoLuong,DonGia,GiamGia) VALUES(@hd,@sp,@sl,@gia,@gg)",c,t);
            cmd.Parameters.AddWithValue("@hd",ma);cmd.Parameters.AddWithValue("@sp",d.MaSP);cmd.Parameters.AddWithValue("@sl",d.SoLuong);
            cmd.Parameters.AddWithValue("@gia",d.DonGia);cmd.Parameters.AddWithValue("@gg",d.PhanTramGiam);cmd.ExecuteNonQuery();
        }
        using var check=new SqlCommand("SELECT TongTien FROM HoaDon WHERE MaHD=@hd",c,t);check.Parameters.AddWithValue("@hd",ma);
        if(Convert.ToDecimal(check.ExecuteScalar())!=gio.TongThanhToan)
            throw new InvalidOperationException("Tổng tiền do CSDL tính không khớp báo giá. Giao dịch đã được hủy.");
    }
}
