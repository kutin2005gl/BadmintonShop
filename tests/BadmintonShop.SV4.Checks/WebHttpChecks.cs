using System.Net;
using System.Text.RegularExpressions;
using System.Text.Json;
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
        // CO-02: moi khach chi dung 1 lan moi ma voucher -> don thu hai cua cung khach dung ma khac (HTTP20B, cung 20%).
        ResetOffers();int product=Product();Offer("SANPHAM",10,product);Offer("VOUCHER",20,code:"HTTP20");Offer("VOUCHER",20,code:"HTTP20B");
        var assembly=typeof(DatHangController).Assembly;
        var builder=WebApplication.CreateBuilder(new WebApplicationOptions { ApplicationName=assembly.GetName().Name,ContentRootPath=Path.Combine(root,"BadmintonShop.Web"),EnvironmentName="Development" });
        builder.Logging.ClearProviders();builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddControllersWithViews().AddApplicationPart(assembly);
        builder.Services.AddSingleton<DanhMucCongKhai>();builder.Services.AddSingleton<OtpService>();
        builder.Services.AddXacThucKhachHang();builder.Services.AddDistributedMemoryCache();builder.Services.AddSession();
        await using var app=builder.Build();app.UseRouting();app.UseSession();app.UseAuthentication();app.UseAuthorization();
        // Test-host-only fixtures. None of these endpoints exists in BadmintonShop.Web's startup.
        app.MapGet("/__test/login",async (HttpContext http)=>await http.DangNhapKhachHangAsync(new KhachHangRepository().LayTheoMa(customer)!));
        int other=Convert.ToInt32(DbHelper.ExecuteScalar("INSERT KhachHang(HoTen,Email,MatKhau) VALUES(N'Other HTTP customer','other-http@example.invalid','test');SELECT CAST(SCOPE_IDENTITY() AS int)"));
        app.MapGet("/__test/other",async (HttpContext http)=>await http.DangNhapKhachHangAsync(new KhachHangRepository().LayTheoMa(other)!));
        app.MapGet("/__test/cart",(HttpContext http)=> { http.Session.LuuGio([new DongGioLuu(product,1)]);http.Session.LuuVoucher(http.Request.Query["v"].FirstOrDefault() ?? "HTTP20");return "ready"; });
        app.MapGet("/__test/token",(HttpContext http,IAntiforgery csrf)=>csrf.GetAndStoreTokens(http).RequestToken!);
        app.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}");
        await app.StartAsync();
        try
        {
            var origin=new Uri(app.Urls.Single());
            await CheckAddCart(origin, product);
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
            await http.GetAsync("/__test/cart?v=HTTP20B");html=await GetHtml(http,"/DatHang");int count=Orders();Price(product,110);
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
    private static async Task CheckAddCart(Uri origin, int product)
    {
        using var handler = new HttpClientHandler { CookieContainer = new CookieContainer(), AllowAutoRedirect = false };
        using var client = new HttpClient(handler) { BaseAddress = origin };
        var token = await client.GetStringAsync("/__test/token");
        const string catalog = "/SanPham?sapXep=gia-tang&trang=2";
        // TV1: gui form gio hang voi tuy chon AJAX va token
        async Task<HttpResponseMessage> PostCart(string action, int quantity, bool ajax, bool csrf = true)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/GioHang/{action}");
            if (ajax) request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = csrf ? token : "",
                ["maSP"] = product.ToString(), ["soLuong"] = quantity.ToString(), ["returnUrl"] = catalog
            });
            return await client.SendAsync(request);
        }

        // TV1: doi chieu trang thai HTTP, cau truc JSON va tong so luong
        async Task<string> CheckJson(HttpResponseMessage response, HttpStatusCode status, bool success, int count)
        {
            Eq(status, response.StatusCode);
            Eq("application/json", response.Content.Headers.ContentType?.MediaType);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var result = json.RootElement;
            Eq(3, result.EnumerateObject().Count());
            Eq(success, result.GetProperty("thanhCong").GetBoolean());
            Eq(count, result.GetProperty("tongSoLuong").GetInt32());
            var message = result.GetProperty("thongBao").GetString();
            True(!string.IsNullOrWhiteSpace(message));
            return message!;
        }

        using var noCsrf = await PostCart("Them", 1, true, false);
        Eq(HttpStatusCode.BadRequest, noCsrf.StatusCode);
        using var added = await PostCart("Them", 1, true);
        await CheckJson(added, HttpStatusCode.OK, true, 1);

        int stock = Stock(product);
        using var overflow = await PostCart("Them", stock, true);
        var stockMessage = await CheckJson(overflow, HttpStatusCode.BadRequest, false, 1);

        using var fallback = await PostCart("Them", 1, false);
        Eq(HttpStatusCode.Redirect, fallback.StatusCode);
        Eq(catalog, fallback.Headers.Location!.ToString());
        using var drawer = await client.GetAsync("/GioHang/NganGio");
        Eq(HttpStatusCode.OK, drawer.StatusCode);
        True(drawer.Headers.CacheControl!.NoStore);
        True(WebUtility.HtmlDecode(await drawer.Content.ReadAsStringAsync()).Contains("Xem giỏ hàng"));

        using var increased = await PostCart("CapNhat", 3, true);
        await CheckJson(increased, HttpStatusCode.OK, true, 3);
        using var beyondStock = await PostCart("CapNhat", stock + 1, true);
        Eq(stockMessage, await CheckJson(beyondStock, HttpStatusCode.BadRequest, false, 3));
        using var decreased = await PostCart("CapNhat", 1, true);
        await CheckJson(decreased, HttpStatusCode.OK, true, 1);
        using var removed = await PostCart("Xoa", 0, true);
        await CheckJson(removed, HttpStatusCode.OK, true, 0);

        using var readded = await PostCart("Them", 1, true);
        await CheckJson(readded, HttpStatusCode.OK, true, 1);
        using var updateFallback = await PostCart("CapNhat", 2, false);
        Eq(HttpStatusCode.Redirect, updateFallback.StatusCode);
        Eq(catalog, updateFallback.Headers.Location!.ToString());
        using var confirmed = await PostCart("Them", 1, true);
        await CheckJson(confirmed, HttpStatusCode.OK, true, 3);
        using var removeFallback = await PostCart("Xoa", 0, false);
        Eq(HttpStatusCode.Redirect, removeFallback.StatusCode);
        Eq(catalog, removeFallback.Headers.Location!.ToString());
        using var emptyDrawer = await client.GetAsync("/GioHang/NganGio");
        True(WebUtility.HtmlDecode(await emptyDrawer.Content.ReadAsStringAsync()).Contains("Giỏ hàng của bạn đang trống."));

        // TV1: trang mua hang phai danh dau "false" de cart.js luu diem quay lai, trang gio danh dau "true"
        True((await client.GetStringAsync(catalog)).Contains("<body data-cart-page=\"false\">"));
        True((await client.GetStringAsync("/GioHang")).Contains("<body data-cart-page=\"true\">"));
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
