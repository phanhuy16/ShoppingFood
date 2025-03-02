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
            var productSeller = await _dataContext.Products.Where(x => x.PriceSale < x.Price).OrderByDescending(x => x.CreatedDate).ToListAsync();
            return View(productSeller);
        }
    }
}
