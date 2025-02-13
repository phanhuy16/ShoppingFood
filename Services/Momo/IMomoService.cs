using ShoppingFood.Models.Momo;
using ShoppingFood.Models.Order;

namespace ShoppingFood.Services.Momo
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> PaymentMomo(OrderInfoModel model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}
