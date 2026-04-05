using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tribean.Data;
using Tribean.Models;
using Tribean.Routers.Admin;

namespace Tribean.Controllers.Admin
{
    public class DiscountController : Controller
    {
        private readonly ApplicationDbContext _db;
        public DiscountController(ApplicationDbContext db) { _db = db; }

        // ==========================================
        // 1. DANH SÁCH MÃ GIẢM GIÁ (CÓ TÌM KIẾM & LỌC)
        // ==========================================
        [HttpGet("/" + DiscountRouter.Index)]
        public async Task<IActionResult> Index(string? search, bool? status, int page = 1)
        {
            const int PageSize = 5;
            var query = _db.Discounts.AsQueryable();

            // 1. Lọc theo tên mã
            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(d => d.DiscountName.ToLower().Contains(lowerSearch));
            }

            // 2. Lọc theo trạng thái
            if (status.HasValue)
            {
                query = query.Where(d => d.IsAvailable == status.Value);
            }

            // 3. Phân trang
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var discounts = await query.OrderByDescending(d => d.StartDate)
                                       .Skip((page - 1) * PageSize)
                                       .Take(PageSize)
                                       .ToListAsync();

            // Truyền dữ liệu ra View
            ViewData["Search"] = search ?? "";
            ViewData["StatusFilter"] = status.HasValue ? status.Value.ToString().ToLower() : "";
            ViewData["Page"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;

            return View("~/Views/Admin/Pages/Discount/Index.cshtml", discounts);
        }

        // ==========================================
        // 2. TẠO MỚI (CREATE)
        // ==========================================
        [HttpGet("/" + DiscountRouter.Create)]
        public IActionResult Create()
        {
            var model = new Discount 
            { 
                StartDate = DateTime.Now, 
                EndDate = DateTime.Now.AddDays(7),
                IsAvailable = true
            };
            return View("~/Views/Admin/Pages/Discount/Create.cshtml", model);
        }

        [HttpPost("/" + DiscountRouter.Create)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Discount model)
        {
            if (!ModelState.IsValid) return View("~/Views/Admin/Pages/Discount/Create.cshtml", model);

            _db.Discounts.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Discount created successfully!";
            return Redirect("/" + DiscountRouter.Index);
        }

        // ==========================================
        // 3. CẬP NHẬT (EDIT)
        // ==========================================
        [HttpGet("/" + DiscountRouter.Edit)]
        public async Task<IActionResult> Edit(string id)
        {
            var discount = await _db.Discounts.FindAsync(id);
            if (discount == null) return NotFound();

            return View("~/Views/Admin/Pages/Discount/Edit.cshtml", discount);
        }

        [HttpPost("/" + DiscountRouter.Edit)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Discount model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View("~/Views/Admin/Pages/Discount/Edit.cshtml", model);

            _db.Discounts.Update(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Discount updated successfully!";
            return Redirect("/" + DiscountRouter.Index);
        }

        // ==========================================
        // 4. BẬT/TẮT TRẠNG THÁI (TOGGLE)
        // ==========================================
        [HttpPost("/" + DiscountRouter.Toggle)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var discount = await _db.Discounts.FindAsync(id);
            if (discount == null) return NotFound();

            discount.IsAvailable = !discount.IsAvailable;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Discount '{discount.DiscountName}' is now {(discount.IsAvailable ? "Active" : "Disabled")}.";
            return Redirect("/" + DiscountRouter.Index);
        }

        // ==========================================
        // 5. XÓA MÃ
        // ==========================================
        [HttpPost("/" + DiscountRouter.Delete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var discount = await _db.Discounts.FindAsync(id);
            if (discount == null) return NotFound();

            _db.Discounts.Remove(discount);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Discount deleted successfully!";
            return Redirect("/" + DiscountRouter.Index);
        }
    }
}