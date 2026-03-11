using ShoppingFood.Models;

namespace ShoppingFood.Services.Product
{
    public interface IProductManagementService
    {
        Task<(bool Success, string Message)> CreateProductAsync(ProductModel model, string username);
        Task<(bool Success, string Message)> EditProductAsync(ProductModel model, string username);
        Task<(bool Success, string Message)> DeleteProductAsync(int id);
        Task<(bool Success, string Message)> DeleteProductImageAsync(int imageId);
        Task<(bool Success, string Message)> AddProductQuantityAsync(int productId, int quantity);
    }
}
