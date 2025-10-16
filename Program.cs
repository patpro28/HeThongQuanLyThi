using HeThongQuanLyThi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
    // opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    // hoặc UseNpgsql(...)
);

// Đăng ký Identity (cookie-based auth)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
// .AddDefaultUI(); // dùng UI mặc định (Razor) nếu là webapp

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/account/login";          // dùng trang Login của bạn
    opt.LogoutPath = "/account/logout";
    opt.AccessDeniedPath = "/account/denied";
    opt.SlidingExpiration = true;
    opt.ExpireTimeSpan = TimeSpan.FromDays(7);
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();             // Trang lỗi chi tiết
    app.UseStatusCodePages();                    // 404/403 đơn giản
    // app.UseHttpsRedirection();                // tuỳ, có thể tắt trong dev
}
else
{
    app.UseExceptionHandler("/Home/Error");      // Trang lỗi chung chung
    app.UseHsts();
}

await DbSeeder.SeedAsync(app.Services); // Tạo admin user

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // << quan trọng
app.UseAuthorization();

// Route cho Areas (Admin)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
