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
        if (gio.Dong.Count == 0) throw new ArgumentException("Giỏ hàng đang trống.");
        if (!gio.CoTheDatHang) throw new ArgumentException("Có sản phẩm không đủ tồn kho, vui lòng chỉnh lại giỏ hàng.");
        if (string.IsNullOrWhiteSpace(diaChiGiao)) throw new ArgumentException("Vui lòng nhập địa chỉ giao hàng.");
        if (!HinhThucHopLe.Contains(hinhThucTT)) throw new ArgumentException("Hình thức thanh toán không hợp lệ.");

        return DbHelper.ExecuteTransaction((c, t) =>
        {
            // khoa dong san pham, kiem lai gia / trang thai / ton ngay trong transaction
            foreach (var d in gio.Dong.OrderBy(x => x.MaSP))
            {
                using var q = new SqlCommand(
                    "SELECT DonGiaBan, SoLuongTon, TrangThai FROM SanPham WITH (UPDLOCK, HOLDLOCK) WHERE MaSP = @ma", c, t);
                q.Parameters.AddWithValue("@ma", d.MaSP);
                using var r = q.ExecuteReader();
                if (!r.Read() || r.GetString(2) != "Đang bán")
                    throw new InvalidOperationException($"“{d.TenSP}” không còn bán. Vui lòng kiểm tra lại giỏ hàng.");
                if (r.GetInt32(1) < d.SoLuong)
                    throw new InvalidOperationException($"“{d.TenSP}” không đủ tồn kho. Vui lòng giảm số lượng.");
                if (r.GetDecimal(0) != d.DonGia)
                    throw new InvalidOperationException($"Giá “{d.TenSP}” vừa thay đổi. Vui lòng xem lại giỏ hàng.");
            }

            if (gio.MaVoucher != null && !new KhuyenMaiRepository().DungVoucher(c, t, gio.MaVoucher))
                throw new InvalidOperationException("Mã giảm giá vừa hết lượt dùng.");

            using var hd = new SqlCommand(@"INSERT INTO HoaDon (MaKH, LoaiBan, HinhThucTT, DiaChiGiao, MaCTKM, TienGiamGia)
                                            VALUES (@kh, N'Online', @tt, @dc, @ctkm, @giam);
                                            SELECT CAST(SCOPE_IDENTITY() AS INT);", c, t);
            hd.Parameters.AddWithValue("@kh", maKH);
            hd.Parameters.AddWithValue("@tt", hinhThucTT);
            hd.Parameters.AddWithValue("@dc", diaChiGiao.Trim());
            hd.Parameters.AddWithValue("@ctkm", DbHelper.GiaTriHoacNull(gio.MaCTKM));
            hd.Parameters.AddWithValue("@giam", gio.TienGiamHoaDon);
            var maHD = (int)hd.ExecuteScalar()!;

            foreach (var d in gio.Dong)
            {
                using var ct = new SqlCommand(
                    "INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia, GiamGia) VALUES (@hd, @sp, @sl, @gia, @gg)", c, t);
                ct.Parameters.AddWithValue("@hd", maHD);
                ct.Parameters.AddWithValue("@sp", d.MaSP);
                ct.Parameters.AddWithValue("@sl", d.SoLuong);
                ct.Parameters.AddWithValue("@gia", d.DonGia);
                ct.Parameters.AddWithValue("@gg", d.PhanTramGiam);
                ct.ExecuteNonQuery();   // trigger tru ton + tinh TongTien (= tong ThanhTien - TienGiamGia)
            }
            return maHD;
        });
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
            @"SELECT MaHD, MaKH, MaNV, NgayLap, LoaiBan, HinhThucTT, DiaChiGiao, MaCTKM, TienGiamGia, TongTien, TrangThai
              FROM HoaDon WHERE MaHD = @hd AND MaKH = @kh",
            new SqlParameter("@hd", maHD), new SqlParameter("@kh", maKH));
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        return new HoaDon
        {
            MaHD = r.Field<int>("MaHD"),
            MaKH = r.Field<int?>("MaKH"),
            MaNV = r.Field<int?>("MaNV"),
            NgayLap = r.Field<DateTime>("NgayLap"),
            LoaiBan = r.Field<string>("LoaiBan")!,
            HinhThucTT = r.Field<string>("HinhThucTT")!,
            DiaChiGiao = r.Field<string?>("DiaChiGiao"),
            MaCTKM = r.Field<int?>("MaCTKM"),
            TienGiamGia = r.Field<decimal>("TienGiamGia"),
            TongTien = r.Field<decimal>("TongTien"),
            TrangThai = r.Field<string>("TrangThai")!,
        };
    }

    // TV3: chi tiet don kem ten san pham (gia, giam la gia LUC DAT, khong phai gia hien tai)
    public DataTable LayChiTietHienThi(int maHD)
        => DbHelper.ExecuteQuery(@"SELECT ct.MaSP, sp.TenSP, sp.HinhAnh, ct.SoLuong, ct.DonGia, ct.GiamGia, ct.ThanhTien
                                   FROM ChiTietHoaDon ct JOIN SanPham sp ON sp.MaSP = ct.MaSP
                                   WHERE ct.MaHD = @hd ORDER BY sp.TenSP",
                                 new SqlParameter("@hd", maHD));

    // TV3: khach huy don cua minh, chi khi con "Chờ xác nhận"; trigger tu hoan ton kho. false = khong huy duoc
    public bool HuyDon(int maHD, int maKH)
        => DbHelper.ExecuteNonQuery(
               @"UPDATE HoaDon SET TrangThai = N'Đã hủy'
                 WHERE MaHD = @hd AND MaKH = @kh AND LoaiBan = N'Online' AND TrangThai = N'Chờ xác nhận'",
               new SqlParameter("@hd", maHD), new SqlParameter("@kh", maKH)) > 0;
}