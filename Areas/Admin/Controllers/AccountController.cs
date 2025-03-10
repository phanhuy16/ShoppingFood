using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShoppingFood.Models;
using ShoppingFood.Repository;
using System.Security.Claims;

namespace ShoppingFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly SignInManager<AppUserModel> _signInManager;
        private readonly INotyfService _notyf;

        public AccountController(DataContext context, UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signInManager, INotyfService notyf)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _notyf = notyf;
            _dataContext = context;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);
                    if (result.Succeeded)
                    {
                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            // Đăng nhập Admin sử dụng AdminScheme
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, user.UserName),
                                new Claim(ClaimTypes.Role, "Admin")
                            };
                            var claimsIdentity = new ClaimsIdentity(claims, "AdminScheme");
                            var authProperties = new AuthenticationProperties { IsPersistent = true };

                            await HttpContext.SignInAsync("AdminScheme", new ClaimsPrincipal(claimsIdentity), authProperties);

                            _notyf.Success("Admin Login successfully!");
                            return Redirect(model.ReturnUrl ?? "/admin/dashboard");
                        }
                        else
                        {
                            // Đăng nhập Client sử dụng ClientScheme
                            _notyf.Success("Client Login successfully!");
                            return Redirect(model.ReturnUrl ?? "/");
                        }
                    }
                }

                _notyf.Error("Login failed!");
                ModelState.AddModelError("", "Invalid username or password");
            }
            return View(model);
        }

    }
}
