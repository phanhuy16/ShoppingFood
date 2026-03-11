using ShoppingFood.Models;
using ShoppingFood.Models.ViewModel;

namespace ShoppingFood.Services.Account
{
    public interface IAccountService
    {
        Task<ProfileViewModel> GetProfileAsync(string userId, string userEmail);
        Task<bool> DeleteWishlistAsync(int id);
        Task<bool> DeleteCompareAsync(int id);
        Task<(bool Success, string Message)> ProcessForgotPasswordAsync(string email, string host, string scheme);
    }
}
