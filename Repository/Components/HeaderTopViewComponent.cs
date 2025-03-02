using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ShoppingFood.Repository.Components
{
    public class HeaderTopViewComponent:ViewComponent
    {
        private readonly DataContext _dataContext;
        public HeaderTopViewComponent(DataContext context)
        {
            _dataContext = context;
        }

      
        public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.Contacts.FirstOrDefaultAsync());
    }
}
