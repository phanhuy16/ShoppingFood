using Microsoft.AspNetCore.Mvc;
using ShoppingFood.Models.Order;
using ShoppingFood.Services.Momo;

namespace ShoppingFood.Controllers
{
    public class PaymentController : Controller
    {
        private IMomoService _momoService;
        public PaymentController(IMomoService momoService)
        {
            _momoService = momoService;
        }

        [HttpPost]
        public async Task<IActionResult> PaymentMomo(OrderInfoModel model)
        {
            var response = await _momoService.PaymentMomo(model);
            return Redirect(response.PayUrl);
        }

        public IActionResult PaymentCallBack()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            return View(response);
        }
    }
}
