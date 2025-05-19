using EventBookingSystem.Dto;
using Microsoft.AspNetCore.Identity;

namespace EventBookingSystem.Repository.IRepository
{
    public interface IAccountRepository
    {
        Task<IdentityResult> RegisterAsync(RegisterDto dto);
        Task<string> LoginAsync(LoginDto dto);
        Task<List<object>> GetAllUsersAsync();
        Task<IdentityResult> AddRoleAsync(RoleDto dto);
        Task<List<object>> GetRolesAsync();
        Task<IdentityRole?> GetRoleByIdAsync(string id);
        Task<IdentityResult> UpdateRoleAsync(string id, RoleDto dto);
        Task<IdentityResult> DeleteRoleAsync(string id);
        Task<IdentityResult> AssignRoleAsync(UserRoleDto dto);
        Task<IdentityResult> UpdateAssignRoleAsync(UserRoleDto dto);
        Task<IdentityResult> DeleteAssignRoleAsync(UserRoleDto dto);
        Task<object> GetAssignedRoleAsync(string username);
        Task<List<object>> GetAllAssignedRolesAsync();
        Task<object> GetProfileAsync(string userId);
        Task<IdentityResult> UpdateProfileAsync(UpdateProfileDto model);
    }
}
