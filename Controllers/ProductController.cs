using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Models;
using ShoppingFood.Models.ViewModel;
using ShoppingFood.Repository;
using System.Security.Claims;

namespace ShoppingFood.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;
        private readonly UserManager<AppUserModel> _userManager;

        public ProductController(DataContext context, INotyfService notyf, UserManager<AppUserModel> userManager)
        {
            _dataContext = context;
            _notyf = notyf;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string sort_by = "", string startprice = "", string endprice = "")
        {
            var products = _dataContext.Products.Include(x => x.Category).Where(x => x.Status == 1);

            var bestSellers = await _dataContext.Products.Include(x => x.Category).Where(x => x.Status == 1 && x.PriceSale < x.Price).OrderByDescending(x => x.Sold).Take(4).ToListAsync();

            if (sort_by == "price_increase")
            {
                products = products.OrderBy(x => x.Price);
            }
            else if (sort_by == "price_decrease")
            {
                products = products.OrderByDescending(x => x.Price);
            }
            else if (sort_by == "price_newest")
            {
                products = products.OrderByDescending(x => x.Id);
            }
            else if (sort_by == "price_oldest")
            {
                products = products.OrderBy(x => x.Id);
            }
            else if (startprice != "" && endprice != "")
            {
                decimal start;
                decimal end;
                if (decimal.TryParse(startprice, out start) && decimal.TryParse(endprice, out end))
                {
                    products = products.Where(x => x.Price >= start && x.Price <= end);
                }
                else
                {
                    products = products.OrderByDescending(x => x.Id);
                }
            }
            else
            {
                products = products.OrderByDescending(x => x.Id);
            }

            ViewBag.BestSellers = bestSellers;

            return View(await products.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Search(string keyword)
        {
            var keys = await _dataContext.Products.Include(x => x.Category).Where(x => x.Name.Contains(keyword) || x.Description.Contains(keyword) || x.Slug.Contains(keyword)).ToListAsync();

            ViewBag.Keyword = keyword;

            return View(keys);
        }

        public async Task<IActionResult> Details(int id, string slug = "")
        {
            if (id <= 0 && slug == null)
            {
                return RedirectToAction("Index");
            }

            var productById = await _dataContext.Products.Include(x => x.Category).Where(x => x.Id == id && x.Slug == slug).FirstOrDefaultAsync();

            var bestSellers = await _dataContext.Products.Include(x => x.Category).Where(x => x.Status == 1 && x.PriceSale < x.Price).OrderByDescending(x => x.Sold).Take(4).ToListAsync();

            if (productById == null)
            {
                _notyf.Error("Product not found!");
            }

            var related = await _dataContext.Products.Where(x => x.CategoryId == productById.CategoryId && x.Id != productById.Id).Include(x => x.Category).Take(4).ToListAsync();

            ViewBag.Related = related;

            var reviews = await _dataContext.Reviews.Where(x => x.ProductId == id).Include(x => x.Users).ToListAsync();

            var review = new ProductRatingViewModel
            {
                Product = productById,
                Reviews = reviews
            };

            ViewBag.BestSellers = bestSellers;

            return View(review);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Comment(ReviewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return Unauthorized();

            if (ModelState.IsValid)
            {
                var reviews = new ReviewModel
                {
                    UserId = user.Id,
                    ProductId = model.ProductId,
                    Comment = model.Comment,
                    Star = model.Star,
                    CreatedDate = DateTime.Now
                };
                await _dataContext.Reviews.AddAsync(reviews);
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Comment successfully!");
                return Redirect(Request.Headers["Referer"]);
            }
            else
            {
                List<string> errors = new List<string>();
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
        }
    }
}
