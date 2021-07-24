using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Identity.ViewModels
{
    public class ManageUserRolesViewModel
    {
        [Required]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public IList<ManageRoleViewModel> ManageRolesViewModel { get; set; }
    }
    public class ManageRoleViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Checked { get; set; }
    }
}
