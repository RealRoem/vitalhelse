using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitalHelse.Services;

namespace VitalHelse.Controllers;

/// <summary>Lets a logged-in customer enter or remove a discount code in the checkout.</summary>
[Authorize]
[Route("rabattkode")]
public class CustomerDiscountController : Controller
{
    private readonly DiscountService _discounts;

    public CustomerDiscountController(DiscountService discounts) => _discounts = discounts;

    [HttpPost("valider")]
    [ValidateAntiForgeryToken]
    public IActionResult Validate(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Json(new { success = false, message = "Skriv inn en kode." });

        var entry = _discounts.FindValid(code);
        if (entry == null)
            return Json(new { success = false, message = "Rabattkoden finnes ikke eller er utløpt." });

        _discounts.SetDiscountCode(entry.Code, entry.DiscountPercent);
        return Json(new { success = true, message = $"Rabattkode {entry.Code} er lagt til (−{entry.DiscountPercent} %)." });
    }

    [HttpPost("fjern")]
    [ValidateAntiForgeryToken]
    public IActionResult Remove()
    {
        _discounts.Clear();
        return Json(new { success = true });
    }
}
