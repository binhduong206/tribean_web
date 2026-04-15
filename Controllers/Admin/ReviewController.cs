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
    }
}