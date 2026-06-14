using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Note: Ensure these namespaces match your actual project structure
using Electronic_Device_Management.Data;
using Electronic_Device_Management.Models;
using Electronic_Device_Management.Models.ViewModels;

namespace Electronic_Device_Management.Controllers
{
    [Authorize]
    public class OrderController : Controller // Or BaseController if you have a custom base class
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Order
        public async Task<IActionResult> Index()
        {
            // Keeping Linq for simple Read queries is standard
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderMasterViewModel
                {
                    OrderId = o.OrderId,
                    CustomerName = o.Customer!.CustomerName,
                    ContactNumber = o.Customer.ContactNumber,
                    ContactAddress = o.Customer.ContactAddress,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    CustomerId = o.CustomerId
                }).ToListAsync();

            return View(orders);
        }

        // GET: Order/Details/5 (Added to handle "View Details")
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.ProductCategory)
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null) return NotFound();

            var model = new OrderMasterViewModel
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer!.CustomerName,
                ContactNumber = order.Customer.ContactNumber,
                ContactAddress = order.Customer.ContactAddress,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderDetailsList = order.OrderDetails.Select(d => new OrderDetailsViewModel
                {
                    ProductCategoryId = d.ProductCategoryId,
                    CategoryName = d.ProductCategory?.CategoryName,
                    ProductId = d.ProductId,
                    ProductName = d.Product?.ProductName,
                    OrderQuantity = d.OrderQuantity,
                    OrderUnit = d.OrderUnit,
                    UnitPrice = d.UnitPrice,
                    Amount = d.Amount
                }).ToList()
            };

            return View(model);
        }

        // GET: Order/Create 
        public async Task<IActionResult> Create()
        {
            ViewBag.ProductCategories = new SelectList(await _context.ProductCategories.ToListAsync(), "ProductCategoryId", "CategoryName");
            return View(new OrderMasterViewModel { OrderDate = DateTime.Now });
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderMasterViewModel model, string? OrderDetailsListJson)
        {
            if (!string.IsNullOrEmpty(OrderDetailsListJson))
                model.OrderDetailsList = JsonSerializer.Deserialize<List<OrderDetailsViewModel>>(OrderDetailsListJson);

            if (ModelState.IsValid && model.OrderDetailsList?.Any() == true)
            {
                try
                {
                    // 1. Convert List to DataTable
                    DataTable dtDetails = CreateDetailsDataTable(model.OrderDetailsList);

                    // 2. Set up SQL Parameters
                    var parameters = new[] {
                        new SqlParameter("@CustomerName", model.CustomerName ?? (object)DBNull.Value),
                        new SqlParameter("@ContactNumber", model.ContactNumber ?? (object)DBNull.Value),
                        new SqlParameter("@ContactAddress", model.ContactAddress ?? (object)DBNull.Value),
                        new SqlParameter("@OrderDetails", SqlDbType.Structured) {
                            TypeName = "dbo.OrderDetailType",
                            Value = dtDetails
                        }
                    };

                    // 3. Execute Stored Procedure
                    await _context.Database.ExecuteSqlRawAsync("EXEC sp_InsertOrder @CustomerName, @ContactNumber, @ContactAddress, @OrderDetails", parameters);

                    TempData["SuccessMessage"] = "Order created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Database Error: " + ex.Message);
                }
            }

            // Reload categories if returning to the view after an error
            ViewBag.ProductCategories = new SelectList(await _context.ProductCategories.ToListAsync(), "ProductCategoryId", "CategoryName");
            return View(model);
        }

        // GET: Order/Edit/5 
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.ProductCategory)
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null) return NotFound();

            var model = new OrderMasterViewModel
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer!.CustomerName,
                ContactNumber = order.Customer.ContactNumber,
                ContactAddress = order.Customer.ContactAddress,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderDetailsList = order.OrderDetails.Select(d => new OrderDetailsViewModel
                {
                    ProductCategoryId = d.ProductCategoryId,
                    CategoryName = d.ProductCategory?.CategoryName,
                    ProductId = d.ProductId,
                    ProductName = d.Product?.ProductName,
                    OrderQuantity = d.OrderQuantity,
                    OrderUnit = d.OrderUnit,
                    UnitPrice = d.UnitPrice,
                    Amount = d.Amount
                }).ToList()
            };

            ViewBag.ProductCategories = new SelectList(await _context.ProductCategories.ToListAsync(), "ProductCategoryId", "CategoryName");
            return View(model);
        }

        // POST: Order/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrderMasterViewModel model, string? OrderDetailsListJson)
        {
            if (!string.IsNullOrEmpty(OrderDetailsListJson))
                model.OrderDetailsList = JsonSerializer.Deserialize<List<OrderDetailsViewModel>>(OrderDetailsListJson);

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Convert List to DataTable
                    DataTable dtDetails = CreateDetailsDataTable(model.OrderDetailsList!);

                    // 2. Set up SQL Parameters
                    var parameters = new[] {
                        new SqlParameter("@OrderId", model.OrderId),
                        new SqlParameter("@CustomerId", model.CustomerId),
                        new SqlParameter("@CustomerName", model.CustomerName ?? (object)DBNull.Value),
                        new SqlParameter("@ContactNumber", model.ContactNumber ?? (object)DBNull.Value),
                        new SqlParameter("@ContactAddress", model.ContactAddress ?? (object)DBNull.Value),
                        new SqlParameter("@OrderDate", model.OrderDate),
                        new SqlParameter("@OrderDetails", SqlDbType.Structured) {
                            TypeName = "dbo.OrderDetailType",
                            Value = dtDetails
                        }
                    };

                    // 3. Execute Stored Procedure
                    await _context.Database.ExecuteSqlRawAsync("EXEC sp_UpdateOrder @OrderId, @CustomerId, @CustomerName, @ContactNumber, @ContactAddress, @OrderDate, @OrderDetails", parameters);

                    TempData["SuccessMessage"] = "Order updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Database Error: " + ex.Message);
                }
            }
            return View(model);
        }

        // GET: Order/Delete/5 (Added to load the Delete confirmation page)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null) return NotFound();

            var model = new OrderMasterViewModel
            {
                OrderId = order.OrderId,
                CustomerName = order.Customer!.CustomerName,
                ContactNumber = order.Customer.ContactNumber,
                ContactAddress = order.Customer.ContactAddress,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount
            };

            return View(model);
        }

        // POST: Order/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var param = new SqlParameter("@OrderId", id);
                await _context.Database.ExecuteSqlRawAsync("EXEC sp_DeleteOrder @OrderId", param);
                TempData["SuccessMessage"] = "Order deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Database Error: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: API to fetch products for the AJAX dropdown in the View
        [HttpGet]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            var products = await _context.Products
                .Where(p => p.ProductCategoryId == categoryId)
                .Select(p => new
                {
                    productId = p.ProductId,
                    productName = p.ProductName,
                    unit = p.Unit,
                    unitPrice = p.UnitPrice,
                    availableQuantity = p.AvailableQuantity
                }).ToListAsync();

            return Json(products);
        }

        // Helper method to map the ViewModel list to the SQL Server Table Type
        private DataTable CreateDetailsDataTable(List<OrderDetailsViewModel> details)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ProductCategoryId", typeof(int));
            dt.Columns.Add("ProductId", typeof(int));
            dt.Columns.Add("OrderQuantity", typeof(int));
            dt.Columns.Add("OrderUnit", typeof(string));
            dt.Columns.Add("UnitPrice", typeof(decimal));
            dt.Columns.Add("Amount", typeof(decimal));

            foreach (var item in details)
            {
                dt.Rows.Add(
                    item.ProductCategoryId,
                    item.ProductId,
                    item.OrderQuantity,
                    item.OrderUnit ?? (object)DBNull.Value,
                    item.UnitPrice,
                    item.Amount
                );
            }
            return dt;
        }
    }
}