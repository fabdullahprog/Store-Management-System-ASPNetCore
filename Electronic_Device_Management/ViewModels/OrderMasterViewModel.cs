using System.ComponentModel.DataAnnotations;

namespace Electronic_Device_Management.Models.ViewModels
{
    public class OrderMasterViewModel
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Customer Name is required")]
        [Display(Name = "Customer Name")]
        [StringLength(200)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact Number is required")]
        [Display(Name = "Contact Number")]
        [StringLength(20)]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact Address is required")]
        [Display(Name = "Contact Address")]
        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string ContactAddress { get; set; } = string.Empty;

        public int? CustomerId { get; set; }

        [Display(Name = "Order Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        public List<OrderDetailsViewModel> OrderDetailsList { get; set; }

        public OrderMasterViewModel()
        {
            OrderDetailsList = new List<OrderDetailsViewModel>();
            OrderDate = DateTime.Now;
        }
    }
}
