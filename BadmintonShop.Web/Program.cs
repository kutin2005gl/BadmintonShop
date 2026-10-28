
using BadmintonShop.Web.Services;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;
using BadmintonShop.Web.Infrastructure;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// OTP mô phỏng
builder.Services.AddSingleton<OtpService>();

// Kết nối SQL Server
DbHelper.ConnectionString =
    builder.Configuration.GetConnectionString("BadmintonShop")!;

// MVC
builder.Services.AddControllersWithViews();

// Xác thực khách hàng
builder.Services.AddXacThucKhachHang();

// Session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromHours(2);
    o.Cookie.Name = "BadmintonShop.Session";
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

var app = builder.Build();

// Xử lý lỗi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// File tĩnh trong wwwroot
app.UseStaticFiles();

// Hình ảnh sản phẩm dùng chung Web và Desktop
var mediaRoot = KhoAnhSanPham.ThuMucGoc;
Console.WriteLine("THU MUC ANH SAN PHAM: " + mediaRoot);
System.Diagnostics.Debug.WriteLine(
    "THU MUC ANH SAN PHAM: " + mediaRoot
);

// Tạo thư mục products nếu chưa tồn tại
Directory.CreateDirectory(
    Path.Combine(mediaRoot, "products")
);

// Phục vụ ảnh từ thư mục MediaRoot qua /media
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaRoot),
    RequestPath = "/media"
});

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
