using Electronic_Device_Management.Data;
using Electronic_Device_Management.Helpers;
using Electronic_Device_Management.Models;
using Electronic_Device_Management.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Electronic_Device_Management.Controllers
{
    [Authorize]
    public class ProductController : BaseController
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment) : base(context)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Product
        public async Task<IActionResult> Index(int? categoryId, string? searchName, bool? isActive)
        {
            var products = _context.Products.Include(p => p.ProductCategory).AsQueryable();

            // Filter by Category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.ProductCategoryId == categoryId.Value);
            }

            // Filter by Product Name
            if (!string.IsNullOrEmpty(searchName))
            {
                products = products.Where(p => p.ProductName.Contains(searchName));
            }

            // Filter by Active status
            if (isActive.HasValue)
            {
                products = products.Where(p => p.IsActive == isActive.Value);
            }

            var productList = await products
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Unit = p.Unit,
                    UnitPrice = p.UnitPrice,
                    AvailableQuantity = p.AvailableQuantity,
                    ProductImage = p.ProductImage,
                    IsActive = p.IsActive,
                    ProductCategoryId = p.ProductCategoryId,
                    CategoryName = p.ProductCategory!.CategoryName,
                })
                .ToListAsync();

            // Send categories to view
            ViewBag.Categories = new SelectList(await _context.ProductCategories.ToListAsync(), "ProductCategoryId", "CategoryName");
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SearchName = searchName;
            ViewBag.IsActive = isActive;

            return View(productList);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var product = await _context.Products
                .Include(p => p.ProductCategory)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // GET: Product/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.ProductCategoryId = new SelectList(await _context.ProductCategories.ToListAsync(), "ProductCategoryId", "CategoryName");
            ViewBag.Units = new SelectList(UnitHelper.GetUnits());
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    ProductName = model.ProductName,
                    Unit = model.Unit,
                    UnitPrice = model.UnitPrice,
                    AvailableQuantity = model.AvailableQuantity,
                    IsActive = model.IsActive,
                    ProductCategoryId = model.ProductCategoryId
                };

                // Handle image upload
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    product.ProductImage = fileName;
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ProductCategoryId = new SelectList(await _context.ProductCategories.ToListAsync(), "ProductCategoryId", "CategoryName", model.ProductCategoryId);
            ViewBag.Units = new SelectList(UnitHelper.GetUnits(), model.Unit);
            return View(model);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var model = new ProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Unit = product.Unit ?? string.Empty,
                UnitPrice = product.UnitPrice,
                AvailableQuantity = product.AvailableQuantity,
                IsActive = product.IsActive,
                ProductImage = product.ProductImage,
                ProductCategoryId = product.ProductCategoryId,
            };

            ViewBag.ProductCategoryId = new SelectList(await _context.ProductCategories.ToListAsync(), "ProductCategoryId", "CategoryName", model.ProductCategoryId);
            ViewBag.Units = new SelectList(UnitHelper.GetUnits(), model.Unit);

            return View(model);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(model.ProductId);
                if (product == null)
                {
                    return NotFound();
                }

                product.ProductName = model.ProductName;
                product.Unit = model.Unit;
                product.UnitPrice = model.UnitPrice;
                product.AvailableQuantity = model.AvailableQuantity;
                product.IsActive = model.IsActive;
                product.ProductCategoryId = model.ProductCategoryId;

                // Handle image upload
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(product.ProductImage))
                    {
                        string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", product.ProductImage);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save new image
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    product.ProductImage = fileName;
                }

                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ProductCategoryId = new SelectList(await _context.ProductCategories.ToListAsync(), "ProductCategoryId", "CategoryName", model.ProductCategoryId);
            ViewBag.Units = new SelectList(UnitHelper.GetUnits(), model.Unit);
            return View(model);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var product = await _context.Products
                .Include(p => p.ProductCategory)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            var model = new ProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Unit = product.Unit ?? string.Empty,
                UnitPrice = product.UnitPrice,
                AvailableQuantity = product.AvailableQuantity,
                IsActive = product.IsActive,
                ProductImage = product.ProductImage,
                ProductCategoryId = product.ProductCategoryId,
                CategoryName = product.ProductCategory?.CategoryName
            };

            return View(model);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                // Delete image if exists
                if (!string.IsNullOrEmpty(product.ProductImage))
                {
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", product.ProductImage);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Cannot delete this product. It may have related orders.";
                return RedirectToAction(nameof(Index));
            }
        }

        // AJAX: Get Products by Category
        [HttpGet]
        public async Task<JsonResult> GetProductsByCategory(int categoryId)
        {
            var products = await _context.Products
                .Where(p => p.ProductCategoryId == categoryId && p.IsActive)
                .Select(p => new
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Unit = p.Unit,
                    UnitPrice = p.UnitPrice,
                    AvailableQuantity = p.AvailableQuantity
                })
                .ToListAsync();

            return Json(products);
        }
    }
}

