using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tribean.Data;
using Tribean.Routers.Admin;
// Đừng quên Using thư viện này nhé sếp
using BCrypt.Net; 

namespace Tribean.Controllers.Admin
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AuthController(ApplicationDbContext db) => _db = db;

        // HIỂN THỊ FORM ĐĂNG NHẬP
        [HttpGet("/" + AuthRouter.Login)]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                return Redirect("/Admin/Dashboard"); 
            }
            return View("~/Views/Admin/Auth/Login.cshtml");
        }

        // XỬ LÝ GỬI FORM ĐĂNG NHẬP
        [HttpPost("/" + AuthRouter.Login)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Username and Password are required.";
                return View("~/Views/Admin/Auth/Login.cshtml");
            }

            // BƯỚC 1: Chỉ tìm User theo Username (Không kiểm tra password ở đây nữa)
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserName == username);
            
            // Nếu tìm thấy user và mật khẩu trong DB đang là text thô (11111) trùng với chữ vừa gõ < Để đăng nhập trước khi mình làm mã hóa sau >
            if (user != null && user.Password == password)
            {
                // Tự động băm nó ra và lưu đè lại vào CSDL ngay lập tức!
                user.Password = BCrypt.Net.BCrypt.HashPassword(password);
                await _db.SaveChangesAsync();
            }

            // BƯỚC 2: Kiểm tra User có tồn tại không VÀ Mật khẩu băm có khớp không
            // Dùng hàm BCrypt.Verify(Mật_Khẩu_Vừa_Gõ, Mật_Khẩu_Băm_Trong_DB)
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                TempData["Error"] = "Invalid username or password!";
                return View("~/Views/Admin/Auth/Login.cshtml");
            }

            if (user.Role == null || !user.Role.RoleName.ToLower().Contains("admin"))
            {
                TempData["Error"] = "Access Denied. You are not an administrator.";
                return View("~/Views/Admin/Auth/Login.cshtml");
            }

            // Gắn "thẻ tên" cho phiên đăng nhập
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role.RoleName),
                new Claim("FullName", $"{user.FirstName} {user.LastName}")
            };
            
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                claims.Add(new Claim("AvatarUrl", user.AvatarUrl));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Cấp Cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity), 
                new AuthenticationProperties { IsPersistent = true });

            return Redirect("/Admin/Dashboard");
        }

        // XỬ LÝ ĐĂNG XUẤT
        [HttpGet("/" + AuthRouter.Logout)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/" + AuthRouter.Login);
        }
    }
}