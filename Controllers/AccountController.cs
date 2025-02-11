using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public AccountController(DataContext context, UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signInManager, INotyfService notyf)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _notyf = notyf;
            _dataContext = context;
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
                order.Status = 3;
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Cancel order successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return RedirectToAction("Profile", "Account");
        }
    }
}
