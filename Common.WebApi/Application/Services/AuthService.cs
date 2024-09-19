using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Common.WebApi.Application.Models.User;
using Common.WebApi.Application.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Common.Core.Data.Identity;
using Common.Core.Generic.Controllers.Response;

namespace Common.Business.Services.Common
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUserBase> _userManager;
        private readonly SignInManager<ApplicationUserBase> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AuthService(
            UserManager<ApplicationUserBase> userManager,
            SignInManager<ApplicationUserBase> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _signInManager = signInManager;
        }

        public async Task<BaseResponse> RegisterAsync(UserDto model)
        {
            var user = new ApplicationUserBase
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);
                var token = GenerateJwtToken(user);
                return new AuthResponse { Success = true, Data = new { Token = token } };
            }

            return new BaseResponse { Success = false, Error = new ApiError() { Errors = result.Errors.Select(e => e.Description).ToList() } };
        }

        public async Task<BaseResponse> LoginAsync(UserDto model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var token = GenerateJwtToken(user);
                return new AuthResponse { Success = true, Data = new { Token = token } };
            }

            return new BaseResponse { Success = false, Message = "Unauthorized" };
        }

        public string GenerateJwtToken(ApplicationUserBase user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddSeconds(Convert.ToDouble(_configuration["JwtSettings:ExpirySeconds"]));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
