#nullable enable
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ShoppingFood.Models;
using ShoppingFood.Models.ViewModel;
using ShoppingFood.Repository;
using ShoppingFood.Services.Cart;
using ShoppingFood.Services.Momo;
using ShoppingFood.Services.Order;
using ShoppingFood.Services.Paypal;
using ShoppingFood.Services.Vnpay;
using System.Security.Claims;

namespace ShoppingFood.Controllers
{
    public class CartController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;
        private IMomoService _momoService;
        private readonly IVnPayService _vnPayService;
        private readonly PaypalClient _paypalClient;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;

        public CartController(UserManager<AppUserModel> userManager, DataContext context, INotyfService notyf,
            IMomoService momoService, IVnPayService vnPayService, PaypalClient paypalClient,
            ICartService cartService, IOrderService orderService)
        {
            _dataContext = context;
            _notyf = notyf;
            _momoService = momoService;
            _userManager = userManager;
            _vnPayService = vnPayService;
            _paypalClient = paypalClient;
            _cartService = cartService;
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            List<CartItemModel> cartItems = _cartService.GetCartFromCookie();

            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;
            if (shippingPriceCookie != null)
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceCookie);

            var coupon = Request.Cookies["CouponTitle"];

            CartItemViewModel cartItemViewModel = new CartItemViewModel()
            {
                CartItems = cartItems,
                GrandTotal = _cartService.CalculateGrandTotal(cartItems),
                ShippingCost = shippingPrice,
                CouponCode = coupon,
            };

            return View(cartItemViewModel);
        }

        public async Task<IActionResult> Add(int id, int? variantId)
        {
            var product = await _dataContext.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            ProductVariantModel? variant = null;
            if (variantId.HasValue)
                variant = await _dataContext.ProductVariants.FindAsync(variantId.Value);

            List<CartItemModel> cart = _cartService.GetCartFromCookie();
            var cartItem = cart.FirstOrDefault(x => x.ProductId == id && x.VariantId == variantId);

            if (cartItem == null)
                cart.Add(new CartItemModel(product, variant));
            else
                cartItem.Quantity += 1;

            _cartService.SaveCartToCookie(cart);

            return Ok(new { success = true, message = "Add item to cart successfully!" });
        }

        public async Task<IActionResult> Decrease(int id, int? variantId)
        {
            await Task.CompletedTask;
            List<CartItemModel> cart = _cartService.GetCartFromCookie();

            if (cart == null)
                return RedirectToAction("Index");

            var cartItem = cart.FirstOrDefault(x => x.ProductId == id && x.VariantId == variantId);

            if (cartItem == null)
                return RedirectToAction("Index");

            if (cartItem.Quantity > 1)
                cartItem.Quantity -= 1;
            else
                cart.RemoveAll(x => x.ProductId == id && x.VariantId == variantId);

            if (cart.Count == 0)
                _cartService.ClearCartCookie();
            else
                _cartService.SaveCartToCookie(cart);

            _notyf.Success("Decrease item quantity in cart successfully!");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Increase(int id, int? variantId)
        {
            var product = await _dataContext.Products.Where(x => x.Id == id).FirstOrDefaultAsync();
            List<CartItemModel> cart = _cartService.GetCartFromCookie();

            if (cart == null || product == null)
                return RedirectToAction("Index");

            var cartItem = cart.FirstOrDefault(x => x.ProductId == id && x.VariantId == variantId);

            if (cartItem == null)
                return RedirectToAction("Index");

            if (cartItem.Quantity < product.Quantity)
            {
                cartItem.Quantity += 1;
                _notyf.Success("Increase item quantity in cart successfully!");
            }
            else
            {
                cartItem.Quantity = product.Quantity;
                _notyf.Warning("Maximum Quantity in Product");
            }

            _cartService.SaveCartToCookie(cart);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int id, int? variantId)
        {
            await Task.CompletedTask;
            List<CartItemModel> cart = _cartService.GetCartFromCookie();
            if (cart != null)
            {
                cart.RemoveAll(x => x.ProductId == id && x.VariantId == variantId);
                if (cart.Count == 0)
                    _cartService.ClearCartCookie();
                else
                    _cartService.SaveCartToCookie(cart);
                _notyf.Success("Remove item from cart successfully!");
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Clear()
        {
            await Task.CompletedTask;
            _cartService.ClearCartCookie();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CheckOut()
        {
            List<CartItemModel> cartItems = _cartService.GetCartFromCookie();

            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;
            if (shippingPriceCookie != null)
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceCookie);

            var coupon = Request.Cookies["CouponTitle"];

            CartItemViewModel cartItemViewModel = new CartItemViewModel()
            {
                CartItems = cartItems,
                GrandTotal = _cartService.CalculateGrandTotal(cartItems),
                ShippingCost = shippingPrice,
                CouponCode = coupon,
            };

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
            var addresses = await _dataContext.UserAddresses.Where(x => x.UserId == userId).ToListAsync();

            ViewBag.User = user;
            ViewBag.Addresses = addresses;
            ViewBag.PaypalClientId = _paypalClient.ClientId;

            return View(cartItemViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(string paymentMethod, string paymentId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null)
                return RedirectToAction("Login", "Account");

            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;
            if (shippingPriceCookie != null)
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceCookie);

            var coupon = Request.Cookies["CouponTitle"];
            List<CartItemModel> cart = _cartService.GetCartFromCookie();

            var orderCode = await _orderService.CreateOrderAsync(email, paymentMethod, paymentId, shippingPrice, coupon, cart);

            decimal totalAmount = _cartService.CalculateGrandTotal(cart) + shippingPrice;

            _cartService.ClearCartCookie();
            Response.Cookies.Delete("ShippingPrice");

            _notyf.Success("Đặt hàng thành công, vui lòng chờ duyệt đơn hàng!");

            if (paymentMethod == "QR")
                return RedirectToAction("PaymentQR", "Cart", new { orderCode = orderCode, amount = totalAmount });

            return RedirectToAction("Profile", "Account");
        }

        public IActionResult PaymentQR(string orderCode, decimal amount)
        {
            ViewBag.OrderCode = orderCode;
            ViewBag.Amount = amount;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetShipping(ShippingModel model, string tinh, string quan, string phuong)
        {
            var shippingPrice = await _orderService.GetShippingPriceAsync(tinh, quan, phuong);
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
                return Ok(new { success = true, price = shippingPrice });
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

            string couponTitle = validCoupon!.Name + " | " + validCoupon.Description;

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
                    Amount = decimal.Parse(request["Amount"]!),
                    OrderInfo = request["orderInfo"],
                    DatePaid = DateTime.Now,
                };
                await _dataContext.MomoInfos.AddAsync(newMomoInsert);
                await _dataContext.SaveChangesAsync();
                var paymentMethod = "Momo";
                await CheckOut(request["orderId"]!, paymentMethod);
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

                await CheckOut(response.PaymentId, response.PaymentMethod);
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
            List<CartItemModel> cartItems = _cartService.GetCartFromCookie();

            if (cartItems != null)
                return Json(new { success = true, count = cartItems.Count });

            return Json(new { success = false, count = 0 });
        }
    }
}
