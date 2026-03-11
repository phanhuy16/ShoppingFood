using Microsoft.AspNetCore.Identity;
using ShoppingFood.Models;

namespace ShoppingFood.Services.Auth
{
    public interface IAuthService
    {
        Task<SignInResult> LoginAsync(LoginModel model);
        Task<(bool Success, IEnumerable<IdentityError> Errors)> RegisterAsync(RegisterModel model);
        Task LogoutAsync();
        Task<(bool Success, string Message, AppUserModel User)> HandleGoogleResponseAsync(IEnumerable<System.Security.Claims.Claim> claims);
    }
}
