using System.Data;
using System.ComponentModel;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;
using BadmintonShop.Desktop;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;

internal static partial class Program
{
    private static void FinalIntegrationChecks()
    {
        if(!new SqlConnectionStringBuilder(DbHelper.ConnectionString).InitialCatalog.StartsWith("BadmintonShop_SV4_Test_",StringComparison.Ordinal))throw new InvalidOperationException("Isolated test database required.");
        var pos=new HoaDonRepository();
        Test("N01 voucher reasons: missing, future, expired, exhausted, disabled, too long; no writes",()=>
        {
            ResetOffers();int p=Product();int future=Offer("VOUCHER",10,code:"FUTURE",endOffset:3);
            DbHelper.ExecuteNonQuery("UPDATE CTKM SET NgayBatDau=@date WHERE MaCTKM=@id",new SqlParameter("@date",DateTime.Today.AddDays(1)),new SqlParameter("@id",future));
            Offer("VOUCHER",10,code:"PAST",endOffset:-1);Offer("VOUCHER",10,code:"USEDUP",uses:0);
            int disabled=Offer("VOUCHER",10,code:"OFF");DbHelper.ExecuteNonQuery("UPDATE CTKM SET TrangThai=0 WHERE MaCTKM=@id",new SqlParameter("@id",disabled));
            int count=Orders(),stock=Stock(p);
            foreach(var (code,reason) in new[]{("NOTHERE","không tồn tại"),("FUTURE","chưa đến hạn"),("PAST","hết hạn"),("USEDUP","hết lượt"),("OFF","ngừng áp dụng"),(new string('x',31),"tối đa 30")})
            {
                var q=Quote(p,1,code);True(q.LoiVoucher!.Contains(reason));
                Reject(()=>pos.ThanhToanPOS(nv,q,100,Guid.NewGuid()));Reject(()=>new DonHangOnlineRepository().TaoDon(customer,"Test","COD",q));
            }
            Eq(count,Orders());Eq(stock,Stock(p));
        });
        Test("N02 cash less/exact/excess measured after product + voucher discounts",()=>
        {
            ResetOffers();int p=Product();Offer("SANPHAM",10,p);Offer("VOUCHER",20,code:"CASH72");var q=Quote(p,1,"CASH72");Eq(72m,q.TongThanhToan);
            int count=Orders();Reject(()=>pos.ThanhToanPOS(nv,q,71,Guid.NewGuid()));Eq(count,Orders());Eq(0,Used("CASH72"));
            int exact=pos.ThanhToanPOS(nv,q,72,Guid.NewGuid()),more=pos.ThanhToanPOS(nv,q,100,Guid.NewGuid());
            Eq(0m,pos.LayTheoMa(exact)!.TienKhachDua!.Value-pos.LayTheoMa(exact)!.TongTien);
            Eq(28m,pos.LayTheoMa(more)!.TienKhachDua!.Value-pos.LayTheoMa(more)!.TongTien);Eq(2,Used("CASH72"));
        });
        Test("N03 POS not ready without unique request-token index; migration restores it",()=>
        {
            True(pos.HoTroPOS());DbHelper.ExecuteNonQuery("DROP INDEX UX_HoaDon_MaYeuCauPOS ON HoaDon");
            try { True(!pos.HoTroPOS()); }
            finally { Batches(File.ReadAllText(Path.Combine(root,"database/04_SV4_Migration.sql")).Replace("USE BadmintonShop;","USE ["+new SqlConnectionStringBuilder(DbHelper.ConnectionString).InitialCatalog+"];",StringComparison.Ordinal)); }
            True(pos.HoTroPOS());
        });
        Test("N04 SQL/tables/Excel/PDF match today/month/year/range and sale-type filters",()=>
        {
            var today=DateTime.Today;
            foreach(var (name,from,to) in new[]{("day",today,today),("month",new DateTime(today.Year,today.Month,1),today),("year",new DateTime(today.Year,1,1),today),("range",today.AddMonths(-13),today)})
            foreach(var type in new string?[]{null,"Online","Tại quầy"})
            {
                var report=BaoCaoService.Tao(new BaoCaoRepository().LayDuLieu(from,to,type),from,to,type);
                decimal expected=Convert.ToDecimal(DbHelper.ExecuteScalar(@"SELECT ISNULL(SUM(TongTien),0) FROM HoaDon WHERE TrangThai=N'Hoàn thành'
                    AND NgayLap>=@from AND NgayLap<DATEADD(day,1,@to) AND (@type IS NULL OR LoaiBan=@type)",new SqlParameter("@from",from),new SqlParameter("@to",to),new SqlParameter("@type",DbHelper.GiaTriHoacNull(type))));
                Eq(expected,report.TongDoanhThu);
                foreach(var table in new[]{"Theo ngày","Theo tháng","Theo năm","Theo danh mục"})Eq(expected,report.Bang[table].AsEnumerable().Sum(r=>r.Field<decimal>("Doanh thu")));
                string prefix=$"filtered-{name}-{(type==null?"all":type=="Online"?"online":"pos")}";
                XuatTaiLieu.Excel(Path.Combine(output,prefix+".xlsx"),report);XuatTaiLieu.BaoCaoPdf(Path.Combine(output,prefix+".pdf"),report);
                using var wb=new XLWorkbook(Path.Combine(output,prefix+".xlsx"));Eq(expected,wb.Worksheet("Theo ngày").Cell(3,2).GetValue<decimal>());
                File.AppendAllText(Path.Combine(output,"filtered-expected.tsv"),$"{prefix}\t{expected.ToString(System.Globalization.CultureInfo.InvariantCulture)}\n");
            }
        });
        Test("N05 demo rerun preserves offers, stock and invoices; 12 distinct sale months",()=>
        {
            int invoices=Orders(),campaigns=ScalarInt("SELECT COUNT(*) FROM CTKM WHERE TenCTKM LIKE N'[[]SV4-DEMO]%'"),stock=ScalarInt("SELECT SUM(SoLuongTon) FROM SanPham WHERE TenSP LIKE N'[[]SV4-DEMO]%'");
            Batches(File.ReadAllText(Path.Combine(root,"database/05_SV4_Demo.sql")));
            Eq(invoices,Orders());Eq(campaigns,ScalarInt("SELECT COUNT(*) FROM CTKM WHERE TenCTKM LIKE N'[[]SV4-DEMO]%'"));Eq(stock,ScalarInt("SELECT SUM(SoLuongTon) FROM SanPham WHERE TenSP LIKE N'[[]SV4-DEMO]%'"));
            Eq(12,ScalarInt("SELECT COUNT(DISTINCT CONVERT(char(7),h.NgayLap,120)) FROM HoaDon h JOIN NhanVien n ON n.MaNV=h.MaNV WHERE n.HoTen LIKE N'[[]SV4-DEMO]%' AND h.LoaiBan=N'Tại quầy'"));Eq(3,campaigns);
        });
    }

    private static void FreshInstallChecks(string server)
    {
        Test("J01 aggregate fresh-install SQL includes migrations 04/06 and repeatable demo",()=>
        {
            var name="BadmintonShop_SV4_Test_Install_"+DateTime.Now.ToString("yyyyMMddHHmmss")+"_"+Guid.NewGuid().ToString("N")[..8];
            var b=new SqlConnectionStringBuilder { DataSource=server,InitialCatalog="master",IntegratedSecurity=true,TrustServerCertificate=true };
            using(var c=new SqlConnection(b.ConnectionString)) { c.Open();using var cmd=new SqlCommand("CREATE DATABASE ["+name+"]",c);cmd.ExecuteNonQuery(); }
            b.InitialCatalog=name;DbHelper.ConnectionString=b.ConnectionString;Ghi("Isolated fresh-install database: "+name);
            // Deliberately omit the aggregate's destructive DROP/CREATE preamble. Never execute it on an existing database.
            string sql=File.ReadAllText(Path.Combine(root,"database/BadmintonShop.sql"));
            sql=sql[sql.IndexOf("-- ====== DANH MUC",StringComparison.Ordinal)..].Replace("USE BadmintonShop;","USE ["+name+"];",StringComparison.Ordinal);
            Batches(sql);True(new HoaDonRepository().HoTroPOS());
            foreach(var script in new[]{"04_SV4_Migration.sql","06_SV4_POS_Request.sql","04_SV4_Migration.sql","06_SV4_POS_Request.sql","05_SV4_Demo.sql","05_SV4_Demo.sql"})
                Batches(File.ReadAllText(Path.Combine(root,"database",script)).Replace("USE BadmintonShop;","USE ["+name+"];",StringComparison.Ordinal));
            True(new HoaDonRepository().HoTroPOS());
            Eq(3,ScalarInt("SELECT COUNT(*) FROM CTKM WHERE TenCTKM LIKE N'[[]SV4-DEMO]%'"));
            int p=ScalarInt("SELECT MaSP FROM SanPham WHERE TenSP=N'[SV4-DEMO] Vợt tập luyện'");
            var quote=Quote(p,1,"SV4DEMO15");True(quote.LoiVoucher==null);Eq(382500m,quote.TongThanhToan);
            Eq(12,ScalarInt("SELECT COUNT(DISTINCT CONVERT(char(7),h.NgayLap,120)) FROM HoaDon h JOIN NhanVien n ON n.MaNV=h.MaNV WHERE n.HoTen LIKE N'[[]SV4-DEMO]%' AND h.LoaiBan=N'Tại quầy'"));
        });
        Test("J02 demo safety guard refuses master before any writes",()=>
        {
            var b=new SqlConnectionStringBuilder { DataSource=server,InitialCatalog="master",IntegratedSecurity=true,TrustServerCertificate=true };
            using var c=new SqlConnection(b.ConnectionString);c.Open();
            var script=File.ReadAllText(Path.Combine(root,"database/05_SV4_Demo.sql"));
            using var cmd=new SqlCommand(script[..script.IndexOf("SET XACT_ABORT",StringComparison.Ordinal)],c);
            try { cmd.ExecuteNonQuery();throw new Exception("Demo safety guard did not reject master"); }
            catch(SqlException ex) when(ex.Number==51004) { }
        });
    }
}
