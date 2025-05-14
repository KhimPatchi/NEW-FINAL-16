using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialWelfarre.Models;
namespace SocialWelfarre.Controllers
{

    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }



        public async Task<IActionResult> ViewCreate()
        {
            // Get users excluding the admin
            var users = await _userManager.Users
                .Where(u => u.Email != "admin@admin.com")
                .ToListAsync();
            // Fetch roles for each user and store in a dictionary
            var userRoles = new Dictionary<string, List<string>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);  // Get the roles for the user
                userRoles[user.Id] = roles.ToList();  // Store the roles in a dictionary with user ID as the key
            }
            // Pass both the users and the userRoles to the view
            ViewBag.UserRoles = userRoles;
            return View(users);
        }



        public async Task<IActionResult> Archived()
        {
            var users = await _userManager.Users.Where(u => u.IsArchived).ToListAsync();
            return View(users);
        }

        public IActionResult CreateUsers()
        {
            ViewBag.Roles = new SelectList(_roleManager.Roles.Where(r => r.Name != "Admin").ToList(), "Name", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUsers(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    First_Name = model.First_Name,
                    Middle_Name = model.Middle_Name,
                    Last_Name = model.Last_Name,
                    Age = model.Age,
                    IsMale = model.IsMale,
                    EmpNo = model.EmpNo,
                    DateOfBirth = model.DateOfBirth
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    return RedirectToAction("Dashboard", "Admin", new { area = "" });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            ViewBag.Roles = new SelectList(_roleManager.Roles.Where(r => r.Name != "Admin").ToList(), "Name", "Name");
            return View(model);
        }

        public async Task<IActionResult> Archive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsArchived = true;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("ViewCreate");
        }

        public async Task<IActionResult> Unarchive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsArchived = false;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("ViewCreate");
        }


    }
}

