using IMS.COMMON.Dtos.Identity;
using IMS.Data;
using IMS.Models.Models.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace IMS.Controllers.Identity
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleBaseController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public RoleBaseController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("Role")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return BadRequest("Role name cannot be empty.");
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    return Ok(new { message = "Role created successfully." });
                }

                return BadRequest(result.Errors);
            }

            return BadRequest("Role already exists.");
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        [HttpGet("roles")]
        public IActionResult GetAllRoles()
        {
            var roles = _roleManager.Roles.ToList();

            if (roles == null || !roles.Any())
            {
                return NotFound("No roles found.");
            }

            var roleList = roles.Select(r => new
            {
                id = r.Id,
                name = r.Name,
                normalizedName = r.NormalizedName
            });

            return Ok(roleList);
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("user-role")]
        public async Task<IActionResult> AssignRoleToUserById([FromBody] UserRoleAssigmentModels model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Search user is he/she is ulready asiigned 
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound($"User with ID '{model.UserId}' not found.");
            }

            // to search is the id is already bind or not 
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null)
            {
                return NotFound($"Role with ID '{model.RoleId}' not found.");
            }

            // check both
            var isInRole = await _userManager.IsInRoleAsync(user, role.Name);
            if (isInRole)
            {
                return BadRequest($"User is already in role '{role.Name}'.");
            }

            // Add user to role 
            var result = await _userManager.AddToRoleAsync(user, role.Name);
            if (result.Succeeded)
            {
                return Ok(new
                {
                    message = $"User '{user.UserName}' added to role '{role.Name}' successfully.",
                    userId = user.Id,
                    roleId = role.Id,
                    roleName = role.Name
                });
            }

            return BadRequest(result.Errors);
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("User-With-Role")]
        public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetUserRoles()
        {
            var userRoles = await _context.Set<IdentityUserRole<string>>()
                .Select(ur => new UserRoleDto
                {
                    UserId = ur.UserId,
                    RoleId = ur.RoleId
                })
                .ToListAsync();

            return Ok(userRoles);
        }

        [HttpDelete("Delete-Role")]
        public async Task<IActionResult> DeleteUserRole(string userId, string roleId)
        {
            // Find the user by their ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found."); // Return 404 if user doesn't exist

            // Find the role by its ID
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound("Role not found."); // Return 404 if role doesn't exist

            // Remove the user from the role using the role's name
            var result = await _userManager.RemoveFromRoleAsync(user, role.Name);

            // If the removal fails, return a 400 with error details
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }

    }
}
