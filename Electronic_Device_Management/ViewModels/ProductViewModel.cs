using System.ComponentModel.DataAnnotations;

namespace Electronic_Device_Management.Models.ViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [Display(Name = "Product Name")]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unit is required")]
        [Display(Name = "Unit")]
        [StringLength(50)]
        public string Unit { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unit Price is required")]
        [Display(Name = "Unit Price")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit Price must be greater than 0")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Available Quantity is required")]
        [Display(Name = "Available Quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Available Quantity cannot be negative")]
        public int AvailableQuantity { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Product Image")]
        public string? ProductImage { get; set; }

        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Product Category is required")]
        [Display(Name = "Product Category")]
        public int? ProductCategoryId { get; set; }

        [Display(Name = "Category Name")]
        public string? CategoryName { get; set; }
    }
}
