using Common.WebApi.Application.Models.User;
using Common.Core.Data.Identity;
using Common.Core.Generic.Controllers.Response;

namespace Common.WebApi.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<BaseResponse> RegisterAsync(UserDto model);
        Task<BaseResponse> LoginAsync(UserDto model);
        string GenerateJwtToken(ApplicationUserBase user);
    }

}
