using IdentityAPI.Shared;
using IdentityApp.ViewModels;

namespace IdentityAPI.Services.Abstract
{
    public interface IUserService
    {
        Task<Response> Register(RegisterViewModel model);
        Task<Response> Login(LoginViewModel model);
        Task<Response> ConfirmEmail(string userId, string token);
        Task<Response> ForgetPassword(string email);

        Task<Response> ResetPassword(ResetPasswordViewModel model);
    }
}