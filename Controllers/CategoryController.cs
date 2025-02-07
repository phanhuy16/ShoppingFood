using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Models;
using ShoppingFood.Repository;

namespace ShoppingFood.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;

        public CategoryController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Index(string slug = "")
        {
            var category = await _dataContext.Categories.Where(x => x.Slug == slug).FirstOrDefaultAsync();
            if (category == null)
            {
                return RedirectToAction("Index");
            }

            var productByCate = await _dataContext.Products.Where(x=>x.CategoryId == category.Id).OrderByDescending(x => x.Id).ToListAsync();

            return View(productByCate);
        }
    }
}
