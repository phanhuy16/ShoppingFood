using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Models;
using ShoppingFood.Models.ViewModel;

namespace ShoppingFood.Repository.Components
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;
        public FooterViewComponent(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var contact = await _dataContext.Contacts.FirstOrDefaultAsync();
            return View(new FooterViewModel { Contact = contact});
        }
    }
}
