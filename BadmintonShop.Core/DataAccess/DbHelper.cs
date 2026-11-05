using System.Data;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

/// <summary>
/// Lop truy xuat CSDL dung chung cho Web va Desktop (ADO.NET).
/// Moi app gan ConnectionString MOT LAN luc khoi dong:
///   - Web:     Program.cs  -> DbHelper.ConnectionString = builder.Configuration.GetConnectionString("BadmintonShop")!;
///   - Desktop: Program.cs  -> DbHelper.ConnectionString = ConfigurationManager.ConnectionStrings["BadmintonShop"].ConnectionString;
/// Quy tac nhom: KHONG viet SQL trong Controller / Form, chi viet trong cac Repository cua Core.
/// Luon dung tham so (@ten) de tranh SQL injection.
/// </summary>
public static class DbHelper
{
    private static string? _connectionString;
    private static long revision;
    public static long PhienBanDuLieu => Interlocked.Read(ref revision);
    public static string DauHieuDuLieu=>$"{PhienBanDuLieu}|{Services.TinHieuDuLieu.Doc(ConnectionString)}";
    public static void BaoDuLieuThayDoi()
    {
        Interlocked.Increment(ref revision);
        if(_connectionString!=null)Services.TinHieuDuLieu.Ghi(_connectionString);
    }

    public static string ConnectionString
    {
        get => _connectionString
               ?? throw new InvalidOperationException("Chua gan DbHelper.ConnectionString trong Program.cs");
        set => _connectionString = value;
    }

    public static SqlConnection TaoKetNoi() => new SqlConnection(ConnectionString);

    /// <summary>SELECT -> DataTable (tien cho DataGridView ben Desktop).</summary>
    public static DataTable ExecuteQuery(string sql, params SqlParameter[] thamSo)
    {
        using var conn = TaoKetNoi();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(thamSo);
        using var da = new SqlDataAdapter(cmd);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    /// <summary>INSERT / UPDATE / DELETE -> so dong bi anh huong.</summary>
    public static int ExecuteNonQuery(string sql, params SqlParameter[] thamSo)
    {
        using var conn = TaoKetNoi();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(thamSo);
        conn.Open();
        var result=cmd.ExecuteNonQuery();
        BaoDuLieuThayDoi();
        return result;
    }

    /// <summary>Lay 1 gia tri (COUNT, SCOPE_IDENTITY...).</summary>
    public static object? ExecuteScalar(string sql, params SqlParameter[] thamSo)
    {
        using var conn = TaoKetNoi();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(thamSo);
        conn.Open();
        var kq = cmd.ExecuteScalar();
        // Scalar is also used for INSERT ... SELECT identity. Conservatively invalidate
        // for non-SELECT batches; no user text is parsed or executed here.
        if(!sql.TrimStart().StartsWith("SELECT",StringComparison.OrdinalIgnoreCase))BaoDuLieuThayDoi();
        return kq == DBNull.Value ? null : kq;
    }

    /// <summary>
    /// Chay nhieu lenh trong MOT transaction (vd. luu PhieuNhap + ChiTietPhieuNhap,
    /// HoaDon + ChiTietHoaDon). Loi o bat ky buoc nao -> rollback toan bo.
    /// </summary>
    public static T ExecuteTransaction<T>(Func<SqlConnection, SqlTransaction, T> congViec,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, bool thayDoiDuLieu = true)
    {
        using var conn = TaoKetNoi();
        conn.Open();
        using var tran = conn.BeginTransaction(isolationLevel);
        try
        {
            var kq = congViec(conn, tran);
            tran.Commit();
            if(thayDoiDuLieu)BaoDuLieuThayDoi();
            return kq;
        }
        catch
        {
            try { tran.Rollback(); } catch (InvalidOperationException) { /* trigger da rollback */ }
            throw;
        }
    }

    /// <summary>Doi DBNull thanh null cho cot cho phep NULL.</summary>
    public static object GiaTriHoacNull(object? giaTri) => giaTri ?? DBNull.Value;

    /// <summary>Kiem tra ket noi (dung o man hinh dang nhap / luc khoi dong).</summary>
    public static bool ThuKetNoi(out string? loi)
    {
        try
        {
            using var conn = TaoKetNoi();
            conn.Open();
            loi = null;
            return true;
        }
        catch (Exception ex)
        {
            loi = ex.Message;
            return false;
        }
    }
}
