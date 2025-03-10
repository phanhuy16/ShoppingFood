using System.Diagnostics;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Models;
using ShoppingFood.Models.ViewModel;
using ShoppingFood.Repository;

namespace ShoppingFood.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly DataContext _dataContext;
        private readonly ILogger<HomeController> _logger;
        private readonly INotyfService _notyf;

        public HomeController(ILogger<HomeController> logger, DataContext context, UserManager<AppUserModel> userManager, INotyfService notyf)
        {
            _userManager = userManager;
            _logger = logger;
            _dataContext = context;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index(string slug = "")
        {
            IQueryable<ProductModel> query = _dataContext.Products.Include(x => x.Category);

            if (!string.IsNullOrEmpty(slug))
            {
                var category = await _dataContext.Categories.FirstOrDefaultAsync(x => x.Slug == slug);
                if (category == null)
                {
                    return RedirectToAction("Index");
                }
            }

            if (!string.IsNullOrEmpty(slug))
            {
                query = query.Where(x => x.Category.Slug == slug);
            }

            var products = await query.OrderByDescending(x => x.Id).ToListAsync();

            var slirders = await _dataContext.Sliders.Where(x => x.Status == 1).ToListAsync();
            ViewBag.Sliders = slirders;

            var categories = await _dataContext.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Categories = categories;

            ViewBag.Slug = slug;

            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddWishlist(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var wishlist = new WishlistModel
            {
                ProductId = id,
                UserId = user.Id
            };

            await _dataContext.Wishlists.AddAsync(wishlist);

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Product added to wishlist successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCompare(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var compare = new CompareModel
            {
                ProductId = id,
                UserId = user.Id
            };

            await _dataContext.Compares.AddAsync(compare);

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Product added to compare successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe(FooterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem email đã tồn tại chưa
                var emailExists = await _dataContext.Subscribes.FirstOrDefaultAsync(s => s.Email == model.Subscribe.Email);
                if (emailExists != null)
                {
                    _notyf.Warning("Email đã được đăng ký.");
                    return RedirectToAction("Index");
                }
                var subscribe = new SubscribeModel
                {
                    Email = model.Subscribe.Email,
                    CreatedDate = DateTime.Now
                };
                await _dataContext.Subscribes.AddAsync(subscribe);
                try
                {
                    await _dataContext.SaveChangesAsync();
                    _notyf.Success("Subscribe successfully.");
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    _notyf.Error("Subscribe failed.");
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Search(string keyword)
        {
            var keys = await _dataContext.Products.Include(x => x.Category).Where(x => x.Name.Contains(keyword) || x.Description.Contains(keyword) || x.Slug.Contains(keyword)).ToListAsync();
            ViewBag.Keyword = keyword;
            return View(keys);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if (statuscode == 404)
            {
                return View("NotFound");
            }
            else
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
}
