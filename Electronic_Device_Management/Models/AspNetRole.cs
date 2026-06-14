using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Electronic_Device_Management.Models;

public partial class AspNetRole : IdentityRole
{
    public virtual ICollection<AspNetRoleClaim> AspNetRoleClaims { get; set; } = new List<AspNetRoleClaim>();
    public virtual ICollection<AspNetUser> Users { get; set; } = new List<AspNetUser>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

