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

    public static readonly string[] Statuses = { "Betalt", "Sendt", "Levert", "Kansellert" };

    [HttpPost("Admin/Ordre/Oppdater")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrder(int orderId, string status, string? trackingNumber)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null) return NotFound();

        if (!Statuses.Contains(status))
        {
            TempData["Error"] = "Ugyldig status.";
            return RedirectToAction(nameof(Orders));
        }

        order.Status = status;
        if (!string.IsNullOrWhiteSpace(trackingNumber))
            order.TrackingNumber = trackingNumber.Trim();

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Ordre #{orderId} er oppdatert.";
        return RedirectToAction(nameof(Orders));
    }
}
