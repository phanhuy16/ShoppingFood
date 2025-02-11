using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ShoppingFood.Areas.Admin.Repository;
using ShoppingFood.Models;
using ShoppingFood.Models.ViewModel;
using ShoppingFood.Repository;
using System.Security.Claims;

namespace ShoppingFood.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        private readonly INotyfService _notyf;

        public CartController(DataContext context, INotyfService notyf, IEmailSender email)
        {
            _dataContext = context;
            _notyf = notyf;
            _emailSender = email;
        }
        public IActionResult Index()
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;

            if (shippingPriceCookie != null)
            {
                var shippingPriceJson = shippingPriceCookie;
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
            }

            // nhận coupon từ cookie
            var coupon = Request.Cookies["CouponTitle"];

            CartItemViewModel cartItemViewModel = new CartItemViewModel()
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(x => x.Quantity * x.Price),
                ShippingCost = shippingPrice,
                CouponCode = coupon,
            };

            return View(cartItemViewModel);
        }

        public async Task<IActionResult> Add(int id)
        {
            var product = await _dataContext.Products.FindAsync(id);

            if (product == null)
            {
                // You could return an error view, a redirect, or a not found status
                return NotFound("Product not found.");
            }

            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            var cartItem = cart.Where(x => x.ProductId == id).FirstOrDefault();
            if (cartItem == null)
            {
                cart.Add(new CartItemModel(product));
            }
            else
            {
                cartItem.Quantity += 1;
            }

            HttpContext.Session.SetJson("Cart", cart);

            _notyf.Success("Add item to cart successfully!");

            return Ok(new { success = true, message = "Add item to cart successfully!" });
        }

        public async Task<IActionResult> Decrease(int id)
        {
            await Task.CompletedTask;
            var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

            if (cart == null)
            {
                return NotFound("Cart not found.");
            }

            var cartItem = cart.Where(x => x.ProductId == id).FirstOrDefault();

            if (cartItem == null)
            {
                return NotFound("Cart not found.");
            }

            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity -= 1;
            }
            else
            {
                cart.RemoveAll(x => x.ProductId == id);
            }

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }

            _notyf.Success("Decrease item quantity to cart successfully!");

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Increase(int id)
        {
            var product = await _dataContext.Products.Where(x => x.Id == id).FirstOrDefaultAsync();
            var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

            if (cart == null)
            {
                return NotFound("Cart is empty or not found.");
            }

            var cartItem = cart.Where(x => x.ProductId == id).FirstOrDefault();

            if (cartItem == null)
            {
                return NotFound($"Product with ID {id} not found in the cart.");
            }

            if (cartItem.Quantity >= 1 && product.Quantity > cartItem.Quantity)
            {
                cartItem.Quantity += 1;
                _notyf.Success("Increase item quantity to cart successfully!");
            }
            else
            {
                cartItem.Quantity = product.Quantity;
                _notyf.Warning("Maximum Quantity in Product");
            }

            HttpContext.Session.SetJson("Cart", cart);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int id)
        {
            await Task.CompletedTask;
            var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            if (cart != null)
            {
                cart.RemoveAll(x => x.ProductId == id);
                if (cart.Count == 0)
                {
                    HttpContext.Session.Remove("Cart");
                }
                else
                {
                    HttpContext.Session.SetJson("Cart", cart);
                }
                _notyf.Success("Remove item of cart successfully!");
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Clear()
        {
            await Task.CompletedTask;
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CheckOut()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var orderCode = Guid.NewGuid().ToString();
                var orderItem = new OrderModel();

                orderItem.OrderCode = orderCode;
                orderItem.UserName = email;

                var shippingPriceCookie = Request.Cookies["ShippingPrice"];
                decimal shippingPrice = 0;

                // nhận coupon từ cookie
                var coupon = Request.Cookies["CouponTitle"];

                if (shippingPriceCookie != null)
                {
                    var shippingPriceJson = shippingPriceCookie;
                    shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
                }
                orderItem.ShippingCode = shippingPrice;
                orderItem.CouponCode = coupon;

                orderItem.CreatedDate = DateTime.Now;
                orderItem.Status = 1;

                await _dataContext.Orders.AddAsync(orderItem);
                await _dataContext.SaveChangesAsync();

                List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                foreach (var item in cart)
                {
                    var orderDetail = new OrderDetailModel();

                    orderDetail.OrderCode = orderCode;
                    orderDetail.Price = item.Price;
                    orderDetail.Quantity = item.Quantity;
                    orderDetail.ProductId = item.ProductId;
                    orderDetail.UserName = email;

                    var product = await _dataContext.Products.Where(x => x.Id == item.ProductId).FirstAsync();
                    product.Quantity -= item.Quantity;
                    product.Sold += item.Quantity;
                    _dataContext.Products.Update(product);

                    await _dataContext.OrderDetails.AddAsync(orderDetail);
                    await _dataContext.SaveChangesAsync();
                }

                HttpContext.Session.Remove("Cart");
                Response.Cookies.Delete("ShippingPrice");

                // Send email
                var receiver = email;
                var subject = "Đặt hàng thành công";
                var message = "Đơn hàng của bạn đã được đặt thành công, mã đơn hàng của bạn là: " + orderCode;

                await _emailSender.SendEmailAsync(receiver, subject, message);

                _notyf.Success("Đặt hàng thành công, vui lòng chờ duyệt đơn hàng!");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetShipping(ShippingModel model, string tinh, string quan, string phuong)
        {
            var existShipping = await _dataContext.Shippings.FirstOrDefaultAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);

            decimal shippingPrice = 0;

            if (existShipping != null)
            {
                shippingPrice = existShipping.Price;
            }
            else
            {
                shippingPrice = 30000;
            }

            var priceConvert = JsonConvert.SerializeObject(shippingPrice);

            try
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = true,
                };

                Response.Cookies.Append("ShippingPrice", priceConvert, cookieOptions);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Coupon(string value)
        {
            var validCoupon = await _dataContext.Coupons.FirstOrDefaultAsync(x => x.Name == value);

            string couponTitle = validCoupon.Name + " | " + validCoupon.Description;

            if (couponTitle != null)
            {
                TimeSpan remainingTime = validCoupon.DateExpired - DateTime.Now;
                int daysRemaining = remainingTime.Days;

                if (daysRemaining >= 0)
                {
                    try
                    {
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                            Secure = true,
                            SameSite = SameSiteMode.Strict
                        };

                        Response.Cookies.Append("CouponTitle", couponTitle, cookieOptions);
                        return Ok(new { success = true, message = "Coupon applied successfully!" });
                    }
                    catch (Exception ex)
                    {
                        StatusCode(500, ex.Message);
                        return Ok(new { success = false, message = "Coupon applied failed" });
                    }
                }else
                {
                    return Ok(new { success = false, message = "Coupon has expired" });
                }
            }
            else
            {
                return Ok(new { success = false, message = "Coupon not expired" });
            }
        }
    }
}
