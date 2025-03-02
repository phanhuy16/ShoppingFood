using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Models;
using ShoppingFood.Repository;

namespace ShoppingFood.Controllers
{
    public class ContactController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;

        public ContactController(DataContext context, UserManager<AppUserModel> userManager, INotyfService notyf)
        {
            _userManager = userManager;
            _dataContext = context;
            _notyf = notyf;
        }
        public async Task<IActionResult> Index()
        {
            var contact = await _dataContext.Contacts.FirstAsync();
            return View(contact);
        }
    }
}
