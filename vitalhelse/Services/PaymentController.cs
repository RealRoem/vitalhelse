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
    /// Creates a checkout session for the logged-in user.
    /// Takes the products and quantities from the shopping cart,
    /// checks their prices, and includes them in the checkout.
    /// </summary>

    /// <returns> A checkout page</returns>
    [HttpPost]
    public async Task<IActionResult> CreateCheckoutSession()
    {
        // Finds the user id of the user creating the checkout session
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Fetch all the items from the cart
        var cartItems = await _db.CartProducts
            .Include(cp => cp.Product)
            .Where(cp => cp.AspNetUsersId == userId)
            .ToListAsync();

        var productService = new ProductService();
        var priceService = new PriceService();
        var lineItems = new List<SessionLineItemOptions>();

        // For every item in the cart
        foreach (var item in cartItems)
        {
            var product = item.Product;

            // If the product doesn't exist on the owners stripe account, create a new one
            if (string.IsNullOrEmpty(product.StripeProductId))
            {
                var stripeProduct = await productService.CreateAsync(new ProductCreateOptions
                {
                    Name = product.ProductName
                });
                product.StripeProductId = stripeProduct.Id;
            }

            // Determine the correct price (campaign price if available)
            var effectivePrice = product.ProductCampaignPrice ?? product.ProductPriceInVAT;
            var newUnitAmount = (long)(effectivePrice * 100);

            bool needNewPrice = true;

            // If a Stripe price exists, check if it matches current price
            if (!string.IsNullOrEmpty(product.StripePriceId))
            {
                var existingPrice = await priceService.GetAsync(product.StripePriceId);

                // If the price is correct, we can reuse it
                if (existingPrice.UnitAmount == newUnitAmount)
                {
                    needNewPrice = false;
                }
            }

            // If no price exists or the price has changed → create a new Stripe Price
            if (needNewPrice)
            {
                var stripePrice = await priceService.CreateAsync(new PriceCreateOptions
                {
                    UnitAmount = newUnitAmount,
                    Currency = "nok",
                    Product = product.StripeProductId
                });

                product.StripePriceId = stripePrice.Id;
            }

            
            lineItems.Add(new SessionLineItemOptions
            {
                Quantity = item.Quantity,
                Price = product.StripePriceId
            });
        }

        await _db.SaveChangesAsync();
        
        var user = await _userManager.FindByIdAsync(userId);
        //if (!cartItems.Any()) return RedirectToAction("Index");
        var method = await _db.ShippingMethods.FirstOrDefaultAsync(s => s.Id == user.DefaultShippingMethodId);
        decimal beforeDiscount = cartItems.Sum(i => i.Product.ProductPriceInVAT * i.Quantity);
        decimal productTotal = cartItems.Sum(i => (i.Product.ProductCampaignPrice ?? i.Product.ProductPriceInVAT) * i.Quantity);
        decimal productDiscount = beforeDiscount - productTotal;
        
        // Get the thresholds
        var thresholds = _db.ShippingPriceThresholds
            .AsEnumerable()
            .OrderBy(t => t.MinOrderAmount)
            .ToList();

        // Get correct shipping price based on cart total
        decimal basePrice = thresholds
            .Where(t => productTotal >= t.MinOrderAmount)
            .Select(t => t.ShippingPrice)
            .DefaultIfEmpty(0)
            .Last();

        // Calculate the shipping price based on thresholds and method
        var shippingPrice = basePrice * method.RateMultiplier;
        
        // Convert to smallest currency unit (øre)
        var shippingPriceMinor = (long)Math.Round(shippingPrice * 100);
        
        lineItems.Add(new SessionLineItemOptions
        {
            Quantity = 1,
            PriceData = new SessionLineItemPriceDataOptions
            {
                UnitAmount = shippingPriceMinor,
                Currency = "nok",
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = "Shipping"
                }
            }
        });

        // Creates checkout details
        var host = $"{Request.Scheme}://{Request.Host}";
        var options = new SessionCreateOptions
        {
            // Makes the checkout embedded to our website
            UiMode = "embedded",
            Mode = "payment",
            LineItems = lineItems,
            ReturnUrl = host + "/return.html?session_id={CHECKOUT_SESSION_ID}",
            
            // Saves the userId for the webhook later
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId }
                }
            }
        };

        var service = new SessionService();
        
        // Create the session with the given details (options)
        Session session = service.Create(options);

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