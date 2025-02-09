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
    public class ShippingController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;

        public ShippingController(DataContext context, INotyfService notyf)
        {
            _dataContext = context;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index()
        {
            var shipping = await _dataContext.Shippings.OrderByDescending(x => x.Id).ToListAsync();
            ViewBag.Shipping = shipping;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Store(ShippingModel model, string tinh, string quan, string phuong, decimal price)
        {
            model.Ward = phuong;
            model.District = quan;
            model.City = tinh;
            model.Price = price;

            try
            {
                var exsitShipping = await _dataContext.Shippings.AnyAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);

                if (exsitShipping)
                {
                    return Ok(new { duplicate = true, message = "Dữ liệu bị trùng lặp" });
                }

                await _dataContext.Shippings.AddAsync(model);
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Thêm phí vận chuyển thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var shipping = await _dataContext.Shippings.FindAsync(id);
            _dataContext.Shippings.Remove(shipping);
            await _dataContext.SaveChangesAsync();
            _notyf.Success("Deleted shipping successfully!");
            return RedirectToAction("Index");
        }
    }
}
