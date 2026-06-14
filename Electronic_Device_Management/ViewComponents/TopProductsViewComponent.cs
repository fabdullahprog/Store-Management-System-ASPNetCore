using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Electronic_Device_Management.Data;

namespace Electronic_Device_Management.ViewComponents
{
    public class TopProductsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public TopProductsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 5)
        {
            var topProducts = await _context.OrderDetails
                .GroupBy(od => new { od.ProductId, od.Product!.ProductName, od.Product.ProductImage })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    ProductImage = g.Key.ProductImage,
                    TotalQuantity = g.Sum(od => od.OrderQuantity),
                    TotalSales = g.Sum(od => od.Amount)
                })
                .OrderByDescending(p => p.TotalSales)
                .Take(count)
                .ToListAsync();

            return View(topProducts);
        }
    }
}

