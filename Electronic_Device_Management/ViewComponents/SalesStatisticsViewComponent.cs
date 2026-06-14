using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Electronic_Device_Management.Data;

namespace Electronic_Device_Management.ViewComponents
{
    public class SalesStatisticsViewComponent: ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public SalesStatisticsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            var statistics = new
            {
                TotalOrders = await _context.Orders.CountAsync(),
                TodayOrders = await _context.Orders.CountAsync(o => o.OrderDate.Date == today),
                TotalSales = await _context.Orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
                MonthSales = await _context.Orders
                    .Where(o => o.OrderDate >= thisMonth)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
                TotalProducts = await _context.Products.CountAsync(),
                ActiveProducts = await _context.Products.CountAsync(p => p.IsActive),
                TotalCustomers = await _context.Customers.CountAsync(),
                LowStockProducts = await _context.Products.CountAsync(p => p.AvailableQuantity < 10)
            };

            return View(statistics);
        }
    }
}

