using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Models;
using ShoppingFood.Repository;

namespace ShoppingFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly INotyfService _notyf;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager, INotyfService notyf, DataContext context)
        {
            _dataContext = context;
            _notyf = notyf;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var usersWithRoles = await (from u in _dataContext.Users
                                        join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                                        join r in _dataContext.Roles on ur.RoleId equals r.Id
                                        select new {User = u, RoleName = r.Name }).ToListAsync();
            return View(usersWithRoles);
        }

        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(new AppUserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUserModel model)
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            if (ModelState.IsValid)
            {
                var result = await _userManager.CreateAsync(model, model.PasswordHash);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var role = await _roleManager.FindByIdAsync(model.RoleId);
                    var addToRole = await _userManager.AddToRoleAsync(user, role.Name);

                    if (!addToRole.Succeeded)
                    {
                        AddIdentityErrors(addToRole);
                        return View(model);
                    }

                    _notyf.Success("User Created Successfully");
                    return RedirectToAction("Index");
                }
                else
                {
                    AddIdentityErrors(result);
                    return View(model);
                }
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

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _notyf.Error("User not found");
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _notyf.Error("User not found");
            }
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppUserModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    _notyf.Error("User not found");
                }
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.PasswordHash = model.PasswordHash;
                user.RoleId = model.RoleId;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _notyf.Success("User Edited Successfully");
                    return RedirectToAction("Index"); 
                }
                else
                {
                    AddIdentityErrors(result);
                    return View(model);
                }
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

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _notyf.Error("User not found");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _notyf.Error("User not found");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _notyf.Success("User Deleted Successfully");
                return RedirectToAction("Index");
            }
            else
            {
                AddIdentityErrors(result);
                return RedirectToAction("Index");
            }

        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
