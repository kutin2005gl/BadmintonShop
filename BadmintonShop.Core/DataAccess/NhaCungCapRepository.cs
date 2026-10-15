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
    // TV1: lay danh sach nha cung cap, loc theo tu khoa (ten / SDT) neu co
    public List<NhaCungCap> LayTatCa(string? tuKhoa = null)
    {
        const string sql = @"SELECT MaNCC, TenNCC, DiaChi, SDT, Email FROM NhaCungCap
                             WHERE @tk IS NULL OR TenNCC LIKE N'%' + @tk + N'%' OR SDT LIKE '%' + @tk + '%'
                             ORDER BY TenNCC";
        var dt = DbHelper.ExecuteQuery(sql, new SqlParameter("@tk", DbHelper.GiaTriHoacNull(tuKhoa)));
        return dt.AsEnumerable().Select(Map).ToList();
    }

    // TV1: lay 1 nha cung cap theo ma, khong co tra ve null
    public NhaCungCap? LayTheoMa(int maNCC)
    {
        var dt = DbHelper.ExecuteQuery(
            "SELECT MaNCC, TenNCC, DiaChi, SDT, Email FROM NhaCungCap WHERE MaNCC = @ma",
            new SqlParameter("@ma", maNCC));
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    // TV1: them nha cung cap moi, tra ve ma NCC vua tao
    public int Them(NhaCungCap ncc)
    {
        const string sql = @"INSERT INTO NhaCungCap (TenNCC, DiaChi, SDT, Email)
                             VALUES (@ten, @dc, @sdt, @email);
                             SELECT CAST(SCOPE_IDENTITY() AS INT);";
        return (int)DbHelper.ExecuteScalar(sql, ThamSo(ncc))!;
    }

    // TV1: cap nhat thong tin nha cung cap theo MaNCC
    public bool Sua(NhaCungCap ncc)
    {
        const string sql = @"UPDATE NhaCungCap SET TenNCC = @ten, DiaChi = @dc, SDT = @sdt, Email = @email
                             WHERE MaNCC = @ma";
        var ps = ThamSo(ncc).Append(new SqlParameter("@ma", ncc.MaNCC)).ToArray();
        return DbHelper.ExecuteNonQuery(sql, ps) > 0;
    }

    // TV1: kiem tra NCC co xoa duoc khong (chua co phieu nhap nao)
    public bool CoTheXoa(int maNCC)
        => (int)DbHelper.ExecuteScalar("SELECT COUNT(*) FROM PhieuNhap WHERE MaNCC = @ma",
                                       new SqlParameter("@ma", maNCC))! == 0;

    // TV1: xoa nha cung cap (goi CoTheXoa truoc de bao ly do)
    public bool Xoa(int maNCC)
        => DbHelper.ExecuteNonQuery("DELETE FROM NhaCungCap WHERE MaNCC = @ma",
                                    new SqlParameter("@ma", maNCC)) > 0;

    // TV1: tao mang tham so SQL tu doi tuong NhaCungCap
    private static SqlParameter[] ThamSo(NhaCungCap ncc) => new[]
    {
        new SqlParameter("@ten",   ncc.TenNCC),
        new SqlParameter("@dc",    DbHelper.GiaTriHoacNull(ncc.DiaChi)),
        new SqlParameter("@sdt",   DbHelper.GiaTriHoacNull(ncc.SDT)),
        new SqlParameter("@email", DbHelper.GiaTriHoacNull(ncc.Email)),
    };

    // TV1: chuyen 1 dong DataRow thanh doi tuong NhaCungCap
    private static NhaCungCap Map(DataRow r) => new()
    {
        MaNCC  = r.Field<int>("MaNCC"),
        TenNCC = r.Field<string>("TenNCC")!,
        DiaChi = r.Field<string?>("DiaChi"),
        SDT    = r.Field<string?>("SDT"),
        Email  = r.Field<string?>("Email"),
    };
}
