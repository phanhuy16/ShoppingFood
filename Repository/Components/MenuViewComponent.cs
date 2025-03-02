using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ShoppingFood.Repository.Components
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;
        public MenuViewComponent(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menu = await _dataContext.Menus.OrderBy(x => x.Position).ToListAsync();
            return View(menu);
        }
    }
}
