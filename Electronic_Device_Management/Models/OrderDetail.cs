using System;
using System.Collections.Generic;

namespace Electronic_Device_Management.Models;

public partial class OrderDetail
{
    public int OrderDetailId { get; set; }

    public int? OrderId { get; set; }

    public int? ProductCategoryId { get; set; }

    public int? ProductId { get; set; }

    public int OrderQuantity { get; set; }

    public string? OrderUnit { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Amount { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Product? Product { get; set; }

    public virtual ProductCategory? ProductCategory { get; set; }
}

