using System;
using System.Collections.Generic;

namespace Electronic_Device_Management.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Unit { get; set; }

    public decimal UnitPrice { get; set; }

    public int AvailableQuantity { get; set; }

    public string? ProductImage { get; set; }

    public int? ProductCategoryId { get; set; }
    public bool IsActive { get; set; }


    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ProductCategory? ProductCategory { get; set; }
}

