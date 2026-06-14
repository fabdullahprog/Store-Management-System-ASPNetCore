using Electronic_Device_Management.Models;

namespace Electronic_Device_Management.Helpers
{
    public static class PermissionHelper
    {
        /// <summary>
        /// Check if user has permission for a specific controller and action
        /// </summary>
        public static bool HasPermission(
            List<RolePermission>? permissions, 
            string controllerName, 
            string actionName, 
            bool isAdmin)
        {
            // Admin always has permission
            if (isAdmin) return true;

            // Check if permission exists
            return permissions?.Any(p =>
                p.ControllerName.Equals(controllerName, StringComparison.OrdinalIgnoreCase) &&
                p.ActionName.Equals(actionName, StringComparison.OrdinalIgnoreCase)) ?? false;
        }

        /// <summary>
        /// Check if user has ANY of the specified permissions
        /// </summary>
        public static bool HasAnyPermission(
            List<RolePermission>? permissions,
            string controllerName,
            string[] actionNames,
            bool isAdmin)
        {
            if (isAdmin) return true;

            if (permissions == null || actionNames == null || actionNames.Length == 0)
                return false;

            return permissions.Any(p =>
                p.ControllerName.Equals(controllerName, StringComparison.OrdinalIgnoreCase) &&
                actionNames.Any(action => p.ActionName.Equals(action, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Check if user has ALL of the specified permissions
        /// </summary>
        public static bool HasAllPermissions(
            List<RolePermission>? permissions,
            string controllerName,
            string[] actionNames,
            bool isAdmin)
        {
            if (isAdmin) return true;

            if (permissions == null || actionNames == null || actionNames.Length == 0)
                return false;

            return actionNames.All(action =>
                permissions.Any(p =>
                    p.ControllerName.Equals(controllerName, StringComparison.OrdinalIgnoreCase) &&
                    p.ActionName.Equals(action, StringComparison.OrdinalIgnoreCase)));
        }
    }
}

