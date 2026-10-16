namespace VitalHelse.Services;

using Data;
using Models.Discount;

/// <summary>
/// Campaign prices are derived data: a product is on sale when it belongs to a category of a campaign that is
/// switched on and currently inside its date range, and the sale price follows the category's discount rate.
/// Recalculating everything in one go keeps overlapping campaigns, expiry and price changes consistent.
/// </summary>
public class CampaignService
{
    private readonly ApplicationDbContext _context;

    public CampaignService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Switches off campaigns that have ended, then recalculates every product's campaign price.</summary>
    public void SyncSchedule()
    {
        var now = DateTime.Now;
        foreach (var expired in _context.Campaigns.Where(c => c.IsActive && c.End < now).ToList())
            expired.IsActive = false;

        _context.SaveChanges();
        RecalculateAll();
    }

    // Kept so older call sites keep working: any campaign change simply recalculates everything
    public void ApplyPricing(Campaign c) => RecalculateAll();
    public void RemovePricing(Campaign c) => RecalculateAll();

    public void RecalculateAll()
    {
        var now = DateTime.Now;

        var running = _context.Campaigns
            .Where(c => c.IsActive && c.Start <= now && c.End >= now)
            .ToList();

        var parentOf = _context.Categories
            .Select(c => new { c.CategoryId, c.ParentCategoryId })
            .ToDictionary(c => c.CategoryId, c => c.ParentCategoryId);

        var rates = _context.CategoryDiscounts.ToDictionary(d => d.CategoryId, d => d.DiscountPercent);

        // The categories chosen in a running campaign, as ancestors-or-self
        var campaignRoots = running.SelectMany(c => c.CategoryIds).ToHashSet();

        // The discount for a category: its own rate, otherwise the nearest ancestor's. Only categories under a
        // campaign category count.
        int RateFor(int categoryId)
        {
            var inCampaign = false;
            var rate = 0;
            var current = (int?)categoryId;
            var guard = 0;

            while (current != null && guard++ < 20)
            {
                if (campaignRoots.Contains(current.Value)) inCampaign = true;
                if (rate == 0 && rates.TryGetValue(current.Value, out var r) && r > 0) rate = r;
                current = parentOf.TryGetValue(current.Value, out var parent) ? parent : null;
            }

            return inCampaign ? rate : 0;
        }

        var categoriesByProduct = _context.ProductCategories
            .Select(pc => new { pc.ProductId, pc.CategoryId })
            .AsEnumerable()
            .GroupBy(pc => pc.ProductId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.CategoryId).ToList());

        foreach (var product in _context.Products.ToList())
        {
            var best = categoriesByProduct.TryGetValue(product.ProductId, out var categories)
                ? categories.Select(RateFor).DefaultIfEmpty(0).Max()
                : 0;

            product.ProductCampaignPrice = best > 0 && product.ProductPriceInVAT > 0
                ? Pricing.Round(product.ProductPriceInVAT * (100 - best) / 100m)
                : null;
        }

        _context.SaveChanges();
    }
}
