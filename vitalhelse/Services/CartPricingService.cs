using Microsoft.EntityFrameworkCore;
using VitalHelse.Data;
using VitalHelse.Models;
using VitalHelse.Models.Shipping;

namespace VitalHelse.Services;

/// <summary>Every figure shown for a cart or placed on an order. Prices include VAT.</summary>
public record CartTotals(
    decimal SumBefore,
    decimal CampaignDiscount,
    decimal SumProducts,
    string? CodeName,
    int CodePercent,
    decimal CodeDiscount,
    decimal Shipping,
    decimal Total,
    decimal VatAmount,
    decimal TotalExVat,
    ShippingMethod? ShippingMethod)
{
    public int TotalInMinorUnits => (int)Math.Round(Total * 100);
}

/// <summary>
/// The one place that calculates cart totals, so the cart, summary, review page, order and Stripe session always agree.
/// </summary>
public class CartPricingService
{
    private readonly ApplicationDbContext _db;

    public CartPricingService(ApplicationDbContext db) => _db = db;

    public CartTotals Calculate(IReadOnlyCollection<CartProduct> items, int? shippingMethodId, ActiveDiscount? code)
    {
        var sumBefore = items.Sum(i => Pricing.Round(i.Product.ProductPriceInVAT * i.Quantity));
        var sumProducts = items.Sum(i => Pricing.Round(Pricing.Effective(i.Product) * i.Quantity));
        var campaignDiscount = sumBefore - sumProducts;

        var codePercent = code?.Percent ?? 0;
        var codeDiscount = Pricing.Round(sumProducts * (codePercent / 100m));

        ShippingMethod? method = shippingMethodId == null
            ? null
            : _db.ShippingMethods.FirstOrDefault(m => m.Id == shippingMethodId);

        decimal shipping = 0;
        if (items.Count > 0 && method != null)
        {
            // The shipping tier is picked by the value of the goods after campaign prices, before any discount code
            var basePrice = _db.ShippingPriceThresholds
                .AsEnumerable()
                .Where(t => sumProducts >= t.MinOrderAmount)
                .OrderBy(t => t.MinOrderAmount)
                .Select(t => t.ShippingPrice)
                .DefaultIfEmpty(0)
                .Last();
            shipping = Pricing.Round(basePrice * method.RateMultiplier);
        }

        var total = sumProducts - codeDiscount + shipping;
        var vat = Pricing.VatPart(total);

        return new CartTotals(sumBefore, campaignDiscount, sumProducts, code?.Code, codePercent, codeDiscount,
            shipping, total, vat, total - vat, method);
    }

    public async Task<List<CartProduct>> GetCartAsync(string userId) =>
        await _db.CartProducts
            .Include(c => c.Product).ThenInclude(p => p.ProductPictures)
            .Where(c => c.AspNetUsersId == userId)
            .OrderBy(c => c.ProductId)
            .ToListAsync();
}
