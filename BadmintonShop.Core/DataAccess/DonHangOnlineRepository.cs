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
    public const int GhiChuToiDa = 500;

    // TV3: tao HoaDon + ChiTietHoaDon trong 1 transaction, tra ve MaHD; gio phai la ket qua cua GioHangService.Tinh
    public int TaoDon(int maKH, string diaChiGiao, string hinhThucTT, KetQuaGioHang gio, string? ghiChu = null)
    {
        if (maKH <= 0) throw new ArgumentException("Cần khách hàng đã đăng nhập.");
        GiaoDichBanHang.KiemTraGio(gio);
        if (!gio.CoTheDatHang) throw new ArgumentException("Có sản phẩm không đủ tồn kho, vui lòng chỉnh lại giỏ hàng.");
        diaChiGiao = QuyTacBanHang.BatBuoc(diaChiGiao, 255, "Địa chỉ giao hàng");
        if (!HinhThucHopLe.Contains(hinhThucTT)) throw new ArgumentException("Hình thức thanh toán không hợp lệ.");
        // TV3: ghi chu khong bat buoc, toi da 500 ky tu, rong -> NULL
        ghiChu = string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim();
        if (ghiChu != null && ghiChu.Length > GhiChuToiDa) throw new ArgumentException($"Ghi chú tối đa {GhiChuToiDa} ký tự.");

        return DbHelper.ExecuteTransaction((c, t) =>
        {
            var ngay = DateTime.Now;
            var fresh = GiaoDichBanHang.TinhLai(c, t, gio, ngay);
            GiaoDichBanHang.DungVoucher(c, t, fresh, ngay, maKH);

            using var hd = new SqlCommand(@"INSERT INTO HoaDon (MaKH, LoaiBan, HinhThucTT, DiaChiGiao, MaCTKM, TienGiamGia, GhiChu)
                                            VALUES (@kh, N'Online', @tt, @dc, @ctkm, @giam, @gc);
                                            SELECT CAST(SCOPE_IDENTITY() AS INT);", c, t);
            hd.Parameters.AddWithValue("@kh", maKH);
            hd.Parameters.AddWithValue("@tt", hinhThucTT);
            hd.Parameters.AddWithValue("@dc", diaChiGiao.Trim());
            hd.Parameters.AddWithValue("@ctkm", DbHelper.GiaTriHoacNull(fresh.MaCTKM));
            hd.Parameters.AddWithValue("@giam", fresh.TienGiamHoaDon);
            hd.Parameters.AddWithValue("@gc", DbHelper.GiaTriHoacNull(ghiChu));
            var maHD = (int)hd.ExecuteScalar()!;

            GiaoDichBanHang.GhiVoucher(c, t, maHD, fresh);
            GiaoDichBanHang.GhiChiTiet(c, t, maHD, fresh);
            return maHD;
        }, IsolationLevel.Serializable);
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
        return dt.Rows.Count == 0 ? null : HoaDonRepository.Map(dt.Rows[0]);
    }

    // TV1: tra cuu don ONLINE bang ma don + SDT (da chuan hoa 0xxx) cua khach so huu don; khop tra MaKH, khong khop tra null
    public int? TraCuuMaKHTheoSDT(int maHD, string sdtChuanHoa)
    {
        // SDT trong CSDL co the luu dang +84..., 84..., co dau cach/cham/gach
        var kq = DbHelper.ExecuteScalar(@"
            SELECT h.MaKH FROM HoaDon h JOIN KhachHang k ON k.MaKH = h.MaKH
            WHERE h.MaHD = @hd AND h.LoaiBan = N'Online'
              AND REPLACE(REPLACE(REPLACE(REPLACE(k.SDT,' ',''),'-',''),'.',''),'+','') IN (@a, @b)",
            new SqlParameter("@hd", maHD),
            new SqlParameter("@a", sdtChuanHoa),
            new SqlParameter("@b", "84" + sdtChuanHoa[1..]));
        return kq is int ma ? ma : null;
    }

    // TV1: danh sach don ONLINE cua (cac) khach co SDT nay (da chuan hoa 0xxx), moi nhat truoc - dung sau khi xac thuc OTP
    public DataTable LayDonTheoSDT(string sdtChuanHoa)
        => DbHelper.ExecuteQuery(@"
            SELECT h.MaHD, h.NgayLap, h.HinhThucTT, h.TongTien, h.TrangThai
            FROM HoaDon h JOIN KhachHang k ON k.MaKH = h.MaKH
            WHERE h.LoaiBan = N'Online'
              AND REPLACE(REPLACE(REPLACE(REPLACE(k.SDT,' ',''),'-',''),'.',''),'+','') IN (@a, @b)
            ORDER BY h.NgayLap DESC, h.MaHD DESC",
            new SqlParameter("@a", sdtChuanHoa),
            new SqlParameter("@b", "84" + sdtChuanHoa[1..]));

    // TV3: chi tiet don kem ten san pham (gia, giam la gia LUC DAT, khong phai gia hien tai)
    public DataTable LayChiTietHienThi(int maHD, int? maKH = null)
        => DbHelper.ExecuteQuery(@"SELECT ct.MaSP, sp.TenSP, sp.HinhAnh, ct.SoLuong, ct.DonGia, ct.GiamGia, ct.ThanhTien
                                   FROM ChiTietHoaDon ct JOIN SanPham sp ON sp.MaSP = ct.MaSP JOIN HoaDon h ON h.MaHD=ct.MaHD
                                   WHERE ct.MaHD = @hd AND (@kh IS NULL OR (h.MaKH=@kh AND h.LoaiBan=N'Online')) ORDER BY sp.TenSP",
                                 new SqlParameter("@hd", maHD), new SqlParameter("@kh", DbHelper.GiaTriHoacNull(maKH)));

    // TV3: khach huy don cua minh, chi khi con "Chờ xác nhận"; trigger tu hoan ton kho. false = khong huy duoc
    public bool HuyDon(int maHD, int maKH)
        => DbHelper.ExecuteNonQuery(
               @"UPDATE HoaDon SET TrangThai = N'Đã hủy', LyDoHuy = N'Khách hủy đơn'
                 WHERE MaHD = @hd AND MaKH = @kh AND LoaiBan = N'Online' AND TrangThai = N'Chờ xác nhận'",
               new SqlParameter("@hd", maHD), new SqlParameter("@kh", maKH)) > 0;
}