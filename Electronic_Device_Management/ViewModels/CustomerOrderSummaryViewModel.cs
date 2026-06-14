namespace Electronic_Device_Management.Models.ViewModels
{
    public class CustomerOrderSummaryViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? ContactNumber { get; set; }
        public string? ContactAddress { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastOrderDate { get; set; }

        // Orders list
        public List<OrderSummaryVM> Orders { get; set; } = new();

        // Order details
        public List<OrderDetailSummaryVM> OrderDetails { get; set; } = new();
    }

    public class OrderSummaryVM
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int NumberOfItems { get; set; }
    }

    public class OrderDetailSummaryVM
    {
        public int OrderId { get; set; }
        public int OrderDetailId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int OrderQuantity { get; set; }
        public string? OrderUnit { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public int CurrentStock { get; set; }
    }
}
