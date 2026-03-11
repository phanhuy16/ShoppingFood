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

    public async Task<(bool success, string message)> UpdateOrderStatusAsync(string orderCode, int status)
    {
        var order = await _dataContext.Orders.FirstOrDefaultAsync(x => x.OrderCode == orderCode);

        if (order == null)
        {
            return (false, "Order not found");
        }

        // Validation: Prevent changes to terminal statuses (Completed = 0, Cancelled = 2)
        if (order.Status == 0 || order.Status == 2)
        {
            return (false, "Cannot update a completed or cancelled order.");
        }

        // Validation: Prevent moving backward (except for cancelling)
        // Sequence: New (1) -> Preparing (3) -> Shipping (4) -> Completed (0)
        // Cancelled (2) is allowed from 1, 3, or 4.
        bool isValidTransition = false;
        if (status == 2) isValidTransition = true; // Can always cancel from non-terminal states
        else if (order.Status == 1 && status == 3) isValidTransition = true;
        else if (order.Status == 3 && status == 4) isValidTransition = true;
        else if (order.Status == 4 && status == 0) isValidTransition = true;
        else if (order.Status == status) isValidTransition = true; // No change

        if (!isValidTransition)
        {
            return (false, "Invalid status transition.");
        }

        order.Status = status;

        if (status == 1 && order.ApprovedDate == null)
        {
            order.ApprovedDate = DateTime.Now;
        }
        else if (status == 3)
        {
            if (order.ApprovedDate == null) order.ApprovedDate = DateTime.Now;
            order.PreparedDate = DateTime.Now;
        }
        else if (status == 4)
        {
            if (order.ApprovedDate == null) order.ApprovedDate = DateTime.Now;
            if (order.PreparedDate == null) order.PreparedDate = DateTime.Now;
            order.DeliveredDate = DateTime.Now; // Used for "Đang giao" date
        }

        if (status == 0)
        {
            if (order.DeliveredDate == null) order.DeliveredDate = DateTime.Now;

            var details = await _dataContext.OrderDetails.Include(x => x.Product).Where(x => x.OrderCode == orderCode).Select(x => new
            {
                x.Quantity,
                x.Product.Price,
                x.Product.CapitalPrice,
            }).ToListAsync();

            var statistical = await _dataContext.Statisticals.FirstOrDefaultAsync(x => x.CreatedDate.Date == order.CreatedDate.Date);

            if (statistical != null)
            {
                foreach (var item in details)
                {
                    statistical.Quantity += 1;
                    statistical.Sold += item.Quantity;
                    statistical.Revenue += item.Quantity * item.Price;
                    statistical.Profit += (item.Price - item.CapitalPrice) * item.Quantity;
                }
                _dataContext.Statisticals.Update(statistical);
            }
            else
            {
                int new_quantity = 0;
                int new_sold = 0;
                decimal new_profit = 0;
                decimal total_revenue = 0;
                foreach (var item in details)
                {
                    new_quantity += 1;
                    new_sold += item.Quantity;
                    new_profit += (item.Price - item.CapitalPrice) * item.Quantity;
                    total_revenue += item.Quantity * item.Price;
                }
                statistical = new StatisticalModel
                {
                    CreatedDate = order.CreatedDate,
                    Quantity = new_quantity,
                    Sold = new_sold,
                    Revenue = total_revenue,
                    Profit = new_profit
                };
                await _dataContext.Statisticals.AddAsync(statistical);
            }
        }

        try
        {
            _dataContext.Orders.Update(order);
            await _dataContext.SaveChangesAsync();
            return (true, "Order status updated successfully!");
        }
        catch (Exception ex)
        {
            return (false, "Error updating order status: " + ex.Message);
        }
    }
  }
}
