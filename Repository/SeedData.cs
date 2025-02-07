using Microsoft.EntityFrameworkCore;
using ShoppingFood.Models;

namespace ShoppingFood.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {
            _context.Database.Migrate();
            if (!_context.Products.Any())
            {
                CategoryModel vegetables = new CategoryModel { Name = "Vegetables", Slug = "vegetables", Description = "Vegetables is Large Brand in the people", Status = 1 };
                CategoryModel fruits = new CategoryModel { Name = "Fruits", Slug = "fruits", Description = "Fruits is Large Brand in the people", Status = 1, };
                BrandModel ceres = new BrandModel { Name = "Ceres", Slug = "ceres", Description = "Ceres is Large Brand in the people", Status = 1, Image = "Ceres.png" };
                BrandModel tipco = new BrandModel { Name = "Tipco", Slug = "tipco", Description = "Tipco is Large Brand in the people", Status = 1, Image = "Tipco.png" };
                _context.Products.AddRange(
                    new ProductModel { Name = "Grapes", Slug = "grapes", Description = "Grapes is best", Price = 200000, Image = "Grapes.png", Category = vegetables, Status = 1, Brand = ceres, BrandId = ceres.Id, CategoryId = vegetables.Id },
                     new ProductModel { Name = "Raspberries", Slug = "raspberries", Description = "Raspberries is best", Price = 220000, Image = "Raspberries.png", Category = fruits, Status = 1, Brand = tipco, BrandId = tipco.Id, CategoryId = fruits.Id }
                );
                _context.SaveChanges();
            }
        }
    }
}
