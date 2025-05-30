using InspireMe.Models;

namespace InspireMe.Contracts
{
    public interface IAdminService
    {
        public Task<List<UserRoleViewModel>> GetAllUsersAsync();
        public Task<bool> UpdateUserRoleAsync(string userId, string selectedRole);


    }
}
