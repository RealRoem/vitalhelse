using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Data;
using VitalHelse.Models;

namespace VitalHelse.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class OrdersModel : PageModel
{
    private readonly UserManager<AspNetUsers> _userManager;
    private readonly ApplicationDbContext _db;

    public OrdersModel(UserManager<AspNetUsers> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public List<Order> Orders { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToPage("/Account/Login");

        Orders = await _db.Orders
            .Where(o => o.AspNetUsersId == user.Id)
            .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return Page();
    }
}