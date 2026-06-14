using Electronic_Device_Management.Data;
using Electronic_Device_Management.Models;
using Electronic_Device_Management.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Electronic_Device_Management.Controllers
{
    [Authorize]
    public class ProductCategoryController : BaseController
    {
        public ProductCategoryController(ApplicationDbContext context) : base(context)
        {
        }

        // GET: ProductCategory
        public async Task<IActionResult> Index()
        {
            var categories = await _context.ProductCategories
                .Select(c => new ProductCategoryViewModel
                {
                    ProductCategoryId = c.ProductCategoryId,
                    CategoryName = c.CategoryName,
                    CategoryDescription = c.CategoryDescription
                })
                .ToListAsync();

            return View(categories);
        }

        // GET: ProductCategory/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var productCategory = await _context.ProductCategories.FindAsync(id);
            if (productCategory == null)
            {
                return NotFound();
            }
            return View(productCategory);
        }

        // GET: ProductCategory/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ProductCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = new ProductCategory
                {
                    CategoryName = model.CategoryName,
                    CategoryDescription = model.CategoryDescription
                };

                _context.ProductCategories.Add(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Product Category created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: ProductCategory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var category = await _context.ProductCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var model = new ProductCategoryViewModel
            {
                ProductCategoryId = category.ProductCategoryId,
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription
            };

            return View(model);
        }

        // POST: ProductCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = await _context.ProductCategories.FindAsync(model.ProductCategoryId);
                if (category == null)
                {
                    return NotFound();
                }

                category.CategoryName = model.CategoryName;
                category.CategoryDescription = model.CategoryDescription;

                _context.Entry(category).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Product Category updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: ProductCategory/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var category = await _context.ProductCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var model = new ProductCategoryViewModel
            {
                ProductCategoryId = category.ProductCategoryId,
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription
            };

            return View(model);
        }

        // POST: ProductCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var category = await _context.ProductCategories.FindAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                _context.ProductCategories.Remove(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Product Category deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Cannot delete this category. It may have related products.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

