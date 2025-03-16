using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Repository;

namespace ShoppingFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DashboardController(DataContext context, INotyfService notyf, IWebHostEnvironment environment)
        {
            _dataContext = context;
            _notyf = notyf;
            _webHostEnvironment = environment;
        }
        public IActionResult Index()
        {
            var count_product = _dataContext.Products.Count();
            var count_order = _dataContext.Orders.Count();
            var count_category = _dataContext.Categories.Count();
            var count_user = _dataContext.Users.Count();
            var count_procate = _dataContext.ProductCategories.Count();

            ViewBag.CountProduct = count_product;
            ViewBag.CountOrder = count_order;
            ViewBag.CountCategory = count_category;
            ViewBag.CountUser = count_user;
            ViewBag.CountProCate = count_procate;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetData()
        {
            var data = await _dataContext.Statisticals.Select(x => new
            {
                date = x.CreatedDate.ToString("yyyy-MM-dd"),
                sold = x.Sold,
                quantity = x.Quantity,
                revenue = x.Revenue,
                profit = x.Profit,
            }).ToListAsync();

            return Json(data);
        }

        [HttpPost]
        public async Task<IActionResult> GetDataByDate(DateTime startDate, DateTime endDate)
        {
            var data = await _dataContext.Statisticals.Where(x => x.CreatedDate >= startDate && x.CreatedDate <= endDate).Select(x => new
            {
                date = x.CreatedDate.ToString("yyyy-MM-dd"),
                sold = x.Sold,
                quantity = x.Quantity,
                revenue = x.Revenue,
                profit = x.Profit,
            }).ToListAsync();

            return Json(data);
        }

        [HttpPost]
        public async Task<IActionResult> FilterDate(DateTime? fromDate, DateTime? toDate)
        {
            var query = _dataContext.Statisticals.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.CreatedDate >= fromDate);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.CreatedDate >= toDate);
            }

            var data = await query.Select(x => new
            {
                date = x.CreatedDate.ToString("yyyy-MM-dd"),
                sold = x.Sold,
                quantity = x.Quantity,
                revenue = x.Revenue,
                profit = x.Profit,
            }).ToListAsync();

            return Json(data);
        }
    }
}
