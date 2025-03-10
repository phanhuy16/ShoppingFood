using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ShoppingFood.Areas.Admin.Repository;
using ShoppingFood.Models;
using ShoppingFood.Models.Order;
using ShoppingFood.Models.ViewModel;
using ShoppingFood.Repository;
using ShoppingFood.Services.Momo;
using ShoppingFood.Services.Paypal;
using ShoppingFood.Services.Vnpay;
using System.Security.Claims;

namespace ShoppingFood.Controllers
{
    public class CartController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        private readonly INotyfService _notyf;
        private IMomoService _momoService;
        private readonly IVnPayService _vnPayService;
        private readonly PaypalClient _paypalClient;

        public CartController(UserManager<AppUserModel> userManager, DataContext context, INotyfService notyf, IEmailSender email, IMomoService momoService, IVnPayService vnPayService, PaypalClient paypalClient)
        {
            _dataContext = context;
            _notyf = notyf;
            _emailSender = email;
            _momoService = momoService;
            _userManager = userManager;
            _vnPayService = vnPayService;
            _paypalClient = paypalClient;
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);

            ViewBag.User = user;

            ViewBag.PaypalClientId = _paypalClient.ClientId;

            return View(cartItemViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(string paymentMethod, string paymentId)
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

                if (paymentId == null || paymentMethod == null)
                {
                    orderItem.PaymentMethod = "COD";
                }
                else
                {
                    orderItem.PaymentMethod = paymentMethod + " " + paymentId;
                }

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
                return RedirectToAction("Profile", "Account");
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
                }
                else
                {
                    return Ok(new { success = false, message = "Coupon has expired" });
                }
            }
            else
            {
                return Ok(new { success = false, message = "Coupon not expired" });
            }
        }

        public async Task<IActionResult> PaymentCallBack()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            var request = HttpContext.Request.Query;
            if (request["resultCode"] != 0)
            {
                var newMomoInsert = new MomoInfoModel
                {
                    OrderId = request["orderId"],
                    FullName = User.FindFirstValue(ClaimTypes.Email),
                    Amount = decimal.Parse(request["Amount"]),
                    OrderInfo = request["orderInfo"],
                    DatePaid = DateTime.Now,
                };
                await _dataContext.MomoInfos.AddAsync(newMomoInsert);
                await _dataContext.SaveChangesAsync();
                var paymentMethod = "Momo";
                await CheckOut(request["orderId"], paymentMethod);
            }
            else
            {
                _notyf.Warning("Giao dịch không thành công.");
                return RedirectToAction("Index", "Cart");
            }
            return View(response);
        }

        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.VnPayResponseCode == "00")
            {
                var newVnpayInsert = new VnpayModel
                {
                    OrderId = response.OrderId,
                    PaymentMethod = response.PaymentMethod,
                    OrderDescription = response.OrderDescription,
                    TransactionId = response.TransactionId,
                    PaymentId = response.PaymentId,
                    CreateDate = DateTime.Now,

                };
                await _dataContext.Vnpays.AddAsync(newVnpayInsert);
                await _dataContext.SaveChangesAsync();

                var paymentMethod = response.PaymentMethod;
                var paymentId = response.PaymentId;

                await CheckOut(paymentId, paymentMethod);
            }
            else
            {
                _notyf.Warning("Giao dịch không thành công.");
                return RedirectToAction("Index", "Cart");
            }

            return View(response);
        }

        public IActionResult ShowCount()
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            if (cartItems != null)
            {
                return Json(new { success = true, count = cartItems.Count });
            }

            return Json(new { success = false, count = 0 });
        }
    }
}
