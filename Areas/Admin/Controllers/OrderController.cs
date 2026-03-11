using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Models;
using ShoppingFood.Repository;
using ShoppingFood.Services.Pdf;
using ShoppingFood.Services.Order;

namespace ShoppingFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;
        private readonly IInvoiceService _invoiceService;
        private readonly IOrderService _orderService;

        public OrderController(DataContext dataContext, INotyfService notyf, IInvoiceService invoiceService, IOrderService orderService)
        {
            _dataContext = dataContext;
            _notyf = notyf;
            _invoiceService = invoiceService;
            _orderService = orderService;
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
            var result = await _orderService.UpdateOrderStatusAsync(orderCode, status);

            if (result.success)
            {
                return Ok(new { success = true, message = result.message });
            }
            else
            {
                if (result.message == "Order not found")
                    return NotFound(new { success = false, message = result.message });
                else if (result.message.StartsWith("Error"))
                    return StatusCode(500, new { success = false, message = result.message });
                else
                    return BadRequest(new { success = false, message = result.message });
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

        public async Task<IActionResult> PaymentMomoInfo (string orderId)
        {
            var momo = await _dataContext.MomoInfos.FirstOrDefaultAsync(x => x.OrderId == orderId);

            if(momo == null)
            {
                _notyf.Error("Momo not found");
            }

            return View(momo);
        }

        public async Task<IActionResult> PaymentVnpayInfo(string orderId)
        {
            var vnpay = await _dataContext.Vnpays.FirstOrDefaultAsync(x => x.PaymentId == orderId);

            if (vnpay == null)
            {
                _notyf.Error("Momo not found");
            }

            return View(vnpay);
        }

        public async Task<IActionResult> ExportInvoicePdf(string code)
        {
            var bytes = await _invoiceService.GenerateInvoicePdfAsync(code);
            if (bytes == null)
            {
                _notyf.Error("Order not found or could not generate PDF");
                return RedirectToAction("Index");
            }
            return File(bytes, "application/pdf", $"Invoice_{code}.pdf");
        }
    }
}
