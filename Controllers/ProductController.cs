using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Models;
using ShoppingFood.Models.ViewModel;
using ShoppingFood.Repository;

namespace ShoppingFood.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;

        public ProductController(DataContext context, INotyfService notyf)
        {
            _dataContext = context;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index(string sort_by = "", string startprice = "", string endprice = "")
        {
            var products = _dataContext.Products.Include(x => x.Category).Where(x => x.Status == 1);

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

            var productById = await _dataContext.Products.Include(x => x.Category).Include(x => x.Rating).Where(x => x.Id == id && x.Slug == slug).FirstOrDefaultAsync();

            var related = await _dataContext.Products.Where(x => x.CategoryId == productById.CategoryId && x.Id != productById.Id).Include(x => x.Category).Take(4).ToListAsync();

            ViewBag.Related = related;

            var ratings = await _dataContext.Ratings.Where(x => x.ProductId == id).ToListAsync();

            var rating = new ProductRatingViewModel
            {
                Product = productById,
                Rating = ratings.Count > 0 ? ratings[0] : null
            };

            return View(rating);
        }

        [HttpPost]
        public async Task<IActionResult> Comment(RatingModel model)
        {
            if (ModelState.IsValid)
            {
                var ratings = new RatingModel
                {
                    ProductId = model.ProductId,
                    Comment = model.Comment,
                    Customer = model.Customer,
                    Email = model.Email,
                    Star = model.Star,
                    ModifierDate = DateTime.Now,
                    ModifierBy = User.Identity.Name,
                    CreatedDate = DateTime.Now
                };
                await _dataContext.Ratings.AddAsync(ratings);
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
