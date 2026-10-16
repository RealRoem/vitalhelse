using System.Security.Claims;
using VitalHelse.Models;

namespace VitalHelse.Helpers;

public static class ProductQueryExtensions
{
    /// <summary>Products marked as hidden (e.g. new from Tripletex, not yet approved) are only visible to staff and admins.</summary>
    public static IQueryable<Product> VisibleFor(this IQueryable<Product> products, ClaimsPrincipal user) =>
        user.IsInRole("Admin") || user.IsInRole("Staff")
            ? products
            : products.Where(p => p.ProductVisibility);

    public static bool IsVisibleFor(this Product product, ClaimsPrincipal user) =>
        product.ProductVisibility || user.IsInRole("Admin") || user.IsInRole("Staff");
}
