using Microsoft.AspNetCore.Mvc;

namespace ShoppingFood.Controllers
{
    public class RealtimeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
