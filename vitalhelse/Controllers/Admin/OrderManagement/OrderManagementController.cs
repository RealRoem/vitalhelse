using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Data;
using VitalHelse.Models;

namespace VitalHelse.Controllers.Admin.OrderManagement;

[Authorize(Roles = "Admin,Staff")]
public class OrderManagementController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ProductController> _logger;
    private readonly UserManager<AspNetUsers> _um;
    
    public OrderManagementController(ApplicationDbContext db, ILogger<ProductController> logger, UserManager<AspNetUsers> um)
    {
        _db = db;
        _logger = logger; 
        _um = um;
    }
    
    [HttpGet("Admin/Ordre")]
    public async Task<IActionResult> Orders()
    {
        var orders = await _db.Orders
            .Include(o => o.AspNetUsers)
            .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }
}