using Identity.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Identity.ViewModels
{
    public class ManageRolePermissionsViewModel
    {
        [Required]
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string PermissionValue { get; set; }
        public PaginatedList<ManageClaimViewModel> ManagePermissionsViewModel { get; set; }
    }
}
