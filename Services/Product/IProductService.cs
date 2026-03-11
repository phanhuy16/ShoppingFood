using ShoppingFood.Models;

namespace ShoppingFood.Services.Product
{
    public interface IProductService
    {
        Task<(List<ProductModel> Products, int TotalCount)> GetFilteredProductsAsync(string sort_by, string startprice, string endprice, int page, int pageSize);
        Task<ProductModel?> GetProductDetailsAsync(int id, string slug);
        Task<List<ProductModel>> GetBestSellersAsync(int count);
        Task<double> GetAverageRatingAsync(int productId);
        Task<Dictionary<int, double>> GetProductRatingsAsync(List<ProductModel> products);
    }
}
