using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tribean.Data;
using Tribean.Models.Enums;
using Tribean.Models.ViewModels;
using Tribean.Routers.Admin;

namespace Tribean.Controllers.Admin
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        public DashboardController(ApplicationDbContext db) { _db = db; }

        [HttpGet("/" + DashboardRouter.Index)]
        public async Task<IActionResult> Index()
        {
            var now = DateTime.UtcNow;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            var vm = new DashboardViewModel();

            // 1. Stats Cards
            var currentMonthRev = await _db.Orders.Where(o => o.OrderDate >= thisMonthStart && o.Status == OrderStatus.Delivered).SumAsync(o => o.TotalPrice);
            var lastMonthRev = await _db.Orders.Where(o => o.OrderDate >= lastMonthStart && o.OrderDate < thisMonthStart && o.Status == OrderStatus.Delivered).SumAsync(o => o.TotalPrice);
            vm.TotalRevenue = currentMonthRev;
            vm.RevenueGrowth = CalculateGrowth((double)currentMonthRev, (double)lastMonthRev);

            var currentMonthOrd = await _db.Orders.CountAsync(o => o.OrderDate >= thisMonthStart);
            var lastMonthOrd = await _db.Orders.CountAsync(o => o.OrderDate >= lastMonthStart && o.OrderDate < thisMonthStart);
            vm.TotalOrders = await _db.Orders.CountAsync();
            vm.OrdersGrowth = CalculateGrowth(currentMonthOrd, lastMonthOrd);

            var currentMonthUsr = await _db.Users.CountAsync(u => u.CreatedAt >= thisMonthStart);
            var lastMonthUsr = await _db.Users.CountAsync(u => u.CreatedAt >= lastMonthStart && u.CreatedAt < thisMonthStart);
            vm.TotalUsers = await _db.Users.CountAsync();
            vm.UsersGrowth = CalculateGrowth(currentMonthUsr, lastMonthUsr);

            vm.TotalProducts = await _db.Products.CountAsync();
            vm.ProductsGrowth = CalculateGrowth(
                await _db.Products.CountAsync(p => p.CreatedAt >= thisMonthStart),
                await _db.Products.CountAsync(p => p.CreatedAt >= lastMonthStart && p.CreatedAt < thisMonthStart)
            );

            // 2. Chart Status
            vm.DeliveredCount = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Delivered);
            vm.ShippingCount = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Shipping);
            vm.ConfirmedCount = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Confirmed);
            vm.PendingCount = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
            vm.CancelledCount = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled);

            // 3. Chart Revenue 7 Days
            for (int i = 6; i >= 0; i--)
            {
                var targetDate = now.Date.AddDays(-i);
                var prevWeekDate = targetDate.AddDays(-7);

                vm.RevenueLast7Days.Add(new DailyRevenueDTO {
                    DayName = targetDate.ToString("ddd"),
                    CurrentRevenue = await _db.Orders.Where(o => o.OrderDate.Date == targetDate && o.Status == OrderStatus.Delivered).SumAsync(o => o.TotalPrice),
                    PreviousRevenue = await _db.Orders.Where(o => o.OrderDate.Date == prevWeekDate && o.Status == OrderStatus.Delivered).SumAsync(o => o.TotalPrice)
                });
            }

            // 4. Tables
            // Lấy 3 Đơn hàng gần nhất
            vm.RecentOrders = await _db.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(3)
                .ToListAsync();

            // Lấy 3 Sản phẩm bán chạy nhất (Đã Fix GroupBy EF Core)
            vm.TopProducts = await _db.OrderDetails
                .Include(od => od.Product).ThenInclude(p => p.Category)
                .Where(od => od.Order != null && od.Order.Status == OrderStatus.Delivered && od.Product != null)
                .GroupBy(od => od.Product) // Nhóm theo object Product
                .Select(g => new TopProductDTO { 
                    Product = g.Key,       // Dùng g.Key thay vì g.First()
                    TotalSold = g.Sum(od => od.Quantity) 
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(3)
                .ToListAsync();

            // Lấy 3 Khách hàng mới đăng ký
            vm.RecentUsers = await _db.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(3)
                .ToListAsync();

            // Lấy 3 Khách hàng chi nhiều tiền nhất (Top Spenders - Đã Fix GroupBy EF Core)
            vm.TopSpenders = await _db.Orders
                .Include(o => o.User)
                .Where(o => o.Status == OrderStatus.Delivered && o.User != null)
                .GroupBy(o => o.User)      // Nhóm theo object User
                .Select(g => new TopSpenderDTO {
                    User = g.Key,          // Dùng g.Key thay vì g.First()
                    TotalOrders = g.Count(),
                    TotalSpent = g.Sum(o => o.TotalPrice)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(3)
                .ToListAsync();

            return View("~/Views/Admin/Pages/Dashboard.cshtml", vm);
        }

        private double CalculateGrowth(double current, double previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return Math.Round(((current - previous) / previous) * 100, 1);
        }
    }
}