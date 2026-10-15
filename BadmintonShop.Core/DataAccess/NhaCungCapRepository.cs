using System.Data;
using BadmintonShop.Core.Models;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

/// <summary>
/// Repository MAU cho ca nhom copy theo: moi bang 1 file XxxRepository.cs,
/// SQL co tham so, map DataRow -> Model o 1 cho.
/// </summary>
public class NhaCungCapRepository
{
    public List<NhaCungCap> LayTatCa(string? tuKhoa = null)
    {
        const string sql = @"SELECT MaNCC, TenNCC, DiaChi, SDT, Email FROM NhaCungCap
                             WHERE @tk IS NULL OR TenNCC LIKE N'%' + @tk + N'%' OR SDT LIKE '%' + @tk + '%'
                             ORDER BY TenNCC";
        var dt = DbHelper.ExecuteQuery(sql, new SqlParameter("@tk", DbHelper.GiaTriHoacNull(tuKhoa)));
        return dt.AsEnumerable().Select(Map).ToList();
    }

    public NhaCungCap? LayTheoMa(int maNCC)
    {
        var dt = DbHelper.ExecuteQuery(
            "SELECT MaNCC, TenNCC, DiaChi, SDT, Email FROM NhaCungCap WHERE MaNCC = @ma",
            new SqlParameter("@ma", maNCC));
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    /// <returns>Ma NCC vua tao</returns>
    public int Them(NhaCungCap ncc)
    {
        const string sql = @"INSERT INTO NhaCungCap (TenNCC, DiaChi, SDT, Email)
                             VALUES (@ten, @dc, @sdt, @email);
                             SELECT CAST(SCOPE_IDENTITY() AS INT);";
        return (int)DbHelper.ExecuteScalar(sql, ThamSo(ncc))!;
    }

    public bool Sua(NhaCungCap ncc)
    {
        const string sql = @"UPDATE NhaCungCap SET TenNCC = @ten, DiaChi = @dc, SDT = @sdt, Email = @email
                             WHERE MaNCC = @ma";
        var ps = ThamSo(ncc).Append(new SqlParameter("@ma", ncc.MaNCC)).ToArray();
        return DbHelper.ExecuteNonQuery(sql, ps) > 0;
    }

    /// <summary>Khong cho xoa NCC da co phieu nhap (FK se bao loi).</summary>
    public bool CoTheXoa(int maNCC)
        => (int)DbHelper.ExecuteScalar("SELECT COUNT(*) FROM PhieuNhap WHERE MaNCC = @ma",
                                       new SqlParameter("@ma", maNCC))! == 0;

    public bool Xoa(int maNCC)
        => DbHelper.ExecuteNonQuery("DELETE FROM NhaCungCap WHERE MaNCC = @ma",
                                    new SqlParameter("@ma", maNCC)) > 0;

    private static SqlParameter[] ThamSo(NhaCungCap ncc) => new[]
    {
        new SqlParameter("@ten",   ncc.TenNCC),
        new SqlParameter("@dc",    DbHelper.GiaTriHoacNull(ncc.DiaChi)),
        new SqlParameter("@sdt",   DbHelper.GiaTriHoacNull(ncc.SDT)),
        new SqlParameter("@email", DbHelper.GiaTriHoacNull(ncc.Email)),
    };

    private static NhaCungCap Map(DataRow r) => new()
    {
        MaNCC  = r.Field<int>("MaNCC"),
        TenNCC = r.Field<string>("TenNCC")!,
        DiaChi = r.Field<string?>("DiaChi"),
        SDT    = r.Field<string?>("SDT"),
        Email  = r.Field<string?>("Email"),
    };
}
