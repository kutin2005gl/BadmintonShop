using BadmintonShop.Core.DataAccess;
using BadmintonShop.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Chuoi ket noi lay tu appsettings.json (hoac bien moi truong ConnectionStrings__BadmintonShop)
DbHelper.ConnectionString = builder.Configuration.GetConnectionString("BadmintonShop")!;

// Email xac nhan don (SV3): cau hinh muc "Email" (mat khau dat bang user-secrets); thieu cau hinh thi bo qua gui mail
BadmintonShop.Core.Services.EmailService.CauHinh =
    builder.Configuration.GetSection("Email").Get<BadmintonShop.Core.Services.EmailCauHinh>() ?? new();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Dang nhap khach hang bang cookie (SV1) - xem Infrastructure/XacThucKhachHang.cs
builder.Services.AddXacThucKhachHang();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromHours(2);
    o.Cookie.Name = "BadmintonShop.Session";
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();   // phai dung TRUOC UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
