using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
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

            CartItemViewModel cartItemViewModel = new CartItemViewModel()
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(x => x.Quantity * x.Price),
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
            await Task.CompletedTask;
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

            if (cartItem.Quantity >= 1)
            {
                cartItem.Quantity += 1;
            }

            HttpContext.Session.SetJson("Cart", cart);

            _notyf.Success("Increase item quantity to cart successfully!");

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

                    await _dataContext.OrderDetails.AddAsync(orderDetail);
                    await _dataContext.SaveChangesAsync();
                }

                HttpContext.Session.Remove("Cart");

                // Send email
                var receiver = email;
                var subject = "Đặt hàng thành công";
                var message = "Đơn hàng của bạn đã được đặt thành công, mã đơn hàng của bạn là: " + orderCode;

                await _emailSender.SendEmailAsync(receiver, subject, message);

                _notyf.Success("Đặt hàng thành công, vui lòng chờ duyệt đơn hàng!");
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
