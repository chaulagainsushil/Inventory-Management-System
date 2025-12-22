
using IMS.COMMON.Dtos.Identity;
using IMS.Models.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;




        private readonly IOptions<JwtSetting> _JwtSettings;

        public AuthController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration, IOptions<JwtSetting> jwtSettings, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _JwtSettings = jwtSettings;
            _roleManager = roleManager;
        }
        //This is code to insert the user information for signup
        //This method will insert the data in identity form 

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] SignUpUser model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                //mapping the element od models 
                UserName = model.Email,
                Email = model.Email,
                NormalizedEmail = model.Email.ToUpper(),
                Name = model.Name,
                PhoneNumber = model.PhoneNumber
            };
            //to take the password from the user with hasing form 
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
                return Ok(new { message = "User registered successfully." });

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LogInModelDto Dtos)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(Dtos.Email);

            if (user != null && await _userManager.CheckPasswordAsync(user, Dtos.Password))
            {
                var token = GenerateJwtToken(user.Email);
                return Ok(new { token });
            }

            return Unauthorized("Invalid login credentials.");
        }


        private string GenerateJwtToken(string username)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JwtSettings.Value.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _JwtSettings.Value.Issuer,
                audience: _JwtSettings.Value.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("MobileUserRegister")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            // Check if user exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest("User already exists");

            // Create user
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Role name
            string roleName = "User";

            // Create role if it doesn't exist
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // Assign role automatically
            await _userManager.AddToRoleAsync(user, roleName);

            return Ok(new
            {
                message = "User created successfully",
                role = roleName
            });
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")] // Optional
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var userList = new List<UserListDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userList.Add(new UserListDto
                {
                    Id = user.Id,
                    FullName = user.Name,
                    Email = user.Email,
                    UserName = user.UserName,
                    Roles = roles
                });
            }

            return Ok(userList);
        }

        [HttpGet("UserCount")]
   
        public async Task<IActionResult> GetTotalUserCount()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            return Ok(new
            {
                totalUsers
            });
        }
    }
}





