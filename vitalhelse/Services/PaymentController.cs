using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using VitalHelse.Configuration;
using VitalHelse.Data;
using VitalHelse.Models;
using VitalHelse.Services;

using VitalHelse.Configuration;
using VitalHelse.Data;
namespace VitalHelse.Controllers;

[Route("create-checkout-session")]
[ApiController]
public class PaymentController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IOptions<StripeOptions> _stripeOptions;
    private readonly UserManager<AspNetUsers> _userManager;

    public PaymentController(ApplicationDbContext db, IOptions<StripeOptions> stripeOptions, UserManager<AspNetUsers> userManager)
    {
        _db = db;
        _stripeOptions = stripeOptions;
        _userManager = userManager;
    }

    /// <summary>
    /// Returns the publishable key so the front end does not need a hardcoded one.
    /// </summary>
    [HttpGet("config")]
    public IActionResult Config() => Json(new { publishableKey = _stripeOptions.Value.PublishableKey });

    /// <summary>
    /// Creates a checkout session for the logged-in user from the cart, using the same totals the customer has seen.
    /// </summary>
    /// <returns>The client secret the embedded Stripe checkout needs</returns>
    [HttpPost]
    public async Task<IActionResult> CreateCheckoutSession(
        [FromServices] CartPricingService pricing, [FromServices] VitalHelse.Services.DiscountService discounts)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var cart = await pricing.GetCartAsync(user.Id);
        if (cart.Count == 0) return BadRequest("Handlekurven er tom.");
        if (user.DefaultShippingMethodId == null) return BadRequest("Leveringsmetode mangler.");

        var code = discounts.GetActive();
        var totals = pricing.Calculate(cart, user.DefaultShippingMethodId, code);

        var lineItems = cart.Select(item => new SessionLineItemOptions
        {
            Quantity = item.Quantity,
            PriceData = new SessionLineItemPriceDataOptions
            {
                Currency = "nok",
                // The discount code is taken off each unit price here, as Stripe cannot discount only part of an order
                UnitAmount = (long)Math.Round(Pricing.Effective(item.Product) * (100 - (code?.Percent ?? 0))),
                ProductData = new SessionLineItemPriceDataProductDataOptions { Name = item.Product.ProductName }
            }
        }).ToList();

        lineItems.Add(new SessionLineItemOptions
        {
            Quantity = 1,
            PriceData = new SessionLineItemPriceDataOptions
            {
                Currency = "nok",
                UnitAmount = (long)Math.Round(totals.Shipping * 100),
                ProductData = new SessionLineItemPriceDataProductDataOptions { Name = "Frakt" }
            }
        });

        var host = $"{Request.Scheme}://{Request.Host}";
        var options = new SessionCreateOptions
        {
            UiMode = "embedded",
            Mode = "payment",
            LineItems = lineItems,
            ReturnUrl = host + "/return.html?session_id={CHECKOUT_SESSION_ID}",
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                // The webhook has no browser session, so what it needs travels with the payment
                Metadata = new Dictionary<string, string>
                {
                    { "userId", user.Id },
                    { "discountCode", code?.Code ?? "" }
                }
            }
        };

        var session = await new SessionService().CreateAsync(options);
        return Json(new { clientSecret = session.ClientSecret });
    }
}

/// <summary>
/// Gets confirmation from stripe when purchase confirmed
/// Not safe to create orders, may be manipulated
/// </summary>
[Route("session-status")]
[ApiController]
public class SessionStatusController : Controller
{
    [HttpGet]
    public ActionResult SessionStatus([FromQuery] string session_id)
    {
        var sessionService = new SessionService();
        Session session = sessionService.Get(session_id);

        return Json(new {status = session.Status,  customer_email = session.CustomerDetails.Email});
    }
}