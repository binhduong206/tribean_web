using Microsoft.EntityFrameworkCore;

using Scalar.AspNetCore;
using Tribean.Data;
using Tribean.Repositories;
using Tribean.Services;

var builder = WebApplication.CreateBuilder(args);


// ─────────────────────────────────────────
// 1. SERVICES
// ─────────────────────────────────────────

// DbContext

using Microsoft.AspNetCore.Authentication.Cookies; // ← Bổ sung thư viện Cookie Auth
using Tribean.Data;
using Tribean.Middlewares.Admin; // ← Bổ sung để gọi Trạm gác Middleware

var builder = WebApplication.CreateBuilder(args);

// ── 1. Đăng ký DbContext ────────────────────────────────────

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Controller (API + MVC)
builder.Services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();

// Session

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


// Dependency Injection
builder.Services.AddScoped<IHomeRepository, HomeRepository>();
builder.Services.AddScoped<IHomeService, HomeService>();

// Swagger / OpenAPI
builder.Services.AddOpenApi();


// 🔥 CORS (PHẢI đặt ở đây)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


// ─────────────────────────────────────────
// 2. BUILD APP
// ─────────────────────────────────────────

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


// ─────────────────────────────────────────
// 3. MIDDLEWARE PIPELINE
// ─────────────────────────────────────────

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


// 🔥 THỨ TỰ QUAN TRỌNG

app.UseHttpsRedirection();

app.UseCors("AllowFrontend"); // 🔥 phải trước MapControllers

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();


app.UseSession();

// ── 5. KÍCH HOẠT MIDDLEWARE BẢO MẬT (THỨ TỰ BẮT BUỘC) ───────
app.UseAuthentication(); // Bỏ vé ra kiểm tra (Ai đây?)
app.UseAuthorization();  // Xét quyền (Được làm gì?)
app.UseMiddleware<AdminAuthMiddleware>(); // Trạm gác riêng cho khu vực /Admin

// ── 6. Route Admin area ─────────────────────────────────────
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");


// ─────────────────────────────────────────
// 4. ROUTES
// ─────────────────────────────────────────

// API
app.MapControllers();

// (Nếu bạn dùng MVC View thì bật lại)
// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}");


// ─────────────────────────────────────────
// 5. AUTO MIGRATE (optional)
// ─────────────────────────────────────────



// ── 7. Tự động migrate khi khởi động ────────────────────────

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}


// ─────────────────────────────────────────
// 6. RUN
// ─────────────────────────────────────────

app.Run();