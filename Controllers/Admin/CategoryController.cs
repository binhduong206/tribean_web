// Controllers/Admin/CategoryController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tribean.Data;
using Tribean.Models;
using Tribean.Routers.Admin;

namespace Tribean.Controllers.Admin
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /Admin/Category
        [HttpGet(CategoryRouter.Index)]
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            const int PageSize = 4; // Số danh mục trên mỗi trang

            // 1. Lấy tổng số sản phẩm toàn hệ thống để tính % Share
            var totalAllProducts = await _db.Products.CountAsync();
            ViewData["TotalProducts"] = totalAllProducts > 0 ? totalAllProducts : 1;

            // 2. Khởi tạo Query
            var query = _db.Categories.Include(c => c.Products).AsQueryable();

            // 3. Tìm kiếm theo tên danh mục
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.CategoryName.Contains(search));
            }

            // 4. Xử lý phân trang
            var totalCategories = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCategories / (double)PageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var categories = await query
                .OrderBy(c => c.CategoryName)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // 5. Truyền dữ liệu ra View
            ViewData["Search"] = search ?? "";
            ViewData["Page"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["Title"] = "Categories";

            return View("~/Views/Admin/Pages/Category/Index.cshtml", categories);
        }

        // GET /Admin/Category/Create (Bổ sung để hiển thị Form Thêm mới)
        [HttpGet(CategoryRouter.Create)]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Category";
            ViewData["PageTitle"] = "Add Category";
            ViewData["Breadcrumb"] = "Catalog / Categories / Add";

            return View("~/Views/Admin/Pages/Category/Add.cshtml", new Category());
        }

        // POST /Admin/Category/Create
        [HttpPost(CategoryRouter.Create)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid)
            {
                // Nếu lỗi, trả lại trang Add kèm thông báo
                return View("~/Views/Admin/Pages/Category/Add.cshtml", model);
            }

            _db.Categories.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Category '{model.CategoryName}' created!";
            return Redirect("/" + CategoryRouter.Index);
        }

        // GET /Admin/Category/Edit/{id}
        [HttpGet(CategoryRouter.Edit)]
        public async Task<IActionResult> Edit(string id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();

            ViewData["Title"] = "Edit Category";
            ViewData["PageTitle"] = "Edit Category";
            ViewData["Breadcrumb"] = "Catalog / Categories / Edit";

            return View("~/Views/Admin/Pages/Category/Edit.cshtml", category);
        }

        // POST /Admin/Category/Update/{id}
        [HttpPost(CategoryRouter.Update)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string id, Category model)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Pages/Category/Edit.cshtml", model);
            }

            category.CategoryName = model.CategoryName;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Category '{model.CategoryName}' updated!";
            return Redirect("/" + CategoryRouter.Index);
        }

        // POST /Admin/Category/Delete/{id}
        [HttpPost(CategoryRouter.Delete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var category = await _db.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            if (category.Products.Any())
            {
                TempData["Error"] = $"Cannot delete '{category.CategoryName}' — it still has {category.Products.Count} product(s).";
                return Redirect("/" + CategoryRouter.Index);
            }

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Category deleted!";
            return Redirect("/" + CategoryRouter.Index);
        }
    }
}