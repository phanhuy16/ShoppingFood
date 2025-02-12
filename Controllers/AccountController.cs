using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Areas.Admin.Repository;
using ShoppingFood.Models;
using ShoppingFood.Repository;
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

        public AccountController(DataContext context, UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signInManager, INotyfService notyf, IEmailSender email)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _notyf = notyf;
            _dataContext = context;
            _emailSender = email;
        }

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
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
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

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var newUser = new AppUserModel
                {
                    UserName = model.Username,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(newUser, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
                    _notyf.Success("Register successfully!");
                    return RedirectToAction("Index", "Home");
                }

                _notyf.Error("Register fail!");
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync();
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

            var orders = await _dataContext.Orders.Where(x => x.UserName == userEmail).OrderByDescending(x => x.Id).ToListAsync();

            var wishlist = await (from w in _dataContext.Wishlists
                                  join p in _dataContext.Products on w.ProductId equals p.Id
                                  join u in _dataContext.Users on w.UserId equals u.Id
                                  select new { User = u, Product = p, Wishlist = w }).ToListAsync();

            var compare = await (from c in _dataContext.Compares
                                 join p in _dataContext.Products on c.ProductId equals p.Id
                                 join u in _dataContext.Users on c.UserId equals u.Id
                                 select new { User = u, Product = p, Compare = c }).ToListAsync();

            ViewBag.Wishlist = wishlist;
            ViewBag.Compare = compare;
            ViewBag.UserEmail = userEmail;

            return View(orders);
        }

        public async Task<IActionResult> DeteleWishlist(int id)
        {
            var wishlist = await _dataContext.Wishlists.FindAsync(id);

            if (wishlist != null)
            {
                _dataContext.Wishlists.Remove(wishlist);
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Deleted successfully!");
                return RedirectToAction("Profile");
            }
            else
            {
                _notyf.Error("Wishlist not found!");
                return RedirectToAction("Profile");
            }
        }

        public async Task<IActionResult> DeteleCompare(int id)
        {
            var compare = await _dataContext.Compares.FindAsync(id);

            if (compare != null)
            {
                _dataContext.Compares.Remove(compare);
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Deleted successfully!");
                return RedirectToAction("Profile");
            }
            else
            {
                _notyf.Error("Compare not found!");
                return RedirectToAction("Profile");
            }
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
            var checkMail = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == model.Email);

            if (checkMail == null)
            {
                _notyf.Error("Email not found!");
                return RedirectToAction("ForgotPassword", "Account");
            }
            else
            {
                string token = Guid.NewGuid().ToString();
                checkMail.Token = token;
                _dataContext.Users.Update(checkMail);
                await _dataContext.SaveChangesAsync();

                var receiver = checkMail.Email;
                var subject = "Change password for user " + checkMail.Email;
                var message = "Click on link to change password " + "<a href='" + $"{Request.Scheme}://{Request.Host}/Account/ForgotPasswordConfirm?email=" + checkMail.Email + "&token=" + token + "'>";

                await _emailSender.SendEmailAsync(receiver, subject, message);
            }
            _notyf.Success("Send email successfully");
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
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return RedirectToAction("Login");
            }
            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claims => new
            {
                claims.Issuer,
                claims.OriginalIssuer,
                claims.Type,
                claims.Value
            });
            var email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            string name = email.Split('@')[0];
            // check user có tồn tại không
            var exitsUser = await _userManager.FindByEmailAsync(email);
            // nếu không tồn tại trong db thì tạo user mới với password hashed mặc địng 1-9
            if (exitsUser == null)
            {
                var password = new PasswordHasher<AppUserModel>();
                var passwordHasher = password.HashPassword(null, "123456789");

                var newUser = new AppUserModel { UserName = name, Email = email, };
                newUser.PasswordHash = passwordHasher;

                var createUser = await _userManager.CreateAsync(newUser);
                if (!createUser.Succeeded)
                {
                    _notyf.Error("Đăng ký tài khoản thất bại. Vui lòng thử lại sau");
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    await _signInManager.SignInAsync(newUser, isPersistent: false);
                    _notyf.Success("Đăng ký tài khoản thành công");
                    return RedirectToAction("Index", "Home");
                }
            } else
            {
                await _signInManager.SignInAsync(exitsUser, isPersistent: false);
            }
            return RedirectToAction("Login", "Account");
        }
    }
}
