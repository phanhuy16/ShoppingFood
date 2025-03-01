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
    [Authorize(Roles = "Admin")]
    public class BannerController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BannerController(DataContext context, INotyfService notyf, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _notyf = notyf;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var banners = await _dataContext.Banners.OrderByDescending(x => x.Id).ToListAsync();
            return View(banners);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BannerModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/banners");
                    string imageName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fileStream = new FileStream(filePath, FileMode.Create);
                    await model.ImageUpload.CopyToAsync(fileStream);
                    fileStream.Close();
                    model.Image = imageName;
                }

                model.CreatedBy = User.Identity.Name;
                model.CreatedDate = DateTime.Now;

                await _dataContext.Banners.AddAsync(model);
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Banner Created Successfully");
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
            var banner = await _dataContext.Banners.FindAsync(id);
            if (banner == null)
            {
                _notyf.Error("Banner Not Found");
            }
            return View(banner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BannerModel model)
        {
            var existBanner = await _dataContext.Banners.FindAsync(model.Id);

            if (ModelState.IsValid)
            {
                if (model.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/banners");
                    string imageName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    string oldFilePath = Path.Combine(uploadsDir, existBanner.Image);
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
                    existBanner.Image = imageName;
                }

                existBanner.Title = model.Title;
                existBanner.SubTitle = model.SubTitle;
                existBanner.Description = model.Description;
                existBanner.ModifierDate = DateTime.Now;
                existBanner.ModifierBy = User.Identity.Name;
                existBanner.Status = model.Status;

                _dataContext.Banners.Update(existBanner);
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
            var banner = await _dataContext.Banners.FindAsync(id);

            if (banner == null)
            {
                _notyf.Error("Brand Not Found");
            }

            string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/banners");
            string filePath = Path.Combine(uploadsDir, banner.Image);

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

            _dataContext.Banners.Remove(banner);
            await _dataContext.SaveChangesAsync();
            _notyf.Success("Xóa thành công!");
            return RedirectToAction("Index");
        }
    }
}
