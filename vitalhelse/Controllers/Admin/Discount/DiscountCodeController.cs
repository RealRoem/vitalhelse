using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitalHelse.Data;
using VitalHelse.Models.Discount;
using VitalHelse.Services;

namespace VitalHelse.Controllers.Admin.Discount;

[Authorize(Roles = "Admin,Staff")]
[AutoValidateAntiforgeryToken]
public class DiscountCodeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly DiscountService _discountService;

    public DiscountCodeController(ApplicationDbContext context, DiscountService discountService)
    {
        _context = context;
        _discountService = discountService;
    }


    [HttpGet("Admin/Rabbatkoder")]
    public IActionResult Index()
    {
        var codes = _context.DiscountCodes
            .OrderByDescending(dc => dc.CreatedAt)
            .ToList();

        return View("DiscountCode", codes);
    }

    [HttpPost("Admin/DiscountCode/Create")]
    public IActionResult Create(string Code, int DiscountPercent, DateTime? ExpiresAt)
    {
        Code = (Code ?? "").Trim();
        if (Code.Length == 0 || DiscountPercent < 1 || DiscountPercent > 100)
        {
            TempData["Error"] = "Koden må ha et navn og en rabatt mellom 1 og 100 %.";
            return RedirectToAction("Index");
        }

        if (_context.DiscountCodes.Any(d => d.Code.ToLower() == Code.ToLower()))
        {
            TempData["Error"] = $"Rabattkoden {Code} finnes allerede.";
            return RedirectToAction("Index");
        }

        var newCode = new DiscountCode
        {
            Code = Code,
            DiscountPercent = DiscountPercent,
            // The form gives local time; codes are compared in UTC
            ExpiresAt = ExpiresAt?.ToUniversalTime(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.DiscountCodes.Add(newCode);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [HttpPost("Admin/DiscountCode/Toggle")]
    public IActionResult Toggle(int id)
    {
        var code = _context.DiscountCodes.Find(id);
        if (code == null) return NotFound();

        code.IsActive = !code.IsActive;
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [HttpPost("Admin/DiscountCode/Delete")]
    public IActionResult Delete(int id)
    {
        var code = _context.DiscountCodes.Find(id);
        if (code == null) return NotFound();

        _context.DiscountCodes.Remove(code);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }
}
