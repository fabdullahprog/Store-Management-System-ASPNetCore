using System;
using System.Collections.Generic;

namespace Electronic_Device_Management.Models;

public partial class RolePermission
{
    public int Id { get; set; }

    public string RoleId { get; set; } = null!;

    public string ControllerName { get; set; } = null!;

    public string ActionName { get; set; } = null!;

    public virtual AspNetRole Role { get; set; } = null!;
}

