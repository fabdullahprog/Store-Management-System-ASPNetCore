using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Electronic_Device_Management.Data;

namespace Electronic_Device_Management.ViewComponents
{
    public class RecentOrdersViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public RecentOrdersViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 5)
        {
            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();

            return View(recentOrders);
        }
    }
}

