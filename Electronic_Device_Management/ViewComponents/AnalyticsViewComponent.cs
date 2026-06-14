using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Electronic_Device_Management.Data;

namespace Electronic_Device_Management.ViewComponents
{
    public class AnalyticsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            //   (Count, Sum)
            var totalOrders = await _context.Orders.CountAsync();
            var totalRevenue = await _context.Orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            //   (Average, Min, Max)
           
            var avgOrderValue = totalOrders > 0 ? await _context.Orders.AverageAsync(o => (decimal)o.TotalAmount) : 0;

            // Highest & Lowest Order (MAX & MIN)
            var maxOrderValue = totalOrders > 0 ? await _context.Orders.MaxAsync(o => (decimal?)o.TotalAmount) ?? 0 : 0;
            var minOrderValue = totalOrders > 0 ? await _context.Orders.MinAsync(o => (decimal?)o.TotalAmount) ?? 0 : 0;

            // Conditional Aggregations (Count with filters)
            var lowStockCount = await _context.Products.CountAsync(p => p.AvailableQuantity < 10);
            var activeCustomers = await _context.Customers.CountAsync();

            // Grouped Aggregation (Group By + Sum) - Top 5 Products
            var topProducts = await _context.OrderDetails
                .Include(od => od.Product)
                .GroupBy(od => new { od.ProductId, od.Product.ProductName })
                .Select(g => new
                {
                    ProductName = g.Key.ProductName,
                    TotalQtySold = g.Sum(od => od.OrderQuantity), // SUM
                    RevenueGenerated = g.Sum(od => od.Amount)      // SUM
                })
                .OrderByDescending(p => p.RevenueGenerated)
                .Take(5)
                .ToListAsync();

            // Recent Activity (Take/OrderBy)
            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

          
            var dashboardViewModel = new
            {
                TotalRevenue = totalRevenue,
                AvgOrder = avgOrderValue,
                MaxOrder = maxOrderValue,
                MinOrder = minOrderValue,
                TotalOrders = totalOrders,
                LowStock = lowStockCount,
                UniqueCustomers = activeCustomers,
                TopProducts = topProducts,
                RecentOrders = recentOrders
            };

            return View(dashboardViewModel);
        }
    }
}
