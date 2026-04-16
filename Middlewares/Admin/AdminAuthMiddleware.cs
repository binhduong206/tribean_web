using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Tribean.Middlewares.Admin
{
    public class AdminAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;

            // Bắt mọi request đi vào /Admin
            if (path.StartsWithSegments("/Admin", System.StringComparison.OrdinalIgnoreCase))
            {
                // Bỏ qua trang Login và Logout để tránh vòng lặp vô hạn
                if (!path.StartsWithSegments("/Admin/Login", System.StringComparison.OrdinalIgnoreCase) && 
                    !path.StartsWithSegments("/Admin/Logout", System.StringComparison.OrdinalIgnoreCase))
                {
                    // Đuổi về nếu chưa đăng nhập hoặc không phải Admin
                    if (context.User.Identity == null || !context.User.Identity.IsAuthenticated || !context.User.IsInRole("Admin"))
                    {
                        context.Response.Redirect("/Admin/Login");
                        return; // Khóa cổng, không cho chạy tiếp vào Controller
                    }
                }
            }

            await _next(context); // Mở cổng cho qua
        }
    }
}