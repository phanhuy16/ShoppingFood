using ShoppingFood.Models;

namespace ShoppingFood.Services.Cart
{
    public interface ICartService
    {
        List<CartItemModel> GetCartFromCookie();
        void SaveCartToCookie(List<CartItemModel> cart);
        void ClearCartCookie();
        decimal CalculateGrandTotal(List<CartItemModel> cart);
    }
}
