using EventBookingSystem.Dto;
using EventBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventBookingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        // POST: api/account/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return BadRequest("Passwords do not match.");
            }

            var existingUser = await _userManager.FindByNameAsync(registerDto.UserName);
            if (existingUser != null)
            {
                return BadRequest("Username already exists.");
            }

            var existingEmail = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingEmail != null)
            {
                return BadRequest("Email already exists.");
            }

            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Registration successful" });
            }

            return BadRequest(result.Errors.Select(e => e.Description));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName!)  // ✅ REQUIRED for GetUserAsync
                };

                authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    expires: DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"])),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                        SecurityAlgorithms.HmacSha256)
                );

                return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
            }

            return Unauthorized();
        }

        // GET: api/account/get-users
        [HttpGet("get-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.Select(u => new { u.UserName }).ToList();
            return Ok(users);
        }



        // POST: api/account/add-role
        [HttpPost("add-role")]
        public async Task<IActionResult> AddRole([FromBody] RoleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Role name is required");

            var roleExists = await _roleManager.RoleExistsAsync(dto.Name);
            if (!roleExists)
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(dto.Name));
                if (result.Succeeded)
                {
                    return Ok(new { Message = "Role created successfully" });
                }
                return BadRequest(result.Errors);
            }
            return BadRequest("Role already exists");
        }


        [HttpGet("get-roles")]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles
                .Select(r => new { id = r.Id, name = r.Name })
                .ToList();

            return Ok(roles);
        }



        // Get role by ID
        [HttpGet("getRoleById")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound("Role not found");

            return Ok(role);
        }

        // PUT: api/account/roleUpdate?id=roleId
        [HttpPut("roleUpdate")]
        public async Task<IActionResult> UpdateRole([FromQuery] string id, [FromBody] RoleDto dto)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Invalid role ID or new role name");

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound("Role not found");

            var existingRole = await _roleManager.FindByNameAsync(dto.Name);
            if (existingRole != null && existingRole.Id != id)
                return BadRequest("A role with the new name already exists");

            role.Name = dto.Name;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
                return Ok(new { Message = "Role updated successfully" });

            return BadRequest(result.Errors);
        }


        // Delete role by ID
        [HttpDelete("roleDelete")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound("Role not found");

            var result = await _roleManager.DeleteAsync(role);

            return result.Succeeded
                ? Ok(new { Message = "Role deleted successfully" })
                : BadRequest(result.Errors);
        }

        // POST: api/account/assign-role
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] UserRoleDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var result = await _userManager.AddToRoleAsync(user, model.Role);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Role assigned successfully" });
            }
            return BadRequest(result.Errors);
        }

        // PUT: api/account/update-assign-role
        [HttpPut("update-assign-role")]
        public async Task<IActionResult> UpdateAssignRole([FromBody] UserRoleDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return BadRequest("Failed to remove existing roles");
            }

            var addResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (addResult.Succeeded)
            {
                return Ok(new { Message = "User role updated successfully" });
            }

            return BadRequest(addResult.Errors);
        }

        // DELETE: api/account/delete-assign-role
        [HttpDelete("delete-assign-role")]
        public async Task<IActionResult> DeleteAssignRole([FromBody] UserRoleDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var result = await _userManager.RemoveFromRoleAsync(user, model.Role);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Role removed from user successfully" });
            }

            return BadRequest(result.Errors);
        }

        // GET: api/account/get-assign-role?username=johndoe
        [HttpGet("get-assign-role")]
        public async Task<IActionResult> GetByIdAssignRole([FromQuery] string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new
            {
                UserName = username,
                Roles = roles
            });
        }

        // GET: api/account/get-all-assign-role
        [HttpGet("get-all-assign-role")]
        public async Task<IActionResult> GetAllAssignRole()
        {
            var users = _userManager.Users.ToList();

            var userRolesList = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userRolesList.Add(new
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles
                });
            }

            return Ok(userRolesList);
        }


        [HttpGet("getprofile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByNameAsync(userId);

            if (user == null)
                return Unauthorized("User not found");

            var profileData = new
            {
                user.UserName,
                user.Email,
                user.PhoneNumber
            };

            return Ok(profileData);
        }

        [Authorize]
        [HttpPut("updateprofile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto model)
        {
            // Retrieve the currently logged-in user
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Update the user's details
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            // Update the user in the database
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(new { message = "Profile updated successfully" });

            }

            return BadRequest(result.Errors);
        }
    }
}
