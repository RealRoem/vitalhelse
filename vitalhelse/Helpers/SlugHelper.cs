using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using VitalHelse.Models;

namespace VitalHelse.Helpers;

/// <summary>
/// Utility class for generating SEO-friendly slugs and URL paths
/// for products and categories in the VitalHelse web application.
/// </summary>
public static class SlugHelper
{
    /// <summary>
    /// Converts any input string into a clean, lowercase, URL-friendly slug.
    /// Removes diacritics, replaces spaces and special characters with hyphens,
    /// and trims redundant or leading/trailing dashes.
    /// </summary>
    /// <param name="input">The string to convert (e.g., a product or category name).</param>
    /// <returns>A normalized slug string (e.g., "Hyaluronic Acid Day Cream" → "hyaluronic-acid-day-cream").</returns>
    public static string ToUrlFriendly(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Convert to lowercase and normalize Unicode characters (e.g. æ → ae)
        var normalized = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var ch in normalized)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(ch);

            // Skip diacritics (accent marks, etc.)
            if (cat == UnicodeCategory.NonSpacingMark)
                continue;

            // Keep only alphanumeric characters
            if (char.IsLetterOrDigit(ch))
                sb.Append(ch);
            // Replace spaces, slashes, and underscores with hyphens
            else if (char.IsWhiteSpace(ch) || ch == '/' || ch == '_' || ch == '-')
                sb.Append('-');
            // Ignore all other characters
        }

        // Replace multiple consecutive hyphens with one and trim ends
        var slug = Regex.Replace(sb.ToString(), "-{2,}", "-").Trim('-');
        return slug;
    }

    /// <summary>
    /// Builds a full SEO-friendly URL path for a product, including
    /// category hierarchy and product name.
    /// </summary>
    /// <example>
    /// For a product in the category "Hudpleie/Ansikt" with name "Aloe Vera Cream",
    /// the output will be: "/produkt/hudpleie/ansikt/aloe-vera-cream".
    /// </example>
    /// <param name="product">The product entity from the database.</param>
    /// <returns>A relative URL path starting with "/produkt/...".</returns>
    public static string BuildProductPath(Product product)
    {
        var segments = new List<string> { "produkt" };

        // Traverse up the category tree using the product's first category as the "primary" one
        var category = product.ProductCategories?.FirstOrDefault()?.Category;
        var stack = new List<string>();

        while (category != null)
        {
            stack.Insert(0, ToUrlFriendly(category.CategoryName));
            category = category.ParentCategory;
        }

        // Combine category segments and product name into the final path
        segments.AddRange(stack);
        segments.Add(ToUrlFriendly(product.ProductName));

        return "/" + string.Join("/", segments);
    }
}
