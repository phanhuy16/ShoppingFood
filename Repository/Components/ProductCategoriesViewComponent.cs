using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ShoppingFood.Repository.Components
{
    public class ProductCategoriesViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;
        public ProductCategoriesViewComponent(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var productCategories = await _dataContext.ProductCategories.OrderByDescending(x => x.CreatedDate).ToListAsync();
            return View(productCategories);
        }
    }
}
