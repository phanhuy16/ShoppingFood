using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Helper;
using ShoppingFood.Models;
using ShoppingFood.Repository;

namespace ShoppingFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ContactController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ContactController(DataContext context, INotyfService notyf, IWebHostEnvironment environment)
        {
            _dataContext = context;
            _notyf = notyf;
            _webHostEnvironment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var contact = await _dataContext.Contacts.OrderByDescending(x=>x.Name).ToListAsync();
            return View(contact);
        }

        public async Task<IActionResult> Edit()
        {
            var contact = await _dataContext.Contacts.FirstOrDefaultAsync();
            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContactModel model)
        {
            var exitsContact = await _dataContext.Contacts.FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                if (model.LogoUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/contacts");
                    string imageName = Guid.NewGuid().ToString() + "_" + model.LogoUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    string oldFilePath = Path.Combine(uploadsDir, exitsContact.Logo);
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
                    await model.LogoUpload.CopyToAsync(fileStream);
                    fileStream.Close();
                    exitsContact.Logo = imageName;
                }

                exitsContact.Name = model.Name;
                exitsContact.Email = model.Email;
                exitsContact.Message = model.Message;
                exitsContact.Map = model.Map;
                exitsContact.Address = model.Address;
                exitsContact.Phone = model.Phone;

                _dataContext.Contacts.Update(exitsContact);
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
    }
}
