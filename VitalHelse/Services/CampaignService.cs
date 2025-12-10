namespace VitalHelse.Services;

using Data;
using Models.Discount;

public class CampaignService
{
    private readonly ApplicationDbContext _context;

    public CampaignService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Henter parent + direkte children
    private List<int> GetAllRelevantCategories(List<int> categoryIds)
    {
        var all = new HashSet<int>(categoryIds);

        // Finn direkte children til parent-kategorier
        var children = _context.Categories
            .Where(c => c.ParentCategoryId != null &&
                        categoryIds.Contains(c.ParentCategoryId.Value))
            .Select(c => c.CategoryId)
            .ToList();

        foreach (var child in children)
            all.Add(child);

        return all.ToList();
    }

    public void ApplyPricing(Campaign c)
    {
        if (!c.IsActive) return;

        // Parent + children
        var relevantCategories = GetAllRelevantCategories(c.CategoryIds);

        // Finn produkter i parent + children
        var products = _context.ProductCategories
            .Where(pc => relevantCategories.Contains(pc.CategoryId))
            .Select(pc => pc.Product)
            .Distinct()
            .ToList();

        // Finn rabatter for parent + children
        var categoryDiscounts = _context.CategoryDiscounts
            .Where(cd => relevantCategories.Contains(cd.CategoryId))
            .ToDictionary(cd => cd.CategoryId, cd => cd.DiscountPercent);

        // ARV rabatt ned til children
        foreach (var parentId in c.CategoryIds)
        {
            if (categoryDiscounts.TryGetValue(parentId, out var parentDiscount))
            {
                // Finn children til denne parenten
                var children = _context.Categories
                    .Where(cat => cat.ParentCategoryId == parentId)
                    .Select(cat => cat.CategoryId)
                    .ToList();

                // Arv discount til alle barn
                foreach (var childId in children)
                {
                    if (!categoryDiscounts.ContainsKey(childId))
                    {
                        categoryDiscounts[childId] = parentDiscount;
                    }
                }
            }
        }


        foreach (var p in products)
        {
            // Finn alle kategorier produktet ligger i
            var productCategories = _context.ProductCategories
                .Where(pc => pc.ProductId == p.ProductId)
                .Select(pc => pc.CategoryId)
                .ToList();

            decimal discountPercent = 0;

            // Finn høyeste rabatt for produktets kategorier
            foreach (var catId in productCategories)
            {
                if (categoryDiscounts.TryGetValue(catId, out var d))
                    discountPercent = Math.Max(discountPercent, d);
            }

            // Sett kampanjepris
            if (discountPercent > 0 && p.ProductPriceInVAT > 0)
            {
                decimal rate = (100m - discountPercent) / 100m;
                p.ProductCampaignPrice = p.ProductPriceInVAT * rate;
            }
            else
            {
                p.ProductCampaignPrice = null;
            }
        }

        _context.SaveChanges();
    }

    public void RemovePricing(Campaign c)
    {
        var relevantCategories = GetAllRelevantCategories(c.CategoryIds);

        var products = _context.ProductCategories
            .Where(pc => relevantCategories.Contains(pc.CategoryId))
            .Select(pc => pc.Product)
            .Distinct()
            .ToList();

        foreach (var p in products)
            p.ProductCampaignPrice = null;

        _context.SaveChanges();
    }
}
