using System.Diagnostics;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Models;
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

        public async Task<IActionResult> Index()
        {
            var products = await _dataContext.Products.OrderByDescending(x => x.Id).ToListAsync();

            var slirders = await _dataContext.Sliders.Where(x => x.Status == 1).ToListAsync();
            ViewBag.Sliders = slirders;

            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Contact()
        {
            var contact = await _dataContext.Contacts.FirstAsync();
            return View(contact);
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
