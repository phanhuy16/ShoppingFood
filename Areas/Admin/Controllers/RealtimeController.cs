using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShoppingFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class RealtimeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
