using Microsoft.CodeAnalysis;

namespace Electronic_Device_Management.Models.ViewModels
{
    public class DashboardStatisticsViewModel
    {
        // Overall Statistics
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public int TotalCategories { get; set; }

        // Monthly Statistics
        public int OrdersThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }

        // Today's Statistics
        public int OrdersToday { get; set; }
        public decimal RevenueToday { get; set; }

        // Stock Alerts
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
    }
}

