using System.Data;
using System.ComponentModel;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;
using BadmintonShop.Desktop;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;

internal static partial class Program
{
    private static int customer;
    private static int Product(decimal price=100,int stock=20)
    {
        var id=new SanPhamRepository().Luu(new SanPham { TenSP="Integration "+Guid.NewGuid().ToString("N")[..8],MaDM=dm1,DonGiaBan=price,DonViTinh="Cái",TrangThai="Đang bán" });
        if(stock>0)new PhieuNhapRepository().Them(new PhieuNhap { MaNV=nv,MaNCC=ncc },[new ChiTietPhieuNhap { MaSP=id,SoLuong=stock,DonGia=1 }]);
        return id;
    }
    private static void ResetOffers()=>DbHelper.ExecuteNonQuery("UPDATE CTKM SET TrangThai=0");
    private static int Offer(string type,decimal percent,int product=0,string code="",int uses=20,decimal threshold=0,int endOffset=1)
    {
        int id=Convert.ToInt32(DbHelper.ExecuteScalar(@"INSERT CTKM(TenCTKM,NgayBatDau,NgayKetThuc,LoaiKM) VALUES(N'Integration test',@from,@to,@type);SELECT CAST(SCOPE_IDENTITY() AS int)",
            new SqlParameter("@from",DateTime.Today.AddDays(-2)),new SqlParameter("@to",DateTime.Today.AddDays(endOffset)),new SqlParameter("@type",type)));
        if(type=="SANPHAM")DbHelper.ExecuteNonQuery("INSERT KM_TheoSP VALUES(@id,@sp,@pt)",new SqlParameter("@id",id),new SqlParameter("@sp",product),new SqlParameter("@pt",percent));
        if(type=="HOADON")DbHelper.ExecuteNonQuery("INSERT KM_TheoHD VALUES(@id,@min,@pt)",new SqlParameter("@id",id),new SqlParameter("@min",threshold),new SqlParameter("@pt",percent));
        if(type=="VOUCHER")DbHelper.ExecuteNonQuery("INSERT KM_TheoVoucher(MaCTKM,MaVoucher,PhanTramGG,SoLuong) VALUES(@id,@code,@pt,@n)",new SqlParameter("@id",id),new SqlParameter("@code",code),new SqlParameter("@pt",percent),new SqlParameter("@n",uses));
        return id;
    }
    private static KetQuaGioHang Quote(int product,int quantity=1,string? code=null)=>new GioHangService().Tinh([(product,quantity)],code);
    private static int Used(string code)=>Convert.ToInt32(DbHelper.ExecuteScalar("SELECT DaDung FROM KM_TheoVoucher WHERE MaVoucher=@code",new SqlParameter("@code",code)));
    private static int Orders()=>ScalarInt("SELECT COUNT(*) FROM HoaDon");
    private static void Price(int product,decimal price)=>DbHelper.ExecuteNonQuery("UPDATE SanPham SET DonGiaBan=@price WHERE MaSP=@id",new SqlParameter("@id",product),new SqlParameter("@price",price));
    private static void SameSaved(int id,KetQuaGioHang quote)
    {
        var repo=new HoaDonRepository();var hd=repo.LayTheoMa(id)!;var details=repo.LayChiTietHienThi(id);
        Eq(quote.TongThanhToan,hd.TongTien);Eq(quote.TienGiamHoaDon,hd.TienGiamGia);Eq(quote.MaCTKM,hd.MaCTKM);
        Eq(quote.TongTienHang,details.AsEnumerable().Sum(r=>r.Field<decimal>("ThanhTien")));
        foreach(var d in quote.Dong) { var row=details.AsEnumerable().Single(r=>r.Field<int>("MaSP")==d.MaSP);Eq(d.DonGia,row.Field<decimal>("DonGia"));Eq(d.PhanTramGiam,row.Field<decimal>("GiamGia"));Eq(d.ThanhTien,row.Field<decimal>("ThanhTien")); }
    }
    private static void PromotionIntegration()
    {
        if(!new SqlConnectionStringBuilder(DbHelper.ConnectionString).InitialCatalog.StartsWith("BadmintonShop_SV4_Test_",StringComparison.Ordinal))throw new InvalidOperationException("Test database required.");
        customer=Convert.ToInt32(DbHelper.ExecuteScalar("INSERT KhachHang(HoTen,Email,MatKhau) VALUES(N'Integration customer','integration@example.invalid',@pw);SELECT CAST(SCOPE_IDENTITY() AS int)",new SqlParameter("@pw",MatKhauHelper.BamMatKhau("Integration@123"))));
        var pos=new HoaDonRepository();var web=new DonHangOnlineRepository();
        Test("P01 no promotion / shared Web-POS calculator",()=>
        {
            ResetOffers();int p=Product();var q=Quote(p,2);Eq(200m,q.TongThanhToan);SameSaved(pos.ThanhToanPOS(nv,q,200,Guid.NewGuid()),q);
        });
        Test("P02 product percent / round line once like computed SQL",()=>
        {
            ResetOffers();int p=Product(5);Offer("SANPHAM",50,p);var q=Quote(p,3);Eq(15m,q.TienHangTruocGiam);Eq(8m,q.TongTienHang);Eq(7m,q.TienGiamSanPham);
            SameSaved(pos.ThanhToanPOS(nv,q,20,Guid.NewGuid()),q);
        });
        Test("P03 invoice tiers / highest qualified threshold",()=>
        {
            ResetOffers();int p=Product();Offer("HOADON",10,threshold:0);int tier=Offer("HOADON",20,threshold:150);var q=Quote(p,2);Eq(tier,q.MaCTKM!.Value);Eq(160m,q.TongThanhToan);SameSaved(pos.ThanhToanPOS(nv,q,200,Guid.NewGuid()),q);
        });
        Test("P04 voucher beats invoice / equal tie selects voucher / normalized code",()=>
        {
            ResetOffers();int p=Product();Offer("HOADON",20);int vc=Offer("VOUCHER",20,code:"P04");var q=Quote(p,2," p04 ");Eq(vc,q.MaCTKM!.Value);Eq("P04",q.MaVoucher);Eq(160m,q.TongThanhToan);
            SameSaved(pos.ThanhToanPOS(nv,q,200,Guid.NewGuid()),q);Eq(1,Used("P04"));
        });
        Test("P05 invoice beats voucher / no stacking / changed losing voucher rejected on retry",()=>
        {
            ResetOffers();int p=Product();int hd=Offer("HOADON",30);Offer("VOUCHER",10,code:"P05A");Offer("VOUCHER",10,code:"P05B");
            var q=Quote(p,1,"P05A");Eq(hd,q.MaCTKM!.Value);True(q.MaVoucher==null);Eq(70m,q.TongThanhToan);
            var token=Guid.NewGuid();int id=pos.ThanhToanPOS(nv,q,100,token);Eq(0,Used("P05A"));
            Reject(()=>pos.ThanhToanPOS(nv,Quote(p,1,"P05B"),100,token));Eq(id,pos.ThanhToanPOS(nv,q,100,token));
        });
        Test("P06 product then invoice OR voucher / discount applied once",()=>
        {
            ResetOffers();int p=Product();Offer("SANPHAM",10,p);Offer("HOADON",20);Offer("VOUCHER",30,code:"P06");
            var hd=Quote(p,3);Eq(270m,hd.TongTienHang);Eq(216m,hd.TongThanhToan);SameSaved(pos.ThanhToanPOS(nv,hd,300,Guid.NewGuid()),hd);
            var vc=Quote(p,3,"P06");Eq(189m,vc.TongThanhToan);SameSaved(web.TaoDon(customer,"Test address","COD",vc),vc);Eq(1,Used("P06"));
        });
        Test("P07 invalid / expired / exhausted / disabled voucher cannot silently checkout",()=>
        {
            ResetOffers();int p=Product();Offer("VOUCHER",10,code:"EXPIRED",endOffset:-1);Offer("VOUCHER",10,code:"EMPTY",uses:0);int disabled=Offer("VOUCHER",10,code:"DISABLED");DbHelper.ExecuteNonQuery("UPDATE CTKM SET TrangThai=0 WHERE MaCTKM=@id",new SqlParameter("@id",disabled));
            int before=Orders(),stock=Stock(p);
            foreach(var code in new[]{"BAD","EXPIRED","EMPTY","DISABLED"}) { var q=Quote(p,1,code);True(q.LoiVoucher!=null);Reject(()=>pos.ThanhToanPOS(nv,q,100,Guid.NewGuid()));Reject(()=>web.TaoDon(customer,"Test","COD",q)); }
            Eq(before,Orders());Eq(stock,Stock(p));
        });
        Test("P08 concurrent last voucher across different products",()=>
        {
            ResetOffers();int a=Product(),b=Product();Offer("VOUCHER",10,code:"LASTVC",uses:1);var qa=Quote(a,1,"LASTVC");var qb=Quote(b,1,"LASTVC");
            using var start=new Barrier(2);
            var tasks=new[]{qa,qb}.Select(q=>Task.Run(()=> { start.SignalAndWait();try { pos.ThanhToanPOS(nv,q,100,Guid.NewGuid());return true; }catch(InvalidOperationException) { return false; } })).ToArray();
            Task.WaitAll(tasks);Eq(1,tasks.Count(t=>t.Result));Eq(1,Used("LASTVC"));Eq(39,Stock(a)+Stock(b));
        });
        Test("P09 concurrent identical token / retry after price-stock-expiry changes",()=>
        {
            ResetOffers();int p=Product(stock:1);int campaign=Offer("VOUCHER",10,code:"RETRY",uses:1);var q=Quote(p,1,"RETRY");var token=Guid.NewGuid();int count=Orders();
            var tasks=Enumerable.Range(0,2).Select(_=>Task.Run(()=>pos.ThanhToanPOS(nv,q,100,token))).ToArray();Task.WaitAll(tasks);Eq(tasks[0].Result,tasks[1].Result);Eq(count+1,Orders());Eq(1,Used("RETRY"));Eq(0,Stock(p));
            Price(p,999);DbHelper.ExecuteNonQuery("UPDATE CTKM SET TrangThai=0 WHERE MaCTKM=@id",new SqlParameter("@id",campaign));
            Eq(tasks[0].Result,pos.ThanhToanPOS(nv,q,100,token));SameSaved(tasks[0].Result,q);Eq(1,Used("RETRY"));
            var changed=Quote(p,1,"OTHER");Reject(()=>pos.ThanhToanPOS(nv,changed,100,token));Reject(()=>pos.ThanhToanPOS(nv,q,101,token));
        });
        Test("P10 changed price requires a new confirmation before collection",()=>
        {
            ResetOffers();int p=Product();var q=Quote(p);int count=Orders();Price(p,110);
            try { pos.ThanhToanPOS(nv,q,200,Guid.NewGuid());throw new Exception("Missing stale quote rejection"); }catch(GiaThanhToanThayDoiException ex) { Eq(110m,ex.GioMoi.TongThanhToan); }
            Eq(count,Orders());Eq(20,Stock(p));var fresh=Quote(p);SameSaved(pos.ThanhToanPOS(nv,fresh,200,Guid.NewGuid()),fresh);
        });
        Test("P11 changed promotion / effectiveness / inactive product / stock / insufficient cash",()=>
        {
            ResetOffers();int p=Product();int offer=Offer("SANPHAM",10,p);var q=Quote(p);int count=Orders();
            DbHelper.ExecuteNonQuery("UPDATE CTKM SET NgayKetThuc=@date WHERE MaCTKM=@id",new SqlParameter("@date",DateTime.Today.AddDays(-1)),new SqlParameter("@id",offer));Reject(()=>pos.ThanhToanPOS(nv,q,100,Guid.NewGuid()));Reject(()=>web.TaoDon(customer,"Test","COD",q));
            var fresh=Quote(p);Reject(()=>pos.ThanhToanPOS(nv,fresh,99,Guid.NewGuid()));new SanPhamRepository().NgungBan(p);Reject(()=>pos.ThanhToanPOS(nv,fresh,100,Guid.NewGuid()));
            int last=Product(stock:1);var one=Quote(last);pos.ThanhToanPOS(nv,one,100,Guid.NewGuid());Reject(()=>pos.ThanhToanPOS(nv,one,100,Guid.NewGuid()));Eq(count+1,Orders());
        });
        Test("P12 rollback after voucher increment and header failure",()=>
        {
            ResetOffers();int p=Product();Offer("VOUCHER",10,code:"FAILHEADER");var q=Quote(p,1,"FAILHEADER");int count=Orders();Reject(()=>pos.ThanhToanPOS(int.MaxValue,q,100,Guid.NewGuid()));Eq(0,Used("FAILHEADER"));Eq(count,Orders());Eq(20,Stock(p));
        });
        Test("P13 injected second-detail failure rolls back voucher/header/first-line/stock",()=>
        {
            ResetOffers();int a=Product(),b=Product();Offer("VOUCHER",10,code:"FAILDETAIL");var q=new GioHangService().Tinh([(a,1),(b,1)],"FAILDETAIL");int count=Orders();
            DbHelper.ExecuteNonQuery($"CREATE OR ALTER TRIGGER trg_SV4_TestFail ON ChiTietHoaDon AFTER INSERT AS BEGIN IF EXISTS(SELECT 1 FROM inserted WHERE MaSP={b}) THROW 51099,'Injected detail failure',1; END");
            try { Reject(()=>pos.ThanhToanPOS(nv,q,200,Guid.NewGuid()));Eq(0,Used("FAILDETAIL"));Eq(count,Orders());Eq(20,Stock(a));Eq(20,Stock(b)); }
            finally { DbHelper.ExecuteNonQuery("DROP TRIGGER trg_SV4_TestFail"); }
        });
        Test("P14 Web create / Desktop progress / Web ownership and historical prices",()=>
        {
            ResetOffers();int p=Product();Offer("SANPHAM",10,p);Offer("VOUCHER",20,code:"WEBFLOW");var q=Quote(p,2,"WEBFLOW");int id=web.TaoDon(customer,"Test delivery","COD",q);Eq(18,Stock(p));
            Eq(id,pos.LayDanhSachHienThi(DateTime.Today,DateTime.Today,tuKhoa:id.ToString()).Rows[0].Field<int>("MaHD"));
            True(web.LayDonCuaKhach(id,customer+9999)==null);Eq(0,web.LayChiTietHienThi(id,customer+9999).Rows.Count);True(!web.HuyDon(id,customer+9999));
            foreach(var (before,after) in new[]{("Chờ xác nhận","Đã xác nhận"),("Đã xác nhận","Đang giao"),("Đang giao","Hoàn thành")})
            { pos.ChuyenTrangThai(id,before,after);Eq(after,web.LayDonCuaKhach(id,customer)!.TrangThai);Eq(18,Stock(p)); }
            True(!web.HuyDon(id,customer));Reject(()=>pos.ChuyenTrangThai(id,"Chờ xác nhận","Đã hủy","Stale"));Price(p,500);ResetOffers();SameSaved(id,q);
            XuatTaiLieu.HoaDonPdf(Path.Combine(output,"promotion-web-invoice.pdf"),pos.LayTheoMa(id)!,pos.LayChiTietHienThi(id));
            File.WriteAllText(Path.Combine(output,"historical-invoice-expected.json"),System.Text.Json.JsonSerializer.Serialize(new { Gross=q.TienHangTruocGiam,ProductDiscount=q.TienGiamSanPham,InvoiceDiscount=q.TienGiamHoaDon,Net=q.TongThanhToan }));
        });
        Test("P15 customer cancel vs staff confirm / mandatory reason / stock restored once / voucher policy unchanged",()=>
        {
            ResetOffers();int p=Product();Offer("VOUCHER",10,code:"CANCELVC");int id=web.TaoDon(customer,"Test","COD",Quote(p,1,"CANCELVC"));Eq(19,Stock(p));Reject(()=>pos.ChuyenTrangThai(id,"Chờ xác nhận","Đã hủy"," "));
            using var start=new Barrier(2);
            var client=Task.Run(()=> { start.SignalAndWait();return web.HuyDon(id,customer); });
            var staff=Task.Run(()=> { start.SignalAndWait();try { pos.ChuyenTrangThai(id,"Chờ xác nhận","Đã xác nhận");return true; }catch(SqlException) { return false; } });
            Task.WaitAll(client,staff);Eq(1,(client.Result?1:0)+(staff.Result?1:0));var state=pos.LayTheoMa(id)!.TrangThai;
            if(state=="Đã xác nhận") { Eq(19,Stock(p));True(!web.HuyDon(id,customer));pos.ChuyenTrangThai(id,state,"Đã hủy","Staff cancel");Eq("Staff cancel",web.LayDonCuaKhach(id,customer)!.LyDoHuy); }
            Eq(20,Stock(p));True(!web.HuyDon(id,customer));Reject(()=>pos.ChuyenTrangThai(id,"Chờ xác nhận","Đã hủy","Retry"));Eq(20,Stock(p));Eq(1,Used("CANCELVC"));
        });
        Test("P16 Web and POS race for final product",()=>
        {
            ResetOffers();int p=Product(stock:1);var q=Quote(p);using var start=new Barrier(2);
            var tasks=new[]{Task.Run(()=> { start.SignalAndWait();try { pos.ThanhToanPOS(nv,q,100,Guid.NewGuid());return true; }catch(InvalidOperationException) { return false; } }),
                Task.Run(()=> { start.SignalAndWait();try { web.TaoDon(customer,"Test","COD",q);return true; }catch(InvalidOperationException) { return false; } })};
            Task.WaitAll(tasks);Eq(1,tasks.Count(t=>t.Result));Eq(0,Stock(p));
        });
        Test("P17 SQL/details/report/category-allocation/Excel/PDF reconciliation",()=>
        {
            var from=DateTime.Today.AddYears(-2);var to=DateTime.Today;var raw=new BaoCaoRepository().LayDuLieu(from,to,null);var report=BaoCaoService.Tao(raw,from,to,null);
            decimal sql=Convert.ToDecimal(DbHelper.ExecuteScalar("SELECT SUM(TongTien) FROM HoaDon WHERE TrangThai=N'Hoàn thành'"));Eq(sql,report.TongDoanhThu);
            foreach(var table in new[]{"Theo ngày","Theo tháng","Theo năm","Theo danh mục"})Eq(sql,report.Bang[table].AsEnumerable().Sum(r=>r.Field<decimal>("Doanh thu")));
            foreach(var group in raw.AsEnumerable().GroupBy(r=>r.Field<int>("MaHD")))Eq(group.First().Field<decimal>("TongTien"),pos.LayChiTietHienThi(group.Key).AsEnumerable().Sum(r=>r.Field<decimal>("ThanhTien"))-pos.LayTheoMa(group.Key)!.TienGiamGia);
            var path=Path.Combine(output,"promotion-report.xlsx");XuatTaiLieu.Excel(path,report);XuatTaiLieu.BaoCaoPdf(Path.Combine(output,"promotion-report.pdf"),report);
            using var wb=new XLWorkbook(path);Eq(sql,wb.Worksheet("Theo danh mục").Cell(3,2).GetValue<decimal>());
            Eq(sql,wb.Worksheet("Theo danh mục").Range(7,3,6+report.Bang["Theo danh mục"].Rows.Count,3).Cells().Sum(c=>c.GetValue<decimal>()));
        });
        Test("P18 real WinForms voucher apply/remove and totals (no payment click)",()=>
        {
            ResetOffers();int p=Product();Offer("SANPHAM",10,p);Offer("VOUCHER",20,code:"POSGUI");
            PhienDangNhap.BatDau(1,nv,"integration","Integration test",VaiTro.Admin);
            using var form=new FrmPOS();form.Show();Pump(form);
            var grids=Controls(form).OfType<DataGridView>().ToList();var products=grids.Single(g=>g.DataSource is not BindingList<DongPOS>);
            products.CurrentCell=products.Rows.Cast<DataGridViewRow>().Single(r=>Convert.ToInt32(r.Cells["MaSP"].Value)==p).Cells["TenSP"];
            Controls(form).OfType<Button>().Single(b=>b.Text=="Thêm vào giỏ").PerformClick();Pump(form);
            var input=Controls(form).OfType<TextBox>().Single(t=>t.AccessibleName=="Mã voucher");input.Text="posgui";
            Controls(form).OfType<Button>().Single(b=>b.Text=="Áp dụng").PerformClick();Pump(form);
            True(Controls(form).OfType<Label>().Any(l=>l.Text.Contains("Tổng thanh toán: 72")));
            True(Controls(form).OfType<Label>().Any(l=>l.Text.Contains("Đã áp dụng POSGUI")));
            var summary=Controls(form).OfType<Label>().Single(l=>l.Text.StartsWith("Tiền hàng trước giảm:"));
            var required=summary.GetPreferredSize(new Size(summary.Width,0));
            True(summary.Height>=required.Height); // All three discount lines must remain visible.
            using(var bmp=new Bitmap(form.Width,form.Height)) { form.DrawToBitmap(bmp,new Rectangle(Point.Empty,bmp.Size));bmp.Save(Path.Combine(output,"POS-promotions.png")); }
            Controls(form).OfType<Button>().Single(b=>b.Text=="Bỏ mã").PerformClick();Pump(form);Eq("",input.Text);
            Controls(form).OfType<Button>().Single(b=>b.Text=="Bỏ dòng giỏ").PerformClick();Pump(form);form.Close();PhienDangNhap.DangXuat();
        });
        Test("P19 repeat Web/POS last-stock interleavings eight times without SQL deadlocks",()=>
        {
            ResetOffers();
            for(int attempt=0;attempt<8;attempt++)
            {
                int p=Product(stock:1);var q=Quote(p);using var start=new Barrier(2);
                var tasks=new[]{Task.Run(()=> { start.SignalAndWait();try { pos.ThanhToanPOS(nv,q,100,Guid.NewGuid());return true; }catch(InvalidOperationException) { return false; } }),
                    Task.Run(()=> { start.SignalAndWait();try { web.TaoDon(customer,"Test","COD",q);return true; }catch(InvalidOperationException) { return false; } })};
                Task.WaitAll(tasks);Eq(1,tasks.Count(t=>t.Result));Eq(0,Stock(p));
            }
        });
    }
    private static void FinalReconciliation()=>Test("F01 final database and persisted exports after all Web/Desktop tests",()=>
    {
        var from=DateTime.Today.AddYears(-2);var to=DateTime.Today;
        var report=BaoCaoService.Tao(new BaoCaoRepository().LayDuLieu(from,to,null),from,to,null);
        decimal total=Convert.ToDecimal(DbHelper.ExecuteScalar("SELECT SUM(TongTien) FROM HoaDon WHERE TrangThai=N'Hoàn thành'"));Eq(total,report.TongDoanhThu);
        Eq(total,report.Bang["Theo danh mục"].AsEnumerable().Sum(r=>r.Field<decimal>("Doanh thu")));
        XuatTaiLieu.Excel(Path.Combine(output,"final-integration-report.xlsx"),report);XuatTaiLieu.BaoCaoPdf(Path.Combine(output,"final-integration-report.pdf"),report);
        File.WriteAllText(Path.Combine(output,"final-expected-revenue.txt"),total.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Ghi("Final SQL completed revenue: "+total.ToString(System.Globalization.CultureInfo.InvariantCulture));
    });
}
