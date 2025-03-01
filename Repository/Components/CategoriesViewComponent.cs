using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ShoppingFood.Repository.Components
{
    public class CategoriesViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;
        public CategoriesViewComponent(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _dataContext.Categories.ToListAsync();
            return View(categories);
        }

    }
}
