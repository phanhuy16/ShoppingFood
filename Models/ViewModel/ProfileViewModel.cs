using ShoppingFood.Models;

namespace ShoppingFood.Models.ViewModel
{
    public class ProfileViewModel
    {
        public List<OrderModel> Orders { get; set; } = new List<OrderModel>();
        public dynamic Wishlist { get; set; }
        public dynamic Compare { get; set; }
        public List<UserAddressModel> Addresses { get; set; } = new List<UserAddressModel>();
        public string UserEmail { get; set; } = string.Empty;
    }
}
