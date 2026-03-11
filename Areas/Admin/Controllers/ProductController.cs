#nullable enable
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShoppingFood.Helper;
using ShoppingFood.Models;
using ShoppingFood.Repository;
using ShoppingFood.Services;
using ShoppingFood.Services.Product;
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
        private readonly IProductManagementService _productManagementService;
        private readonly INotyfService _notyf;

        public ProductController(
            IGenericRepository<ProductModel> productRepo,
            IGenericRepository<CategoryModel> categoryRepo,
            IGenericRepository<BrandModel> brandRepo,
            IGenericRepository<ProductCategoryModel> productCategoryRepo,
            IGenericRepository<ProductQuantityModel> quantityRepo,
            IProductManagementService productManagementService,
            INotyfService notyf)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _brandRepo = brandRepo;
            _productCategoryRepo = productCategoryRepo;
            _quantityRepo = quantityRepo;
            _productManagementService = productManagementService;
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
                var username = User.Identity?.Name ?? "Unknown";
                var result = await _productManagementService.CreateProductAsync(model, username);

                if (result.Success)
                {
                    _notyf.Success(result.Message);
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Name", result.Message);
                }
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
                var username = User.Identity?.Name ?? "System";
                var result = await _productManagementService.EditProductAsync(model, username);

                if (result.Success)
                {
                    _notyf.Success(result.Message);
                    return RedirectToAction("Index");
                }
                else
                {
                    _notyf.Error(result.Message);
                    if (result.Message == "Product Not Found")
                        return RedirectToAction("Index");
                }
            }

            await LoadCreateLists(model);
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var result = await _productManagementService.DeleteProductImageAsync(imageId);
            if (result.Success)
            {
                return Ok(new { success = true });
            }
            return NotFound();
        }

        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productManagementService.DeleteProductAsync(id);
            if (result.Success)
            {
                _notyf.Success(result.Message);
            }
            else
            {
                _notyf.Error(result.Message);
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
            var result = await _productManagementService.AddProductQuantityAsync(model.ProductId, model.Quantity);

            if (result.Success)
            {
                _notyf.Success(result.Message);
            }
            else
            {
                _notyf.Error(result.Message);
            }

            return RedirectToAction("Index");
        }
    }
}
