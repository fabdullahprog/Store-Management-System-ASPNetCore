using Electronic_Device_Management.Data;
using Electronic_Device_Management.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Electronic_Device_Management.Controllers;

public class BaseController : Controller
{
    protected readonly ApplicationDbContext _context;

    public BaseController(ApplicationDbContext context)
    {
        _context = context;
    }

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        base.OnActionExecuting(filterContext);

        // Get controller and action names
        var controllerName = filterContext.RouteData.Values["controller"]?.ToString();
        var actionName = filterContext.RouteData.Values["action"]?.ToString();

        // These methods are used for dropdowns
        var ajaxMethods = new[] {
                "GetProductsByCategory",
                "GetProductDetails",
                "GetUserRolesAjax"
            };

        bool isAjaxMethod = ajaxMethods.Contains(actionName);

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                var userObj = _context.Users.Find(userId);

                if (userObj != null)
                {
                    List<RolePermission> permissions = new List<RolePermission>();

                    // Admin users get all permissions
                    if (User.IsInRole("Admin"))
                    {
                        // For admin, we can show all possible actions
                        permissions = _context.RolePermissions.ToList();
                    }
                    else
                    {
                        // For other users, get permissions based on their roles
                        var userRoles = _context.UserRoles
                            .Where(ur => ur.UserId == userId)
                            .Select(ur => ur.RoleId)
                            .ToList();

                        permissions = _context.RolePermissions
                            .Where(p => userRoles.Contains(p.RoleId))
                            .ToList();

                        if (!isAjaxMethod && !string.IsNullOrEmpty(controllerName) && !string.IsNullOrEmpty(actionName))
                        {
                            var skipControllers = new[] { "Home", "Account" };

                            if (!skipControllers.Contains(controllerName))
                            {
                                bool hasPermission = permissions.Any(p =>
                                    p.ControllerName == controllerName &&
                                    p.ActionName == actionName);

                                if (!hasPermission)
                                {
                                    filterContext.Result = new ForbidResult();
                                    return;
                                }
                            }
                        }
                    }

                    ViewBag.UserPermissions = permissions;
                }
            }
        }
    }

    protected bool HasPermission(string controllerName, string actionName)
    {
        if (User.IsInRole("Admin"))
            return true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return false;

        var userRoles = _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToList();

        return _context.RolePermissions
            .Any(p => userRoles.Contains(p.RoleId)
                && p.ControllerName == controllerName
                && p.ActionName == actionName);
    }
}
