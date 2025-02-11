using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            try
            {
                _dataContext.Orders.Update(order);
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true });
            }
            catch (Exception)
            {
                _notyf.Error("Error updating order status");
                return StatusCode(500);
            }

        }

        public async Task<IActionResult>Delete(int id)
        {
            var order = await _dataContext.Orders.FindAsync(id);
            if(order !=null)
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
