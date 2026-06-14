using Electronic_Device_Management.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Electronic_Device_Management.Data
{
    public partial class ApplicationDbContext
        : IdentityDbContext<AspNetUser, AspNetRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Identity
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

        // Application Tables
        public virtual DbSet<RolePermission> RolePermissions { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductCategory> ProductCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // =============================
            // Identity configuration
            // =============================
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AspNetUserLogin>()
                .HasKey(x => new { x.LoginProvider, x.ProviderKey });

            modelBuilder.Entity<AspNetUserToken>()
                .HasKey(x => new { x.UserId, x.LoginProvider, x.Name });

            modelBuilder.Entity<IdentityUserRole<string>>()
                .HasKey(x => new { x.UserId, x.RoleId });

            modelBuilder.Entity<AspNetUser>()
                .HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity<IdentityUserRole<string>>(
                    j => j.HasOne<AspNetRole>().WithMany().HasForeignKey(x => x.RoleId),
                    j => j.HasOne<AspNetUser>().WithMany().HasForeignKey(x => x.UserId),
                    j =>
                    {
                        j.ToTable("AspNetUserRoles");
                        j.HasKey(x => new { x.UserId, x.RoleId });
                    });

            // =============================
            // RolePermissions
            // =============================
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.Property(e => e.ControllerName).HasMaxLength(100);
                entity.Property(e => e.ActionName).HasMaxLength(100);
                entity.Property(e => e.RoleId).HasMaxLength(450);

                entity.HasOne(d => d.Role)
                      .WithMany(p => p.RolePermissions)
                      .HasForeignKey(d => d.RoleId);
            });

            // =============================
            // Customers
            // =============================
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);
            });

            // =============================
            // Orders
            // =============================
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.OrderDate).HasColumnType("datetime");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");

                entity.HasOne(d => d.Customer)
                      .WithMany(p => p.Orders)
                      .HasForeignKey(d => d.CustomerId);
            });

            // =============================
            // Order Details
            // =============================
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.OrderDetailId);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");

                entity.HasOne(d => d.Order)
                      .WithMany(p => p.OrderDetails)
                      .HasForeignKey(d => d.OrderId);

                entity.HasOne(d => d.Product)
                      .WithMany(p => p.OrderDetails)
                      .HasForeignKey(d => d.ProductId);

                entity.HasOne(d => d.ProductCategory)
                      .WithMany(p => p.OrderDetails)
                      .HasForeignKey(d => d.ProductCategoryId);
            });

            // =============================
            // Products
            // =============================
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(d => d.ProductCategory)
                      .WithMany(p => p.Products)
                      .HasForeignKey(d => d.ProductCategoryId);
            });

            // =============================
            // Product Categories
            // =============================
            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.HasKey(e => e.ProductCategoryId);
            });

            // =============================
            // SEED DATA (PERFUME STORE)
            // =============================

            // Roles
            modelBuilder.Entity<AspNetRole>().HasData(
                new AspNetRole
                {
                    Id = "ROLE_ADMIN",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "admin-role"
                },
                new AspNetRole
                {
                    Id = "ROLE_CUSTOMER",
                    Name = "Customer",
                    NormalizedName = "CUSTOMER",
                    ConcurrencyStamp = "customer-role"
                }
            );

            // Role Permissions
            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { Id = 1, RoleId = "ROLE_ADMIN", ControllerName = "Products", ActionName = "Index" },
                new RolePermission { Id = 2, RoleId = "ROLE_ADMIN", ControllerName = "Products", ActionName = "Create" },
                new RolePermission { Id = 3, RoleId = "ROLE_ADMIN", ControllerName = "Products", ActionName = "Edit" },
                new RolePermission { Id = 4, RoleId = "ROLE_ADMIN", ControllerName = "Products", ActionName = "Delete" },
                new RolePermission { Id = 5, RoleId = "ROLE_CUSTOMER", ControllerName = "Products", ActionName = "Index" },
                new RolePermission { Id = 6, RoleId = "ROLE_CUSTOMER", ControllerName = "Orders", ActionName = "Create" }
            );

            // Product Categories
            modelBuilder.Entity<ProductCategory>().HasData(
            new ProductCategory { ProductCategoryId = 1, CategoryName = "Mobile Phones", CategoryDescription = "Latest smartphones and mobile devices" },
            new ProductCategory { ProductCategoryId = 2, CategoryName = "Laptops", CategoryDescription = "High-performance laptops and notebooks" },
            new ProductCategory { ProductCategoryId = 3, CategoryName = "Accessories", CategoryDescription = "Electronic accessories and gadgets" });

            // Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    ProductName = "iPhone 15 Pro",
                    Unit = "Piece",
                    UnitPrice = 30000,
                    AvailableQuantity = 25,
                    ProductImage = "iphone15pro.jpg",
                    ProductCategoryId = 1,
                    IsActive = true
                },
                new Product
                {
                    ProductId = 2,
                    ProductName = "Dell XPS 13",
                    Unit = "Piece",
                    UnitPrice = 90000,
                    AvailableQuantity = 15,
                    ProductImage = "dellxps13.jpg",
                    ProductCategoryId = 2,
                    IsActive = true
                },
                new Product
                {
                    ProductId = 3,
                    ProductName = "Wireless Bluetooth Headphones",
                    Unit = "Piece",
                    UnitPrice = 3000,
                    AvailableQuantity = 100,
                    ProductImage = "headphones.jpg",
                    ProductCategoryId = 3,
                    IsActive = true
                }
            );

            // Customers
            modelBuilder.Entity<Customer>().HasData(
                new Customer
                {
                    CustomerId = 1,
                    CustomerName = "Faizan",
                    ContactNumber = "0770000000",
                    ContactAddress = "Ctg"
                },
                new Customer
                {
                    CustomerId = 2,
                    CustomerName = "Mutacim",
                    ContactNumber = "0780000000",
                    ContactAddress = "Irbid"
                }
            );

            // Orders
            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    OrderId = 1,
                    CustomerId = 1,
                    OrderDate = new DateTime(2025, 1, 10),
                    TotalAmount = 120m
                },
                new Order
                {
                    OrderId = 2,
                    CustomerId = 2,
                    OrderDate = new DateTime(2025, 1, 12),
                    TotalAmount = 365m
                }
            );

            // Order Details
            modelBuilder.Entity<OrderDetail>().HasData(
                new OrderDetail
                {
                    OrderDetailId = 1,
                    OrderId = 1,
                    ProductId = 1,
                    ProductCategoryId = 1,
                    OrderQuantity = 1,
                    OrderUnit = "pcs",
                    UnitPrice = 30000,
                    Amount = 30000
                },
                new OrderDetail
                {
                    OrderDetailId = 2,
                    OrderId = 2,
                    ProductId = 2,
                    ProductCategoryId = 2,
                    OrderQuantity = 1,
                    OrderUnit = "pcs",
                    UnitPrice = 90000,
                    Amount = 90000
                },
                new OrderDetail
                {
                    OrderDetailId = 3,
                    OrderId = 2,
                    ProductId = 3,
                    ProductCategoryId = 3,
                    OrderQuantity = 1,
                    OrderUnit = "pcs",
                    UnitPrice = 3000,
                    Amount = 3000
                }
            );

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}