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
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;

        public CategoryController(DataContext context, INotyfService notyf)
        {
            _dataContext = context;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var categories = await _dataContext.Categories.OrderByDescending(x => x.Id).ToListAsync();

            const int pageSize = 10; //10 items/trang

            if (page < 1) //page < 1;
            {
                page = 1; //page ==1
            }
            int recsCount = categories.Count(); //33 items;

            var pager = new Paginate(recsCount, page, pageSize);

            int recSkip = (page - 1) * pageSize; //(3 - 1) * 10; 

            var data = categories.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Page = pager;

            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel model)
        {
            if (ModelState.IsValid)
            {
                model.Slug = SlugHelper.GenerateSlug(model.Name);
                var slug = await _dataContext.Categories.FirstOrDefaultAsync(x => x.Slug == model.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("Name", "Category Name Already Exist");
                    return View(model);
                }

                model.CreatedBy = User.Identity.Name;
                model.CreatedDate = DateTime.Now;

                await _dataContext.Categories.AddAsync(model);
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Category Created Successfully");
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
            var category = await _dataContext.Categories.FindAsync(id);
            if (category == null)
            {
                _notyf.Error("Category Not Found");
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryModel model)
        {
            var existCategory = await _dataContext.Categories.FindAsync(model.Id);
            if (ModelState.IsValid)
            {
                existCategory.Slug = SlugHelper.GenerateSlug(model.Name);
                existCategory.Name = model.Name;
                existCategory.Description = model.Description;
                existCategory.Status = model.Status;
                existCategory.ModifierBy = User.Identity.Name;
                existCategory.ModifierDate = DateTime.Now;

                _dataContext.Categories.Update(existCategory);
                await _dataContext.SaveChangesAsync();
                _notyf.Success("Category Updated Successfully");

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
            var category = await _dataContext.Categories.FindAsync(id);
            if (category == null)
            {
                _notyf.Error("Category Not Found");
            }
            _dataContext.Categories.Remove(category);
            await _dataContext.SaveChangesAsync();
            _notyf.Success("Category Deleted Successfully");
            return RedirectToAction("Index");
        }
    }
}
