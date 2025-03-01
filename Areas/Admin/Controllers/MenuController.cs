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
    public class MenuController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;

        public MenuController(DataContext context, INotyfService notyf)
        {
            _dataContext = context;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index()
        {
            var menus = await _dataContext.Menus.OrderByDescending(x => x.Id).ToListAsync();
            return View(menus);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuModel model)
        {
            if (ModelState.IsValid)
            {
                model.Slug = SlugHelper.GenerateSlug(model.Name);
                var slug = await _dataContext.Categories.FirstOrDefaultAsync(x => x.Slug == model.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("Name", "Menu Name Already Exist");
                    return View(model);
                }

                await _dataContext.Menus.AddAsync(model);
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Menu Created Successfully");
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
            var menu = await _dataContext.Menus.FindAsync(id);
            if (menu == null)
            {
                _notyf.Error("Menu Not Found");
            }
            return View(menu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MenuModel model)
        {
            var existMenu = await _dataContext.Menus.FindAsync(model.Id);
            if (ModelState.IsValid)
            {
                existMenu.Slug = SlugHelper.GenerateSlug(model.Name);
                existMenu.Name = model.Name;
                existMenu.Status = model.Status;
                existMenu.Position = model.Position;

                _dataContext.Menus.Update(existMenu);
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Menu Updated Successfully");

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
            var menu = await _dataContext.Menus.FindAsync(id);
            if (menu == null)
            {
                _notyf.Error("Menu Not Found");
            }
            _dataContext.Menus.Remove(menu);
            await _dataContext.SaveChangesAsync();
            _notyf.Success("Menu Deleted Successfully");
            return RedirectToAction("Index");
        }
    }
}
