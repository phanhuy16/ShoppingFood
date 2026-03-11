using Microsoft.EntityFrameworkCore;
using ShoppingFood.Models;
using ShoppingFood.Models.ViewModel;
using ShoppingFood.Repository;

namespace ShoppingFood.Services.Product
{
    public class ProductService : IProductService
    {
        private readonly DataContext _dataContext;

        public ProductService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<(List<ProductModel> Products, int TotalCount)> GetFilteredProductsAsync(
            string sort_by, string startprice, string endprice, int page, int pageSize)
        {
            var query = _dataContext.Products.Include(x => x.Category).Where(x => x.Status == 1);

            if (sort_by == "price_increase")
                query = query.OrderBy(x => x.Price);
            else if (sort_by == "price_decrease")
                query = query.OrderByDescending(x => x.Price);
            else if (sort_by == "price_newest")
                query = query.OrderByDescending(x => x.Id);
            else if (sort_by == "price_oldest")
                query = query.OrderBy(x => x.Id);
            else if (!string.IsNullOrEmpty(startprice) && !string.IsNullOrEmpty(endprice)
                     && decimal.TryParse(startprice, out var start) && decimal.TryParse(endprice, out var end))
                query = query.Where(x => x.Price >= start && x.Price <= end);
            else
                query = query.OrderByDescending(x => x.Id);

            int totalCount = await query.CountAsync();
            int skip = (page - 1) * pageSize;
            var products = await query.Skip(skip).Take(pageSize).ToListAsync();

            return (products, totalCount);
        }

        public async Task<ProductModel?> GetProductDetailsAsync(int id, string slug)
        {
            // Thử tìm theo cả Id + Slug
            var product = await _dataContext.Products
                .Include(x => x.Category)
                .Include(x => x.ProductImages)
                .Include(x => x.ProductVariants)
                .FirstOrDefaultAsync(x => x.Id == id && x.Slug == slug);

            // Fallback: chỉ tìm theo Id
            if (product == null)
            {
                product = await _dataContext.Products
                    .Include(x => x.Category)
                    .Include(x => x.ProductImages)
                    .Include(x => x.ProductVariants)
                    .FirstOrDefaultAsync(x => x.Id == id);
            }

            return product;
        }

        public async Task<List<ProductModel>> GetBestSellersAsync(int count)
        {
            return await _dataContext.Products
                .Include(x => x.Category)
                .Where(x => x.Status == 1 && x.PriceSale < x.Price)
                .OrderByDescending(x => x.Sold)
                .Take(count)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingAsync(int productId)
        {
            var reviews = await _dataContext.Reviews
                .Where(x => x.ProductId == productId)
                .ToListAsync();

            return reviews.Any() ? reviews.Average(x => x.Star) : 0;
        }

        public async Task<Dictionary<int, double>> GetProductRatingsAsync(List<ProductModel> products)
        {
            var ratings = new Dictionary<int, double>();
            foreach (var product in products)
            {
                ratings[product.Id] = await GetAverageRatingAsync(product.Id);
            }
            return ratings;
        }
    }
}
