using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VitalHelse.Data;
using VitalHelse.Models;
using VitalHelse.Models.Shipping;

namespace VitalHelse.Controllers.Admin.ShippingManagement;

[Authorize]
public class ShippingManagementController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ProductController> _logger;
    private readonly UserManager<AspNetUsers> _um;
    
    public ShippingManagementController(ApplicationDbContext db, ILogger<ProductController> logger, UserManager<AspNetUsers> um)
    {
        _db = db;
        _logger = logger; 
        _um = um;
    }
    
    [HttpGet("Admin/Frakt")]
    public IActionResult Shipping()
    {
        var vm = new ShippingViewModel
        {
            Thresholds = _db.ShippingPriceThresholds.AsEnumerable().OrderBy(t => t.MinOrderAmount).ToList(),
            Methods = _db.ShippingMethods.ToList()
        };

        return View(vm);
    }

    [HttpPost("Admin/Frakt/legg-til-grense")]
    public IActionResult AddThreshold(decimal minOrderAmount, decimal shippingPrice)
    {
        _db.ShippingPriceThresholds.Add(new ShippingPriceThreshold
        {
            MinOrderAmount = minOrderAmount,
            ShippingPrice = shippingPrice
        });

        _db.SaveChanges();
        return RedirectToAction("Shipping");
    }

    [HttpPost("Admin/Frakt/legg-til-metode")]
    public IActionResult AddMethod(string name, decimal rate)
    {
        _db.ShippingMethods.Add(new ShippingMethod
        {
            MethodName = name,
            RateMultiplier = rate
        });

        _db.SaveChanges();
        return RedirectToAction("Shipping");
    }

    [HttpPost("Admin/Shipping/DeleteThreshold")]
    public IActionResult DeleteThreshold(int id)
    {
        var item = _db.ShippingPriceThresholds.Find(id);
        if (item != null)
        {
            _db.ShippingPriceThresholds.Remove(item);
            _db.SaveChanges();
        }

        return RedirectToAction("Shipping");
    }

    [HttpPost("Admin/Shipping/DeleteMethod")]
    public IActionResult DeleteMethod(int id)
    {
        var item = _db.ShippingMethods.Find(id);
        if (item != null)
        {
            _db.ShippingMethods.Remove(item);
            _db.SaveChanges();
        }

        return RedirectToAction("Shipping");
    }

}