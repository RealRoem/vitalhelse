using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitalHelse.Data;
using VitalHelse.Models.Discount;
using VitalHelse.Services;

namespace VitalHelse.Controllers.Admin.Discount;

[Authorize(Roles = "Admin,Staff")]
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
        var newCode = new DiscountCode
        {
            Code = Code,
            DiscountPercent = DiscountPercent,
            ExpiresAt = ExpiresAt,
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

    [HttpPost]
    public IActionResult ValidateDiscountCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Json(new { success = false, message = "Ugyldig kode." });

        var now = DateTime.UtcNow;

        var discount = _context.DiscountCodes
            .FirstOrDefault(d =>
                d.Code.ToLower() == code.ToLower() &&
                d.IsActive &&
                (d.ExpiresAt == null || d.ExpiresAt > now));

        if (discount == null)
            return Json(new { success = false, message = "Rabattkoden finnes ikke eller er utløpt." });

        _discountService.SetDiscountCode(discount.Code, discount.DiscountPercent);

        return Json(new
        {
            success = true,
            percent = discount.DiscountPercent,
            message = $"Rabattkode brukt! -{discount.DiscountPercent}%"
        });
    }

}