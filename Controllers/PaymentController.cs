using Microsoft.AspNetCore.Mvc;
using ShoppingFood.Models.Order;
using ShoppingFood.Models.Vnpay;
using ShoppingFood.Services.Momo;
using ShoppingFood.Services.Vnpay;

namespace ShoppingFood.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IMomoService _momoService;
        private readonly IVnPayService _vnPayService;

        public PaymentController(IMomoService momoService, IVnPayService vnPayService)
        {
            _momoService = momoService;
            _vnPayService = vnPayService;
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

        [HttpPost]
        public IActionResult PaymentVnPay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }
    }
}
