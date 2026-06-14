using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Electronic_Device_Management.Models;

public partial class AspNetUser : IdentityUser
{
    // Extra navigation properties if needed
    public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; } = new List<AspNetUserClaim>();
    public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; } = new List<AspNetUserLogin>();
    public virtual ICollection<AspNetUserToken> AspNetUserTokens { get; set; } = new List<AspNetUserToken>();
    public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();
}

