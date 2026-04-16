using Tribean.Models;

namespace Tribean.Models.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; } // Đã chuyển sang decimal
        public double RevenueGrowth { get; set; }
        public int TotalOrders { get; set; }
        public double OrdersGrowth { get; set; }
        public int TotalUsers { get; set; }
        public double UsersGrowth { get; set; }
        public int TotalProducts { get; set; }
        public double ProductsGrowth { get; set; }

        public int DeliveredCount { get; set; }
        public int ShippingCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int PendingCount { get; set; }
        public int CancelledCount { get; set; }

        public List<DailyRevenueDTO> RevenueLast7Days { get; set; } = new();
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<TopProductDTO> TopProducts { get; set; } = new List<TopProductDTO>();
        public List<User> RecentUsers { get; set; } = new();
        public List<TopSpenderDTO> TopSpenders { get; set; } = new();
    }

    public class DailyRevenueDTO
    {
        public string DayName { get; set; } = "";
        public decimal CurrentRevenue { get; set; } // Đã chuyển sang decimal
        public decimal PreviousRevenue { get; set; } // Đã chuyển sang decimal
    }

    public class TopProductDTO
    {
        public Product? Product { get; set; }
        public int TotalSold { get; set; }
    }

    public class TopSpenderDTO
    {
        public User? User { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }
}