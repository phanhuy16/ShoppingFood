using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Helper;
using ShoppingFood.Models;
using ShoppingFood.Repository;

namespace ShoppingFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(DataContext context, INotyfService notyf, IWebHostEnvironment environment)
        {
            _dataContext = context;
            _notyf = notyf;
            _webHostEnvironment = environment;
        }
        public async Task<IActionResult> Index()
        {
            var products = await _dataContext.Products.OrderByDescending(x => x.Id).Include(x => x.Category).ToListAsync();
            return View(products);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel model)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", model.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", model.BrandId);
            if (ModelState.IsValid)
            {
                model.Slug = SlugHelper.GenerateSlug(model.Name);
                var slug = await _dataContext.Products.FirstOrDefaultAsync(x => x.Slug == model.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("Name", "Tên sản phẩm đã tồn tại");
                    return View(model);
                }
                else
                {
                    if (model.ImageUpload != null)
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                        string imageName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadsDir, imageName);

                        FileStream fileStream = new FileStream(filePath, FileMode.Create);
                        await model.ImageUpload.CopyToAsync(fileStream);
                        fileStream.Close();
                        model.Image = imageName;
                    }
                }
                model.Status = 1;
                _dataContext.Products.Add(model);
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Thêm sản phẩm thành công!");
                return RedirectToAction("Index");
            }
            else
            {
                List<string> errors = new List<string>();
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _dataContext.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductModel model)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", model.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", model.BrandId);

            var existProduct = await _dataContext.Products.FindAsync(model.Id);

            if (ModelState.IsValid)
            {
                if (model.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    string oldFilePath = Path.Combine(uploadsDir, existProduct.Image);
                    try
                    {
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("Error", ex.Message);
                    }
                    FileStream fileStream = new FileStream(filePath, FileMode.Create);
                    await model.ImageUpload.CopyToAsync(fileStream);
                    fileStream.Close();
                    existProduct.Image = imageName;
                }

                existProduct.Name = model.Name;
                existProduct.Slug = SlugHelper.GenerateSlug(model.Name);
                existProduct.Description = model.Description;
                existProduct.Detail = model.Detail;
                existProduct.Status = model.Status;
                existProduct.Price = model.Price;
                existProduct.CategoryId = model.CategoryId;
                existProduct.BrandId = model.BrandId;
                existProduct.CapitalPrice = model.CapitalPrice;

                _dataContext.Products.Update(existProduct);
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Cập nhật sản phẩm thành công!");
                return RedirectToAction("Index");
            }
            else
            {
                List<string> errors = new List<string>();
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _dataContext.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
            string filePath = Path.Combine(uploadsDir, product.Image);

            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", ex.Message);
            }

            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            _notyf.Success("Xóa sản phẩm thành công!");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AddQuantity(int id)
        {
            var quantity = await _dataContext.ProductQuantities.Where(x => x.ProductId == id).ToListAsync();

            ViewBag.Quantity = quantity;
            ViewBag.Id = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuantity(ProductQuantityModel model)
        {
            var product = await _dataContext.Products.FindAsync(model.ProductId);
            if (product == null)
            {
                _notyf.Error("Product not found");
            }
            product.Quantity += model.Quantity;

            var newQuantity = new ProductQuantityModel
            {
                Quantity = model.Quantity,
                ProductId = model.ProductId,
                CreateDate = DateTime.Now
            };

            await _dataContext.ProductQuantities.AddAsync(newQuantity);
            await _dataContext.SaveChangesAsync();
            _notyf.Success("Add quantity successfully!");
            return RedirectToAction("Index");
        }
    }
}
