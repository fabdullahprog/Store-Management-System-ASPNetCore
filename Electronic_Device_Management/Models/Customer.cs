using System;
using System.Collections.Generic;

namespace Electronic_Device_Management.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string? ContactNumber { get; set; }

    public string? ContactAddress { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

