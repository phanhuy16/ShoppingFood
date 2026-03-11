using ShoppingFood.Models;

namespace ShoppingFood.Services.Address
{
    public interface IAddressService
    {
        Task AddAddressAsync(UserAddressModel model, string userId);
        Task DeleteAddressAsync(int addressId, string userId);
        Task SetDefaultAddressAsync(int addressId, string userId);
        Task<List<UserAddressModel>> GetAddressesByUserAsync(string userId);
    }
}
