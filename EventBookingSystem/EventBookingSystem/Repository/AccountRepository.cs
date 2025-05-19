using EventBookingSystem.Dto;
using EventBookingSystem.Models;
using EventBookingSystem.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventBookingSystem.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public AccountRepository(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterDto dto)
        {
            var user = new User { UserName = dto.UserName, Email = dto.Email };
            return await _userManager.CreateAsync(user, dto.Password);
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                expires: DateTime.Now.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"]!)),
                claims: claims,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Task<List<object>> GetAllUsersAsync() =>
            Task.FromResult(_userManager.Users.Select(u => new { u.UserName }).ToList<object>());

        public async Task<IdentityResult> AddRoleAsync(RoleDto dto)
        {
            if (!await _roleManager.RoleExistsAsync(dto.Name))
                return await _roleManager.CreateAsync(new IdentityRole(dto.Name));
            return IdentityResult.Failed(new IdentityError { Description = "Role already exists." });
        }

        public Task<List<object>> GetRolesAsync() =>
            Task.FromResult(_roleManager.Roles.Select(r => new { r.Id, r.Name }).ToList<object>());

        public Task<IdentityRole?> GetRoleByIdAsync(string id) =>
            _roleManager.FindByIdAsync(id);

        public async Task<IdentityResult> UpdateRoleAsync(string id, RoleDto dto)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return IdentityResult.Failed(new IdentityError { Description = "Role not found." });

            var existing = await _roleManager.FindByNameAsync(dto.Name);
            if (existing != null && existing.Id != id)
                return IdentityResult.Failed(new IdentityError { Description = "Role name already exists." });

            role.Name = dto.Name;
            return await _roleManager.UpdateAsync(role);
        }

        public async Task<IdentityResult> DeleteRoleAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            return role == null
                ? IdentityResult.Failed(new IdentityError { Description = "Role not found." })
                : await _roleManager.DeleteAsync(role);
        }

        public async Task<IdentityResult> AssignRoleAsync(UserRoleDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            return user == null
                ? IdentityResult.Failed(new IdentityError { Description = "User not found." })
                : await _userManager.AddToRoleAsync(user, dto.Role);
        }

        public async Task<IdentityResult> UpdateAssignRoleAsync(UserRoleDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            var currentRoles = await _userManager.GetRolesAsync(user);
            var remove = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!remove.Succeeded) return remove;

            return await _userManager.AddToRoleAsync(user, dto.Role);
        }

        public async Task<IdentityResult> DeleteAssignRoleAsync(UserRoleDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            return user == null
                ? IdentityResult.Failed(new IdentityError { Description = "User not found." })
                : await _userManager.RemoveFromRoleAsync(user, dto.Role);
        }

        public async Task<object> GetAssignedRoleAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return new { username, roles };
        }

        public async Task<List<object>> GetAllAssignedRolesAsync()
        {
            var users = _userManager.Users.ToList();
            var result = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new { user.Id, user.UserName, user.Email, Roles = roles });
            }

            return result;
        }

        public async Task<object> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new { user.UserName, user.Email, user.PhoneNumber };
        }

        public async Task<IdentityResult> UpdateProfileAsync(UpdateProfileDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            return await _userManager.UpdateAsync(user);
        }
    }
}
