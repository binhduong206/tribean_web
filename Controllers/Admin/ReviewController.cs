using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tribean.Data;
using Tribean.Models;
using Tribean.Routers.Admin;

namespace Tribean.Controllers.Admin
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ReviewController(ApplicationDbContext db) { _db = db; }

        // ==========================================
        // 1. DANH SÁCH BÌNH LUẬN (KIỂM DUYỆT)
        // ==========================================
        [HttpGet("/" + ReviewRouter.Index)]
        public async Task<IActionResult> Index(int page = 1)
        {
            const int PageSize = 5;
            // Join 3 bảng lại với nhau: Review + User + Product
            var query = _db.Reviews.Include(r => r.User).Include(r => r.Product).AsQueryable();

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var reviews = await query.OrderByDescending(r => r.CreatedAt)
                                     .Skip((page - 1) * PageSize)
                                     .Take(PageSize)
                                     .ToListAsync();

            ViewData["Page"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;

            return View("~/Views/Admin/Pages/Review/Index.cshtml", reviews);
        }

        // ==========================================
        // 2. XÓA BÌNH LUẬN (Dọn dẹp rác/spam)
        // ==========================================
        [HttpPost("/" + ReviewRouter.Delete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var review = await _db.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            _db.Reviews.Remove(review);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Review has been removed.";
            return Redirect("/" + ReviewRouter.Index);
        }

        // ==========================================
        // 3. [API ẨN] TẠO DỮ LIỆU GIẢ ĐỂ TEST UI
        // ==========================================
        [HttpGet("/" + ReviewRouter.Seed)]
        public async Task<IActionResult> SeedData()
        {
            var users = await _db.Users.ToListAsync();
            var products = await _db.Products.ToListAsync();

            // Phải có ít nhất 1 user và 1 product mới tạo review giả được
            if (!users.Any() || !products.Any())
            {
                TempData["Error"] = "Warning: You need at least 1 User and 1 Product in database to generate reviews.";
                return Redirect("/" + ReviewRouter.Index);
            }

            var random = new Random();
            var fakeComments = new[] {
                "Cà phê rất ngon, đậm đà, sẽ quay lại ủng hộ shop! Cà phê rất ngon, đậm đà, sẽ quay lại ủng hộ shop! Cà phê rất ngon, đậm đà, sẽ quay lại ủng hộ shop! Cà phê rất ngon, đậm đà, sẽ quay lại ủng hộ shop! Cà phê rất ngon, đậm đà, sẽ quay lại ủng hộ shop! Cà phê rất ngon, đậm đà, sẽ quay lại ủng hộ shop! Cà phê rất ngon, đậm đà, sẽ quay lại ủng hộ shop! Cà phê rất ngon, đậm đà, sẽ quay lại ủng hộ shop! Cà phê rất ngon, đậm đà, sẽ quay lại ủng hộ shop!"
            };

            for (int i = 0; i < 1; i++) // Bơm thẳng 15 review
            {
                var rUser = users[random.Next(users.Count)];
                var rProduct = products[random.Next(products.Count)];

                _db.Reviews.Add(new Review
                {
                    UserId = rUser.Id,
                    ProductId = rProduct.Id,
                    Rating = random.Next(3, 6), // Khách VIP toàn rate 3-5 sao thôi =))
                    Comment = fakeComments[random.Next(fakeComments.Length)],
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)) // Random ngày mua trong tháng qua
                });
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Successfully generated 15 fake reviews!";
            return Redirect("/" + ReviewRouter.Index);
        }
    }
}