using System.Text.Json;
using ShoppingFood.Helper;
using ShoppingFood.Models;
using ShoppingFood.Repository;

namespace ShoppingFood.Services.Product
{
    public class ProductManagementService : IProductManagementService
    {
        private readonly IGenericRepository<ProductModel> _productRepo;
        private readonly IGenericRepository<ProductImageModel> _imageRepo;
        private readonly IGenericRepository<ProductVariantModel> _variantRepo;
        private readonly IGenericRepository<ProductQuantityModel> _quantityRepo;
        private readonly IFileService _fileService;

        public ProductManagementService(
            IGenericRepository<ProductModel> productRepo,
            IGenericRepository<ProductImageModel> imageRepo,
            IGenericRepository<ProductVariantModel> variantRepo,
            IGenericRepository<ProductQuantityModel> quantityRepo,
            IFileService fileService)
        {
            _productRepo = productRepo;
            _imageRepo = imageRepo;
            _variantRepo = variantRepo;
            _quantityRepo = quantityRepo;
            _fileService = fileService;
        }

        public async Task<(bool Success, string Message)> CreateProductAsync(ProductModel model, string username)
        {
            model.Slug = SlugHelper.GenerateSlug(model.Name);
            var existing = await _productRepo.GetFirstOrDefaultAsync(x => x.Slug == model.Slug);
            if (existing != null)
            {
                return (false, "Tên sản phẩm đã tồn tại");
            }

            if (model.ImageUpload != null)
            {
                model.Image = await _fileService.UploadFileAsync(model.ImageUpload, "media/products");
            }
            else
            {
                model.Image = "noimage.jpg";
            }

            model.CreatedBy = username;
            model.CreatedDate = DateTime.Now;

            await _productRepo.AddAsync(model);
            await _productRepo.SaveAsync();

            if (model.ImageGalleryUpload != null && model.ImageGalleryUpload.Any())
            {
                foreach (var imageFile in model.ImageGalleryUpload)
                {
                    var imageName = await _fileService.UploadFileAsync(imageFile, "media/products");
                    var productImage = new ProductImageModel
                    {
                        ProductId = model.Id,
                        ImageUrl = imageName
                    };
                    await _imageRepo.AddAsync(productImage);
                }
                await _imageRepo.SaveAsync();
            }

            if (!string.IsNullOrEmpty(model.VariantJson))
            {
                var variants = JsonSerializer.Deserialize<List<ProductVariantModel>>(model.VariantJson);
                if (variants != null)
                {
                    foreach (var variant in variants)
                    {
                        variant.ProductId = model.Id;
                        await _variantRepo.AddAsync(variant);
                    }
                    await _variantRepo.SaveAsync();
                }
            }

            return (true, "Thêm sản phẩm thành công!");
        }

        public async Task<(bool Success, string Message)> EditProductAsync(ProductModel model, string username)
        {
            var existProduct = await _productRepo.GetFirstOrDefaultAsync(x => x.Id == model.Id);
            if (existProduct == null)
            {
                return (false, "Product Not Found");
            }

            if (model.ImageUpload != null)
            {
                _fileService.DeleteFile(existProduct.Image, "media/products");
                existProduct.Image = await _fileService.UploadFileAsync(model.ImageUpload, "media/products");
            }

            existProduct.Name = model.Name;
            existProduct.Slug = SlugHelper.GenerateSlug(model.Name);
            existProduct.Description = model.Description;
            existProduct.Detail = model.Detail;
            existProduct.Status = model.Status;
            existProduct.Price = model.Price;
            existProduct.CategoryId = model.CategoryId;
            existProduct.BrandId = model.BrandId;
            existProduct.ProductCategoryId = model.ProductCategoryId;
            existProduct.CapitalPrice = model.CapitalPrice;
            existProduct.PriceSale = model.PriceSale;
            existProduct.ModifierDate = DateTime.Now;
            existProduct.ModifierBy = username;

            _productRepo.Update(existProduct);
            await _productRepo.SaveAsync();

            if (model.ImageGalleryUpload != null && model.ImageGalleryUpload.Any())
            {
                foreach (var imageFile in model.ImageGalleryUpload)
                {
                    var imageName = await _fileService.UploadFileAsync(imageFile, "media/products");
                    var productImage = new ProductImageModel
                    {
                        ProductId = model.Id,
                        ImageUrl = imageName
                    };
                    await _imageRepo.AddAsync(productImage);
                }
                await _imageRepo.SaveAsync();
            }

            if (!string.IsNullOrEmpty(model.VariantJson))
            {
                var existingVariants = await _variantRepo.GetAllAsync(x => x.ProductId == model.Id);
                _variantRepo.RemoveRange(existingVariants);
                await _variantRepo.SaveAsync();

                var variants = JsonSerializer.Deserialize<List<ProductVariantModel>>(model.VariantJson);
                if (variants != null)
                {
                    foreach (var variant in variants)
                    {
                        variant.ProductId = model.Id;
                        await _variantRepo.AddAsync(variant);
                    }
                    await _variantRepo.SaveAsync();
                }
            }
            return (true, "Cập nhật sản phẩm thành công!");
        }

        public async Task<(bool Success, string Message)> DeleteProductImageAsync(int imageId)
        {
            var image = await _imageRepo.GetFirstOrDefaultAsync(x => x.Id == imageId);
            if (image != null)
            {
                _fileService.DeleteFile(image.ImageUrl, "media/products");
                _imageRepo.Remove(image);
                await _imageRepo.SaveAsync();
                return (true, "Image deleted successfully");
            }
            return (false, "Image not found");
        }

        public async Task<(bool Success, string Message)> DeleteProductAsync(int id)
        {
            var product = await _productRepo.GetFirstOrDefaultAsync(x => x.Id == id);
            if (product != null)
            {
                _fileService.DeleteFile(product.Image, "media/products");
                _productRepo.Remove(product);
                await _productRepo.SaveAsync();
                return (true, "Xóa sản phẩm thành công!");
            }
            return (false, "Không tìm thấy sản phẩm!");
        }

        public async Task<(bool Success, string Message)> AddProductQuantityAsync(int productId, int quantity)
        {
            var product = await _productRepo.GetFirstOrDefaultAsync(x => x.Id == productId);
            if (product == null)
            {
                return (false, "Product not found");
            }

            product.Quantity += quantity;
            _productRepo.Update(product);
            await _productRepo.SaveAsync();

            var newQuantity = new ProductQuantityModel
            {
                Quantity = quantity,
                ProductId = productId,
                CreateDate = DateTime.Now
            };

            await _quantityRepo.AddAsync(newQuantity);
            await _quantityRepo.SaveAsync();
            
            return (true, "Add quantity successfully!");
        }
    }
}
