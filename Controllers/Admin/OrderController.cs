using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tribean.Data;
using Tribean.Models;
using Tribean.Models.Enums;
using Tribean.Routers.Admin;

namespace Tribean.Controllers.Admin
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        public OrderController(ApplicationDbContext db) { _db = db; }

        [HttpGet("/" + OrderRouter.Index)]
        public async Task<IActionResult> Index(string? search, OrderStatus? status, int page = 1)
        {
            const int PageSize = 4;
            var query = _db.Orders.Include(o => o.User).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(o => 
                    o.Id.ToLower().Contains(lowerSearch) ||
                    (o.ReceiverName != null && o.ReceiverName.ToLower().Contains(lowerSearch)) ||
                    (o.ReceiverPhoneNumber != null && o.ReceiverPhoneNumber.Contains(search)));
            }

            if (status.HasValue) query = query.Where(o => o.Status == status.Value);

            ViewData["TotalOrders"] = await _db.Orders.CountAsync();
            ViewData["PendingOrders"] = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
            ViewData["Revenue"] = await _db.Orders.Where(o => o.Status != OrderStatus.Cancelled).SumAsync(o => o.TotalPrice);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var orders = await query.OrderByDescending(o => o.OrderDate).Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

            ViewData["Search"] = search ?? "";
            ViewData["StatusFilter"] = status;
            ViewData["Page"] = page;
            ViewData["TotalPages"] = totalPages;

            return View("~/Views/Admin/Pages/Order/Index.cshtml", orders);
        }

        [HttpGet("/" + OrderRouter.Detail)]
        public async Task<IActionResult> Detail(string id)
        {
            var order = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Size)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View("~/Views/Admin/Pages/Order/Detail.cshtml", order);
        }

        [HttpPost("/" + OrderRouter.UpdateStatus)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string id, OrderStatus newStatus, PaymentStatus newPaymentStatus)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = newStatus;
            order.PaymentStatus = newPaymentStatus;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Order #{id.Substring(0,8).ToUpper()} status updated!";
            return Redirect("/" + OrderRouter.Detail.Replace("{id}", id));
        }

        [HttpPost("/" + OrderRouter.Cancel)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string id)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status == OrderStatus.Delivered)
            {
                TempData["Error"] = "Cannot cancel a delivered order.";
                return Redirect("/" + OrderRouter.Detail.Replace("{id}", id));
            }

            order.Status = OrderStatus.Cancelled;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Order #{id.Substring(0,8).ToUpper()} has been cancelled.";
            return Redirect("/" + OrderRouter.Index);
        }
        // ==========================================
        // [API REAL-TIME] LẤY SỐ LƯỢNG ĐƠN CHỜ DUYỆT
        // ==========================================
        [HttpGet("/Admin/Order/GetPendingCount")]
        public async Task<IActionResult> GetPendingCount()
        {
            var count = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
            return Json(new { count = count });
        }
    }
}