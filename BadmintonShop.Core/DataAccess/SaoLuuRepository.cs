using System.Data;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

/// <summary>
/// Sao luu / phuc hoi CSDL BadmintonShop (SV1 - ROADMAP 4.1). Chi ADMIN duoc goi.
/// Lenh BACKUP/RESTORE chay tren CSDL master, ten CSDL lay tu chuoi ket noi hien tai.
/// Luu y: duong dan file la duong dan tren MAY CHAY SQL SERVER (LocalDB / SQL Server cung may thi nhu nhau).
/// </summary>
public class SaoLuuRepository
{
    /// <summary>Ten CSDL dang ket noi (Initial Catalog trong chuoi ket noi).</summary>
    public string TenCSDL => new SqlConnectionStringBuilder(DbHelper.ConnectionString).InitialCatalog;

    // TV1: mo ket noi toi master tren cung server (BACKUP/RESTORE khong chay duoc khi dang dung chinh CSDL do)
    private static SqlConnection KetNoiMaster()
    {
        var b = new SqlConnectionStringBuilder(DbHelper.ConnectionString) { InitialCatalog = "master", Pooling = false };
        var conn = new SqlConnection(b.ConnectionString);
        conn.Open();
        return conn;
    }

    // TV1: chay 1 lenh tren master, khong gioi han thoi gian (sao luu/phuc hoi CSDL lon co the lau)
    private static void ChayLenh(SqlConnection conn, string sql, params SqlParameter[] thamSo)
    {
        using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 0 };
        cmd.Parameters.AddRange(thamSo);
        cmd.ExecuteNonQuery();
    }

    // TV1: lay thu muc sao luu mac dinh cua SQL Server (goi y cho nguoi dung), loi thi tra ve null
    public string? LayThuMucMacDinh()
    {
        try
        {
            using var conn = KetNoiMaster();
            using var cmd = new SqlCommand("SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(400))", conn);
            return cmd.ExecuteScalar() is string s && s.Length > 0 ? s : null;   // LocalDB tra ve chuoi rong
        }
        catch (SqlException) { return null; }
    }

    // TV1: sao luu toan bo CSDL ra file .bak trong thuMuc, ten file kem ngay gio; tra ve duong dan file
    public string SaoLuu(string thuMuc)
    {
        var ten = TenCSDL;
        var file = Path.Combine(thuMuc, $"{ten}_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
        using var conn = KetNoiMaster();
        ChayLenh(conn, $"BACKUP DATABASE {QuoteName(ten)} TO DISK = @file WITH INIT, NAME = @nhan",
                 new SqlParameter("@file", file),
                 new SqlParameter("@nhan", $"{ten} - sao luu {DateTime.Now:dd/MM/yyyy HH:mm}"));
        return file;
    }

    // TV1: phuc hoi CSDL tu file .bak: kiem tra file -> SINGLE_USER -> RESTORE WITH REPLACE (+MOVE) -> MULTI_USER
    public void PhucHoi(string fileBak)
    {
        var ten = TenCSDL;
        SqlConnection.ClearAllPools();   // dong cac ket noi dang mo cua chinh app nay
        using var conn = KetNoiMaster();

        ChayLenh(conn, "RESTORE VERIFYONLY FROM DISK = @file", new SqlParameter("@file", fileBak));
        var move = TaoMenhDeMove(conn, ten, fileBak);

        ChayLenh(conn, $"IF DB_ID(@ten) IS NOT NULL ALTER DATABASE {QuoteName(ten)} SET SINGLE_USER WITH ROLLBACK IMMEDIATE",
                 new SqlParameter("@ten", ten));
        try
        {
            ChayLenh(conn, $"RESTORE DATABASE {QuoteName(ten)} FROM DISK = @file WITH REPLACE{move.Sql}",
                     move.ThamSo.Prepend(new SqlParameter("@file", fileBak)).ToArray());
        }
        finally
        {
            // du phuc hoi loi hay khong cung tra CSDL ve nhieu nguoi dung
            ChayLenh(conn, $"IF DB_ID(@ten) IS NOT NULL ALTER DATABASE {QuoteName(ten)} SET MULTI_USER",
                     new SqlParameter("@ten", ten));
        }
        SqlConnection.ClearAllPools();
    }

    // TV1: tao menh de MOVE de file .bak tu may khac (duong dan .mdf/.ldf khac) van phuc hoi duoc vao dung cho CSDL hien tai
    private static (string Sql, List<SqlParameter> ThamSo) TaoMenhDeMove(SqlConnection conn, string ten, string fileBak)
    {
        var dsFileBak = new DataTable();
        using (var cmd = new SqlCommand("RESTORE FILELISTONLY FROM DISK = @file", conn) { CommandTimeout = 0 })
        {
            cmd.Parameters.AddWithValue("@file", fileBak);
            using var r = cmd.ExecuteReader();
            dsFileBak.Load(r);
        }

        // file vat ly hien tai cua CSDL (theo loai: D = du lieu, L = log)
        var hienTai = new Dictionary<string, string>();
        using (var cmd = new SqlCommand(@"SELECT CASE type WHEN 0 THEN 'D' WHEN 1 THEN 'L' END, physical_name
                                          FROM sys.master_files WHERE database_id = DB_ID(@ten) AND type IN (0, 1)", conn))
        {
            cmd.Parameters.AddWithValue("@ten", ten);
            using var r = cmd.ExecuteReader();
            while (r.Read()) hienTai.TryAdd(r.GetString(0), r.GetString(1));
        }
        if (hienTai.Count == 0) return ("", new List<SqlParameter>());   // CSDL chua ton tai -> dung duong dan trong file .bak

        var sql = "";
        var ps = new List<SqlParameter>();
        var i = 0;
        foreach (DataRow row in dsFileBak.Rows)
        {
            var loai = row.Field<string>("Type")!;
            if (!hienTai.TryGetValue(loai, out var duongDan)) continue;
            sql += $", MOVE @ln{i} TO @pn{i}";
            ps.Add(new SqlParameter($"@ln{i}", row.Field<string>("LogicalName")!));
            ps.Add(new SqlParameter($"@pn{i}", duongDan));
            i++;
        }
        return (sql, ps);
    }

    // TV1: boc ten CSDL trong [ ] an toan (ten khong truyen duoc bang tham so trong BACKUP/ALTER DATABASE)
    private static string QuoteName(string ten) => "[" + ten.Replace("]", "]]") + "]";
}
