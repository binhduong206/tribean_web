// Controllers/Admin/RoleController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tribean.Data;
using Tribean.Models;

namespace Tribean.Controllers.Admin
{
    // Tạm thời định nghĩa Route trực tiếp ở đây. 
    // Nếu bạn có file RoleRouter, hãy thay thế bằng các hằng số tương ứng nhé.
    [Route("Admin/Role")]
    public class RoleController : Controller
    {
        private readonly ApplicationDbContext _db;
        private const int PageSize = 5;

        public RoleController(ApplicationDbContext db)
        {
            _db = db;
        }

        // =========================================================
        // 1. GET: Danh sách Roles
        // =========================================================
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var query = _db.Roles.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.RoleName.Contains(search));
            }

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)PageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var roles = await query
                .OrderBy(r => r.RoleName)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewData["Search"] = search ?? "";
            ViewData["Page"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["Title"] = "Roles";
            ViewData["PageTitle"] = "Roles";
            ViewData["Breadcrumb"] = "People / Roles";

            return View("~/Views/Admin/Pages/Role/Index.cshtml", roles);
        }

        // =========================================================
        // 2. GET: Hiển thị Form Thêm Role
        // =========================================================
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Role";
            ViewData["PageTitle"] = "Add Role";
            ViewData["Breadcrumb"] = "People / Roles / Add";

            return View("~/Views/Admin/Pages/Role/Add.cshtml", new Role());
        }

        // =========================================================
        // 3. POST: Xử lý lưu Role mới
        // =========================================================
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Pages/Role/Add.cshtml", model);
            }

            // Kiểm tra trùng tên Role
            var exists = await _db.Roles.AnyAsync(r => r.RoleName.ToLower() == model.RoleName.ToLower());
            if (exists)
            {
                ModelState.AddModelError("RoleName", "This role name already exists.");
                return View("~/Views/Admin/Pages/Role/Add.cshtml", model);
            }

            try
            {
                _db.Roles.Add(model);
                await _db.SaveChangesAsync();

                TempData["Success"] = $"Role '{model.RoleName}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Error saving to database.");
                return View("~/Views/Admin/Pages/Role/Add.cshtml", model);
            }
        }

        // =========================================================
        // 4. GET: Hiển thị Form Sửa Role
        // =========================================================
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null) return NotFound();

            ViewData["Title"] = "Edit Role";
            ViewData["PageTitle"] = "Edit Role";
            ViewData["Breadcrumb"] = "People / Roles / Edit";

            return View("~/Views/Admin/Pages/Role/Edit.cshtml", role);
        }

        // =========================================================
        // 5. POST: Xử lý cập nhật Role
        // =========================================================
        [HttpPost("Update/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string id, Role model)
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Pages/Role/Edit.cshtml", model);
            }

            // Kiểm tra trùng tên Role (trừ Role hiện tại)
            var exists = await _db.Roles.AnyAsync(r => r.RoleName.ToLower() == model.RoleName.ToLower() && r.Id != id);
            if (exists)
            {
                ModelState.AddModelError("RoleName", "This role name already exists.");
                return View("~/Views/Admin/Pages/Role/Edit.cshtml", model);
            }

            try
            {
                role.RoleName = model.RoleName;
                await _db.SaveChangesAsync();

                TempData["Success"] = $"Role '{role.RoleName}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Error updating database.");
                return View("~/Views/Admin/Pages/Role/Edit.cshtml", model);
            }
        }

        // =========================================================
        // 6. POST: Xóa Role
        // =========================================================
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _db.Roles.Include(r => r.Users).FirstOrDefaultAsync(r => r.Id == id);
            if (role == null) return NotFound();

            // Ràng buộc: Không cho xóa Role nếu đang có User sử dụng
            if (role.Users.Any())
            {
                TempData["Error"] = $"Cannot delete role '{role.RoleName}' because it is assigned to {role.Users.Count} user(s).";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _db.Roles.Remove(role);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Role deleted successfully!";
            }
            catch (Exception)
            {
                TempData["Error"] = "Could not delete the role at this time.";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}