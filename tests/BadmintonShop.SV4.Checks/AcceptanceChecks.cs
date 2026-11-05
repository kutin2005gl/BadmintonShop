using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;
using BadmintonShop.Desktop;
using Microsoft.Data.SqlClient;

internal static partial class Program
{
    private static int AcceptanceDesktopReview()
    {
        DbHelper.ConnectionString=Environment.GetEnvironmentVariable("ConnectionStrings__BadmintonShop") ?? throw new ArgumentException("Explicit test connection required");
        if(!new SqlConnectionStringBuilder(DbHelper.ConnectionString).InitialCatalog.StartsWith("BadmintonShop_SV4_Test_",StringComparison.Ordinal))throw new ArgumentException("Only isolated test databases");
        int employee=ScalarInt("SELECT MIN(MaNV) FROM NhanVien");
        PhienDangNhap.BatDau(1,employee,"ui-review","Nghiệm thu DB thử",VaiTro.Admin);
        Application.Run(new FrmMain());PhienDangNhap.DangXuat();return 0;
    }
    // A new database is mandatory; no existing catalog is accepted by this entry point.
    private static int Acceptance(string[] args)
    {
        int index=Array.IndexOf(args,"--server");
        if(index<0 || index+1>=args.Length)throw new ArgumentException("Requires --server; creates a NEW test database.");
        TaoCsdlTest(args[index+1]);ReportDatabaseChecks();
        // The production product page also reads reviews. Apply only its DDL, no sample users/reviews.
        var reviews=File.ReadAllText(Path.Combine(root,"database/07_SV2_DanhGiaSanPham.sql"));
        reviews=reviews[..reviews.IndexOf("-- Danh gia mau",StringComparison.Ordinal)];
        Batches(reviews.Replace("USE BadmintonShop;","",StringComparison.Ordinal));
        if(args.Contains("--acceptance-ui-setup"))
        {
            // Prepare a fresh UI fixture without placing or completing an order for the operator.
            DbHelper.ExecuteNonQuery("INSERT KhachHang(HoTen,Email,MatKhau) VALUES(N'Khách nghiệm thu','acceptance@example.invalid',@hash)",new SqlParameter("@hash",MatKhauHelper.BamMatKhau("TestOnly@123")));
            Ghi("UI fixture ready; no acceptance order created. Production Desktop login is NOT exercised.");
            Ghi($"RESULT: {passed} passed, {failed} failed. Output: {output}");
            File.WriteAllLines(Path.Combine(output,"results.txt"),log);
            return failed==0?0:1;
        }
        PhienDangNhap.BatDau(1,nv,"acceptance","Nghiệm thu DB thử",VaiTro.Admin);
        Test("E01 production Web process checkout -> Desktop form transition -> cached report refresh",()=>
        {
            int kh=Convert.ToInt32(DbHelper.ExecuteScalar("INSERT KhachHang(HoTen,Email,MatKhau) VALUES(N'Khách nghiệm thu','acceptance@example.invalid',@hash);SELECT CAST(SCOPE_IDENTITY() AS int)",new SqlParameter("@hash",MatKhauHelper.BamMatKhau("TestOnly@123"))));
            var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();int port=((IPEndPoint)listener.LocalEndpoint).Port;listener.Stop();
            var start=new ProcessStartInfo("dotnet") { UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden,WorkingDirectory=Path.Combine(root,"BadmintonShop.Web"),RedirectStandardOutput=true,RedirectStandardError=true };
            start.ArgumentList.Add(Path.Combine(root,"BadmintonShop.Web/bin/Debug/net8.0/BadmintonShop.Web.dll"));
            start.Environment["ConnectionStrings__BadmintonShop"]=DbHelper.ConnectionString;
            start.Environment["ASPNETCORE_URLS"]=$"http://127.0.0.1:{port}";
            start.Environment["ASPNETCORE_ENVIRONMENT"]="Development";
            using var web=Process.Start(start)!;
            var webLog=new System.Text.StringBuilder();
            web.OutputDataReceived+=(_,e)=> { if(e.Data!=null)lock(webLog)webLog.AppendLine(e.Data); };
            web.ErrorDataReceived+=(_,e)=> { if(e.Data!=null)lock(webLog)webLog.AppendLine(e.Data); };
            web.BeginOutputReadLine();web.BeginErrorReadLine();
            try
            {
                using var report=new FrmThongKe();report.Show();WaitUi(report);
                var dates=Controls(report).OfType<DateTimePicker>().ToArray();dates[0].Value=new(2025,1,1);dates[1].Value=new(2026,12,31);
                var view=Controls(report).OfType<Button>().Single(b=>b.Text=="Xem báo cáo");view.PerformClick();WaitUi(report);
                KetQuaBaoCao Snapshot()=>(KetQuaBaoCao)typeof(FrmThongKe).GetField("ketQua",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(report)!;
                Eq(511m,Snapshot().TongDoanhThu);
                using var handler=new HttpClientHandler { CookieContainer=new CookieContainer(),AllowAutoRedirect=false };
                using var http=new HttpClient(handler) { BaseAddress=new Uri($"http://127.0.0.1:{port}"),Timeout=TimeSpan.FromSeconds(5) };
                string login="";
                for(int attempt=0;attempt<40;attempt++)
                { try { login=http.GetStringAsync("/TaiKhoan/DangNhap").GetAwaiter().GetResult();break; }catch(HttpRequestException) { Thread.Sleep(250); } }
                True(login.Length>0);
                True(WebUtility.HtmlDecode(GetHtml(http,$"/SanPham/ChiTiet/{sp1}").GetAwaiter().GetResult()).Contains("Vợt thử"));
                Eq(HttpStatusCode.Redirect,http.GetAsync("/DonHang/ChiTietDong/1").GetAwaiter().GetResult().StatusCode);
                var response=http.PostAsync("/TaiKhoan/DangNhap",new FormUrlEncodedContent(new Dictionary<string,string> { ["__RequestVerificationToken"]=Token(login),["Email"]="acceptance@example.invalid",["MatKhau"]="TestOnly@123" })).GetAwaiter().GetResult();Eq(HttpStatusCode.Redirect,response.StatusCode);
                string cart=GetHtml(http,"/GioHang").GetAwaiter().GetResult();
                response=http.PostAsync("/GioHang/Them",new FormUrlEncodedContent(new Dictionary<string,string> { ["__RequestVerificationToken"]=Token(cart),["maSP"]=sp1.ToString(),["soLuong"]="1" })).GetAwaiter().GetResult();Eq(HttpStatusCode.Redirect,response.StatusCode);
                string checkout=GetHtml(http,"/DatHang").GetAwaiter().GetResult();
                response=Checkout(http,checkout).GetAwaiter().GetResult();Eq(HttpStatusCode.Redirect,response.StatusCode);int id=RedirectId(response);
                Eq(kh,new HoaDonRepository().LayTheoMa(id)!.MaKH);Eq(100m,new HoaDonRepository().LayTheoMa(id)!.TongTien);
                var detail=http.GetAsync($"/DonHang/ChiTietDong/{id}").GetAwaiter().GetResult();Eq(HttpStatusCode.OK,detail.StatusCode);
                True(detail.Headers.CacheControl!.NoStore);True(!detail.Content.ReadAsStringAsync().GetAwaiter().GetResult().Contains("<html"));
                int foreignId=ScalarInt("SELECT MIN(MaHD) FROM HoaDon WHERE MaKH IS NULL");
                Eq(HttpStatusCode.NotFound,http.GetAsync($"/DonHang/ChiTietDong/{foreignId}").GetAwaiter().GetResult().StatusCode);
                string history=GetHtml(http,"/DonHang").GetAwaiter().GetResult();True(history.Contains("data-order-detail"));True(history.Contains("order-main-sub.js"));
                view.PerformClick();WaitUi(report);Eq(511m,Snapshot().TongDoanhThu);
                using var orders=new FrmDonHang();orders.Show();WaitUi(orders);
                var grid=Controls(orders).OfType<DataGridView>().Single(g=>g.Columns.Contains("MaHD"));
                grid.CurrentCell=grid.Rows.Cast<DataGridViewRow>().Single(r=>Convert.ToInt32(r.Cells["MaHD"].Value)==id).Cells["MaHD"];WaitUi(orders);
                foreach(var (before,after) in new[]{("Chờ xác nhận","Đã xác nhận"),("Đã xác nhận","Đang giao"),("Đang giao","Hoàn thành")})
                {
                    // Exercise the form's asynchronous save handler; MessageBox is reserved for UI acceptance.
                    WaitAcceptance(()=>Controls(orders).OfType<DataGridView>().All(g=>g.Enabled));
                    typeof(FrmDonHang).GetMethod("GhiTrangThai",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(orders,[id,before,after,null]);
                    WaitAcceptance(()=>Convert.ToString(grid.Rows.Cast<DataGridViewRow>().Single(r=>Convert.ToInt32(r.Cells["MaHD"].Value)==id).Cells["TrangThai"].Value)==after && grid.Enabled);
                    Eq(after,new HoaDonRepository().LayTheoMa(id)!.TrangThai);
                    string html=GetHtml(http,$"/DonHang/ChiTiet/{id}").GetAwaiter().GetResult();True(WebUtility.HtmlDecode(html).Contains(after));
                    File.WriteAllText(Path.Combine(output,$"order-{id}-{Array.IndexOf(QuyTacBanHang.TrangThaiDon,after)}.html"),html);
                }
                using var count=new SqlCounter();view.PerformClick();WaitUi(report);Eq(611m,Snapshot().TongDoanhThu);Eq(2,count.Count);
                view.PerformClick();WaitUi(report);Eq(2,count.Count);
                Controls(report).OfType<Button>().Single(b=>b.Text=="Làm mới").PerformClick();WaitUi(report);Eq(4,count.Count);Eq(611m,Snapshot().TongDoanhThu);
                XuatTaiLieu.Excel(Path.Combine(output,"acceptance-report.xlsx"),Snapshot());XuatTaiLieu.BaoCaoPdf(Path.Combine(output,"acceptance-report.pdf"),Snapshot());ExportExpected("acceptance-report",Snapshot());
                Ghi($"Separate PIDs Web={web.Id}, Desktop forms={Environment.ProcessId}; order={id}; revenue 511 -> 511 pending -> 611 completed; report reload=2 SQL, reuse=0, manual refresh=2. HTTP uses production login/cart/checkout, no test endpoints.");
                orders.Close();report.Close();
            }
            finally { if(!web.HasExited)web.Kill(true);web.WaitForExit();lock(webLog)File.WriteAllText(Path.Combine(output,"production-web.log"),webLog.ToString()); }
        });
        Ghi($"RESULT: {passed} passed, {failed} failed. Output: {output}");File.WriteAllLines(Path.Combine(output,"results.txt"),log);
        if(args.Contains("--interactive-import"))
        {
            Ghi("Interactive import on SAME isolated mutable test database. Output: "+output);
            Application.Run(new FrmNhapSanPhamExcel());
            File.WriteAllText(Path.Combine(output,"import-after-ui.txt"),$"Products after UI: {DbHelper.ExecuteScalar("SELECT COUNT(*) FROM SanPham")}");
        }
        PhienDangNhap.DangXuat();return failed==0?0:1;
    }
    private static void WaitAcceptance(Func<bool> completed)
    {
        var timer=Stopwatch.StartNew();
        do { Application.DoEvents();Thread.Sleep(30);if(timer.ElapsedMilliseconds>300 && completed())return; }while(timer.Elapsed<TimeSpan.FromSeconds(20));
        throw new TimeoutException("Acceptance UI operation did not finish");
    }
}
