using System.Data;
using BadmintonShop.Core.Models;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

/// <summary>Truy xuat bang NhanVien (SV1).</summary>
public class NhanVienRepository
{
    private const string Cot = "MaNV, HoTen, SDT, Email, DiaChi, ChucVu, Luong, NgayVaoLam";

    // TV1: lay danh sach nhan vien, loc theo tu khoa (ho ten / SDT / email) neu co
    public List<NhanVien> LayTatCa(string? tuKhoa = null)
    {
        var sql = $@"SELECT {Cot} FROM NhanVien
                     WHERE @tk IS NULL OR HoTen LIKE N'%' + @tk + N'%'
                           OR SDT LIKE '%' + @tk + '%' OR Email LIKE '%' + @tk + '%'
                     ORDER BY HoTen";
        var dt = DbHelper.ExecuteQuery(sql, new SqlParameter("@tk", DbHelper.GiaTriHoacNull(tuKhoa)));
        return dt.AsEnumerable().Select(Map).ToList();
    }

    // TV1: lay 1 nhan vien theo ma, khong co tra ve null
    public NhanVien? LayTheoMa(int maNV)
    {
        var dt = DbHelper.ExecuteQuery($"SELECT {Cot} FROM NhanVien WHERE MaNV = @ma",
                                       new SqlParameter("@ma", maNV));
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    // TV1: lay cac nhan vien chua co tai khoan (dung cho ComboBox khi cap tai khoan)
    public List<NhanVien> LayChuaCoTaiKhoan()
    {
        var sql = $@"SELECT {Cot} FROM NhanVien nv
                     WHERE NOT EXISTS (SELECT 1 FROM TaiKhoan tk WHERE tk.MaNV = nv.MaNV)
                     ORDER BY HoTen";
        return DbHelper.ExecuteQuery(sql).AsEnumerable().Select(Map).ToList();
    }

    // TV1: them nhan vien moi, tra ve ma NV vua tao
    public int Them(NhanVien nv)
    {
        const string sql = @"INSERT INTO NhanVien (HoTen, SDT, Email, DiaChi, ChucVu, Luong, NgayVaoLam)
                             VALUES (@ten, @sdt, @email, @dc, @cv, @luong, @ngay);
                             SELECT CAST(SCOPE_IDENTITY() AS INT);";
        return (int)DbHelper.ExecuteScalar(sql, ThamSo(nv))!;
    }

    // TV1: cap nhat thong tin nhan vien theo MaNV
    public bool Sua(NhanVien nv)
    {
        const string sql = @"UPDATE NhanVien SET HoTen = @ten, SDT = @sdt, Email = @email, DiaChi = @dc,
                                    ChucVu = @cv, Luong = @luong, NgayVaoLam = @ngay
                             WHERE MaNV = @ma";
        var ps = ThamSo(nv).Append(new SqlParameter("@ma", nv.MaNV)).ToArray();
        return DbHelper.ExecuteNonQuery(sql, ps) > 0;
    }

    // TV1: kiem tra NV co xoa duoc khong (chua co tai khoan, phieu nhap, hoa don)
    public bool CoTheXoa(int maNV)
    {
        const string sql = @"SELECT (SELECT COUNT(*) FROM TaiKhoan  WHERE MaNV = @ma)
                                  + (SELECT COUNT(*) FROM PhieuNhap WHERE MaNV = @ma)
                                  + (SELECT COUNT(*) FROM HoaDon    WHERE MaNV = @ma)";
        return (int)DbHelper.ExecuteScalar(sql, new SqlParameter("@ma", maNV))! == 0;
    }

    // TV1: xoa nhan vien (goi CoTheXoa truoc de bao ly do)
    public bool Xoa(int maNV)
        => DbHelper.ExecuteNonQuery("DELETE FROM NhanVien WHERE MaNV = @ma",
                                    new SqlParameter("@ma", maNV)) > 0;

    // TV1: tao mang tham so SQL tu doi tuong NhanVien
    private static SqlParameter[] ThamSo(NhanVien nv) => new[]
    {
        new SqlParameter("@ten",   nv.HoTen),
        new SqlParameter("@sdt",   DbHelper.GiaTriHoacNull(nv.SDT)),
        new SqlParameter("@email", DbHelper.GiaTriHoacNull(nv.Email)),
        new SqlParameter("@dc",    DbHelper.GiaTriHoacNull(nv.DiaChi)),
        new SqlParameter("@cv",    nv.ChucVu),
        new SqlParameter("@luong", DbHelper.GiaTriHoacNull(nv.Luong)),
        new SqlParameter("@ngay",  nv.NgayVaoLam == default ? DateTime.Today : nv.NgayVaoLam.Date),
    };

    // TV1: chuyen 1 dong DataRow thanh doi tuong NhanVien
    private static NhanVien Map(DataRow r) => new()
    {
        MaNV       = r.Field<int>("MaNV"),
        HoTen      = r.Field<string>("HoTen")!,
        SDT        = r.Field<string?>("SDT"),
        Email      = r.Field<string?>("Email"),
        DiaChi     = r.Field<string?>("DiaChi"),
        ChucVu     = r.Field<string>("ChucVu")!,
        Luong      = r.Field<decimal?>("Luong"),
        NgayVaoLam = r.Field<DateTime>("NgayVaoLam"),
    };
}
