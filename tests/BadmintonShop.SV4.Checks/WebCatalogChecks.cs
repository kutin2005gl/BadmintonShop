using BadmintonShop.Core.DataAccess;
using BadmintonShop.Web.Controllers;
using BadmintonShop.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static partial class Program
{
    private static void WebCatalogChecks()=>Test("DB-C03 HTTP catalog across requests / refresh / local mutation",()=>WebCatalogAsync().GetAwaiter().GetResult());
    private static async Task WebCatalogAsync()
    {
        var builder=WebApplication.CreateBuilder(new WebApplicationOptions { ApplicationName=typeof(SanPhamController).Assembly.GetName().Name,
            ContentRootPath=Path.Combine(root,"BadmintonShop.Web"),EnvironmentName="Development" });
        builder.Logging.ClearProviders();builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddControllersWithViews().AddApplicationPart(typeof(SanPhamController).Assembly);
        builder.Services.AddSingleton<DanhMucCongKhai>();
        await using var app=builder.Build();app.UseRouting();app.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}");
        await app.StartAsync();
        try
        {
            using var http=new HttpClient { BaseAddress=new Uri(app.Urls.Single()) };using var counter=new SqlCounter();
            await GetHtml(http,"/SanPham");int initial=counter.Count;Eq(2,initial);
            foreach(var word in new[]{"Vợt","Cầu","1","2","3","a","b","c","thử",""})
                await GetHtml(http,"/SanPham?tuKhoa="+Uri.EscapeDataString(word));
            Eq(initial,counter.Count);
            await GetHtml(http,"/SanPham?lamMoi=true");Eq(initial+2,counter.Count);
            DbHelper.ExecuteNonQuery("UPDATE SanPham SET TenSP=N'Web đổi sau lưu' WHERE MaSP=@ma",new Microsoft.Data.SqlClient.SqlParameter("@ma",sp2));
            int before=counter.Count;var html=await GetHtml(http,"/SanPham?tuKhoa=Web");Eq(before+2,counter.Count);
            True(System.Net.WebUtility.HtmlDecode(html).Contains("Web đổi sau lưu"));
            Ghi("SQL HTTP catalog initial=2; ten requests extra=0; refresh=2; after mutation=2");
        }
        finally { await app.StopAsync(); }
    }
}
