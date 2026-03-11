using Microsoft.AspNetCore.Identity;
using ShoppingFood.Models;
using System.Security.Claims;

namespace ShoppingFood.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly SignInManager<AppUserModel> _signInManager;

        public AuthService(UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<SignInResult> LoginAsync(LoginModel model)
        {
            return await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
        }

        public async Task<(bool Success, IEnumerable<IdentityError> Errors)> RegisterAsync(RegisterModel model)
        {
            var newUser = new AppUserModel
            {
                UserName = model.Username,
                Email = model.Email
            };

            IdentityResult result = await _userManager.CreateAsync(newUser, model.Password);

            if (result.Succeeded)
            {
                var roleAssign = await _userManager.AddToRoleAsync(newUser, "User");

                if (!roleAssign.Succeeded)
                {
                    return (false, roleAssign.Errors);
                }

                await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
                return (true, Enumerable.Empty<IdentityError>());
            }

            return (false, result.Errors);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<(bool Success, string Message, AppUserModel User)> HandleGoogleResponseAsync(IEnumerable<Claim> claims)
        {
            var email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return (false, "Email not found in Google response", null);

            string name = email.Split('@')[0];
            var exitsUser = await _userManager.FindByEmailAsync(email);

            if (exitsUser == null)
            {
                var password = new PasswordHasher<AppUserModel>();
                var passwordHasher = password.HashPassword(null, "Abc@123");

                var newUser = new AppUserModel { UserName = name, Email = email, };
                newUser.PasswordHash = passwordHasher;

                var createUser = await _userManager.CreateAsync(newUser);
                if (!createUser.Succeeded)
                {
                    return (false, "Đăng ký tài khoản thất bại. Vui lòng thử lại sau", null);
                }
                else
                {
                    var roleExist = await _userManager.IsInRoleAsync(newUser, "User");
                    if (!roleExist)
                    {
                        var roleAssignResult = await _userManager.AddToRoleAsync(newUser, "User");

                        if (!roleAssignResult.Succeeded)
                        {
                            return (false, "Đăng ký tài khoản thất bại. Vui lòng thử lại sau", null);
                        }
                    }

                    await _signInManager.SignInAsync(newUser, isPersistent: false);
                    return (true, "Đăng ký tài khoản thành công", newUser);
                }
            }
            else
            {
                await _signInManager.SignInAsync(exitsUser, isPersistent: false);
                return (true, "Đăng nhập thành công", exitsUser);
            }
        }
    }
}
