using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Areas.Admin.Repository;
using ShoppingFood.Models;
using ShoppingFood.Models.ViewModel;
using ShoppingFood.Repository;

namespace ShoppingFood.Services.Account
{
    public class AccountService : IAccountService
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IEmailSender _emailSender;

        public AccountService(DataContext context, UserManager<AppUserModel> userManager, IEmailSender emailSender)
        {
            _dataContext = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task<ProfileViewModel> GetProfileAsync(string userId, string userEmail)
        {
            var orders = await _dataContext.Orders.Where(x => x.UserName == userEmail).OrderByDescending(x => x.Id).ToListAsync();

            var wishlist = await (from w in _dataContext.Wishlists
                                  join p in _dataContext.Products on w.ProductId equals p.Id
                                  join u in _dataContext.Users on w.UserId equals u.Id
                                  select new { User = u, Product = p, Wishlist = w }).ToListAsync();

            var compare = await (from c in _dataContext.Compares
                                 join p in _dataContext.Products on c.ProductId equals p.Id
                                 join u in _dataContext.Users on c.UserId equals u.Id
                                 select new { User = u, Product = p, Compare = c }).ToListAsync();

            var addresses = await _dataContext.UserAddresses.Where(x => x.UserId == userId).ToListAsync();

            return new ProfileViewModel
            {
                Orders = orders,
                Wishlist = wishlist,
                Compare = compare,
                Addresses = addresses,
                UserEmail = userEmail
            };
        }

        public async Task<bool> DeleteWishlistAsync(int id)
        {
            var wishlist = await _dataContext.Wishlists.FindAsync(id);
            if (wishlist != null)
            {
                _dataContext.Wishlists.Remove(wishlist);
                await _dataContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteCompareAsync(int id)
        {
            var compare = await _dataContext.Compares.FindAsync(id);
            if (compare != null)
            {
                _dataContext.Compares.Remove(compare);
                await _dataContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<(bool Success, string Message)> ProcessForgotPasswordAsync(string email, string host, string scheme)
        {
            var checkMail = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (checkMail == null)
            {
                return (false, "Email not found!");
            }

            string token = Guid.NewGuid().ToString();
            checkMail.Token = token;
            _dataContext.Users.Update(checkMail);
            await _dataContext.SaveChangesAsync();

            var receiver = checkMail.Email;
            var subject = "Change password for user " + checkMail.Email;
            var message = "Click on link to change password " + "<a href='" + $"{scheme}://{host}/Account/ForgotPasswordConfirm?email=" + checkMail.Email + "&token=" + token + "'>";

            await _emailSender.SendEmailAsync(receiver, subject, message);

            return (true, "Send email successfully");
        }
    }
}
