using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Models;
using ShoppingFood.Repository;
using ShoppingFood.Services.Pdf;

namespace ShoppingFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;
        private readonly IInvoiceService _invoiceService;

        public OrderController(DataContext dataContext, INotyfService notyf, IInvoiceService invoiceService)
        {
            _dataContext = dataContext;
            _notyf = notyf;
            _invoiceService = invoiceService;
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
                return NotFound(new { success = false, message = "Order not found" });
            }

            // Validation: Prevent changes to terminal statuses (Completed = 0, Cancelled = 2)
            if (order.Status == 0 || order.Status == 2)
            {
                return BadRequest(new { success = false, message = "Cannot update a completed or cancelled order." });
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
                return BadRequest(new { success = false, message = "Invalid status transition." });
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
                return Ok(new { success = true, message = "Order status updated successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error updating order status: " + ex.Message });
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
