using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Helper;
using ShoppingFood.Models;
using ShoppingFood.Repository;

namespace ShoppingFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class ProductCategoryController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;

        public ProductCategoryController(DataContext context, INotyfService notyf)
        {
            _dataContext = context;
            _notyf = notyf;
        }
        public async Task<IActionResult> Index()
        {
            var productCategories = await _dataContext.ProductCategories.OrderByDescending(x => x.Id).ToListAsync();
            return View(productCategories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCategoryModel model)
        {
            if (ModelState.IsValid)
            {
                model.Slug = SlugHelper.GenerateSlug(model.Name);
                var slug = await _dataContext.ProductCategories.FirstOrDefaultAsync(x => x.Slug == model.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("Name", "Product Category Name Already Exist");
                    return View(model);
                }

                model.CreatedBy = User.Identity.Name;
                model.CreatedDate = DateTime.Now;

                await _dataContext.ProductCategories.AddAsync(model);
                await _dataContext.SaveChangesAsync();

                _notyf.Success("Product Category Created Successfully!");

                return RedirectToAction(nameof(Index));
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
            var productCategory = await _dataContext.ProductCategories.FindAsync(id);
            if (productCategory == null)
            {
                _notyf.Error("Product Category Not Found!");
            }
            return View(productCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductCategoryModel model)
        {
            if (ModelState.IsValid)
            {
                var existProductCategory = await _dataContext.ProductCategories.FindAsync(model.Id);
                if (existProductCategory == null)
                {
                    _notyf.Error("Product Category Not Found!");
                }

                existProductCategory.Slug = SlugHelper.GenerateSlug(model.Name);
                existProductCategory.Status = model.Status;
                existProductCategory.Name = model.Name;
                existProductCategory.ModifierDate = DateTime.Now;
                existProductCategory.ModifierBy = User.Identity.Name;

                _dataContext.ProductCategories.Update(existProductCategory);
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Product Category Updated Successfully!");
                return RedirectToAction(nameof(Index));
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
            var productcategory = await _dataContext.ProductCategories.FindAsync(id);
            if (productcategory == null)
            {
                _notyf.Error("Product Category Not Found!");
            }
            _dataContext.ProductCategories.Remove(productcategory);
            await _dataContext.SaveChangesAsync();
            _notyf.Success("ProductCateory Deleted Successfully");
            return RedirectToAction(nameof(Index));
        }
    }
}
