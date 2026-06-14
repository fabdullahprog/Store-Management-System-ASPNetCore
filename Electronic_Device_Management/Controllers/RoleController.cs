using Electronic_Device_Management.Data;
using Electronic_Device_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Electronic_Device_Management.Controllers
{
    [Authorize]
    public class RoleController : BaseController
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly RoleManager<AspNetRole> _roleManager;

        public RoleController(
            ApplicationDbContext context,
            UserManager<AspNetUser> userManager,
            RoleManager<AspNetRole> roleManager) : base(context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Roles
        public async Task<IActionResult> Index()
        {
            var rolelist = await _context.Roles
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem
                {
                    Value = r.Id,
                    Text = r.Name
                })
                .ToListAsync();

            var userlist = await _context.Users
                .OrderBy(u => u.UserName)
                .Select(u => new SelectListItem
                {
                    Value = u.UserName,
                    Text = u.UserName
                })
                .ToListAsync();

            ViewBag.Roles = rolelist;
            ViewBag.Users = userlist;
            ViewBag.Message = "";

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetAvailableRolesForUser(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return Json(new List<SelectListItem>());
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return Json(new List<SelectListItem>());
            }

            // Get roles user already has
            var userRoles = await _userManager.GetRolesAsync(user);

            // Get all roles
            var allRoles = await _context.Roles
                .OrderBy(r => r.Name)
                .ToListAsync();

            // Filter out roles user already has
            var availableRoles = allRoles
                .Where(r => !userRoles.Contains(r.Name))
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                })
                .ToList();

            return Json(availableRoles);
        }

        // GET: /Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string RoleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(RoleName))
                {
                    TempData["Error"] = "Role name is required!";
                    return View();
                }

                var roleExists = await _roleManager.RoleExistsAsync(RoleName);
                if (roleExists)
                {
                    TempData["Error"] = "Role already exists!";
                    return View();
                }

                var result = await _roleManager.CreateAsync(new AspNetRole { Name = RoleName });
                if (result.Succeeded)
                {
                    TempData["Message"] = "Role created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Error creating role: " + string.Join(", ", result.Errors.Select(e => e.Description));
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return View();
            }
        }

        // POST: Delete Role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string RoleName)
        {
            try
            {
                var role = await _roleManager.FindByNameAsync(RoleName);
                if (role == null)
                {
                    TempData["Error"] = "Role not found!";
                    return RedirectToAction(nameof(Index));
                }

                // Check if role is assigned to any user
                var usersInRole = await _userManager.GetUsersInRoleAsync(RoleName);
                if (usersInRole.Count > 0)
                {
                    TempData["Error"] = $"Cannot delete role '{RoleName}' - it is assigned to {usersInRole.Count} user(s). Please remove the role from all users first.";
                    return RedirectToAction(nameof(Index));
                }

                // Delete related permissions first
                var permissions = _context.RolePermissions.Where(p => p.RoleId == role.Id);
                _context.RolePermissions.RemoveRange(permissions);
                await _context.SaveChangesAsync();

                // Then delete the role
                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    TempData["Message"] = $"Role '{RoleName}' deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Error deleting role: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting role: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Roles/Edit/5
        public async Task<IActionResult> Edit(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: /Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string name)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }

                role.Name = name;
                var result = await _roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    TempData["Message"] = "Role updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Error updating role: " + string.Join(", ", result.Errors.Select(e => e.Description));
                return View(role);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Adding Roles to a user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoleAddToUser(string UserName, string RoleName)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(UserName);
                if (user == null)
                {
                    TempData["Error"] = "User not found!";
                    return RedirectToAction(nameof(Index));
                }

                var roleExists = await _roleManager.RoleExistsAsync(RoleName);
                if (!roleExists)
                {
                    TempData["Error"] = "Role not found!";
                    return RedirectToAction(nameof(Index));
                }

                var isInRole = await _userManager.IsInRoleAsync(user, RoleName);
                if (isInRole)
                {
                    TempData["Error"] = "User already has this role!";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userManager.AddToRoleAsync(user, RoleName);
                if (result.Succeeded)
                {
                    TempData["Message"] = "Role added to user successfully!";
                }
                else
                {
                    TempData["Error"] = "Error adding role: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Getting a List of Roles for a User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetRoles(string UserName)
        {
            // Always populate users and roles for the view
            var userlist = await _context.Users
                .OrderBy(u => u.UserName)
                .Select(u => new SelectListItem
                {
                    Value = u.UserName,
                    Text = u.UserName
                })
                .ToListAsync();
            ViewBag.Users = userlist;

            var allRolesList = await _context.Roles
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                })
                .ToListAsync();
            ViewBag.Roles = allRolesList;

            if (!string.IsNullOrWhiteSpace(UserName))
            {
                var user = await _userManager.FindByNameAsync(UserName);
                if (user != null)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    ViewBag.RolesForThisUser = userRoles.ToList();
                    ViewBag.SelectedUserName = UserName;
                    ViewBag.SelectedUserForDisplay = UserName;

                    // Create a list of only the roles assigned to this user for the Remove Role dropdown
                    var userRoleList = userRoles.Select(roleName => new SelectListItem
                    {
                        Value = roleName,
                        Text = roleName
                    }).ToList();

                    ViewBag.UserRoles = userRoleList;
                    ViewBag.Message = "Roles retrieved successfully!";
                }
                else
                {
                    ViewBag.Message = "User not found!";
                }
            }

            return View("Index");
        }

        // POST: Remove role from user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoleForUser(string UserName, string RoleName)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(UserName);
                if (user == null)
                {
                    TempData["Error"] = "User not found!";
                    return RedirectToAction(nameof(Index));
                }

                var isInRole = await _userManager.IsInRoleAsync(user, RoleName);
                if (isInRole)
                {
                    var result = await _userManager.RemoveFromRoleAsync(user, RoleName);
                    if (result.Succeeded)
                    {
                        TempData["Message"] = "Role removed from user successfully!";
                    }
                    else
                    {
                        TempData["Error"] = "Error removing role: " + string.Join(", ", result.Errors.Select(e => e.Description));
                    }
                }
                else
                {
                    TempData["Error"] = "User does not have this role!";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper method to get controller actions
        private List<string> GetControllerActions()
        {
            List<string> actions = new List<string>();
            string[] allowedControllers = { "ProductCategory", "Product", "Order" };

            var assembly = Assembly.GetExecutingAssembly();
            var controllerTypes = assembly.GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type)
                       && !type.IsAbstract
                       && allowedControllers.Contains(type.Name.Replace("Controller", "")));

            foreach (var type in controllerTypes)
            {
                var methods = type.GetMethods(BindingFlags.Instance |
                                              BindingFlags.DeclaredOnly |
                                              BindingFlags.Public)
                    .Where(m => typeof(IActionResult).IsAssignableFrom(m.ReturnType)
                             || typeof(Task<IActionResult>).IsAssignableFrom(m.ReturnType));

                foreach (var method in methods)
                {
                    // Skip methods with [NonAction] attribute
                    if (method.GetCustomAttribute<NonActionAttribute>() != null)
                        continue;

                    actions.Add(type.Name.Replace("Controller", "") + "-" + method.Name);
                }
            }

            return actions.Distinct().OrderBy(a => a).ToList();
        }

        // GET: Manage Permissions for a Role
        public async Task<IActionResult> ManagePermission(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                TempData["Error"] = "Role not found!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.RoleName = role.Name;
            ViewBag.RoleId = roleId;
            ViewBag.AllActions = GetControllerActions();

            var currentPermissions = await _context.RolePermissions
                .Where(p => p.RoleId == roleId)
                .ToListAsync();

            return View(currentPermissions);
        }

        // POST: Save Permissions for a Role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePermission(string roleId, List<string>? selectedActions)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    TempData["Error"] = "Role not found!";
                    return RedirectToAction(nameof(Index));
                }

                // Remove old permissions
                var oldPermissions = _context.RolePermissions.Where(p => p.RoleId == roleId);
                _context.RolePermissions.RemoveRange(oldPermissions);

                // Add new permissions
                if (selectedActions != null && selectedActions.Count > 0)
                {
                    foreach (var action in selectedActions)
                    {
                        if (!string.IsNullOrWhiteSpace(action))
                        {
                            var split = action.Split('-');
                            if (split.Length == 2)
                            {
                                _context.RolePermissions.Add(new RolePermission
                                {
                                    RoleId = roleId,
                                    ControllerName = split[0].Trim(),
                                    ActionName = split[1].Trim()
                                });
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = "Permissions updated successfully for role: " + role.Name;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error saving permissions: " + ex.Message;
                return RedirectToAction("ManagePermission", new { roleId = roleId });
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetUserRolesAjax(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return Json(new List<string>());
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return Json(new List<string>());
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            return Json(userRoles.ToList());
        }
    }
}
