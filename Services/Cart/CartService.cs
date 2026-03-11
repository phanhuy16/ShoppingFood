using Newtonsoft.Json;
using ShoppingFood.Models;

namespace ShoppingFood.Services.Cart
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public List<CartItemModel> GetCartFromCookie()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var cartCookie = request?.Cookies["Cart"];
            if (!string.IsNullOrEmpty(cartCookie))
            {
                return JsonConvert.DeserializeObject<List<CartItemModel>>(cartCookie) ?? new List<CartItemModel>();
            }
            return new List<CartItemModel>();
        }

        public void SaveCartToCookie(List<CartItemModel> cart)
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            if (response == null) return;

            var options = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                Secure = true,
                SameSite = SameSiteMode.Strict
            };
            try
            {
                var cartJson = JsonConvert.SerializeObject(cart);
                if (!string.IsNullOrEmpty(cartJson))
                {
                    response.Cookies.Append("Cart", cartJson, options);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving cart cookie: " + ex.Message);
            }
        }

        public void ClearCartCookie()
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete("Cart");
        }

        public decimal CalculateGrandTotal(List<CartItemModel> cart)
        {
            return cart.Sum(x => x.Quantity * x.Price);
        }
    }
}
