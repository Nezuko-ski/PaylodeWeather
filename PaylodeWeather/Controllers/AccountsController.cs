using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaylodeWeather.Core.AppSettings;
using PaylodeWeather.Core.DTOs;
using PaylodeWeather.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PaylodeWeather.Controllers
{
    /// <summary>
    /// Accounts controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly Jwt _jwt;
        private readonly IMapper _mapper;
        private readonly PaylodeWeatherDbContext _context;

        /// <summary>
        /// Accounts constructor
        /// </summary>
        public AccountsController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser>
            signInManager, IServiceProvider provider, IMapper mapper, PaylodeWeatherDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwt = provider.GetRequiredService<Jwt>();
            _mapper = mapper;
            _context = context;
        }

        /// <summary>
        /// Create a user
        /// </summary>
        /// <param name="creds"></param>
        /// <returns></returns>
        [HttpPost("create-user")]
        public async Task<ActionResult<AuthenticationResponse>> CreateUserAsync([FromBody] UserCredentials creds)
        {
            var user = new IdentityUser
            {
                UserName = creds.Email.Split('@')[0],
                Email = creds.Email
            };
            var result = await _userManager.CreateAsync(user, creds.Password);
            if (result.Succeeded)
            {
                return await BuildToken(creds);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="creds"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] UserCredentials creds)
        {
            var result = await _signInManager.PasswordSignInAsync(creds.Email.Split('@')[0], creds.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return await BuildToken(creds);
            }
            else
            {
                return BadRequest("Invalid login attempt!");
            }
        }

        /// <summary>
        /// List users
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "RequireAdminRole")]
        [HttpGet("list-users")]
        public async Task<ActionResult<List<UserDTO>>> ListUsersAsync()
        {
            var queryable = _context.Users.AsQueryable();
            var users = await queryable.OrderBy(v => v.UserName).ToListAsync();
            return _mapper.Map<List<UserDTO>>(users);
        }

        /// <summary>
        /// Assign admin role to a user by userId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "RequireAdminRole")]
        [HttpPost("assign-admin-role")]
        public async Task<ActionResult> AssignAdminRoleAsync([FromBody] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) 
            {
                return NotFound();
            }
            var result = await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, "admin"));
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        /// <summary>
        /// Remove admin role from a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "RequireAdminRole")]
        [HttpPost("remove-admin-role")]
        public async Task<ActionResult> RemoveAdminRoleAsync([FromBody] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            var result = await _userManager.RemoveClaimAsync(user, new Claim(ClaimTypes.Role, "admin"));
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }


        private async Task<AuthenticationResponse> BuildToken(UserCredentials userCredentials)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, userCredentials.Email),
                new Claim(ClaimTypes.GivenName, userCredentials.Email.Split('@')[0])
            };
            var user = await _userManager.FindByNameAsync(userCredentials.Email.Split('@')[0]);
            var claimsDb = await _userManager.GetClaimsAsync(user);
            claims.AddRange(claimsDb);
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Token));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddYears(1);
            var token = new JwtSecurityToken(
                    issuer: _jwt.Issuer,
                    audience: _jwt.Audience,
                    claims: claims,
                    expires: expiration,
                    signingCredentials: creds
                );
            return new AuthenticationResponse()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }
    }
}
