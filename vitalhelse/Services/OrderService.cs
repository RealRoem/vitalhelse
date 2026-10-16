using Microsoft.EntityFrameworkCore;
using VitalHelse.Data;
using VitalHelse.Models;

namespace VitalHelse.Services;

public record PlaceOrderResult(Order? Order, CartTotals? Totals, string? Error);

/// <summary>
/// Turns a customer's cart into an order. Used by the demo checkout and by the Stripe webhook,
/// so both create identical orders.
/// </summary>
public class OrderService
{
    private readonly ApplicationDbContext _db;
    private readonly CartPricingService _pricing;
    private readonly DiscountService _discounts;

    public OrderService(ApplicationDbContext db, CartPricingService pricing, DiscountService discounts)
    {
        _db = db;
        _pricing = pricing;
        _discounts = discounts;
    }

    /// <param name="enforceStock">False when the customer has already paid (webhook): the order is created regardless.</param>
    public async Task<PlaceOrderResult> PlaceOrderAsync(AspNetUsers user, ActiveDiscount? code, string status,
        string? paymentIntentId, bool enforceStock)
    {
        var cart = await _pricing.GetCartAsync(user.Id);
        if (cart.Count == 0)
            return new PlaceOrderResult(null, null, "Handlekurven er tom.");

        var address = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == user.DefaultUserAddressId && a.AspNetUserId == user.Id);
        if (address == null)
            return new PlaceOrderResult(null, null, "Du må velge en leveringsadresse.");

        var method = await _db.ShippingMethods.FirstOrDefaultAsync(m => m.Id == user.DefaultShippingMethodId);
        if (method == null)
            return new PlaceOrderResult(null, null, "Du må velge en leveringsmetode.");

        if (enforceStock)
        {
            var short1 = cart.FirstOrDefault(i => i.Product.StockCount != null && i.Quantity > Math.Max(0, i.Product.StockCount.Value));
            if (short1 != null)
            {
                var left = Math.Max(0, short1.Product.StockCount!.Value);
                return new PlaceOrderResult(null, null,
                    left == 0
                        ? $"{short1.Product.ProductName} er dessverre utsolgt. Fjern den fra handlekurven for å gå videre."
                        : $"Vi har bare {left} stk av {short1.Product.ProductName} på lager.");
            }
        }

        var totals = _pricing.Calculate(cart, method.Id, code);

        await using var tx = await _db.Database.BeginTransactionAsync();

        var order = new Order
        {
            StripePaymentIntentId = paymentIntentId,
            OrderDate = DateTime.Now,
            Status = status,
            TotalCost = totals.Total,
            AspNetUsersId = user.Id,
            DiscountCodeId = code == null ? null : $"{code.Code} (−{code.Percent} %)",
            ShippingProvider = method.MethodName,
            ShippingFirstName = address.FirstName ?? "",
            ShippingLastName = address.LastName ?? "",
            ShippingStreet = address.Street,
            ShippingPostalCode = address.PostalCode,
            ShippingCity = address.City,
            ShippingPhoneNumber = address.PhoneNumber,
            ShippingMethodName = method.MethodName,
            ShippingMethodRateMultiplier = method.RateMultiplier,
            ShippingPrice = totals.Shipping
        };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        foreach (var item in cart)
        {
            _db.OrderProducts.Add(new OrderProduct
            {
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = Pricing.Effective(item.Product)
            });

            if (item.Product.StockCount != null)
                item.Product.StockCount = Math.Max(0, item.Product.StockCount.Value - item.Quantity);
        }

        _db.CartProducts.RemoveRange(cart);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        _discounts.RegisterUsage(code?.Code);

        return new PlaceOrderResult(order, totals, null);
    }
}
