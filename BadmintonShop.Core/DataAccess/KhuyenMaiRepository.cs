using System.Data;
using BadmintonShop.Core.Models;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

/// <summary>
/// Truy xuat CTKM, KM_TheoSP, KM_TheoHD, KM_TheoVoucher (SV3).
/// Chi tinh CTKM "con hieu luc": TrangThai = 1 va ngay nam trong [NgayBatDau, NgayKetThuc].
/// </summary>
public class KhuyenMaiRepository
{
    // TV3: % giam cao nhat dang ap dung cho 1 san pham vao ngay 'ngay', khong co tra ve 0
    public decimal LayPhanTramSanPham(int maSP, DateTime ngay)
    {
        const string sql = @"SELECT MAX(sp.PhanTramGG)
                             FROM KM_TheoSP sp JOIN CTKM k ON k.MaCTKM = sp.MaCTKM
                             WHERE sp.MaSP = @sp AND k.LoaiKM = 'SANPHAM' AND k.TrangThai = 1
                               AND @ngay BETWEEN k.NgayBatDau AND k.NgayKetThuc";
        var kq = DbHelper.ExecuteScalar(sql,
            new SqlParameter("@sp", maSP),
            new SqlParameter("@ngay", SqlDbType.Date) { Value = ngay.Date });
        return kq is decimal d ? d : 0m;
    }

    // TV3: bac giam theo hoa don cao nhat dat duoc (TongTienToiThieu lon nhat <= tongTienHang), khong co tra ve null
    public KMTheoHD? LayBacHoaDon(decimal tongTienHang, DateTime ngay)
    {
        const string sql = @"SELECT TOP 1 h.MaCTKM, h.TongTienToiThieu, h.PhanTramGG
                             FROM KM_TheoHD h JOIN CTKM k ON k.MaCTKM = h.MaCTKM
                             WHERE k.LoaiKM = 'HOADON' AND k.TrangThai = 1
                               AND @ngay BETWEEN k.NgayBatDau AND k.NgayKetThuc
                               AND h.TongTienToiThieu <= @tong
                             ORDER BY h.TongTienToiThieu DESC, h.PhanTramGG DESC";
        var dt = DbHelper.ExecuteQuery(sql,
            new SqlParameter("@tong", tongTienHang),
            new SqlParameter("@ngay", SqlDbType.Date) { Value = ngay.Date });
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        return new KMTheoHD
        {
            MaCTKM = r.Field<int>("MaCTKM"),
            TongTienToiThieu = r.Field<decimal>("TongTienToiThieu"),
            PhanTramGG = r.Field<decimal>("PhanTramGG"),
        };
    }

    // TV3: lay voucher con hieu luc (CTKM dang bat, dung ngay, DaDung < SoLuong); sai / het han / het luot tra ve null
    public KMTheoVoucher? LayVoucherHopLe(string maVoucher, DateTime ngay)
    {
        const string sql = @"SELECT v.MaCTKM, v.MaVoucher, v.PhanTramGG, v.SoLuong, v.DaDung
                             FROM KM_TheoVoucher v JOIN CTKM k ON k.MaCTKM = v.MaCTKM
                             WHERE v.MaVoucher = @ma AND k.LoaiKM = 'VOUCHER' AND k.TrangThai = 1
                               AND @ngay BETWEEN k.NgayBatDau AND k.NgayKetThuc
                               AND v.DaDung < v.SoLuong";
        var dt = DbHelper.ExecuteQuery(sql,
            new SqlParameter("@ma", SqlDbType.VarChar, 30) { Value = (maVoucher ?? "").Trim().ToUpperInvariant() },
            new SqlParameter("@ngay", SqlDbType.Date) { Value = ngay.Date });
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        return new KMTheoVoucher
        {
            MaCTKM = r.Field<int>("MaCTKM"),
            MaVoucher = r.Field<string>("MaVoucher")!,
            PhanTramGG = r.Field<decimal>("PhanTramGG"),
            SoLuong = r.Field<int>("SoLuong"),
            DaDung = r.Field<int>("DaDung"),
        };
    }

    // TV3: tang DaDung cua voucher them 1 BEN TRONG transaction dat hang; false = vua het luot (co nguoi dung truoc)
    public bool DungVoucher(SqlConnection conn, SqlTransaction tran, string maVoucher)
    {
        using var cmd = new SqlCommand(
            "UPDATE KM_TheoVoucher SET DaDung = DaDung + 1 WHERE MaVoucher = @ma AND DaDung < SoLuong", conn, tran);
        cmd.Parameters.AddWithValue("@ma", maVoucher.Trim().ToUpperInvariant());
        return cmd.ExecuteNonQuery() > 0;
    }
}