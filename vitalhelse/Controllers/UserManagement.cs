using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Models;

namespace VitalHelse.Controllers;

[Authorize(Roles = "Admin")]
[Route("Admin/[controller]")]
public class UserManagementController : Controller
{
    private readonly UserManager<AspNetUsers> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserManagementController(UserManager<AspNetUsers> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var userList = new List<UserRoleViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userList.Add(new UserRoleViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                CurrentRoles = roles.ToList()
            });
        }

        return View("~/Views/Admin/Usermanagement/Index.cshtml", userList);
    }

    
    [HttpGet("EditRoles/{id}")]
    public async Task<IActionResult> EditRoles(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);
        var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

        var model = new EditUserRolesViewModel
        {
            UserId = user.Id,
            Email = user.Email,
            Roles = allRoles.Select(role => new RoleSelection
            {
                RoleName = role,
                IsSelected = userRoles.Contains(role)
            }).ToList()
        };

        return View("~/Views/Admin/UserManagement/EditRoles.cshtml",model);
    }

    [HttpPost("UpdateRoles")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRoles(EditUserRolesViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            Console.WriteLine("User not found!");
            return NotFound();
        }

        Console.WriteLine($"User found: {user.Email}");

        var userRoles = await _userManager.GetRolesAsync(user);
        Console.WriteLine($"Current roles: {string.Join(", ", userRoles)}");
            
        var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName).ToList();
        Console.WriteLine($"Selected roles: {string.Join(", ", selectedRoles)}");

        var rolesToRemove = userRoles.Except(selectedRoles).ToList();
        if (rolesToRemove.Any())
        {
            //Console.WriteLine($"Removing: {string.Join(", ", rolesToRemove)}");
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
        }

        var rolesToAdd = selectedRoles.Except(userRoles).ToList();
        if (rolesToAdd.Any())
        {
            //Console.WriteLine($"Adding: {string.Join(", ", rolesToAdd)}");
            await _userManager.AddToRolesAsync(user, rolesToAdd);
        }

        //Console.WriteLine("Changes saved!");
        TempData["Success"] = $"Roller oppdatert for {user.Email}";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("AddUser")]
    public async Task<IActionResult> AddUser()
    {
        var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        
        var model = new AddUserViewModel
        {
            Roles = allRoles.Select(role => new RoleSelection
            {
                RoleName = role,
                IsSelected = false
            }).ToList()
        };
        
        return View("~/Views/Admin/UserManagement/AddUser.cshtml",model);
    }

    [HttpPost("AddUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddUser(AddUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            model.Roles = allRoles.Select(role => new RoleSelection
            {
                RoleName = role,
                IsSelected = model.Roles?.Any(r => r.RoleName == role && r.IsSelected) ?? false
            }).ToList();
            
            return View("~/Views/Admin/UserManagement/AddUser.cshtml", model);
        }

        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "En bruker med denne e-postadressen finnes allerede");
            
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            model.Roles = allRoles.Select(role => new RoleSelection
            {
                RoleName = role,
                IsSelected = model.Roles?.Any(r => r.RoleName == role && r.IsSelected) ?? false
            }).ToList();
            
            return View("~/Views/Admin/UserManagement/AddUser.cshtml", model);
        }

        var newUser = new AspNetUsers
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true  
        };

        var createResult = await _userManager.CreateAsync(newUser, model.Password);

        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            model.Roles = allRoles.Select(role => new RoleSelection
            {
                RoleName = role,
                IsSelected = model.Roles?.Any(r => r.RoleName == role && r.IsSelected) ?? false
            }).ToList();
            
            return View(model);
        }

        var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName).ToList();
        if (selectedRoles.Any())
        {
            await _userManager.AddToRolesAsync(newUser, selectedRoles);
        }

        TempData["Success"] = $"Bruker {model.Email} ble opprettet med {selectedRoles.Count} rolle(r)";
        return RedirectToAction(nameof(Index));
    }
}

