using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using VitalHelse.Configuration;
using VitalHelse.Data;
using VitalHelse.Models;
using VitalHelse.Services;

namespace VitalHelse.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StripeWebHook : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<AspNetUsers> _userManager;
    private readonly StripeOptions _stripeOptions;
    private readonly EmailService _emailService;
    private readonly OrderService _orders;
    private readonly VitalHelse.Services.DiscountService _discounts;
    private readonly ILogger<StripeWebHook> _logger;

    public StripeWebHook(ApplicationDbContext db, UserManager<AspNetUsers> userManager, IOptions<StripeOptions> stripeOptions,
        EmailService emailService, OrderService orders, VitalHelse.Services.DiscountService discounts, ILogger<StripeWebHook> logger)
    {
        _db = db;
        _userManager = userManager;
        _stripeOptions = stripeOptions.Value;
        _emailService = emailService;
        _orders = orders;
        _discounts = discounts;
        _logger = logger;
    }

    /// <summary>
    /// Stripe tells us when a payment has gone through. The order is created from the customer's cart,
    /// exactly like the demo checkout does, and a receipt is sent.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Index()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        Event stripeEvent;
        try
        {
            // Verify the signature so forged webhooks are rejected
            stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"],
                _stripeOptions.WebhookSecret, throwOnApiVersionMismatch: false);
        }
        catch (StripeException e)
        {
            _logger.LogWarning("Stripe webhook rejected: {Message}", e.Message);
            return BadRequest();
        }

        if (stripeEvent.Type != "payment_intent.succeeded" || stripeEvent.Data.Object is not PaymentIntent paymentIntent)
            return Ok();

        if (!paymentIntent.Metadata.TryGetValue("userId", out var userId))
        {
            _logger.LogWarning("Payment {Id} has no userId in its metadata", paymentIntent.Id);
            return Ok();
        }

        // Stripe may deliver the same event twice
        if (await _db.Orders.AnyAsync(o => o.StripePaymentIntentId == paymentIntent.Id))
            return Ok();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Ok();

        // The code was already applied in the amount the customer paid, so it is honoured even if it has changed since
        ActiveDiscount? code = null;
        if (paymentIntent.Metadata.TryGetValue("discountCode", out var codeName) && !string.IsNullOrWhiteSpace(codeName))
        {
            var entry = await _db.DiscountCodes.FirstOrDefaultAsync(d => d.Code == codeName);
            if (entry != null) code = new ActiveDiscount(entry.Code, entry.DiscountPercent);
        }

        var result = await _orders.PlaceOrderAsync(user, code, "Betalt", paymentIntent.Id, enforceStock: false);
        if (result.Order == null)
        {
            _logger.LogError("Payment {Id} succeeded but the order could not be created: {Error}", paymentIntent.Id, result.Error);
            return Ok();
        }

        try
        {
            await SendReceiptAsync(user, result);
        }
        catch (Exception ex)
        {
            // A failed receipt must not make Stripe retry the webhook and create the order again
            _logger.LogWarning(ex, "Receipt for order {OrderId} could not be sent", result.Order.OrderId);
        }

        return Ok();
    }

    private async Task SendReceiptAsync(AspNetUsers user, PlaceOrderResult result)
    {
        var order = result.Order!;
        var totals = result.Totals!;

        var lines = await _db.OrderProducts
            .Include(op => op.Product)
            .Where(op => op.OrderId == order.OrderId)
            .ToListAsync();

        var text = string.Join("\n", lines.Select(l => $"{l.Product.ProductName} – {l.Quantity} stk – {Pricing.Money(l.UnitPrice)}"));
        var html = string.Concat(lines.Select(l => $"<li>{l.Product.ProductName} – {l.Quantity} stk – {Pricing.Money(l.UnitPrice)}</li>"));
        var discountText = totals.CodeDiscount > 0 ? $"Rabattkode {totals.CodeName}: −{Pricing.Money(totals.CodeDiscount)}\n" : "";

        await _emailService.SendEmailAsync(
            to: user.Email!,
            firstAndLastName: $"{user.FirstName} {user.LastName}".Trim(),
            subject: "Kvittering for kjøp hos Vital Helse",
            body: $"Takk for at du handlet hos Vital Helse!\nOrdrenummer: {order.OrderId}\nDato: {order.OrderDate:dd.MM.yyyy}\n\n{text}\n\n" +
                  discountText + $"Frakt: {Pricing.Money(totals.Shipping)}\nTotalt: {Pricing.Money(totals.Total)} (herav MVA {Pricing.Money(totals.VatAmount)})",
            htmlBody: $"<h1>Takk for at du handlet hos Vital Helse!</h1><p><strong>Ordrenummer:</strong> {order.OrderId}</p>" +
                      $"<p><strong>Dato:</strong> {order.OrderDate:dd.MM.yyyy}</p><h2>Bestilling</h2><ul>{html}</ul>" +
                      (totals.CodeDiscount > 0 ? $"<p>Rabattkode {totals.CodeName}: −{Pricing.Money(totals.CodeDiscount)}</p>" : "") +
                      $"<p><strong>Frakt:</strong> {Pricing.Money(totals.Shipping)}</p>" +
                      $"<p><strong>Totalt:</strong> {Pricing.Money(totals.Total)} (herav MVA {Pricing.Money(totals.VatAmount)})</p>");
    }
}
