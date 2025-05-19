using EventBookingSystem.Dto;
using EventBookingSystem.Models;
using EventBookingSystem.Repository.IRepository;
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
        private readonly IAccountRepository _repo;

        public AccountController(IAccountRepository repo)
        {
            _repo = repo;
        }

        // POST: api/account/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest("Passwords do not match.");

            var result = await _repo.RegisterAsync(dto);
            return result.Succeeded ? Ok("Registered") : BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var token = await _repo.LoginAsync(dto);
            return token != null ? Ok(new { Token = token }) : Unauthorized("Invalid credentials");
        }

        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers() => Ok(await _repo.GetAllUsersAsync());

        [HttpPost("add-role")]
        public async Task<IActionResult> AddRole(RoleDto dto) =>
            Ok(await _repo.AddRoleAsync(dto));

        [HttpGet("get-roles")]
        public async Task<IActionResult> GetRoles() =>
            Ok(await _repo.GetRolesAsync());

        [HttpGet("getRoleById")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var role = await _repo.GetRoleByIdAsync(id);
            return role == null ? NotFound() : Ok(role);
        }

        [HttpPut("roleUpdate")]
        public async Task<IActionResult> UpdateRole(string id, RoleDto dto) =>
            Ok(await _repo.UpdateRoleAsync(id, dto));

        [HttpDelete("roleDelete")]
        public async Task<IActionResult> DeleteRole(string id) =>
            Ok(await _repo.DeleteRoleAsync(id));

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole(UserRoleDto dto) =>
            Ok(await _repo.AssignRoleAsync(dto));

        [HttpPut("update-assign-role")]
        public async Task<IActionResult> UpdateRole(UserRoleDto dto) =>
            Ok(await _repo.UpdateAssignRoleAsync(dto));

        [HttpDelete("delete-assign-role")]
        public async Task<IActionResult> DeleteRole(UserRoleDto dto) =>
            Ok(await _repo.DeleteAssignRoleAsync(dto));

        [HttpGet("get-assign-role")]
        public async Task<IActionResult> GetAssignedRole(string username)
        {
            var result = await _repo.GetAssignedRoleAsync(username);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("get-all-assign-role")]
        public async Task<IActionResult> GetAllRoles() =>
            Ok(await _repo.GetAllAssignedRolesAsync());

        [Authorize]
        [HttpGet("getprofile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _repo.GetProfileAsync(userId!);
            return result == null ? NotFound() : Ok(result);
        }

        [Authorize]
        [HttpPut("updateprofile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto) =>
            Ok(await _repo.UpdateProfileAsync(dto));
    }
}
