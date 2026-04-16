// Controllers/Admin/UserController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; 
using System.IO; 
using System;
using System.Linq;
using System.Threading.Tasks;
using Tribean.Data;
using Tribean.Models;
// Thêm thư viện BCrypt
using BCrypt.Net; 

namespace Tribean.Controllers.Admin
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ==========================================
        // 1. TRANG DANH SÁCH (INDEX)
        // ==========================================
        [HttpGet("/Admin/User")]
        public async Task<IActionResult> Index(string? search, string? role, int page = 1)
        {
            const int PageSize = 4;

            // 1. Tính toán số liệu cho Khối thống kê
            ViewData["TotalUsers"] = await _db.Users.CountAsync();
            ViewData["TotalAdmins"] = await _db.Users.CountAsync(u => u.Role != null && u.Role.RoleName.Contains("Admin"));
            ViewData["TotalClients"] = await _db.Users.CountAsync(u => u.Role == null || !u.Role.RoleName.Contains("Admin"));

            // 2. Khởi tạo Query lấy User kèm Role
            var query = _db.Users.Include(u => u.Role).AsQueryable();

            // 3. Lọc theo tìm kiếm (Tên, Email, SĐT)
            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(u => 
                    (u.FirstName + " " + u.LastName).ToLower().Contains(lowerSearch) ||
                    u.UserName.ToLower().Contains(lowerSearch) ||
                    (u.Email != null && u.Email.ToLower().Contains(lowerSearch)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(search))
                );
            }

            // 4. Lọc theo Quyền (Role)
            if (!string.IsNullOrEmpty(role))
            {
                if (role == "Admin") {
                    query = query.Where(u => u.Role != null && u.Role.RoleName.Contains("Admin"));
                } else if (role == "Client") {
                    query = query.Where(u => u.Role == null || !u.Role.RoleName.Contains("Admin"));
                }
            }

            // 5. Phân trang
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // 6. Trả dữ liệu ra View
            ViewData["Search"] = search ?? "";
            ViewData["RoleFilter"] = role ?? "";
            ViewData["Page"] = page;
            ViewData["TotalPages"] = totalPages;

            return View("~/Views/Admin/Pages/User/Index.cshtml", users);
        }

        // ==========================================
        // 2. TRANG THÊM MỚI (ADD)
        // ==========================================
        [HttpGet("/Admin/User/Create")]
        public async Task<IActionResult> Create()
        {
            // Truyền danh sách Role ra để chọn
            ViewData["Roles"] = await _db.Roles.ToListAsync();
            return View("~/Views/Admin/Pages/User/Add.cshtml", new User());
        }

        [HttpPost("/Admin/User/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User model, string password, IFormFile? mainImage)
        {
            // Ngắt kiểm tra lỗi ảo cho thuộc tính Navigation
            ModelState.Remove("Role");

            // --- SERVER-SIDE VALIDATION ---
            // 1. Kiểm tra trùng Username
            if (await _db.Users.AnyAsync(u => u.UserName == model.UserName))
            {
                ModelState.AddModelError("UserName", "Tên đăng nhập này đã tồn tại!");
            }

            // 2. Kiểm tra trùng Email
            if (!string.IsNullOrEmpty(model.Email) && await _db.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng!");
            }

            // 3. Nếu có lỗi, trả lại View
            if (!ModelState.IsValid)
            {
                ViewData["Roles"] = await _db.Roles.ToListAsync();
                TempData["Error"] = "Vui lòng kiểm tra lại thông tin nhập vào.";
                return View("~/Views/Admin/Pages/User/Add.cshtml", model);
            }
            // ------------------------------

            // --- XỬ LÝ UPLOAD ẢNH AVATAR ---
            if (mainImage != null && mainImage.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(mainImage.FileName);
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatar");
                Directory.CreateDirectory(uploadPath);

                string filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await mainImage.CopyToAsync(stream);
                }

                model.AvatarUrl = "/uploads/avatar/" + fileName;
            }
            // -------------------------------

            // Gán các giá trị mặc định
            model.Id = Guid.NewGuid().ToString();
            model.CreatedAt = DateTime.UtcNow;
            
            // 👇 BĂM MẬT KHẨU Ở ĐÂY TRƯỚC KHI LƯU 👇
            model.Password = BCrypt.Net.BCrypt.HashPassword(password); 

            // Lưu ý: Ngày sinh (BirthDay) đã tự động được Entity Framework map từ Form vào model.BirthDay
            _db.Users.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"User '{model.UserName}' created successfully!";
            return Redirect("/Admin/User");
        }

        // ==========================================
        // 3. TRANG SỬA CHI TIẾT (EDIT)
        // ==========================================
        [HttpGet("/Admin/User/Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            ViewData["Roles"] = await _db.Roles.ToListAsync();
            return View("~/Views/Admin/Pages/User/Edit.cshtml", user);
        }

        [HttpPost("/Admin/User/Update/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string id, User model, string? newPassword, IFormFile? mainImage)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Ngắt kiểm tra lỗi ảo để tránh kẹt form
            ModelState.Remove("Password");
            ModelState.Remove("Role");

            // --- SERVER-SIDE VALIDATION CHO EDIT ---
            // 1. Kiểm tra nếu Admin đổi Username và Username mới đó đã bị người khác lấy
            if (model.UserName != user.UserName && await _db.Users.AnyAsync(u => u.UserName == model.UserName))
            {
                ModelState.AddModelError("UserName", "Tên đăng nhập này đã tồn tại!");
            }

            // 2. Kiểm tra nếu Admin đổi Email và Email mới đó đã bị người khác lấy
            if (model.Email != user.Email && !string.IsNullOrEmpty(model.Email) && await _db.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng!");
            }

            // 3. Nếu có lỗi, trả lại View kèm dữ liệu cũ để tránh mất công nhập lại
            if (!ModelState.IsValid)
            {
                ViewData["Roles"] = await _db.Roles.ToListAsync();
                TempData["Error"] = "Vui lòng kiểm tra lại thông tin nhập vào.";
                model.Id = id; // Giữ lại ID
                model.AvatarUrl = user.AvatarUrl; // Giữ lại ảnh cũ để hiển thị preview
                return View("~/Views/Admin/Pages/User/Edit.cshtml", model);
            }
            // ---------------------------------------

            // --- XỬ LÝ UPLOAD ẢNH AVATAR MỚI (NẾU CÓ) ---
            if (mainImage != null && mainImage.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(mainImage.FileName);
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatar");
                Directory.CreateDirectory(uploadPath);

                string filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await mainImage.CopyToAsync(stream);
                }

                // Cập nhật URL ảnh mới
                user.AvatarUrl = "/uploads/avatar/" + fileName;
            }
            // ---------------------------------------------

            // Cập nhật các thông tin khác
            user.UserName = model.UserName; 
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.RoleId = model.RoleId;
            
            // LƯU NGÀY SINH TỪ FORM VÀO DATABASE
            user.BirthDay = model.BirthDay;

            // 👇 NẾU CÓ ĐỔI MẬT KHẨU THÌ BĂM MẬT KHẨU MỚI 👇
            if (!string.IsNullOrEmpty(newPassword))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword); 
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = $"Profile of '{user.UserName}' updated!";
            return Redirect("/Admin/User");
        }

        // ==========================================
        // 4. TRANG XEM CHI TIẾT (DETAIL)
        // ==========================================
        [HttpGet("/Admin/User/Detail/{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            var user = await _db.Users
                .Include(u => u.Role)
                .Include(u => u.Orders)
                .Include(u => u.Reviews)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            return View("~/Views/Admin/Pages/User/Detail.cshtml", user);
        }

        // ==========================================
        // 5. CHỨC NĂNG XÓA (DELETE)
        // ==========================================
        [HttpPost("/Admin/User/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _db.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            // Ràng buộc an toàn: Không cho xóa user nếu họ đã mua hàng
            if (user.Orders.Any())
            {
                TempData["Error"] = $"Cannot delete {user.UserName}. This user has order history.";
                return Redirect("/Admin/User");
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = "User deleted successfully!";
            return Redirect("/Admin/User");
        }
    }
}