using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ShoppingFood.Repository.Components
{

    public class ProductByCateViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;
        public ProductByCateViewComponent(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var productByCate = await _dataContext.Products.Include(x => x.Category).Where(x => x.Category.Name == "Vegetables").OrderByDescending(x => x.Id).ToListAsync();
            return View(productByCate);
        }
    }
}
