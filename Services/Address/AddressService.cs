using Microsoft.EntityFrameworkCore;
using ShoppingFood.Areas.Admin.Repository;
using ShoppingFood.Models;
using ShoppingFood.Repository;

namespace ShoppingFood.Services.Address
{
    public class AddressService : IAddressService
    {
        private readonly DataContext _dataContext;

        public AddressService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task AddAddressAsync(UserAddressModel model, string userId)
        {
            model.UserId = userId;

            var existingAddrs = await _dataContext.UserAddresses
                .Where(x => x.UserId == userId).ToListAsync();

            // Nếu là địa chỉ đầu tiên, tự động set mặc định
            if (existingAddrs.Count == 0)
            {
                model.IsDefault = true;
            }
            else if (model.IsDefault)
            {
                foreach (var addr in existingAddrs)
                    addr.IsDefault = false;
                _dataContext.UserAddresses.UpdateRange(existingAddrs);
            }

            _dataContext.UserAddresses.Add(model);
            await _dataContext.SaveChangesAsync();
        }

        public async Task DeleteAddressAsync(int addressId, string userId)
        {
            var address = await _dataContext.UserAddresses
                .FirstOrDefaultAsync(x => x.Id == addressId && x.UserId == userId);

            if (address != null)
            {
                _dataContext.UserAddresses.Remove(address);
                await _dataContext.SaveChangesAsync();
            }
        }

        public async Task SetDefaultAddressAsync(int addressId, string userId)
        {
            var addrs = await _dataContext.UserAddresses
                .Where(x => x.UserId == userId).ToListAsync();

            foreach (var addr in addrs)
                addr.IsDefault = (addr.Id == addressId);

            _dataContext.UserAddresses.UpdateRange(addrs);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<List<UserAddressModel>> GetAddressesByUserAsync(string userId)
        {
            return await _dataContext.UserAddresses
                .Where(x => x.UserId == userId).ToListAsync();
        }
    }
}
