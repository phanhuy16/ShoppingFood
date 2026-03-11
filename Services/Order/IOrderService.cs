#nullable enable
using ShoppingFood.Models;

namespace ShoppingFood.Services.Order
{
  public interface IOrderService
  {
    Task<string> CreateOrderAsync(string email, string paymentMethod, string? paymentId, decimal shippingPrice, string? coupon, List<CartItemModel> cart);
    Task<decimal> GetShippingPriceAsync(string tinh, string quan, string phuong);
    Task<bool> CancelOrderAsync(string orderCode, string userEmail);
  }
}
