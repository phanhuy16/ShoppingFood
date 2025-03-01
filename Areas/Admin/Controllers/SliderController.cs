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
    [Authorize(Roles = "Admin")]
    public class SliderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SliderController(DataContext context, INotyfService notyf, IWebHostEnvironment environment)
        {
            _dataContext = context;
            _notyf = notyf;
            _webHostEnvironment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var sliders = await _dataContext.Sliders.OrderByDescending(x=>x.Id).ToListAsync();
            return View(sliders);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SliderModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
                    string imageName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fileStream = new FileStream(filePath, FileMode.Create);
                    await model.ImageUpload.CopyToAsync(fileStream);
                    fileStream.Close();
                    model.Image = imageName;
                }

                model.CreatedDate = DateTime.Now;
                model.CreatedBy = User.Identity.Name;

                await _dataContext.Sliders.AddAsync(model);
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Slider Created Successfully!");
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
            var slider = await _dataContext.Sliders.FindAsync(id);
            if (slider == null)
            {
                return NotFound();
            }
            return View(slider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SliderModel model)
        {
            var existSlider = await _dataContext.Sliders.FindAsync(model.Id);

            if (ModelState.IsValid)
            {
                if (model.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    string oldFilePath = Path.Combine(uploadsDir, existSlider.Image);
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
                    existSlider.Image = imageName;
                }

                existSlider.Title = model.Title;
                existSlider.Description = model.Description;
                existSlider.Status = model.Status;
                existSlider.ModifierDate = DateTime.Now;
                existSlider.ModifierBy = User.Identity.Name;

                _dataContext.Sliders.Update(existSlider);
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
            var slider = await _dataContext.Sliders.FindAsync(id);

            if (slider == null)
            {
                _notyf.Error("Slider not found!");
            }

            string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
            string filePath = Path.Combine(uploadsDir, slider.Image);

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

            _dataContext.Sliders.Remove(slider);
            await _dataContext.SaveChangesAsync();
            _notyf.Success("Xóa thành công!");
            return RedirectToAction("Index");
        }
    }
}
