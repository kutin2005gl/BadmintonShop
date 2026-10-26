using BadmintonShop.Core.Services;
using System.Data;
using BadmintonShop.Core.Models;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

/// <summary>
/// Dat hang online, lich su, huy don cua KHACH HANG (SV3). Ton kho + TongTien do trigger tu cap nhat.
/// Loi nghiep vu nem ArgumentException / InvalidOperationException (thong bao tieng Viet, hien thang cho khach);
/// loi CSDL la SqlException -> hien bang LoiCsdl.ThongBao(ex).
/// </summary>
public class DonHangOnlineRepository
{
    public static readonly string[] HinhThucHopLe = { "COD", "Online" };

    // TV3: tao HoaDon + ChiTietHoaDon trong 1 transaction, tra ve MaHD; gio phai la ket qua cua GioHangService.Tinh
    public int TaoDon(int maKH, string diaChiGiao, string hinhThucTT, KetQuaGioHang gio)
    {
        if(maKH<=0)throw new ArgumentException("Cần khách hàng đã đăng nhập.");
        GiaoDichBanHang.KiemTraGio(gio);
        if (!gio.CoTheDatHang) throw new ArgumentException("Có sản phẩm không đủ tồn kho, vui lòng chỉnh lại giỏ hàng.");
        diaChiGiao=QuyTacBanHang.BatBuoc(diaChiGiao,255,"Địa chỉ giao hàng");
        if (!HinhThucHopLe.Contains(hinhThucTT)) throw new ArgumentException("Hình thức thanh toán không hợp lệ.");

        return DbHelper.ExecuteTransaction((c, t) =>
        {
            var ngay=DateTime.Now;
            var fresh=GiaoDichBanHang.TinhLai(c,t,gio,ngay);
            GiaoDichBanHang.DungVoucher(c,t,fresh,ngay);

            using var hd = new SqlCommand(@"INSERT INTO HoaDon (MaKH, LoaiBan, HinhThucTT, DiaChiGiao, MaCTKM, TienGiamGia)
                                            VALUES (@kh, N'Online', @tt, @dc, @ctkm, @giam);
                                            SELECT CAST(SCOPE_IDENTITY() AS INT);", c, t);
            hd.Parameters.AddWithValue("@kh", maKH);
            hd.Parameters.AddWithValue("@tt", hinhThucTT);
            hd.Parameters.AddWithValue("@dc", diaChiGiao.Trim());
            hd.Parameters.AddWithValue("@ctkm", DbHelper.GiaTriHoacNull(fresh.MaCTKM));
            hd.Parameters.AddWithValue("@giam", fresh.TienGiamHoaDon);
            var maHD = (int)hd.ExecuteScalar()!;

            GiaoDichBanHang.GhiChiTiet(c,t,maHD,fresh);
            return maHD;
        },IsolationLevel.Serializable);
    }

    // TV3: lich su mua hang cua 1 khach, moi nhat truoc (de do len bang)
    public DataTable LayLichSu(int maKH)
        => DbHelper.ExecuteQuery(@"SELECT MaHD, NgayLap, HinhThucTT, TienGiamGia, TongTien, TrangThai
                                   FROM HoaDon WHERE MaKH = @kh AND LoaiBan = N'Online'
                                   ORDER BY NgayLap DESC, MaHD DESC",
                                 new SqlParameter("@kh", maKH));

    // TV3: lay 1 don cua DUNG khach do (khong phai cua minh -> null, tranh xem don nguoi khac)
    public HoaDon? LayDonCuaKhach(int maHD, int maKH)
    {
        var dt = DbHelper.ExecuteQuery(
            @"SELECT * FROM HoaDon WHERE MaHD=@hd AND MaKH=@kh AND LoaiBan=N'Online'",
            new SqlParameter("@hd", maHD), new SqlParameter("@kh", maKH));
        return dt.Rows.Count==0?null:HoaDonRepository.Map(dt.Rows[0]);
    }

    // TV3: chi tiet don kem ten san pham (gia, giam la gia LUC DAT, khong phai gia hien tai)
    public DataTable LayChiTietHienThi(int maHD,int? maKH=null)
        => DbHelper.ExecuteQuery(@"SELECT ct.MaSP, sp.TenSP, sp.HinhAnh, ct.SoLuong, ct.DonGia, ct.GiamGia, ct.ThanhTien
                                   FROM ChiTietHoaDon ct JOIN SanPham sp ON sp.MaSP = ct.MaSP JOIN HoaDon h ON h.MaHD=ct.MaHD
                                   WHERE ct.MaHD = @hd AND (@kh IS NULL OR (h.MaKH=@kh AND h.LoaiBan=N'Online')) ORDER BY sp.TenSP",
                                 new SqlParameter("@hd", maHD),new SqlParameter("@kh",DbHelper.GiaTriHoacNull(maKH)));

    // TV3: khach huy don cua minh, chi khi con "Chờ xác nhận"; trigger tu hoan ton kho. false = khong huy duoc
    public bool HuyDon(int maHD, int maKH)
        => DbHelper.ExecuteNonQuery(
               @"UPDATE HoaDon SET TrangThai = N'Đã hủy'
                 WHERE MaHD = @hd AND MaKH = @kh AND LoaiBan = N'Online' AND TrangThai = N'Chờ xác nhận'",
               new SqlParameter("@hd", maHD), new SqlParameter("@kh", maKH)) > 0;
}