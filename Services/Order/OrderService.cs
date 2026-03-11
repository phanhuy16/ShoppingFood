#nullable enable
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Areas.Admin.Repository;
using ShoppingFood.Models;
using ShoppingFood.Repository;

namespace ShoppingFood.Services.Order
{
  public class OrderService : IOrderService
  {
    private readonly DataContext _dataContext;
    private readonly IEmailSender _emailSender;

    public OrderService(DataContext dataContext, IEmailSender emailSender)
    {
      _dataContext = dataContext;
      _emailSender = emailSender;
    }

    public async Task<string> CreateOrderAsync(string email, string paymentMethod, string? paymentId, decimal shippingPrice, string? coupon, List<CartItemModel> cart)
    {
      var orderCode = Guid.NewGuid().ToString();

      var order = new OrderModel
      {
        OrderCode = orderCode,
        UserName = email,
        PaymentMethod = string.IsNullOrEmpty(paymentId) ? "COD" : $"{paymentMethod} {paymentId}",
        ShippingCode = shippingPrice,
        CouponCode = coupon,
        CreatedDate = DateTime.Now,
        Status = 1
      };

      await _dataContext.Orders.AddAsync(order);
      await _dataContext.SaveChangesAsync();

      foreach (var item in cart)
      {
        var orderDetail = new OrderDetailModel
        {
          OrderCode = orderCode,
          Price = item.Price,
          Quantity = item.Quantity,
          ProductId = item.ProductId,
          VariantId = item.VariantId,
          VariantName = item.VariantName,
          UserName = email
        };

        var product = await _dataContext.Products.Where(x => x.Id == item.ProductId).FirstAsync();
        product.Quantity -= item.Quantity;
        product.Sold += item.Quantity;
        _dataContext.Products.Update(product);

        await _dataContext.OrderDetails.AddAsync(orderDetail);
        await _dataContext.SaveChangesAsync();
      }

      // Gửi email xác nhận
      await _emailSender.SendEmailAsync(
          email,
          "Đặt hàng thành công",
          $"Đơn hàng của bạn đã được đặt thành công, mã đơn hàng của bạn là: {orderCode}"
      );

      return orderCode;
    }

    public async Task<decimal> GetShippingPriceAsync(string tinh, string quan, string phuong)
    {
      var existShipping = await _dataContext.Shippings
          .FirstOrDefaultAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);

      return existShipping?.Price ?? 30000m;
    }

    public async Task<bool> CancelOrderAsync(string orderCode, string userEmail)
    {
      var order = await _dataContext.Orders
          .FirstOrDefaultAsync(x => x.OrderCode == orderCode && x.UserName == userEmail);

      if (order == null || order.Status != 1)
        return false;

      order.Status = 2; // 2 = Đã hủy
      await _dataContext.SaveChangesAsync();
      return true;
    }
  }
}
