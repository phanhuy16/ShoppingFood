using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ShoppingFood.Repository.Components
{
    public class CouponViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;
        public CouponViewComponent(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var coupons = await _dataContext.Coupons.OrderByDescending(x => x.CreatedDate).Take(3).ToListAsync();
            return View(coupons);
        }
    }
}
