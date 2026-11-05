using System.Net;
using System.Text.RegularExpressions;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;
using BadmintonShop.Web.Controllers;
using BadmintonShop.Web.Infrastructure;
using BadmintonShop.Web.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static partial class Program
{
    private static void WebHttpChecks()=>Test("H01 live Kestrel MVC checkout / anti-forgery / ownership / Desktop state / stale quote",()=>Task.Run(WebHttpAsync).GetAwaiter().GetResult());
    private static async Task WebHttpAsync()
    {
        ResetOffers();int product=Product();Offer("SANPHAM",10,product);Offer("VOUCHER",20,code:"HTTP20");
        var assembly=typeof(DatHangController).Assembly;
        var builder=WebApplication.CreateBuilder(new WebApplicationOptions { ApplicationName=assembly.GetName().Name,ContentRootPath=Path.Combine(root,"BadmintonShop.Web"),EnvironmentName="Development" });
        builder.Logging.ClearProviders();builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddControllersWithViews().AddApplicationPart(assembly);
        builder.Services.AddSingleton<DanhMucCongKhai>();
        builder.Services.AddXacThucKhachHang();builder.Services.AddDistributedMemoryCache();builder.Services.AddSession();
        await using var app=builder.Build();app.UseRouting();app.UseSession();app.UseAuthentication();app.UseAuthorization();
        // Test-host-only fixtures. None of these endpoints exists in BadmintonShop.Web's startup.
        app.MapGet("/__test/login",async (HttpContext http)=>await http.DangNhapKhachHangAsync(new KhachHangRepository().LayTheoMa(customer)!));
        int other=Convert.ToInt32(DbHelper.ExecuteScalar("INSERT KhachHang(HoTen,Email,MatKhau) VALUES(N'Other HTTP customer','other-http@example.invalid','test');SELECT CAST(SCOPE_IDENTITY() AS int)"));
        app.MapGet("/__test/other",async (HttpContext http)=>await http.DangNhapKhachHangAsync(new KhachHangRepository().LayTheoMa(other)!));
        app.MapGet("/__test/cart",(HttpContext http)=> { http.Session.LuuGio([new DongGioLuu(product,1)]);http.Session.LuuVoucher("HTTP20");return "ready"; });
        app.MapGet("/__test/token",(HttpContext http,IAntiforgery csrf)=>csrf.GetAndStoreTokens(http).RequestToken!);
        app.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}");
        await app.StartAsync();
        try
        {
            var origin=new Uri(app.Urls.Single());
            using var handler=new HttpClientHandler { CookieContainer=new CookieContainer(),AllowAutoRedirect=false };
            using var http=new HttpClient(handler) { BaseAddress=origin };
            Eq(HttpStatusCode.Redirect,(await http.GetAsync("/DonHang")).StatusCode);
            await http.GetAsync("/__test/login");await http.GetAsync("/__test/cart");
            string html=await GetHtml(http,"/DatHang");
            var noCsrf=await http.PostAsync("/DatHang/XacNhan",new FormUrlEncodedContent(new Dictionary<string,string> { ["diaChi"]="HTTP test",["hinhThucTT"]="COD" }));
            Eq(HttpStatusCode.BadRequest,noCsrf.StatusCode);
            var created=await Checkout(http,html);Eq(HttpStatusCode.Redirect,created.StatusCode);int id=RedirectId(created);
            Eq(72m,new HoaDonRepository().LayTheoMa(id)!.TongTien);Eq(19,Stock(product));Eq(1,Used("HTTP20"));
            var repo=new HoaDonRepository();
            foreach(var (before,after) in new[]{("Chờ xác nhận","Đã xác nhận"),("Đã xác nhận","Đang giao"),("Đang giao","Hoàn thành")})
            {
                repo.ChuyenTrangThai(id,before,after);html=await GetHtml(http,$"/DonHang/ChiTiet/{id}");True(WebUtility.HtmlDecode(html).Contains(after));Eq(19,Stock(product));
            }
            File.WriteAllText(Path.Combine(output,"web-order-completed.html"),html);
            // A different authenticated customer cannot read or cancel this order.
            using var otherHandler=new HttpClientHandler { CookieContainer=new CookieContainer(),AllowAutoRedirect=false };
            using var otherClient=new HttpClient(otherHandler) { BaseAddress=origin };await otherClient.GetAsync("/__test/other");
            Eq(HttpStatusCode.NotFound,(await otherClient.GetAsync($"/DonHang/ChiTiet/{id}")).StatusCode);
            string token=await otherClient.GetStringAsync("/__test/token");
            await otherClient.PostAsync($"/DonHang/Huy/{id}",new FormUrlEncodedContent(new Dictionary<string,string> { ["__RequestVerificationToken"]=token }));
            Eq("Hoàn thành",repo.LayTheoMa(id)!.TrangThai);
            // A changed price between GET confirmation and POST never silently creates an order.
            await http.GetAsync("/__test/cart");html=await GetHtml(http,"/DatHang");int count=Orders();Price(product,110);
            var stale=await Checkout(http,html);Eq(HttpStatusCode.OK,stale.StatusCode);Eq(count,Orders());
            html=await stale.Content.ReadAsStringAsync();True(WebUtility.HtmlDecode(html).Contains("xác nhận lại"));
            created=await Checkout(http,html);Eq(HttpStatusCode.Redirect,created.StatusCode);int cancellable=RedirectId(created);
            Eq(79m,repo.LayTheoMa(cancellable)!.TongTien);Eq(18,Stock(product));
            html=await GetHtml(http,$"/DonHang/ChiTiet/{cancellable}");
            await http.PostAsync($"/DonHang/Huy/{cancellable}",new FormUrlEncodedContent(new Dictionary<string,string> { ["__RequestVerificationToken"]=Token(html) }));
            Eq("Đã hủy",repo.LayTheoMa(cancellable)!.TrangThai);Eq(19,Stock(product));
            await http.PostAsync($"/DonHang/Huy/{cancellable}",new FormUrlEncodedContent(new Dictionary<string,string> { ["__RequestVerificationToken"]=Token(html) }));Eq(19,Stock(product));
        }
        finally { await app.StopAsync(); }
    }
    private static async Task<string> GetHtml(HttpClient client,string url)
    {
        var response=await client.GetAsync(url);var html=await response.Content.ReadAsStringAsync();
        if(response.StatusCode!=HttpStatusCode.OK)throw new Exception($"HTTP {url}: {response.StatusCode}: {html[..Math.Min(1200,html.Length)]}");return html;
    }
    private static string Token(string html)
    {
        var match=Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        if(!match.Success)throw new Exception("Missing anti-forgery token in MVC view.");return WebUtility.HtmlDecode(match.Groups[1].Value);
    }
    private static Task<HttpResponseMessage> Checkout(HttpClient http,string html)=>http.PostAsync("/DatHang/XacNhan",new FormUrlEncodedContent(new Dictionary<string,string> { ["__RequestVerificationToken"]=Token(html),["diaChi"]="HTTP integration test",["hinhThucTT"]="COD" }));
    private static int RedirectId(HttpResponseMessage response)
    {
        var match=Regex.Match(response.Headers.Location!.ToString(),@"(?:/|id=)(\d+)(?:$|&)");
        if(!match.Success)throw new Exception("Missing order ID in redirect.");return int.Parse(match.Groups[1].Value);
    }
}
