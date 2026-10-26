using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;
using BadmintonShop.Desktop;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using Microsoft.Data.SqlClient;
using PdfSharp.Pdf.IO;

// SV4: bộ kiểm tra chạy thật, không gọi vào CSDL BadmintonShop; integration luôn tạo CSDL tên riêng mới.
internal static partial class Program
{
    private static int passed,failed;private static string root="",output="";private static int nv,ncc,sp1,sp2,dm1,dm2;
    private static readonly List<string> log=[];
    [STAThread]
    private static int Main(string[] args)
    {
        Console.OutputEncoding=Encoding.UTF8;
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture=System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture=System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
        root=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../../"));
        if (!File.Exists(Path.Combine(root,"BadmintonShop.sln")) && File.Exists(Path.Combine(Environment.CurrentDirectory,"BadmintonShop.sln"))) root=Environment.CurrentDirectory;
        output=Path.Combine(Path.GetTempPath(),"BadmintonShop-SV4-Checks",DateTime.Now.ToString("yyyyMMdd-HHmmss"));Directory.CreateDirectory(output);
        if(args.Contains("--schema-readonly"))return SchemaReadOnly();
        if(args.Contains("--lifecycle-debug"))return LifecycleDebug();
        Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
        // Keep one STA UI context while opening/closing many independent test forms, as Application.Run does.
        SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
        Unit();
        if(args.Contains("--install-checks"))
        {
            int serverIndex=Array.IndexOf(args,"--server");
            if(serverIndex<0 || serverIndex+1>=args.Length)throw new ArgumentException("Install checks require --server; creates an isolated test database.");
            FreshInstallChecks(args[serverIndex+1]);
        }
        if(args.Contains("--lifecycle-checks"))LifecycleChecks();
        if(args.Contains("--ui-readonly")) UiReadOnly();
        if(args.Contains("--integration"))
        {
            int index=Array.IndexOf(args,"--server");
            if(index<0 || index+1>=args.Length)throw new ArgumentException("Integration yêu cầu --server <SQL Server>; luôn tạo CSDL test mới.");
            try { TaoCsdlTest(args[index+1]);Integration();PromotionIntegration();WebHttpChecks();Render();UiReadOnly(true);FinalIntegrationChecks();FinalReconciliation(); }
            catch(Exception ex) { failed++;Ghi("FAIL SETUP: "+ex); }
        }
        Ghi($"RESULT: {passed} passed, {failed} failed. Output: {output}");
        File.WriteAllLines(Path.Combine(output,"results.txt"),log);return failed==0?0:1;
    }
    private static void Ghi(string message) { Console.WriteLine(message);log.Add(message); }
    private static void Test(string ten,Action a)
    { try { a();passed++;Ghi("PASS "+ten); }catch(Exception ex) { failed++;Ghi("FAIL "+ten+": "+ex); } }
    private static void Eq<T>(T expected,T actual) { if(!EqualityComparer<T>.Default.Equals(expected,actual))throw new Exception($"Expected {expected}, actual {actual}"); }
    private static void True(bool ok) { if(!ok)throw new Exception("Assertion failed"); }
    private static void Reject(Action a)
    {
        try { a(); }catch(Exception ex) when(ex is ArgumentException or InvalidOperationException or SqlException) { return; }
        throw new Exception("Expected rejection");
    }
    private static DataTable Source()
    {
        var t=new DataTable();foreach(var (n,ty) in new (string,Type)[]{("MaHD",typeof(int)),("NgayLap",typeof(DateTime)),("LoaiBan",typeof(string)),
            ("TrangThai",typeof(string)),("TongTien",typeof(decimal)),("MaSP",typeof(int)),("TenSP",typeof(string)),("MaDM",typeof(int)),
            ("TenDanhMuc",typeof(string)),("SoLuong",typeof(int)),("ThanhTien",typeof(decimal))})t.Columns.Add(n,ty);
        return t;
    }
    private static KetQuaBaoCao Fixture()
    {
        var t=Source();var date=new DateTime(2026,1,2);
        t.Rows.Add(1,date,"Online","Hoàn thành",101m,1,"Vợt kiểm thử",1,"Vợt",1,100m);
        t.Rows.Add(1,date,"Online","Hoàn thành",101m,2,"Cầu kiểm thử",2,"Cầu",2,100m);
        t.Rows.Add(2,date.AddMonths(1),"Tại quầy","Hoàn thành",60m,1,"Vợt kiểm thử",1,"Vợt",3,60m);
        t.Rows.Add(3,date,"Online","Đã hủy",500m,1,"Vợt kiểm thử",1,"Vợt",99,500m);
        return BaoCaoService.Tao(t,date,date.AddMonths(2),null);
    }
    private static void Unit()
    {
        Test("U01 required name / trim / length",()=> { Eq("Vợt",QuyTacBanHang.BatBuoc(" Vợt ",100,"Tên"));Reject(()=>QuyTacBanHang.BatBuoc("  ",100,"Tên"));Reject(()=>QuyTacBanHang.BatBuoc(new string('a',101),100,"Tên")); });
        Test("U02 money range and whole dong",()=> { QuyTacBanHang.KiemTraTien(0);Reject(()=>QuyTacBanHang.KiemTraTien(-1));Reject(()=>QuyTacBanHang.KiemTraTien(0.5m));Reject(()=>QuyTacBanHang.KiemTraTien(1000000000000000000m)); });
        Test("U03 all 25 state pairs",()=>
        { var valid=new HashSet<(string,string)> { ("Chờ xác nhận","Đã xác nhận"),("Đã xác nhận","Đang giao"),("Đang giao","Hoàn thành"),("Chờ xác nhận","Đã hủy"),("Đã xác nhận","Đã hủy") };
          foreach(var a in QuyTacBanHang.TrangThaiDon)foreach(var b in QuyTacBanHang.TrangThaiDon)Eq(valid.Contains((a,b)),QuyTacBanHang.DuocChuyen(a,b)); });
        Test("U04 role matrix and session",()=>
        { PhienDangNhap.DangXuat();True(!PhienDangNhap.CoQuyen(ChucNang.DonHangPOS));
          foreach(var q in Enum.GetValues<ChucNang>()) { True(PhanQuyen.CoQuyen(VaiTro.Admin,q));Eq(q==ChucNang.DonHangPOS,PhanQuyen.CoQuyen(VaiTro.NhanVien,q)); }
          True(!PhanQuyen.CoQuyen("INVALID",ChucNang.DonHangPOS));True(!PhanQuyen.CoQuyen(VaiTro.QuanLy,ChucNang.SaoLuuPhucHoi)); });
        Test("U05 relative image safety",()=>
        { True(KhoAnhSanPham.DuongDanDayDu("products/test.jpg")!.StartsWith(KhoAnhSanPham.ThuMucGoc));
          foreach(var p in new[]{"C:/private/a.jpg","/etc/passwd","products/../a.jpg","products//a.jpg","https://example.com/a.jpg","products/..\\a.jpg"})Reject(()=>KhoAnhSanPham.DuongDanDayDu(p)); });
        Test("U06 allocation rounding tie and zero",()=> { var a=BaoCaoService.PhanBo(101,new Dictionary<int,decimal>{{1,100},{2,100}});Eq(51m,a[1]);Eq(50m,a[2]);Eq(0m,BaoCaoService.PhanBo(0,new Dictionary<int,decimal>{{1,0}})[1]); });
        Test("U07 allocation 1000 random and large money",()=>
        { var r=new Random(4);for(int i=0;i<1000;i++) { var w=Enumerable.Range(1,20).ToDictionary(x=>x,_=>(decimal)r.Next(1,10000));var net=r.Next(100000);var a=BaoCaoService.PhanBo(net,w);Eq((decimal)net,a.Values.Sum());True(a.Values.All(x=>x>=0)); }
          Eq(999999999999999999m,BaoCaoService.PhanBo(999999999999999999m,new Dictionary<int,decimal>{{1,999999999999999999m},{2,1}}).Values.Sum()); });
        Test("U08 report complete only / counts / quantities / category sum",()=>
        { var k=Fixture();Eq(161m,k.TongDoanhThu);foreach(var n in new[]{"Theo ngày","Theo tháng","Theo năm","Theo danh mục"})Eq(k.TongDoanhThu,k.Bang[n].AsEnumerable().Sum(r=>r.Field<decimal>("Doanh thu")));
          Eq(4L,k.Bang["Bán chạy"].Rows[0].Field<long>("Số lượng"));Eq(3,k.Bang["Trạng thái đơn"].AsEnumerable().Sum(r=>r.Field<int>("Số đơn"))); });
        Test("U09 empty report",()=> { var k=BaoCaoService.Tao(Source(),DateTime.Today,DateTime.Today,null);Eq(0m,k.TongDoanhThu);Eq(0,k.Bang["Theo ngày"].Rows.Count);Eq(5,k.Bang["Trạng thái đơn"].Rows.Count); });
        Test("U10 constructors safe without DB or login",()=> { foreach(var f in Forms())using(f)True(!f.IsDisposed); });
        Test("U11 XLSX roundtrip and OpenXML schema",()=>
        { var path=Path.Combine(output,"report.xlsx");XuatTaiLieu.Excel(path,Fixture());using(var wb=new XLWorkbook(path)) { Eq(6,wb.Worksheets.Count);Eq(161m,wb.Worksheet("Theo tháng").Cell(3,2).GetValue<decimal>());Eq(161m,wb.Worksheet("Theo tháng").Range("B7:B8").Cells().Sum(c=>c.GetValue<decimal>())); }
          using var doc=SpreadsheetDocument.Open(path,false);var errors=new OpenXmlValidator().Validate(doc).ToList();True(errors.Count==0); });
        Test("U12 PDF report / invoice / empty / multipage reopen",()=>
        {
            var k=Fixture();var path=Path.Combine(output,"report.pdf");XuatTaiLieu.BaoCaoPdf(path,k);using(var p=PdfReader.Open(path,PdfDocumentOpenMode.Import))Eq(6,p.PageCount);
            var lines=new DataTable();lines.Columns.Add("Sản phẩm");lines.Columns.Add("ThanhTien",typeof(decimal));
            for(int i=0;i<180;i++)lines.Rows.Add("Vợt kiểm thử tiếng Việt "+i,100m);
            path=Path.Combine(output,"invoice.pdf");XuatTaiLieu.HoaDonPdf(path,new HoaDon { MaHD=123,NgayLap=new DateTime(2026,1,1),LoaiBan="Tại quầy",HinhThucTT="Tiền mặt",TrangThai="Hoàn thành",TongTien=18000,TienKhachDua=20000 },lines);
            using(var p=PdfReader.Open(path,PdfDocumentOpenMode.Import))True(p.PageCount>1);
            var empty=BaoCaoService.Tao(Source(),DateTime.Today,DateTime.Today,null);XuatTaiLieu.Excel(Path.Combine(output,"empty.xlsx"),empty);XuatTaiLieu.BaoCaoPdf(Path.Combine(output,"empty.pdf"),empty);
        });
        Test("U13 image decode / reencode / bad / missing / size limit",()=>
        {
            var old=Environment.GetEnvironmentVariable("BadmintonShop__MediaRoot");Environment.SetEnvironmentVariable("BadmintonShop__MediaRoot",Path.Combine(output,"media"));
            try
            {
                var original=Path.Combine(output,"image.png");using(var img=new Bitmap(80,60)) { using var g=Graphics.FromImage(img);g.Clear(Color.SeaGreen);img.Save(original,System.Drawing.Imaging.ImageFormat.Png); }
                using(var img=AnhSanPham.Doc(original)) { var key=AnhSanPham.Luu(img);True(key.StartsWith("products/"));using var reopen=AnhSanPham.Doc(KhoAnhSanPham.DuongDanDayDu(key)!);Eq(80,reopen.Width);Eq(60,reopen.Height); }
                var bad=Path.Combine(output,"not-image.jpg");File.WriteAllText(bad,"not an image");Reject(()=>AnhSanPham.Doc(bad));
                try { AnhSanPham.Doc(Path.Combine(output,"missing.jpg"));throw new Exception("Expected missing file"); }catch(FileNotFoundException) { }
                var large=Path.Combine(output,"large.jpg");using(var file=File.Create(large))file.SetLength(10*1024*1024+1);Reject(()=>AnhSanPham.Doc(large));
            }
            finally { Environment.SetEnvironmentVariable("BadmintonShop__MediaRoot",old); }
        });
    }
    private static IEnumerable<Form> Forms() => [new FrmDanhMuc(),new FrmSanPham(),new FrmSanPhamSua(),new FrmDonHang(),new FrmPOS(),new FrmThongKe()];
    private static void TaoCsdlTest(string server)
    {
        string name="BadmintonShop_SV4_Test_"+DateTime.Now.ToString("yyyyMMddHHmmss")+"_"+Guid.NewGuid().ToString("N")[..8];
        var b=new SqlConnectionStringBuilder { DataSource=server,InitialCatalog="master",IntegratedSecurity=true,TrustServerCertificate=true,ConnectTimeout=30 };
        using(var c=new SqlConnection(b.ConnectionString)) { c.Open();using var cmd=new SqlCommand($"CREATE DATABASE [{name}]",c);cmd.ExecuteNonQuery(); }
        b.InitialCatalog=name;DbHelper.ConnectionString=b.ConnectionString;Ghi("Isolated test database: "+name);
        var schema=File.ReadAllText(Path.Combine(root,"database/01_CreateDatabase.sql"));schema=schema[schema.IndexOf("-- ====== DANH MUC",StringComparison.Ordinal)..];Batches(schema);
        foreach(var file in new[]{"02_Triggers.sql","04_SV4_Migration.sql","04_SV4_Migration.sql","06_SV4_POS_Request.sql","06_SV4_POS_Request.sql"})Batches(File.ReadAllText(Path.Combine(root,"database",file)).Replace("USE BadmintonShop;",$"USE [{name}];",StringComparison.Ordinal));
        nv=ScalarInt("INSERT NhanVien(HoTen,ChucVu) VALUES(N'SV4 TEST',N'Nhân viên bán hàng');SELECT CAST(SCOPE_IDENTITY() AS int)");
        DbHelper.ExecuteNonQuery("INSERT TaiKhoan(TenDangNhap,MatKhau,VaiTro,MaNV) VALUES('sv4test',@hash,'ADMIN',@nv)",new SqlParameter("@hash",MatKhauHelper.BamMatKhau("TestOnly@123")),new SqlParameter("@nv",nv));
        ncc=ScalarInt("INSERT NhaCungCap(TenNCC) VALUES(N'SV4 TEST');SELECT CAST(SCOPE_IDENTITY() AS int)");
        dm1=new DanhMucRepository().Luu(new DanhMuc { TenDanhMuc="SV4 TEST Vợt" });dm2=new DanhMucRepository().Luu(new DanhMuc { TenDanhMuc="SV4 TEST Cầu" });
        sp1=new SanPhamRepository().Luu(new SanPham { TenSP="Vợt thử",MaDM=dm1,DonGiaBan=100,DonViTinh="Cây",TrangThai="Đang bán",SoLuongTon=999 });
        sp2=new SanPhamRepository().Luu(new SanPham { TenSP="Cầu thử",MaDM=dm2,DonGiaBan=50,DonViTinh="Ống",TrangThai="Đang bán" });
    }
    private static void Batches(string sql)
    { foreach(var batch in Regex.Split(sql,@"^\s*GO\s*$",RegexOptions.Multiline|RegexOptions.IgnoreCase))if(!string.IsNullOrWhiteSpace(batch))DbHelper.ExecuteNonQuery(batch); }
    private static int ScalarInt(string sql)=>Convert.ToInt32(DbHelper.ExecuteScalar(sql));
    private static int Stock(int sp)=>new SanPhamRepository().LayTheoMa(sp)!.SoLuongTon;
    private static List<ChiTietHoaDon> Cart(int qty=1)=>[new ChiTietHoaDon { MaSP=sp1,SoLuong=qty,DonGia=100 }];
    private static int Online(string state="Chờ xác nhận",int qty=1)=>DbHelper.ExecuteTransaction((c,t)=>
    {
        using var h=new SqlCommand("INSERT HoaDon(LoaiBan,HinhThucTT,TrangThai) VALUES(N'Online',N'COD',@tt); SELECT CAST(SCOPE_IDENTITY() AS int);",c,t);h.Parameters.AddWithValue("@tt",state);var ma=(int)h.ExecuteScalar()!;
        using var line=new SqlCommand("INSERT ChiTietHoaDon(MaHD,MaSP,SoLuong,DonGia) VALUES(@ma,@sp,@sl,100)",c,t);line.Parameters.AddWithValue("@ma",ma);line.Parameters.AddWithValue("@sp",sp1);line.Parameters.AddWithValue("@sl",qty);line.ExecuteNonQuery();return ma;
    });
    private static void Integration()
    {
        var dm=new DanhMucRepository();var sp=new SanPhamRepository();var hd=new HoaDonRepository();
        Test("I01 migration repeat / new stock always zero",()=> { Eq(0,Stock(sp1));Eq(0,Stock(sp2)); });
        Test("I02 category CRUD / duplicate / used delete",()=>
        { var id=dm.Luu(new DanhMuc { TenDanhMuc="  Temporary  " });Eq("Temporary",dm.LayTheoMa(id)!.TenDanhMuc);
          Reject(()=>dm.Luu(new DanhMuc { TenDanhMuc="Temporary" }));dm.Luu(new DanhMuc { MaDM=id,TenDanhMuc="Changed" });dm.Xoa(id);True(dm.LayTheoMa(id)==null);Reject(()=>dm.Xoa(dm1)); });
        Test("I03 invalid products / imports / edits preserve stock",()=>
        { Reject(()=>sp.Luu(new SanPham { TenSP="",MaDM=dm1,DonViTinh="Cây",TrangThai="Đang bán" }));Reject(()=>sp.Luu(new SanPham { TenSP="Bad",MaDM=dm1,DonViTinh="Cây",TrangThai="Đang bán",DonGiaBan=-1 }));
          new PhieuNhapRepository().Them(new PhieuNhap { MaNV=nv,MaNCC=ncc },[new ChiTietPhieuNhap { MaSP=sp1,SoLuong=100,DonGia=70 },new ChiTietPhieuNhap { MaSP=sp2,SoLuong=100,DonGia=30 }]);
          var p=sp.LayTheoMa(sp1)!;p.SoLuongTon=100000;p.ThuongHieu="SV4";sp.Luu(p);Eq(100,Stock(sp1)); });
        Test("I04 POS success / trigger / cash / walk-in / retry",()=>
        { int before=Stock(sp1);var token=Guid.NewGuid();var id=hd.ThanhToanPOS(nv,Cart(2),250,token);var h=hd.LayTheoMa(id)!;
          Eq("Tại quầy",h.LoaiBan);Eq("Hoàn thành",h.TrangThai);True(h.MaKH==null);Eq(nv,h.MaNV!.Value);Eq(200m,h.TongTien);Eq(250m,h.TienKhachDua!.Value);Eq(before-2,Stock(sp1));
          Eq(id,hd.ThanhToanPOS(nv,Cart(2),250,token));Eq(before-2,Stock(sp1));Reject(()=>hd.ThanhToanPOS(nv,Cart(3),250,token)); });
        Test("I05 POS invalid/price/inactive/insufficient leaves no partial rows",()=>
        { int count=ScalarInt("SELECT COUNT(*) FROM HoaDon"),stock=Stock(sp1);
          Reject(()=>hd.ThanhToanPOS(nv,Cart(100000),100000000,Guid.NewGuid()));Reject(()=>hd.ThanhToanPOS(nv,Cart(),99,Guid.NewGuid()));
          Reject(()=>hd.ThanhToanPOS(nv,[new ChiTietHoaDon { MaSP=sp1,SoLuong=1,DonGia=99 }],100,Guid.NewGuid()));
          Reject(()=>hd.ThanhToanPOS(nv,[..Cart(),..Cart()],200,Guid.NewGuid()));
          sp.NgungBan(sp1);Reject(()=>hd.ThanhToanPOS(nv,Cart(),100,Guid.NewGuid()));var p=sp.LayTheoMa(sp1)!;p.TrangThai="Đang bán";sp.Luu(p);
          Eq(count,ScalarInt("SELECT COUNT(*) FROM HoaDon"));Eq(stock,Stock(sp1)); });
        Test("I06 trigger rollback after header and first valid line",()=>
        { int count=ScalarInt("SELECT COUNT(*) FROM HoaDon"),s1=Stock(sp1),s2=Stock(sp2);
          Reject(()=>DbHelper.ExecuteTransaction((c,t)=>
          { using var cmd=new SqlCommand("INSERT HoaDon DEFAULT VALUES;DECLARE @id int=SCOPE_IDENTITY();INSERT ChiTietHoaDon(MaHD,MaSP,SoLuong,DonGia) VALUES(@id,@sp1,1,100);INSERT ChiTietHoaDon(MaHD,MaSP,SoLuong,DonGia) VALUES(@id,@sp2,1000000,50);",c,t);
            cmd.Parameters.AddWithValue("@sp1",sp1);cmd.Parameters.AddWithValue("@sp2",sp2);cmd.ExecuteNonQuery();return 0; }));
          Eq(count,ScalarInt("SELECT COUNT(*) FROM HoaDon"));Eq(s1,Stock(sp1));Eq(s2,Stock(sp2)); });
        Test("I07 cancel reason / once / stale / no reopen",()=>
        { int id=Online(),before=Stock(sp1);Reject(()=>hd.ChuyenTrangThai(id,"Chờ xác nhận","Đã hủy","  "));
          hd.ChuyenTrangThai(id,"Chờ xác nhận","Đã hủy","Khách yêu cầu");Eq(before+1,Stock(sp1));Eq("Khách yêu cầu",hd.LayTheoMa(id)!.LyDoHuy);
          Reject(()=>hd.ChuyenTrangThai(id,"Chờ xác nhận","Đã hủy","Lặp"));Eq(before+1,Stock(sp1));Reject(()=>hd.ChuyenTrangThai(id,"Đã hủy","Đã xác nhận"));
          Reject(()=>DbHelper.ExecuteNonQuery("UPDATE HoaDon SET TrangThai=N'Chờ xác nhận' WHERE MaHD=@ma",new SqlParameter("@ma",id)));Eq(before+1,Stock(sp1)); });
        Test("I08 online sequence / stale update / no cancel after shipping",()=>
        { int id=Online();hd.ChuyenTrangThai(id,"Chờ xác nhận","Đã xác nhận");Reject(()=>hd.ChuyenTrangThai(id,"Chờ xác nhận","Đã hủy","Stale"));
          hd.ChuyenTrangThai(id,"Đã xác nhận","Đang giao");Reject(()=>hd.ChuyenTrangThai(id,"Đang giao","Đã hủy","Too late"));hd.ChuyenTrangThai(id,"Đang giao","Hoàn thành");Eq("Hoàn thành",hd.LayTheoMa(id)!.TrangThai); });
        Test("I09 concurrent identical POS only once",()=>
        { int stock=Stock(sp1);var token=Guid.NewGuid();var tasks=Enumerable.Range(0,2).Select(_=>Task.Run(()=>hd.ThanhToanPOS(nv,Cart(),100,token))).ToArray();Task.WaitAll(tasks);Eq(tasks[0].Result,tasks[1].Result);Eq(stock-1,Stock(sp1)); });
        Test("I10 competing orders for final stock",()=>
        { var product=sp.Luu(new SanPham { TenSP="Last stock",MaDM=dm1,DonGiaBan=10,DonViTinh="Cái",TrangThai="Đang bán" });
          new PhieuNhapRepository().Them(new PhieuNhap { MaNV=nv,MaNCC=ncc },[new ChiTietPhieuNhap { MaSP=product,SoLuong=1,DonGia=1 }]);
          var tasks=Enumerable.Range(0,2).Select(_=>Task.Run(()=> { try { hd.ThanhToanPOS(nv,[new ChiTietHoaDon { MaSP=product,SoLuong=1,DonGia=10 }],10,Guid.NewGuid());return true; }catch(InvalidOperationException) { return false; } })).ToArray();
          Task.WaitAll(tasks);Eq(1,tasks.Count(x=>x.Result));Eq(0,Stock(product)); });
        Test("I11 order date/state/type/search filters",()=>
        { var all=hd.LayDanhSachHienThi(DateTime.Today,DateTime.Today);True(all.Rows.Count>0);
          var id=(int)all.Rows[0]["MaHD"];Eq(1,hd.LayDanhSachHienThi(DateTime.Today,DateTime.Today,tuKhoa:id.ToString()).Rows.Count);
          True(hd.LayDanhSachHienThi(DateTime.Today,DateTime.Today,"Đã hủy","Online").AsEnumerable().All(r=>r.Field<string>("TrangThai")=="Đã hủy"));
          Reject(()=>hd.LayDanhSachHienThi(DateTime.Today,DateTime.Today.AddDays(-1))); });
        Test("I12 independent SV4 demo / rerun / monthly data / filters",()=>
        { var cs=new SqlConnectionStringBuilder(DbHelper.ConnectionString);var sql=File.ReadAllText(Path.Combine(root,"database/05_SV4_Demo.sql")).Replace("USE BadmintonShop;",$"USE [{cs.InitialCatalog}];");
          Batches(sql);int count=ScalarInt("SELECT COUNT(*) FROM HoaDon");Batches(sql);Eq(count,ScalarInt("SELECT COUNT(*) FROM HoaDon"));
          var tu=DateTime.Today.AddMonths(-13);var den=DateTime.Today;var src=new BaoCaoRepository().LayDuLieu(tu,den,null);var k=BaoCaoService.Tao(src,tu,den,null);
          var actual=Convert.ToDecimal(DbHelper.ExecuteScalar("SELECT SUM(TongTien) FROM HoaDon WHERE TrangThai=N'Hoàn thành'"));Eq(actual,k.TongDoanhThu);True(k.Bang["Theo tháng"].Rows.Count>=12);
          foreach(var ten in new[]{"Theo ngày","Theo tháng","Theo năm","Theo danh mục"})Eq(actual,k.Bang[ten].AsEnumerable().Sum(r=>r.Field<decimal>("Doanh thu")));
          var online=BaoCaoService.Tao(new BaoCaoRepository().LayDuLieu(tu,den,"Online"),tu,den,"Online");var pos=BaoCaoService.Tao(new BaoCaoRepository().LayDuLieu(tu,den,"Tại quầy"),tu,den,"Tại quầy");Eq(actual,online.TongDoanhThu+pos.TongDoanhThu);
          XuatTaiLieu.Excel(Path.Combine(output,"database-report.xlsx"),k);XuatTaiLieu.BaoCaoPdf(Path.Combine(output,"database-report.pdf"),k);
          using var wb=new XLWorkbook(Path.Combine(output,"database-report.xlsx"));Eq(actual,wb.Worksheet("Theo danh mục").Cell(3,2).GetValue<decimal>());
          Eq(actual,wb.Worksheet("Theo danh mục").Range(7,3,6+k.Bang["Theo danh mục"].Rows.Count,3).Cells().Sum(x=>x.GetValue<decimal>())); });
        Test("I13 simultaneous cancel returns stock once",()=>
        {
            int id=Online(),before=Stock(sp1);
            var tasks=Enumerable.Range(0,2).Select(_=>Task.Run(()=> { try { hd.ChuyenTrangThai(id,"Chờ xác nhận","Đã hủy","Concurrent test");return true; }catch(SqlException) { return false; } })).ToArray();
            Task.WaitAll(tasks);Eq(1,tasks.Count(x=>x.Result));Eq(before+1,Stock(sp1));
        });
        Test("I14 date inclusive end / empty date / product read compatibility",()=>
        {
            int id=Online();DbHelper.ExecuteNonQuery("UPDATE HoaDon SET NgayLap='20260131 23:59:59.997' WHERE MaHD=@ma",new SqlParameter("@ma",id));
            Eq(1,hd.LayDanhSachHienThi(new DateTime(2026,1,31),new DateTime(2026,1,31),tuKhoa:id.ToString()).Rows.Count);
            Eq(0,hd.LayDanhSachHienThi(new DateTime(2026,2,1),new DateTime(2026,2,1),tuKhoa:id.ToString()).Rows.Count);
            Eq(1,sp.LayTatCa("Vợt thử",dm1).Count);True(sp.LayDanhSachHienThi().Columns.Contains("TenDanhMuc"));
        });
        Test("I15 customer name/phone/email filters and saved receipt",()=>
        {
            int kh=Convert.ToInt32(DbHelper.ExecuteScalar("INSERT KhachHang(HoTen,SDT,Email,MatKhau) VALUES(N'Khách kiểm thử SV4','0900000000','sv4-test@example.invalid',@hash);SELECT CAST(SCOPE_IDENTITY() AS int)",new SqlParameter("@hash",MatKhauHelper.BamMatKhau("TestOnly@123"))));
            int id=Online();DbHelper.ExecuteNonQuery("UPDATE HoaDon SET MaKH=@kh WHERE MaHD=@hd",new SqlParameter("@kh",kh),new SqlParameter("@hd",id));
            foreach(var keyword in new[]{"Khách kiểm thử","0900000000","sv4-test@example.invalid"})Eq(id,hd.LayDanhSachHienThi(DateTime.Today,DateTime.Today,tuKhoa:keyword).Rows[0].Field<int>("MaHD"));
            var ma=hd.ThanhToanPOS(nv,Cart(),120,Guid.NewGuid());var header=hd.LayTheoMa(ma)!;var details=hd.LayChiTietHienThi(ma);
            XuatTaiLieu.HoaDonPdf(Path.Combine(output,"database-invoice.pdf"),header,details);
            Eq(header.TongTien,details.AsEnumerable().Sum(r=>r.Field<decimal>("ThanhTien"))-header.TienGiamGia);
        });
        Test("I16 product and invoice discounts applied once in SQL/report",()=>
        {
            int id=DbHelper.ExecuteTransaction((c,t)=>
            {
                using var cmd=new SqlCommand(@"INSERT HoaDon(LoaiBan,TrangThai,TienGiamGia) VALUES(N'Online',N'Hoàn thành',30);
                    DECLARE @hd int=SCOPE_IDENTITY();
                    INSERT ChiTietHoaDon(MaHD,MaSP,SoLuong,DonGia,GiamGia) VALUES(@hd,@sp1,2,100,7.5),(@hd,@sp2,1,50,0);
                    SELECT @hd;",c,t);cmd.Parameters.AddWithValue("@sp1",sp1);cmd.Parameters.AddWithValue("@sp2",sp2);return (int)cmd.ExecuteScalar()!;
            });
            Eq(205m,hd.LayTheoMa(id)!.TongTien);Eq(235m,hd.LayChiTietHienThi(id).AsEnumerable().Sum(r=>r.Field<decimal>("ThanhTien")));
            XuatTaiLieu.HoaDonPdf(Path.Combine(output,"discount-invoice.pdf"),hd.LayTheoMa(id)!,hd.LayChiTietHienThi(id));
            var raw=new BaoCaoRepository().LayDuLieu(DateTime.Today,DateTime.Today,null);var filtered=raw.Clone();
            foreach(var r in raw.AsEnumerable().Where(x=>x.Field<int>("MaHD")==id))filtered.ImportRow(r);
            var k=BaoCaoService.Tao(filtered,DateTime.Today,DateTime.Today,null);Eq(205m,k.TongDoanhThu);Eq(205m,k.Bang["Theo danh mục"].AsEnumerable().Sum(r=>r.Field<decimal>("Doanh thu")));
            var start=DateTime.Today.AddMonths(-13);var final=BaoCaoService.Tao(new BaoCaoRepository().LayDuLieu(start,DateTime.Today,null),start,DateTime.Today,null);
            Eq(Convert.ToDecimal(DbHelper.ExecuteScalar("SELECT SUM(TongTien) FROM HoaDon WHERE TrangThai=N'Hoàn thành'")),final.TongDoanhThu);
            XuatTaiLieu.Excel(Path.Combine(output,"database-report.xlsx"),final);XuatTaiLieu.BaoCaoPdf(Path.Combine(output,"database-report.pdf"),final);
        });
    }
    private static void Render()
    {
        PhienDangNhap.BatDau(1,nv,"sv4-test","SV4 TEST",VaiTro.Admin);
        foreach(var form in Forms())Test("GUI render "+form.GetType().Name,()=>
        {
            using(form)
            {
                form.Show();Pump(form);
                if(form is FrmDanhMuc)
                {
                    var controls=Controls(form).ToList();var grid=controls.OfType<DataGridView>().Single();grid.Rows[0].Selected=true;Application.DoEvents();
                    controls.OfType<Button>().Single(x=>x.Text=="Mới").PerformClick();Pump(form);
                    True(controls.OfType<TextBox>().All(x=>x.Text.Length==0));True(controls.OfType<Label>().Any(x=>x.Text=="Danh mục mới"));
                }
                if(form is FrmPOS)
                {
                    var products=Controls(form).OfType<DataGridView>().Single(g=>g.DataSource is not System.ComponentModel.BindingList<DongPOS>);
                    var available=products.Rows.Cast<DataGridViewRow>().First(r=>Convert.ToInt32(r.Cells["SoLuongTon"].Value)>0);
                    products.CurrentCell=available.Cells["TenSP"];
                    var controls=Controls(form).ToList();controls.OfType<Button>().Single(x=>x.Text=="Thêm vào giỏ").PerformClick();Pump(form);
                    var cart=controls.OfType<DataGridView>().Single(x=>x.DataSource is System.ComponentModel.BindingList<DongPOS>);
                    var list=(System.ComponentModel.BindingList<DongPOS>)cart.DataSource!;Eq(1,list.Count);
                    controls.OfType<Button>().Single(x=>x.Text=="Bỏ dòng giỏ").PerformClick();Pump(form);Eq(0,list.Count);
                }
                using var bmp=new Bitmap(form.Width,form.Height);form.DrawToBitmap(bmp,new Rectangle(Point.Empty,bmp.Size));bmp.Save(Path.Combine(output,form.GetType().Name+".png"));form.Close();
            }
        });
        PhienDangNhap.DangXuat();
    }
    private static IEnumerable<Control> Controls(Control parent)
    { foreach(Control child in parent.Controls) { yield return child;foreach(var descendant in Controls(child))yield return descendant; } }
}
