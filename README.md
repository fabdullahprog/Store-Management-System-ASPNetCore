# 📱 Electronic Device Management System

[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core%20MVC-8.0-blue)](https://learn.microsoft.com/aspnet/core/mvc/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-green)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB%20%7C%20SQL%20Server-red)](https://www.microsoft.com/sql-server)

A professional **ASP.NET Core MVC 8** application for managing electronic products, inventory, customer orders, stock updates, authentication, authorization, and role-based permissions.

> Built with **ASP.NET Core MVC 8**, **Entity Framework Core**, **ASP.NET Core Identity**, **SQL Server**, **Stored Procedures**, and **Razor Views** to simulate a real-world business inventory and sales workflow.

---

## 🚀 Features

* Product CRUD with image upload & status management
* Product category management
* Customer management & order history
* Order creation, update, and deletion (transactional)
* Automatic stock deduction during order creation
* Automatic stock restoration during order deletion/update
* ASP.NET Core Identity authentication (email/password)
* Role-based authorization (Admin, Customer)
* Custom permission system (`RolePermission`: Controller + Action)
* Dashboard with recent orders, sales summary, analytics widgets
* AJAX-based dynamic product selection
* SQL Server Stored Procedures for order processing

---

## 🛠 Technologies Used

* ASP.NET Core MVC 8
* ASP.NET Core Identity
* Entity Framework Core 8
* SQL Server / LocalDB
* Razor Views + Razor Pages
* Bootstrap 5
* JavaScript + AJAX
* LINQ
* SQL Stored Procedures
* SQL Server Table-Valued Parameters

---

## 📁 Architecture (MVC)

The project follows a clean MVC architecture.

**Flow**

```text
Controllers → Helpers/Services → Data Layer (EF Core + Stored Procedures) → Views
```

| Layer       | Components                                                             |
| ----------- | ---------------------------------------------------------------------- |
| Controllers | Home, Product, ProductCategory, Order, Role                            |
| Data Layer  | ApplicationDbContext, DbInitializer, Migrations                        |
| Models      | Customer, Product, ProductCategory, Order, OrderDetail, RolePermission |
| ViewModels  | OrderFormVM, ProductFormVM, DashboardVM                                |
| Others      | ViewComponents, Helpers, wwwroot                                       |

---

## 🔐 Authentication & Authorization

### Authentication

Implemented using **ASP.NET Core Identity**

* Email/password login
* Password hashing
* Confirmed account login support
* Custom identity models (`AspNetUser`, `AspNetRole`)

### Authorization

Uses a custom permission system.

* Role-based authorization (`Admin`, `Customer`)
* Permissions stored in `RolePermission` table
* `BaseController` checks controller/action permissions dynamically
* No hardcoded `[Authorize(Roles="Admin")]` everywhere

---

## 📦 Main Modules

### 1. Product Management

* Add product
* Edit product
* Delete product
* Upload product image
* Manage stock quantity
* Toggle active/inactive status

---

### 2. Category Management

Manage product categories such as:

* Mobile Phones
* Laptops
* Accessories
* Tablets
* Computer Components

---

### 3. Customer Management

Store customer data:

* Customer name
* Contact number
* Address
* Order history

---

### 4. Order Management

Handles:

* Create order
* Update order
* Delete order
* View order details
* Automatic stock management

Uses **SQL Server Stored Procedures** for transactional consistency.

---

### 5. Role & Permission Management

* Assign roles to users
* Manage permissions by Controller + Action
* Restrict unauthorized access

Roles:

* Admin
* Customer

---

### 6. Dashboard

Displays:

* Recent orders
* Sales summary
* Product overview
* Analytics widgets

---

## 🗄 Database Design

### Core Relationships

```text
Customer (1) ────── (M) Order (1) ────── (M) OrderDetail
                          │
ProductCategory (1) ─────┼───── (M) Product
```

### Main Entities

* Customer
* Product
* ProductCategory
* Order
* OrderDetail
* RolePermission
* AspNetUser
* AspNetRole

---

## ⚙ SQL Server Objects

The project depends on these SQL Server objects.

| Object                | Description                                      |
| --------------------- | ------------------------------------------------ |
| `dbo.OrderDetailType` | Table-valued parameter for order details         |
| `sp_InsertOrder`      | Creates customer, inserts order, updates stock   |
| `sp_UpdateOrder`      | Restores stock, updates order, deducts new stock |
| `sp_DeleteOrder`      | Restores stock and deletes order                 |

> Run `sp.sql` before using the order module.

---

## 🔄 Stored Procedure Workflow

### `sp_InsertOrder`

* Create customer if not exists
* Create order master record
* Insert order details
* Deduct stock automatically

### `sp_UpdateOrder`

* Restore previous stock
* Update customer/order
* Replace order details
* Deduct new stock

### `sp_DeleteOrder`

* Restore stock
* Delete order details
* Delete order master

---

## 🧩 Entity Framework Core Integration

The project uses **Entity Framework Core** as the main ORM.

Includes:

* `ApplicationDbContext`
* DbSet definitions
* Fluent API relationship configuration
* Identity integration
* Database seeding
* Automatic migration execution

Base context:

```csharp
IdentityDbContext<AspNetUser, AspNetRole, string>
```

---

## ⚙ Installation Guide

### Prerequisites

Required software:

* .NET 8 SDK
* SQL Server or LocalDB
* Visual Studio 2022 / VS Code

---

### Step 1 — Clone Repository

```bash
git clone https://github.com/your-username/Store-Management-MVC-Core.git
cd Store-Management-MVC-Core
```

---

### Step 2 — Restore Packages

```bash
dotnet restore
```

---

### Step 3 — Configure Database Connection

Open `appsettings.json`

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Electronic_Device_Management_DB;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

---

### Step 4 — Apply Database Migration

```bash
dotnet ef database update
```

---

### Step 5 — Run SQL Script

Execute:

```text
sp.sql
```

This creates:

* dbo.OrderDetailType
* sp_InsertOrder
* sp_UpdateOrder
* sp_DeleteOrder

---

### Step 6 — Run Application

```bash
dotnet run
```

Open:

```text
https://localhost:5001
```

---

## 🧪 Testing Guide

| Test Case       | Expected Result                        |
| --------------- | -------------------------------------- |
| Create Product  | Product saved successfully             |
| Create Order    | Order created + stock deducted         |
| Update Order    | Old stock restored + new stock updated |
| Delete Order    | Stock restored                         |
| Login           | Valid user logs in successfully        |
| Role Permission | Restricted pages blocked correctly     |

---

## 📁 Project Structure

```text
Store-Management-MVC-Core/
│
├── Controllers/
│   ├── HomeController.cs
│   ├── ProductController.cs
│   ├── ProductCategoryController.cs
│   ├── OrderController.cs
│   └── RoleController.cs
│
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DbInitializer.cs
│
├── Models/
│   ├── Customer.cs
│   ├── Product.cs
│   ├── ProductCategory.cs
│   ├── Order.cs
│   ├── OrderDetail.cs
│   ├── RolePermission.cs
│   └── AspNetUser.cs
│
├── ViewModels/
├── Views/
├── ViewComponents/
├── Helpers/
├── Migrations/
├── wwwroot/
│   └── product-images/
│
├── Program.cs
├── appsettings.json
├── sp.sql
└── README.md
```

---

## 🚀 Future Improvements

Planned upgrades:

* REST API integration
* PDF invoice generation
* Advanced reporting (Excel / CSV export)
* Payment gateway integration
* Customer portal system
* Real-time stock alerts using SignalR
* Search and filtering optimization

---

## 👨‍💻 Author

**Abdullah Al Foysal**

---



```
```
