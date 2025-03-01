using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Models;

namespace ShoppingFood.Repository
{
    public class DataContext : IdentityDbContext<AppUserModel>
    {
        public DataContext() { }
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<BrandModel> Brands { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderDetailModel> OrderDetails { get; set; }
        public DbSet<ReviewModel> Reviews { get; set; }
        public DbSet<SliderModel> Sliders { get; set; }
        public DbSet<ContactModel> Contacts { get; set; }
        public DbSet<WishlistModel> Wishlists { get; set; }
        public DbSet<CompareModel> Compares { get; set; }
        public DbSet<ProductQuantityModel> ProductQuantities { get; set; }
        public DbSet<ShippingModel> Shippings { get; set; }
        public DbSet<CouponModel> Coupons { get; set; }
        public DbSet<StatisticalModel> Statisticals { get; set; }
        public DbSet<MomoInfoModel> MomoInfos { get; set; }
        public DbSet<VnpayModel> Vnpays { get; set; }
        public DbSet<ProductCategoryModel> ProductCategories { get; set; }
        public DbSet<MenuModel> Menus { get; set; }
        public DbSet<BannerModel> Banners { get; set; }
    }
}
