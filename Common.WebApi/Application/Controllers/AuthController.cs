namespace Common.WebApi.Application.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Common.WebApi.Application.Models.User;
    using Common.WebApi.Application.Services.Interfaces;
    using Common.WebApi.Infrastructure.CustomAttributes;
    using System.Threading.Tasks;
    using Common.Core.Data.Identity;
    using Common.Core.Generic.Controllers.Response;

    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService, ILogger<AuthController> log)
        {
            _authService = authService;
        }

        [HttpPost("register", Name = $"auth/{nameof(Register)}")]
        public async Task<IActionResult> Register(UserDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.RegisterAsync(model);
            if (response.Success)
            {
                return Ok(((AuthResponse)response).Data);
            }
            return BadRequest(response);
        }

        [HttpPost("login", Name = $"auth/{nameof(Login)}")]
        public async Task<IActionResult> Login([FromBody] UserDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(model);
            if (response.Success)
            {
                return Ok(((AuthResponse)response).Data);
            }
            return Unauthorized(response);
        }

        /// <summary>
        /// Check admin authorization
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("checkAdmin", Name = $"auth/checkAdmin")]
        [CustomAuthorize(ApplicationRoleType.Admin)]
        public async Task<IActionResult> CheckAdmin([FromBody] UserDto model)
        {
            return await Login(model);
        }

        /// <summary>
        /// Check user authorization
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("checkUser", Name = $"auth/checkUser")]
        [CustomAuthorize(ApplicationRoleType.User)]
        public async Task<IActionResult> CheckUser([FromBody] UserDto model)
        {
            return await Login(model);
        }

    }

}
