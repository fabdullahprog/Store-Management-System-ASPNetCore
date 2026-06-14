using System.ComponentModel.DataAnnotations;

namespace Electronic_Device_Management.Models.ViewModels
{
    public class ProductCategoryViewModel
    {
        public int ProductCategoryId { get; set; }

        [Required(ErrorMessage = "Category Name is required")]
        [Display(Name = "Category Name")]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Category Description")]
        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string? CategoryDescription { get; set; }
    }
}
