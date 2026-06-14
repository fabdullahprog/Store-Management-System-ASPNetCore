using Electronic_Device_Management.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Electronic_Device_Management.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndPermissionsAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AspNetRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AspNetUser>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure database exists and migrations applied (safe)
            await db.Database.MigrateAsync();

            // 1) Ensure Roles exist (NO duplicates)
            var adminRole = await roleManager.FindByNameAsync("Admin");
            if (adminRole == null)
            {
                adminRole = new AspNetRole { Name = "Admin", NormalizedName = "ADMIN" };
                await roleManager.CreateAsync(adminRole);
            }

            var customerRole = await roleManager.FindByNameAsync("Customer");
            if (customerRole == null)
            {
                customerRole = new AspNetRole { Name = "Customer", NormalizedName = "CUSTOMER" };
                await roleManager.CreateAsync(customerRole);
            }

            // 2) Ensure default admin user exists
            var adminEmail = configuration["AdminCredentials:Email"] ?? "admin@gmail.com";
            var adminPassword = configuration["AdminCredentials:Password"] ?? "Admin@123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new AspNetUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (!createResult.Succeeded)
                {
                    throw new Exception("Failed to create default admin user: " + string.Join("; ", createResult.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (!roleResult.Succeeded)
                {
                    throw new Exception("Failed to assign Admin role to default user: " + string.Join("; ", roleResult.Errors.Select(e => e.Description)));
                }
            }

            // 3) Seed RolePermissions (insert only missing)
            void AddPermissionIfMissing(string roleId, string controller, string action)
            {
                bool exists = db.RolePermissions.Any(p =>
                    p.RoleId == roleId &&
                    p.ControllerName == controller &&
                    p.ActionName == action);

                if (!exists)
                {
                    db.RolePermissions.Add(new RolePermission
                    {
                        RoleId = roleId,
                        ControllerName = controller,
                        ActionName = action
                    });
                }
            }

            // Admin permissions
            AddPermissionIfMissing(adminRole.Id, "Products", "Index");
            AddPermissionIfMissing(adminRole.Id, "Products", "Create");
            AddPermissionIfMissing(adminRole.Id, "Products", "Edit");
            AddPermissionIfMissing(adminRole.Id, "Products", "Delete");
            AddPermissionIfMissing(adminRole.Id, "Orders", "Index");

            // Customer permissions
            AddPermissionIfMissing(customerRole.Id, "Products", "Index");
            AddPermissionIfMissing(customerRole.Id, "Orders", "Create");
            AddPermissionIfMissing(customerRole.Id, "Orders", "MyOrders");

            await db.SaveChangesAsync();
        }
    }
}