using System.Collections.Generic;
using Identity.Helpers;
using Identity.Models;
using Identity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Identity.Permissions;

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
            if (user == null) return RedirectToAction("Index");
            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.OrderBy(x => x.Name).ToListAsync();
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

        
        [HttpGet]
        [Authorize(Policy = Permissions.Permissions.Users.ManageClaims)]
        public async Task<IActionResult> ManagePermissions(string userId, string permissionValue, int? pageNumber, int? pageSize)
        {

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return RedirectToAction("Index");
            var userPermissions = await _userManager.GetClaimsAsync(user);
            var allPermissions = PermissionHelper.GetAllPermissions();
            if (!string.IsNullOrWhiteSpace(permissionValue))
            {
                allPermissions = allPermissions.Where(x => x.Value.ToLower().Contains(permissionValue.Trim().ToLower()))
                    .ToList();
            }
            var managePermissionsClaim = new List<ManageUserClaimViewModel>();
            foreach (var permission in allPermissions)
            {
                var managePermissionClaim = new ManageUserClaimViewModel { Type = permission.Type, Value = permission.Value };
                if (userPermissions.Any(x => x.Value == permission.Value))
                {
                    managePermissionClaim.Checked = true;
                }
                managePermissionsClaim.Add(managePermissionClaim);
            }

            var paginatedList = PaginatedList<ManageUserClaimViewModel>.CreateFromLinqQueryable(managePermissionsClaim.AsQueryable(),
                pageNumber ?? 1, pageSize ?? 12);
            var manageUserPermissionsViewModel = new ManageUserPermissionsViewModel
            {
                UserId = userId,
                UserName = user.UserName,
                PermissionValue = permissionValue,
                ManagePermissionsViewModel = paginatedList
            };
            
            return View(manageUserPermissionsViewModel);
        }


       
        [HttpPost]
        [Authorize(Policy = Permissions.Permissions.Users.ManageClaims)]
        public async Task<IActionResult> ManageClaims(ManageUserClaimViewModel manageUserClaimViewModel)
        {

            var userById = await _userManager.FindByIdAsync(manageUserClaimViewModel.UserId);
            var userByName = await _userManager.FindByNameAsync(manageUserClaimViewModel.UserName);

            if (userById != userByName)
                return Json(new { Succeeded = false, Message = "Fail" });

            var allClaims = await _userManager.GetClaimsAsync(userById);
            var claimExists =
                allClaims.Where(x => x.Type == manageUserClaimViewModel.Type && x.Value == manageUserClaimViewModel.Value).ToList();
            switch (manageUserClaimViewModel.Checked)
            {
                case true when claimExists.Count == 0:
                {
                    await _userManager.AddClaimAsync(userById,
                        new Claim(manageUserClaimViewModel.Type, manageUserClaimViewModel.Value));
                    break;
                }
                case false when claimExists.Count > 0:
                {
                    await _userManager.RemoveClaimsAsync(userById, claimExists);
                    break;
                }
            }
            return Json(new {Succeeded = true, Message="Success"});
        }
    }
}
