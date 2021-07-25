using System.ComponentModel.DataAnnotations;

namespace Identity.ViewModels
{
    public class AddRoleViewModel
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
