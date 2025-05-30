using InspireMe.Contracts;
using InspireMe.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InspireMe.Services
{
    public class AdminService:IAdminService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager) 
        {
            _userManager = userManager;
            _roleManager = roleManager;

        }

        public async Task<List<UserRoleViewModel>> GetAllUsersAsync()
        {
            var users=await _userManager.Users.ToListAsync();
            var userRolesView = new List<UserRoleViewModel>();

            foreach (var user in users) {
                var roles = await _userManager.GetRolesAsync(user);
                userRolesView.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    CurrentRole = roles.FirstOrDefault() ?? "None",
                    AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList()
                });
            }
            return userRolesView;

        }

        public async Task<bool> UpdateUserRoleAsync(string userId, string selectedRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, selectedRole);

            return true;
        }

        public async Task<List<UserRoleViewModel>> GetAllUsersWithRolesAsync(string searchTerm = null)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                usersQuery = usersQuery.Where(u =>
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.Name.ToLower().Contains(searchTerm));
            }

            var users = usersQuery.ToList();

            var userRoles = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    CurrentRole = roles.FirstOrDefault() ?? "None",
                    AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList()
                });
            }

            return userRoles;
        }


    }
}
