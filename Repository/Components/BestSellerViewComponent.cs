using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ShoppingFood.Repository.Components
{
    public class BestSellerViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;
        public BestSellerViewComponent(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var productSeller = await _dataContext.Products.Where(x => x.PriceSale < x.Price).OrderByDescending(x => x.Sold).ToListAsync();

            // Dictionary để lưu trung bình sao cho từng sản phẩm best-seller
            var productRatings = new Dictionary<int, double>();
            foreach (var product in productSeller)
            {
                var starList = await _dataContext.Reviews
                    .Where(x => x.ProductId == product.Id)
                    .ToListAsync();
                double avgRating = starList.Any() ? starList.Average(x => x.Star) : 0;
                productRatings[product.Id] = avgRating;
            }

            // Truyền dictionary vào ViewBag
            ViewBag.ProductRatings = productRatings;

            return View(productSeller);
        }
    }
}
