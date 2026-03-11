using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Areas.Admin.Repository;
using ShoppingFood.Models;
using ShoppingFood.Models.ViewModel;
using ShoppingFood.Repository;
using ShoppingFood.Services.Address;
using ShoppingFood.Services.Account;
using ShoppingFood.Services.Auth;
using System.Security.Claims;

namespace ShoppingFood.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly SignInManager<AppUserModel> _signInManager;
        private readonly INotyfService _notyf;
        private readonly IEmailSender _emailSender;
        private readonly IAddressService _addressService;
        private readonly IAccountService _accountService;
        private readonly IAuthService _authService;

        public AccountController(DataContext context, UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signInManager, INotyfService notyf, IEmailSender email, IAddressService addressService, IAccountService accountService, IAuthService authService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _notyf = notyf;
            _dataContext = context;
            _emailSender = email;
            _addressService = addressService;
            _accountService = accountService;
            _authService = authService;
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
                var result = await _authService.LoginAsync(model);
                if (result.Succeeded)
                {
                    _notyf.Success("Login successfully!");
                    return Redirect(model.ReturnUrl ?? "/");
                }
                _notyf.Error("Login fail!");
                ModelState.AddModelError("", "Invalid username or password");
            }
            return View(model);
        }

        public IActionResult Register(string returnUrl)
        {
            return View(new RegisterModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.RegisterAsync(model);

                if (result.Success)
                {
                    _notyf.Success("Register successfully!");
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                _notyf.Error("Register fail!");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _authService.LogoutAsync();
            return Redirect(returnUrl);
        }

        public async Task<IActionResult> Profile()
        {
            if ((bool)!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var profileView = await _accountService.GetProfileAsync(userId, userEmail);

            ViewBag.Wishlist = profileView.Wishlist;
            ViewBag.Compare = profileView.Compare;
            ViewBag.Addresses = profileView.Addresses;
            ViewBag.UserEmail = profileView.UserEmail;

            return View(profileView.Orders);
        }

        public async Task<IActionResult> DeteleWishlist(int id)
        {
            var result = await _accountService.DeleteWishlistAsync(id);

            if (result)
            {
                _notyf.Success("Deleted successfully!");
            }
            else
            {
                _notyf.Error("Wishlist not found!");
            }
            return RedirectToAction("Profile");
        }

        public async Task<IActionResult> DeteleCompare(int id)
        {
            var result = await _accountService.DeleteCompareAsync(id);

            if (result)
            {
                _notyf.Success("Deleted successfully!");
            }
            else
            {
                _notyf.Error("Compare not found!");
            }
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> AddAddress(UserAddressModel model)
        {
            if ((bool)!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _addressService.AddAddressAsync(model, userId!);

            _notyf.Success("Thêm địa chỉ thành công!");
            return RedirectToAction("Profile");
        }

        public async Task<IActionResult> DeleteAddress(int id)
        {
            if ((bool)!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _addressService.DeleteAddressAsync(id, userId!);
            _notyf.Success("Xóa địa chỉ thành công!");
            return RedirectToAction("Profile");
        }

        public async Task<IActionResult> SetDefaultAddress(int id)
        {
            if ((bool)!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _addressService.SetDefaultAddressAsync(id, userId!);
            _notyf.Success("Đổi địa chỉ mặc định thành công!");
            return RedirectToAction("Profile");
        }

        public async Task<IActionResult> CancelOrder(string code)
        {
            if ((bool)!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var order = await _dataContext.Orders.Where(x => x.OrderCode == code).FirstAsync();
                order.Status = 2;
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Cancel order successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return RedirectToAction("Profile", "Account");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(AppUserModel model)
        {
            var result = await _accountService.ProcessForgotPasswordAsync(model.Email, Request.Host.ToString(), Request.Scheme);

            if (result.Success)
            {
                _notyf.Success(result.Message);
            }
            else
            {
                _notyf.Error(result.Message);
            }
            
            return RedirectToAction("ForgotPassword", "Account");
        }

        public async Task<IActionResult> ForgotPasswordConfirm(AppUserModel model, string token)
        {
            var checkUser = await _userManager.Users.Where(x => x.Email == model.Email).Where(x => x.Token == token).FirstOrDefaultAsync();

            if (checkUser != null)
            {
                ViewBag.Email = checkUser.Email;
                ViewBag.Token = token;
            }
            else
            {
                _notyf.Error("Email not found or tokne is not right");
                return RedirectToAction("ForgotPassword", "Account");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePasswordConfirm(AppUserModel model, string token)
        {
            var checkUser = await _userManager.Users.Where(x => x.Email == model.Email).Where(x => x.Token == token).FirstOrDefaultAsync();

            if (checkUser != null)
            {
                string newToken = Guid.NewGuid().ToString();
                var password = new PasswordHasher<AppUserModel>();
                var passwordHash = password.HashPassword(checkUser, model.PasswordHash);

                checkUser.PasswordHash = passwordHash;
                checkUser.Token = newToken;

                await _userManager.UpdateAsync(checkUser);
                _notyf.Success("Password updated successfully");
                return RedirectToAction("Login", "Account");
            }
            else
            {
                _notyf.Error("Email not found or tokne is not right");
                return RedirectToAction("ForgotPassword", "Account");
            }
        }

        public async Task<IActionResult> Edit()
        {
            if ((bool)!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AppUserModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                _notyf.Error("User not found");
            }
            else
            {
                var password = new PasswordHasher<AppUserModel>();
                var passwordHash = password.HashPassword(user, model.PasswordHash);

                user.PasswordHash = passwordHash;
                user.PhoneNumber = model.PhoneNumber;
                _dataContext.Users.Update(user);
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Edited Account infomation successfully!");
                return RedirectToAction("Edit", "Account");
            }

            return View(user);
        }

        public async Task LoginGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            });
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return RedirectToAction("Login");
            }
            
            var claims = result.Principal.Identities.FirstOrDefault().Claims;
            var authResult = await _authService.HandleGoogleResponseAsync(claims);

            if (authResult.Success)
            {
                _notyf.Success(authResult.Message);
            }
            else
            {
                _notyf.Error(authResult.Message);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
