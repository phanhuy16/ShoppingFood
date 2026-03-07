#nullable enable
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShoppingFood.Helper;
using ShoppingFood.Models;
using ShoppingFood.Repository;
using ShoppingFood.Services;
using System.Text.Json;

namespace ShoppingFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IGenericRepository<ProductModel> _productRepo;
        private readonly IGenericRepository<CategoryModel> _categoryRepo;
        private readonly IGenericRepository<BrandModel> _brandRepo;
        private readonly IGenericRepository<ProductCategoryModel> _productCategoryRepo;
        private readonly IGenericRepository<ProductQuantityModel> _quantityRepo;
        private readonly IGenericRepository<ProductImageModel> _imageRepo;
        private readonly IGenericRepository<ProductVariantModel> _variantRepo;
        private readonly IFileService _fileService;
        private readonly INotyfService _notyf;

        public ProductController(
            IGenericRepository<ProductModel> productRepo,
            IGenericRepository<CategoryModel> categoryRepo,
            IGenericRepository<BrandModel> brandRepo,
            IGenericRepository<ProductCategoryModel> productCategoryRepo,
            IGenericRepository<ProductQuantityModel> quantityRepo,
            IGenericRepository<ProductImageModel> imageRepo,
            IGenericRepository<ProductVariantModel> variantRepo,
            IFileService fileService,
            INotyfService notyf)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _brandRepo = brandRepo;
            _productCategoryRepo = productCategoryRepo;
            _quantityRepo = quantityRepo;
            _imageRepo = imageRepo;
            _variantRepo = variantRepo;
            _fileService = fileService;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepo.GetAllAsync(includeProperties: "Category", orderBy: q => q.OrderByDescending(x => x.Id));
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _categoryRepo.GetAllAsync(), "Id", "Name");
            ViewBag.Brands = new SelectList(await _brandRepo.GetAllAsync(), "Id", "Name");
            ViewBag.ProductCategories = new SelectList(await _productCategoryRepo.GetAllAsync(), "Id", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel model)
        {
            if (ModelState.IsValid)
            {
                model.Slug = SlugHelper.GenerateSlug(model.Name);
                var existing = await _productRepo.GetFirstOrDefaultAsync(x => x.Slug == model.Slug);
                if (existing != null)
                {
                    ModelState.AddModelError("Name", "Tên sản phẩm đã tồn tại");
                    await LoadCreateLists(model);
                    return View(model);
                }

                if (model.ImageUpload != null)
                {
                    model.Image = await _fileService.UploadFileAsync(model.ImageUpload, "media/products");
                }
                else
                {
                    model.Image = "noimage.jpg"; // Default image name
                }

                model.CreatedBy = User.Identity?.Name ?? "Unknown";
                model.CreatedDate = DateTime.Now;

                await _productRepo.AddAsync(model);
                await _productRepo.SaveAsync(); // Actually persist model to get ID

                // Handle Gallery Images
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

                // Handle Variants
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

                _notyf.Success("Thêm sản phẩm thành công!");
                return RedirectToAction("Index");
            }

            _notyf.Error("Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.");
            await LoadCreateLists(model);
            return View(model);
        }

        private async Task LoadCreateLists(ProductModel? model = null)
        {
            ViewBag.Categories = new SelectList(await _categoryRepo.GetAllAsync(), "Id", "Name", model?.CategoryId);
            ViewBag.Brands = new SelectList(await _brandRepo.GetAllAsync(), "Id", "Name", model?.BrandId);
            ViewBag.ProductCategories = new SelectList(await _productCategoryRepo.GetAllAsync(), "Id", "Name", model?.ProductCategoryId);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepo.GetFirstOrDefaultAsync(x => x.Id == id, includeProperties: "ProductImages,ProductVariants");
            if (product == null)
            {
                _notyf.Error("Product Not Found");
                return RedirectToAction("Index");
            }
            await LoadCreateLists(product);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductModel model)
        {
            if (ModelState.IsValid)
            {
                var existProduct = await _productRepo.GetFirstOrDefaultAsync(x => x.Id == model.Id);
                if (existProduct == null)
                {
                    _notyf.Error("Product Not Found");
                    return RedirectToAction("Index");
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
                existProduct.ModifierBy = User.Identity?.Name ?? "System";

                _productRepo.Update(existProduct);
                await _productRepo.SaveAsync();

                // Handle Gallery Images
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

                // Handle Variants (Refresh all)
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
                _notyf.Success("Cập nhật sản phẩm thành công!");
                return RedirectToAction("Index");
            }

            await LoadCreateLists(model);
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var image = await _imageRepo.GetFirstOrDefaultAsync(x => x.Id == imageId);
            if (image != null)
            {
                _fileService.DeleteFile(image.ImageUrl, "media/products");
                _imageRepo.Remove(image);
                await _imageRepo.SaveAsync();
                return Ok(new { success = true });
            }
            return NotFound();
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepo.GetFirstOrDefaultAsync(x => x.Id == id);
            if (product != null)
            {
                _fileService.DeleteFile(product.Image, "media/products");
                _productRepo.Remove(product);
                await _productRepo.SaveAsync();
                _notyf.Success("Xóa sản phẩm thành công!");
            }
            else
            {
                _notyf.Error("Không tìm thấy sản phẩm!");
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AddQuantity(int id)
        {
            var quantities = await _quantityRepo.GetAllAsync(x => x.ProductId == id);
            ViewBag.Quantity = quantities;
            ViewBag.Id = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuantity(ProductQuantityModel model)
        {
            var product = await _productRepo.GetFirstOrDefaultAsync(x => x.Id == model.ProductId);
            if (product == null)
            {
                _notyf.Error("Product not found");
                return RedirectToAction("Index");
            }

            product.Quantity += model.Quantity;
            _productRepo.Update(product);
            await _productRepo.SaveAsync();

            var newQuantity = new ProductQuantityModel
            {
                Quantity = model.Quantity,
                ProductId = model.ProductId,
                CreateDate = DateTime.Now
            };

            await _quantityRepo.AddAsync(newQuantity);
            await _quantityRepo.SaveAsync();
            _notyf.Success("Add quantity successfully!");
            return RedirectToAction("Index");
        }
    }
}
