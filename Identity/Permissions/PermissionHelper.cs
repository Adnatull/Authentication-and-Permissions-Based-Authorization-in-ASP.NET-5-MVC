using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;

namespace Identity.Permissions
{
    public static class PermissionHelper
    {
        private static List<Claim> _allPermissions = new();
        public static List<Claim> GetAllPermissions()
        {
            if (_allPermissions.Count > 0) return _allPermissions;
            var allPermissions = new List<Claim>();
            var permissionClass = typeof(Permissions);
            var allModulesPermissions = permissionClass.GetNestedTypes().Where(x => x.IsClass).ToList();
            foreach (var modulePermissions in allModulesPermissions)
            {
                var permissions = modulePermissions.GetFields(BindingFlags.Static | BindingFlags.Public);
                allPermissions.AddRange(permissions.Select(permission =>
                    new Claim(CustomClaimTypes.Permission, permission.GetValue(null)?.ToString())));
            }
            _allPermissions = allPermissions;
            return _allPermissions;
        }
    }
}
