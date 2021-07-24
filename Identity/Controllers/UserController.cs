using Identity.Helpers;
using Identity.Models;
using Identity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public UserController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.Permissions.Users.View)]
        public async Task<IActionResult> Index(int? pageNumber, int? pageSize)
        {
            var users = _userManager.Users.OrderBy(x => x.UserName);
            var rs = await PaginatedList<AppUser>.CreateFromEfQueryableAsync(users.AsNoTracking(), pageNumber ?? 1,
                pageSize ?? 12);
            var userViewModels = rs.Select(user => new UserViewModel
                {Id = user.Id, UserName = user.UserName, Email = user.Email}).ToList();
            var response = new PaginatedList<UserViewModel>(userViewModels, rs.Count, pageNumber ?? 1, pageSize ?? 12);
            return View(response);
        }


        [HttpGet]
        [Authorize(Policy = Permissions.Permissions.Users.ManageRoles)]
        public async Task<IActionResult> ManageRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return View();
            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();
            var allRolesViewModel = allRoles.Select(role => new ManageRoleViewModel
                {Name = role.Name, Description = role.Description}).ToList();
            foreach (var roleViewModel in allRolesViewModel.Where(roleViewModel =>
                userRoles.Contains(roleViewModel.Name)))
            {
                roleViewModel.Checked = true;
            }
            var manageUserRolesViewModel = new ManageUserRolesViewModel
            {
                UserId = userId,
                UserName = user.UserName,
                ManageRolesViewModel = allRolesViewModel
            };

            return View(manageUserRolesViewModel);
        }
        
        [HttpPost]
        [Authorize(Policy = Permissions.Permissions.Users.ManageRoles)]
        public async Task<IActionResult> ManageRoles(ManageUserRolesViewModel manageUserRolesViewModel)
        {
            if (!ModelState.IsValid) return View(manageUserRolesViewModel);

            var user = await _userManager.FindByIdAsync(manageUserRolesViewModel.UserId);
            if (user == null)
                return View(manageUserRolesViewModel);
            var existingRoles = await _userManager.GetRolesAsync(user);
            foreach (var roleViewModel in manageUserRolesViewModel.ManageRolesViewModel)
            {
                var roleExists = existingRoles.FirstOrDefault(x => x == roleViewModel.Name);
                switch (roleViewModel.Checked)
                {
                    case true when roleExists == null:
                        await _userManager.AddToRoleAsync(user, roleViewModel.Name);
                        break;
                    case false when roleExists != null:
                        await _userManager.RemoveFromRoleAsync(user, roleViewModel.Name);
                        break;
                }
            }

            return RedirectToAction("Index", "User", new {  id = manageUserRolesViewModel.UserId, succeeded = true, message = "Succeeded" });
            
        }
    }
}
