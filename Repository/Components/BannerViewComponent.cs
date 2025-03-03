using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ShoppingFood.Repository.Components
{
    public class BannerViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;
        public BannerViewComponent(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string position) => View(await _dataContext.Banners.FirstOrDefaultAsync(x => x.Position == position));
    }
}
