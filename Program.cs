// Program.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies; // ← Bổ sung thư viện Cookie Auth
using Tribean.Data;
using Tribean.Middlewares.Admin; // ← Bổ sung để gọi Trạm gác Middleware

var builder = WebApplication.CreateBuilder(args);

// ── 1. Đăng ký DbContext ────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.CommandTimeout(30)
    )
);

// ── 2. Cấu hình MVC & Runtime Compilation ───────────────────
builder.Services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();

// ── 3. Session ──────────────────────────────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── 4. CẤU HÌNH COOKIE AUTHENTICATION (MỚI) ─────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login"; 
        options.LogoutPath = "/Admin/Logout";
        options.AccessDeniedPath = "/Admin/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Giữ đăng nhập 8 tiếng
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession(); 

// ── 5. KÍCH HOẠT MIDDLEWARE BẢO MẬT (THỨ TỰ BẮT BUỘC) ───────
app.UseAuthentication(); // Bỏ vé ra kiểm tra (Ai đây?)
app.UseAuthorization();  // Xét quyền (Được làm gì?)
app.UseMiddleware<AdminAuthMiddleware>(); // Trạm gác riêng cho khu vực /Admin

// ── 6. Route Admin area ─────────────────────────────────────
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ── 7. Tự động migrate khi khởi động ────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();
