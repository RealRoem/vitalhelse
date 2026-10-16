using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.V5.Pages.Account.Manage.Internal;
using VitalHelse.Areas.Identity.Pages.Account;
using VitalHelse.Data;

namespace VitalHelse.Controllers;
using Microsoft.AspNetCore.Identity;
using VitalHelse.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize]
public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ProductController> _logger;
    private readonly UserManager<AspNetUsers> _um;
    private readonly SignInManager<AspNetUsers> _si;
    public AccountController(ApplicationDbContext db, ILogger<ProductController> logger, UserManager<AspNetUsers> um, SignInManager<AspNetUsers> si)
    {
        _db = db;
        _logger = logger; 
        _um = um;
        _si = si;
    }
    
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _um.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("/Account/Login");
        }
        
        return View(user);
    }
    [HttpPost]
    public async Task<IActionResult> UpdateProfile(AspNetUsers updatedUser)
    {
        var user = await _um.GetUserAsync(User); 
        user.Email = updatedUser.Email;
        user.FirstName = updatedUser.FirstName;
        user.LastName = updatedUser.LastName;
        user.PhoneNumber = updatedUser.PhoneNumber;
        user.Address = updatedUser.Address;
        user.PostalCode = updatedUser.PostalCode;

        await _db.SaveChangesAsync();
        await _um.UpdateAsync(user);
        await _si.RefreshSignInAsync(user);
        return RedirectToAction("Index");
    }
    
}