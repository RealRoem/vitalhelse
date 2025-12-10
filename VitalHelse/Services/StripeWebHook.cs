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

    public StripeWebHook(ApplicationDbContext db, UserManager<AspNetUsers> userManager, IOptions<StripeOptions> stripeOptions, EmailService emailService)
    {
        _db = db;
        _userManager = userManager;
        _stripeOptions = stripeOptions.Value;
        _emailService = emailService;
    }
        
    /// <summary>
    /// Webhook, only implemented when purchase is completed, may be changed later.
    /// When purchase is completed, remove all items from the shopping cart and create a new order
    /// </summary>
    /// <returns>A new order</returns>
    [HttpPost]
    public async Task<IActionResult> Index()
    {
        // Reads the webhook
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        Event stripeEvent;

        try
        {
            // Verify signature, safety against false webhooks
            stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _stripeOptions.WebhookSecret,
                throwOnApiVersionMismatch: false);
        }
        catch (StripeException e)
        {
            Console.WriteLine($"Stripe error: {e.Message}");
            return BadRequest();
        }
        if (stripeEvent.Type != "payment_intent.succeeded")
        {
            return Ok();
        }
        
        // Event when the payment is succeeded
        if (stripeEvent.Type == "payment_intent.succeeded")
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
                return Ok();

            if (!paymentIntent.Metadata.TryGetValue("userId", out var userId))
            {
                Console.WriteLine("Missing userId in payment metadata");
                return Ok();
            }
            
            var user = await _userManager.FindByIdAsync(userId);
            
            var paymentIntentId = paymentIntent.Id;

            // Test to avoid duplicate order in case of network error
            var existingOrder = await _db.Orders
                .FirstOrDefaultAsync(o => o.StripePaymentIntentId == paymentIntentId);

            if (existingOrder != null)
            {
                Console.WriteLine($"Order already exists for PaymentIntent {paymentIntentId}");
                return Ok();
            }

            Console.WriteLine($"Payment succeeded for user {userId}");

            // Get cart
            var cartItems = await _db.CartProducts
                .Include(cp => cp.Product)
                .Where(cp => cp.AspNetUsersId == userId)
                .ToListAsync();

            //if (!cartItems.Any()) return RedirectToAction("Index");

            decimal beforeDiscount = cartItems.Sum(i => i.Product.ProductPriceInVAT * i.Quantity);
            decimal productTotal = cartItems.Sum(i => (i.Product.ProductCampaignPrice ?? i.Product.ProductPriceInVAT) * i.Quantity);
            decimal productDiscount = beforeDiscount - productTotal;

            
            
            // Get user address
            var address = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == user.DefaultUserAddressId);
            // Get user shipping method
            var method = await _db.ShippingMethods.FirstOrDefaultAsync(s => s.Id == user.DefaultShippingMethodId);

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
            decimal shippingPrice = basePrice * method.RateMultiplier;
            
            int percent = HttpContext.Session.GetInt32("DiscountPercent") ?? 0;
            decimal discountAmount = productTotal * (percent / 100m);
            decimal total = productTotal - discountAmount + shippingPrice;

            // Create order
            var order = new Order
            {
                OrderDate = DateTime.UtcNow,
                Status = "Paid",
                TotalCost = total,
                AspNetUsersId = userId,

                DiscountCodeId = percent > 0 ? $"{percent}%" : null,
                ShippingProvider = method.MethodName,
                
                ShippingFirstName = address.FirstName,
                ShippingLastName = address.LastName,
                ShippingStreet = address.Street,
                ShippingPostalCode = address.PostalCode,
                ShippingCity = address.City,
                ShippingPhoneNumber = address.PhoneNumber,
                
                ShippingMethodName = method.MethodName,
                ShippingMethodRateMultiplier = method.RateMultiplier,
                ShippingPrice = shippingPrice
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync(); 
            
            // Add products
            foreach (var item in cartItems)
            {
                _db.OrderProducts.Add(new OrderProduct
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                });
            }

            await _db.SaveChangesAsync();

            // Clear cart
            _db.CartProducts.RemoveRange(cartItems);
            await _db.SaveChangesAsync();

            // Remove discount code after use
            HttpContext.Session.Remove("DiscountPercent");

            Console.WriteLine($"Created order {order.OrderId} for user {userId}");
            
           
            // Lists for the product of the customer
            var productListHtml = "";
            var productListText = "";

            // Adds all products, quantity and their price to the list
            foreach (var product in cartItems)
            {
                productListHtml += $"<li>{product.Product.ProductName} – {product.Quantity} stk – {product.Product.ProductPriceInVAT} kr</li>";
                productListText += $"{product.Product.ProductName} - {product.Quantity} stk - {product.Product.ProductPriceInVAT} kr\n";
                
                // Reduce stock
                product.Product.StockCount -= product.Quantity;
                _db.Products.Update(product.Product);
            }
            await _db.SaveChangesAsync();


            string firstName = user.FirstName;
            string lastName = user.LastName;
            string email = user.Email;
            
            // Sends email
            await _emailService.SendEmailAsync(
                to: email,
                firstAndLastName: $"{lastName} {firstName}",
                subject: "Kvittering for kjøp hos Vital Helse",
                body: 
                "Takk for at du handlet hos Vital Helse!"+
                "Ordrenummer: "+ order.OrderId +
                "Dato: "+ order.OrderDate +
                "Produkter:\n" + productListText + "\n" +
                $"Pris: {paymentIntent.Amount / 100m} {paymentIntent.Currency}"+ 
                $"Frakt: {shippingPrice}"+
                $"Totalt: {(paymentIntent.Amount / 100m)+shippingPrice} {paymentIntent.Currency}"+
                "Har du spørsmål? Kontakt oss på VitalHelse@butikk.no eller besøk VitalHelse.no",
                htmlBody: 
                "<h1>Takk for at du handlet hos Vital Helse!</h1>\n\n" +
                $"<p><strong>Ordrenummer:</strong> {order.OrderId}</p>\n" +
                $"<p><strong>Dato:</strong> {order.OrderDate:dd.MM.yyyy}</p>\n\n" +
                "<h2>Bestilling</h2>\n" +
                $"<ul>{productListHtml}</ul>\n" +
                $"<p><strong>Pris:</strong> {paymentIntent.Amount / 100m} {paymentIntent.Currency.ToUpper()}</p>\n" +
                $"<p><strong>Frakt:</strong></p> {shippingPrice}\n" +
                $"<p><strong>Totalt:</strong></p> {(paymentIntent.Amount / 100m)+shippingPrice} {paymentIntent.Currency}\n\n" +
                "<p><strong>Spor pakken din her:</strong> \n" +
                "</p>\n\n<p>\n" +
                "Har du spørsmål? Kontakt oss på \n" +
                "<a href=\"mailto:VitalHelse@butikk.no\">VitalHelse@butikk.no</a> \n" +
                "eller besøk \n" +
                "<a href=\"https://vitalhelse.no\">VitalHelse.no</a>.\n" +
                "</p>");
        }
        else
        { 
            Console.WriteLine($"Unhandled event type: {stripeEvent.Type}");
        }
        return Ok();
    }
}

