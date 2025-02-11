namespace ShoppingFood.Models.ViewModel
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems { get; set; } = null!;

        public decimal GrandTotal { get; set; }

        public decimal ShippingCost { get; set; }
        public string CouponCode { get; set; }
    }
}
