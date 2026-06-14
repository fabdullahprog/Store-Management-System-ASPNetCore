using System.ComponentModel.DataAnnotations;

namespace Electronic_Device_Management.Models.ViewModels
{
    public class OrderDetailsViewModel
    {
        public int OrderDetailId { get; set; }
        public int? OrderId { get; set; }

        [Required(ErrorMessage = "Product Category is required")]
        [Display(Name = "Product Category")]
        public int? ProductCategoryId { get; set; }

        [Display(Name = "Category Name")]
        public string? CategoryName { get; set; }

        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int? ProductId { get; set; }

        [Display(Name = "Product Name")]
        public string? ProductName { get; set; }

        [Required(ErrorMessage = "Order Quantity is required")]
        [Display(Name = "Order Quantity")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int OrderQuantity { get; set; }

        [Required(ErrorMessage = "Order Unit is required")]
        [Display(Name = "Order Unit")]
        [StringLength(50)]
        public string OrderUnit { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unit Price is required")]
        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit Price must be greater than 0")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        public int AvailableQuantity { get; set; }
    }
}
