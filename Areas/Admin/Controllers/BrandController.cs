using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Helper;
using ShoppingFood.Models;
using ShoppingFood.Repository;

namespace ShoppingFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BrandController(DataContext context, INotyfService notyf, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _notyf = notyf;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var brands = await _dataContext.Brands.OrderByDescending(x => x.Id).ToListAsync();
            return View(brands);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandModel model)
        {
            if (ModelState.IsValid)
            {
                model.Slug = SlugHelper.GenerateSlug(model.Name);
                var slug = await _dataContext.Brands.FirstOrDefaultAsync(x => x.Slug == model.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("Name", "Brand Name Already Exist");
                    return View(model);
                }

                if (model.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/brands");
                    string imageName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fileStream = new FileStream(filePath, FileMode.Create);
                    await model.ImageUpload.CopyToAsync(fileStream);
                    fileStream.Close();
                    model.Image = imageName;
                }

                model.CreatedBy = User.Identity.Name;
                model.CreatedDate = DateTime.Now;

                await _dataContext.Brands.AddAsync(model);
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Brand Created Successfully");
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
            var brand = await _dataContext.Brands.FindAsync(id);
            if (brand == null)
            {
                _notyf.Error("Brand Not Found");
            }
            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BrandModel model)
        {
            var existBrand = await _dataContext.Brands.FindAsync(model.Id);

            if (ModelState.IsValid)
            {
                if (model.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/brands");
                    string imageName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    string oldFilePath = Path.Combine(uploadsDir, existBrand.Image);
                    try
                    {
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("Error", ex.Message);
                    }
                    FileStream fileStream = new FileStream(filePath, FileMode.Create);
                    await model.ImageUpload.CopyToAsync(fileStream);
                    fileStream.Close();
                    existBrand.Image = imageName;
                }

                existBrand.Name = model.Name;
                existBrand.Slug = SlugHelper.GenerateSlug(model.Name);
                existBrand.Description = model.Description;
                existBrand.ModifierDate = DateTime.Now;
                existBrand.ModifierBy = User.Identity.Name;
                existBrand.Status = model.Status;

                _dataContext.Brands.Update(existBrand);
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Cập nhật thành công!");

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
            var brand = await _dataContext.Brands.FindAsync(id);

            if (brand == null)
            {
                _notyf.Error("Brand Not Found");
            }

            string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/brands");
            string filePath = Path.Combine(uploadsDir, brand.Image);

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

            _dataContext.Brands.Remove(brand);
            await _dataContext.SaveChangesAsync();
            _notyf.Success("Xóa thành công!");
            return RedirectToAction("Index");
        }
    }
}
