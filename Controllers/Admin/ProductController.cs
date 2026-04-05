// Controllers/Admin/ProductController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Tribean.Data;
using Tribean.Models;
using Tribean.Routers.Admin;

namespace Tribean.Controllers.Admin
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        
        // Đã sửa thành 4 sản phẩm / trang
        private const int PageSize = 4;

        public ProductController(ApplicationDbContext db)
        {
            _db = db;
        }

        // =========================================================
        // 1. GET: Danh sách sản phẩm
        // =========================================================
        [HttpGet(ProductRouter.Index)]
        public async Task<IActionResult> Index(string? search, string? category, string? status, int page = 1)
        {
            var query = _db.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.ProductName.Contains(search)
                                      || (p.Description != null && p.Description.Contains(search)));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(p => p.CategoryId == category);

            if (!string.IsNullOrEmpty(status) && bool.TryParse(status, out var statusBool))
                query = query.Where(p => p.Status == statusBool);

            // 1. Lấy toàn bộ danh sách (đã qua bộ lọc) cho Grid View
            var allProducts = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var total      = allProducts.Count;
            var totalPages = (int)Math.Ceiling(total / (double)PageSize);
            page           = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            // 2. Lấy 5 sản phẩm cho Table View
            var pagedProducts = allProducts
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewData["Categories"]     = await _db.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewData["Search"]         = search ?? "";
            ViewData["CategoryFilter"] = category ?? "";
            ViewData["StatusFilter"]   = status ?? "";
            ViewData["Page"]           = page;
            ViewData["TotalPages"]     = totalPages;
            ViewData["Title"]          = "Products";
            ViewData["PageTitle"]      = "Products";
            ViewData["Breadcrumb"]     = "Catalog / Products";
            
            // Truyền danh sách FULL sang View để Grid sử dụng
            ViewData["AllProducts"]    = allProducts; 

            // Trả về danh sách PHÂN TRANG cho Model chính
            return View("~/Views/Admin/Pages/Product/Index.cshtml", pagedProducts);
        }

        // =========================================================
        // 2. GET: Hiển thị Form Thêm Sản Phẩm
        // =========================================================
        [HttpGet(ProductRouter.Create)]
        public async Task<IActionResult> Create()
        {
            ViewData["Categories"] = await _db.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewData["Title"]      = "Add Product";
            ViewData["PageTitle"]  = "Add Product";
            ViewData["Breadcrumb"] = "Catalog / Products / Add";
            
            return View("~/Views/Admin/Pages/Product/Add.cshtml", new Product());
        }

        // =========================================================
        // 3. POST: Xử lý lưu Sản Phẩm mới vào Database
        // =========================================================
        [HttpPost(ProductRouter.Create)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model, IFormFile? mainImage)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Categories"] = await _db.Categories.OrderBy(c => c.CategoryName).ToListAsync();
                return View("~/Views/Admin/Pages/Product/Add.cshtml", model);
            }

            try
            {
                if (mainImage != null && mainImage.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(mainImage.FileName)}";
                    var folder   = Path.Combine("wwwroot", "uploads", "products");
                    Directory.CreateDirectory(folder);
                    var savePath = Path.Combine(folder, fileName);
                    await using var stream = System.IO.File.Create(savePath);
                    await mainImage.CopyToAsync(stream);
                    model.MainImgUrl = $"/uploads/products/{fileName}";
                }

                model.CreatedAt = DateTime.UtcNow;

                _db.Products.Add(model);
                await _db.SaveChangesAsync();

                TempData["Success"] = $"Product '{model.ProductName}' created successfully!";
                return Redirect("/" + ProductRouter.Index);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Không thể lưu vào Database. Vui lòng kiểm tra Terminal.");
                ViewData["Categories"] = await _db.Categories.OrderBy(c => c.CategoryName).ToListAsync();
                return View("~/Views/Admin/Pages/Product/Add.cshtml", model);
            }
        }

        // =========================================================
        // 4. GET: Hiển thị Form Sửa Sản Phẩm
        // =========================================================
        [HttpGet(ProductRouter.Edit)]
        public async Task<IActionResult> Edit(string id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            ViewData["Categories"] = await _db.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewData["Title"]      = "Edit Product";
            ViewData["PageTitle"]  = "Edit Product";
            ViewData["Breadcrumb"] = "Catalog / Products / Edit";
            
            return View("~/Views/Admin/Pages/Product/Edit.cshtml", product);
        }

        // =========================================================
        // 5. POST: Xử lý cập nhật Sản Phẩm
        // =========================================================
        [HttpPost(ProductRouter.Update)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string id, Product model, IFormFile? mainImage)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["Categories"] = await _db.Categories.OrderBy(c => c.CategoryName).ToListAsync();
                return View("~/Views/Admin/Pages/Product/Edit.cshtml", model);
            }

            try
            {
                if (mainImage != null && mainImage.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(mainImage.FileName)}";
                    var folder   = Path.Combine("wwwroot", "uploads", "products");
                    Directory.CreateDirectory(folder);
                    var savePath = Path.Combine(folder, fileName);
                    await using var stream = System.IO.File.Create(savePath);
                    await mainImage.CopyToAsync(stream);
                    product.MainImgUrl = $"/uploads/products/{fileName}";
                }

                product.ProductName = model.ProductName;
                product.Description = model.Description;
                product.Price       = model.Price;
                product.Discount    = model.Discount; // Đã thêm Discount
                product.CategoryId  = model.CategoryId;
                product.Status      = model.Status;

                await _db.SaveChangesAsync();

                TempData["Success"] = $"Product '{product.ProductName}' updated!";
                return Redirect("/" + ProductRouter.Index);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Lỗi cập nhật Database. Vui lòng thử lại.");
                ViewData["Categories"] = await _db.Categories.OrderBy(c => c.CategoryName).ToListAsync();
                return View("~/Views/Admin/Pages/Product/Edit.cshtml", model);
            }
        }

        // =========================================================
        // 6. POST: Xóa Sản Phẩm
        // =========================================================
        [HttpPost(ProductRouter.Delete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            try
            {
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Product deleted successfully!";
            }
            catch (Exception)
            {
                TempData["Error"] = "Không thể xóa sản phẩm lúc này.";
            }
            
            return Redirect("/" + ProductRouter.Index);
        }

        // =========================================================
        // 7. GET: Xem chi tiết Sản Phẩm
        // =========================================================
        [HttpGet(ProductRouter.Detail)]
        public async Task<IActionResult> Detail(string id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            ViewData["Title"]      = product.ProductName;
            ViewData["PageTitle"]  = product.ProductName;
            ViewData["Breadcrumb"] = "Catalog / Products / Detail";
            
            return View("~/Views/Admin/Pages/Product/Detail.cshtml", product);
        }

        // =========================================================
        // 8. POST: Bật/Tắt trạng thái sản phẩm (AJAX)
        // =========================================================
        [HttpPost("/Admin/Product/ToggleStatus/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) 
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
            }

            // Đảo ngược trạng thái hiện tại
            product.Status = !product.Status; 
            await _db.SaveChangesAsync();

            // Trả về kết quả cho Javascript xử lý
            return Json(new { success = true, newStatus = product.Status });
        }
    }
}