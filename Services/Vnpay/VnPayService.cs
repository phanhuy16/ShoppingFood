using ShoppingFood.Libraries;
using ShoppingFood.Models.Vnpay;
using Microsoft.Extensions.Options;
using ShoppingFood.Models.Configuration;

namespace ShoppingFood.Services.Vnpay
{
    public class VnPayService : IVnPayService
    {
        private readonly IOptions<VnpaySettings> _vnpaySettings;
        private readonly IConfiguration _configuration;

        public VnPayService(IOptions<VnpaySettings> vnpaySettings, IConfiguration configuration)
        {
            _vnpaySettings = vnpaySettings;
            _configuration = configuration;
        }

        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallBack = _vnpaySettings.Value.PaymentBackReturnUrl;

            pay.AddRequestData("vnp_Version", _vnpaySettings.Value.Version);
            pay.AddRequestData("vnp_Command", _vnpaySettings.Value.Command);
            pay.AddRequestData("vnp_TmnCode", _vnpaySettings.Value.TmnCode);
            pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _vnpaySettings.Value.CurrCode);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _vnpaySettings.Value.Locale);
            pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {model.Amount}");
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl =
                pay.CreateRequestUrl(_vnpaySettings.Value.BaseUrl, _vnpaySettings.Value.HashSecret);

            return paymentUrl;
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _vnpaySettings.Value.HashSecret);

            return response;
        }
    }
}
