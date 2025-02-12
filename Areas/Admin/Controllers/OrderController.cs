using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Models;
using ShoppingFood.Repository;

namespace ShoppingFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;

        public OrderController(DataContext dataContext, INotyfService notyf)
        {
            _dataContext = dataContext;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index()
        {
            var order = await _dataContext.Orders.OrderByDescending(x => x.Id).ToListAsync();
            return View(order);
        }

        public async Task<IActionResult> ViewOrder(string code)
        {
            var detail = await _dataContext.OrderDetails.Include(x => x.Product).Where(x => x.OrderCode == code).ToListAsync();

            var orders = _dataContext.Orders.Where(x => x.OrderCode == code).First();
            ViewBag.ShippingCost = orders.ShippingCode;
            ViewBag.Status = orders.Status;

            return View(detail);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string orderCode, int status)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(x => x.OrderCode == orderCode);

            if (order == null)
            {
                _notyf.Error("Order not found");
            }

            order.Status = status;
            _dataContext.Orders.Update(order);
            if (status == 0)
            {
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
                        statistical.Profit += item.Price - item.CapitalPrice;
                    }
                    _dataContext.Statisticals.Update(statistical);
                }
                else
                {
                    int new_quantity = 0;
                    int new_sold = 0;
                    decimal new_profit = 0;
                    foreach (var item in details)
                    {
                        new_quantity += 1;
                        new_sold += item.Quantity;
                        new_profit += item.Price - item.CapitalPrice;
                        statistical = new StatisticalModel
                        {
                            CreatedDate = order.CreatedDate,
                            Quantity = new_quantity,
                            Sold = new_sold,
                            Revenue = item.Quantity * item.Price,
                            Profit = new_profit
                        };
                    }
                    await _dataContext.Statisticals.AddAsync(statistical);
                }

            }
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Order status update successfully!" });
            }
            catch (Exception)
            {
                _notyf.Error("Error updating order status");
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var order = await _dataContext.Orders.FindAsync(id);
            if (order != null)
            {
                _dataContext.Orders.Remove(order);
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Deleted order successfully!");
                return RedirectToAction("Index");
            }
            _notyf.Error("Order not found!");
            return View(order);
        }
    }
}
