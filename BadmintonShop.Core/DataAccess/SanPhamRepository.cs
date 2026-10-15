using System.Data;
using BadmintonShop.Core.Models;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

/// <summary>
/// Truy xuat bang SanPham - phan DOC dung chung cho ca nhom (SV1 tao).
/// SV2/SV4 bo sung ham ghi (Them/Sua/Xoa...) vao file nay.
/// </summary>
public class SanPhamRepository
{
    private const string Cot = "MaSP, TenSP, MaDM, ThuongHieu, DonGiaBan, SoLuongTon, DonViTinh, HinhAnh, MoTa, TrangThai";

    // TV1: lay danh sach san pham, loc theo tu khoa (ten / thuong hieu) va danh muc neu co
    public List<SanPham> LayTatCa(string? tuKhoa = null, int? maDM = null)
    {
        var sql = $@"SELECT {Cot} FROM SanPham
                     WHERE (@tk IS NULL OR TenSP LIKE N'%' + @tk + N'%' OR ThuongHieu LIKE N'%' + @tk + N'%')
                       AND (@dm IS NULL OR MaDM = @dm)
                     ORDER BY TenSP";
        var dt = DbHelper.ExecuteQuery(sql,
            new SqlParameter("@tk", SqlDbType.NVarChar, 150) { Value = DbHelper.GiaTriHoacNull(tuKhoa) },
            new SqlParameter("@dm", SqlDbType.Int) { Value = DbHelper.GiaTriHoacNull(maDM) });
        return dt.AsEnumerable().Select(Map).ToList();
    }

    // TV1: lay 1 san pham theo ma, khong co tra ve null
    public SanPham? LayTheoMa(int maSP)
    {
        var dt = DbHelper.ExecuteQuery($"SELECT {Cot} FROM SanPham WHERE MaSP = @ma",
                                       new SqlParameter("@ma", maSP));
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    // TV1: chuyen 1 dong DataRow thanh doi tuong SanPham
    private static SanPham Map(DataRow r) => new()
    {
        MaSP       = r.Field<int>("MaSP"),
        TenSP      = r.Field<string>("TenSP")!,
        MaDM       = r.Field<int>("MaDM"),
        ThuongHieu = r.Field<string?>("ThuongHieu"),
        DonGiaBan  = r.Field<decimal>("DonGiaBan"),
        SoLuongTon = r.Field<int>("SoLuongTon"),
        DonViTinh  = r.Field<string>("DonViTinh")!,
        HinhAnh    = r.Field<string?>("HinhAnh"),
        MoTa       = r.Field<string?>("MoTa"),
        TrangThai  = r.Field<string>("TrangThai")!,
    };
}
