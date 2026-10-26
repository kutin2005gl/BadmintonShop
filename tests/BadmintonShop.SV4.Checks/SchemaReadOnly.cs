using System.Data;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;

internal static partial class Program
{
    // SV4: SELECT metadata only. No migrations, seeds, sales, authentication writes or connection-string output.
    private static int SchemaReadOnly()
    {
        var configured = Environment.GetEnvironmentVariable("ConnectionStrings__BadmintonShop");
        var desktop = configured ?? XDocument.Load(Path.Combine(root,"BadmintonShop.Desktop/App.config"))
            .Descendants("add").Single(x=>(string?)x.Attribute("name")=="BadmintonShop").Attribute("connectionString")!.Value;
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"BadmintonShop.Web/appsettings.json")));
        var web = configured ?? json.RootElement.GetProperty("ConnectionStrings").GetProperty("BadmintonShop").GetString()!;
        foreach(var (app,connection) in new[]{("Desktop",desktop),("Web",web)})
        {
            try
            {
                var b=new SqlConnectionStringBuilder(connection) { ConnectTimeout=5 };
                Ghi($"{app}: configured server={b.DataSource}; database={b.InitialCatalog}; credentials omitted");
                using var c=new SqlConnection(b.ConnectionString);c.Open();
                using(var cmd=new SqlCommand("SELECT CAST(SERVERPROPERTY('ServerName') AS nvarchar(128)),DB_NAME(),CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(128))",c))
                using(var r=cmd.ExecuteReader()) { r.Read();Ghi($"Connected: server={r.GetString(0)}; database={r.GetString(1)}; SQL={r.GetString(2)}"); }
                var columns=new (string Table,string Column,string Type,int Length,int Precision,int Scale)[] {
                    ("HoaDon","LyDoHuy","nvarchar",1000,0,0),("HoaDon","MaYeuCauPOS","uniqueidentifier",16,0,0),
                    ("HoaDon","TienKhachDua","decimal",9,18,0),("HoaDon","DauVanTayPOS","varchar",64,0,0),
                    ("HoaDon","MaCTKM","int",4,0,0),("HoaDon","TienGiamGia","decimal",9,18,0),
                    ("ChiTietHoaDon","GiamGia","decimal",5,5,2),("ChiTietHoaDon","ThanhTien","decimal",9,18,0) };
                foreach(var col in columns)
                {
                    using var cmd=new SqlCommand(@"SELECT TYPE_NAME(system_type_id),max_length,precision,scale,is_computed
                        FROM sys.columns WHERE object_id=OBJECT_ID(@table) AND name=@name",c);
                    cmd.Parameters.AddWithValue("@table","dbo."+col.Table);cmd.Parameters.AddWithValue("@name",col.Column);
                    using var r=cmd.ExecuteReader();
                    if(!r.Read()) { Ghi($"MISSING {col.Table}.{col.Column}; apply migration 04 then 06 on a backed-up database after review");continue; }
                    bool shape=r.GetString(0)==col.Type && r.GetInt16(1)==col.Length && (col.Precision==0 || r.GetByte(2)==col.Precision && r.GetByte(3)==col.Scale);
                    Ghi($"{(shape?"OK":"MISMATCH")} {col.Table}.{col.Column}: {r.GetString(0)}, bytes={r.GetInt16(1)}, precision={r.GetByte(2)}, scale={r.GetByte(3)}, computed={r.GetBoolean(4)}");
                }
                using(var cmd=new SqlCommand(@"SELECT i.name,i.is_unique,i.is_disabled,i.filter_definition,c.name,ic.key_ordinal
                    FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
                    JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
                    WHERE i.object_id=OBJECT_ID('dbo.HoaDon') AND c.name='MaYeuCauPOS'",c))
                using(var r=cmd.ExecuteReader())
                {
                    bool any=false;while(r.Read()) { any=true;Ghi($"INDEX {r.GetString(0)}: unique={r.GetBoolean(1)}, disabled={r.GetBoolean(2)}, filter={(r.IsDBNull(3)?"none":r.GetString(3))}, key ordinal={r.GetByte(5)}"); }
                    if(!any)Ghi("MISSING index UX_HoaDon_MaYeuCauPOS; review migration 04");
                }
                using(var cmd=new SqlCommand("SELECT name,is_disabled FROM sys.triggers WHERE name IN ('trg_CTPN_TangTon','trg_CTHD_GiamTon','trg_HoaDon_CapNhat') ORDER BY name",c))
                using(var r=cmd.ExecuteReader()) { while(r.Read())Ghi($"TRIGGER {r.GetString(0)}: disabled={r.GetBoolean(1)}"); }
            }
            catch(Exception ex) { failed++;Ghi($"UNVERIFIED {app}: {ex.GetType().Name}; no database writes attempted"); }
        }
        File.WriteAllLines(Path.Combine(output,"schema-readonly.txt"),log);Ghi("Read-only schema report: "+output);
        return failed==0?0:1;
    }
}
