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

        public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.Banners.FirstOrDefaultAsync());
    }
}
