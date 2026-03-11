using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Models;
using ShoppingFood.Models.ViewModel;
using ShoppingFood.Repository;
using ShoppingFood.Services.Product;
using System.Security.Claims;

namespace ShoppingFood.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IProductService _productService;

        public ProductController(DataContext context, INotyfService notyf, UserManager<AppUserModel> userManager, IProductService productService)
        {
            _dataContext = context;
            _notyf = notyf;
            _userManager = userManager;
            _productService = productService;
        }

        public async Task<IActionResult> Index(string sort_by = "", string startprice = "", string endprice = "", int page = 1)
        {
            const int pageSize = 6;
            if (page < 1) page = 1;

            var (products, totalCount) = await _productService.GetFilteredProductsAsync(sort_by, startprice, endprice, page, pageSize);

            var pager = new Paginate(totalCount, page, pageSize);
            ViewBag.Page = pager;

            var bestSellers = await _productService.GetBestSellersAsync(4);
            ViewBag.BestSellers = bestSellers;
            ViewBag.ProductRatings = await _productService.GetProductRatingsAsync(bestSellers);

            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> Search(string keyword)
        {
            var keys = await _dataContext.Products
                .Include(x => x.Category)
                .Where(x => x.Name.Contains(keyword) || x.Description.Contains(keyword) || x.Slug.Contains(keyword))
                .ToListAsync();

            var bestSellers = await _productService.GetBestSellersAsync(4);
            ViewBag.BestSellers = bestSellers;
            ViewBag.ProductRatings = await _productService.GetProductRatingsAsync(bestSellers);
            ViewBag.Keyword = keyword;

            return View(keys);
        }

        public async Task<IActionResult> Details(int id, string slug = "")
        {
            if (id <= 0 && slug == null)
                return RedirectToAction("Index");

            var productById = await _productService.GetProductDetailsAsync(id, slug);

            if (productById == null)
            {
                _notyf.Error("Product not found!");
                return RedirectToAction("Index");
            }

            var bestSellers = await _productService.GetBestSellersAsync(4);
            var related = await _dataContext.Products
                .Where(x => x.CategoryId == productById.CategoryId && x.Id != productById.Id)
                .Include(x => x.Category)
                .Take(4)
                .ToListAsync();

            var reviews = await _dataContext.Reviews
                .Where(x => x.ProductId == id)
                .Include(x => x.User)
                .ToListAsync();

            ViewBag.Related = related;
            ViewBag.AverageRating = await _productService.GetAverageRatingAsync(id);
            ViewBag.ProductRatings = await _productService.GetProductRatingsAsync(bestSellers);
            ViewBag.BestSellers = bestSellers;

            var review = new ProductRatingViewModel
            {
                Product = productById,
                Reviews = reviews
            };

            return View(review);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Comment(ReviewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) return Unauthorized();

            if (ModelState.IsValid)
            {
                var reviews = new ReviewModel
                {
                    UserId = user.Id,
                    ProductId = model.ProductId,
                    Comment = model.Comment,
                    Star = model.Star,
                    CreatedDate = DateTime.Now,
                    User = user
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
                    foreach (var error in modelState.Errors)
                        errors.Add(error.ErrorMessage);

                return BadRequest(string.Join("\n", errors));
            }
        }
    }
}
