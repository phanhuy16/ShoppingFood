using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Models;
using ShoppingFood.Repository;

namespace ShoppingFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(RoleManager<IdentityRole> roleManager, INotyfService notyf, DataContext context)
        {
            _dataContext = context;
            _notyf = notyf;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            var roles = await _dataContext.Roles.OrderByDescending(x => x.Id).ToListAsync();
            return View(roles);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IdentityRole model)
        {
            // avoid duplicate role
            if (ModelState.IsValid)
            {
                var roleExits = await _roleManager.RoleExistsAsync(model.Name);

                if (!roleExits)
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.Name));
                }
                _notyf.Success("Role created successfully");
                return RedirectToAction("Index");
            }
            _notyf.Error("Role not found");
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _notyf.Error("Role not found");
            }

            var role = await _roleManager.FindByIdAsync(id);

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IdentityRole model, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _notyf.Error("Role not found");
            }

            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    _notyf.Error("Role not found");
                }
                role.Name = model.Name;
                try
                {
                    await _roleManager.UpdateAsync(role);
                    _notyf.Success("Role updated successfully");
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    _notyf.Error("Role not found");
                }
            }
            return View(model ?? new IdentityRole { Id = id });
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _notyf.Error("Role not found");
            }

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                _notyf.Error("Role not found");
            }

            try
            {
                await _roleManager.DeleteAsync(role);
                _notyf.Success("Role deleted successfully");
            }
            catch (Exception)
            {
                _notyf.Error("Role not found");
            }
            return RedirectToAction("Index");
        }
    }
}
