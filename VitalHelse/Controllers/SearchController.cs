using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Data;
using VitalHelse.Models;

namespace VitalHelse.Controllers;

/// <summary>
/// SearchController
/// Inherits from controller class
/// Contains methods for searching products
/// </summary>
public class SearchController : Controller
{
    private readonly ApplicationDbContext _context;

    public SearchController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a list of suggested products based on a search term.
    /// </summary>
    /// <param name="term">
    /// A string representing the search term used to match product names or descriptions.
    /// </param>
    /// <param name="maxResults">
    /// An optional integer specifying the maximum number of suggestions to return. Default value is 10.
    /// </param>
    /// <returns>
    /// An asynchronous result containing a JSON object with a list of product suggestions.
    /// Each suggestion includes product ID, name, truncated description, and price.
    /// If the search term is null, empty, or whitespace, an empty JSON list is returned.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> SearchSuggestions(string term, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return Json(new List<object>());
        }

        term = term.ToLower();

        var suggestions = await _context.Products
            .Where(p =>
                (p.ProductName != null && p.ProductName.ToLower().Contains(term)) ||
                (p.ProductDescription != null && p.ProductDescription.ToLower().Contains(term))
            )
            .Take(maxResults)
            //Transforms the result set into a list of objects with the following properties:
            .Select(p => new
            {
                id = p.ProductId,
                name = p.ProductName,
                description = p.ProductDescription != null && p.ProductDescription.Length > 50
                    ? p.ProductDescription.Substring(0, 50) + "..."
                    : p.ProductDescription,
                price = p.ProductPriceInVAT
            })
            .ToListAsync();
        //Returns the list of suggestions as a JSON object
        return Json(suggestions);
    }

    /// <summary>
    /// Retrieves a list of products based on a search term.
    /// Also eventual filters can be applied in the filter sidebar on the seach index page
    /// </summary>
    /// <param name="query">
    /// A string representing the search term used to match product names or descriptions.
    /// </param>
    /// <returns>
    /// An asynchronous result containing a view with a list of products.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> Search(
        string query,
        List<int> categoryIds,
        int? tagId,
        int? sizeId,
        decimal? minPrice,
        decimal? maxPrice,
        bool? discountOnly,
        bool? inStockOnly)
    
    {
        var productsQuery = _context.Products
            .Include(p => p.ProductPictures)
            .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.PhysicalProductAttributes).ThenInclude(pa => pa.PhysicalAttribute)
            .AsQueryable();

        ViewData["SearchQuery"] = query;
        
        // Search text
        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.ToLower();
            productsQuery = productsQuery.Where(p =>
                (p.ProductName != null && p.ProductName.ToLower().Contains(lowerQuery)) ||
                (p.ProductDescription != null && p.ProductDescription.ToLower().Contains(lowerQuery)));
        }

        // Filter by Category
        if (categoryIds != null && categoryIds.Any())
            productsQuery = productsQuery.Where(p =>
                p.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryId)));

        // Filter by Tag
        if (tagId.HasValue)
            productsQuery = productsQuery.Where(p =>
                p.ProductTags.Any(pt => pt.TagId == tagId.Value));

        // Filter by Size (PhysicalAttribute)
        if (sizeId.HasValue)
            productsQuery = productsQuery.Where(p =>
                p.PhysicalProductAttributes.Any(pa => pa.PhysicalAttributeId == sizeId.Value));

        // Filter by Price range
        if (minPrice.HasValue)
            productsQuery = productsQuery.Where(p => p.ProductPriceInVAT >= minPrice.Value);

        if (maxPrice.HasValue)
            productsQuery = productsQuery.Where(p => p.ProductPriceInVAT <= maxPrice.Value);

        // Filter by discount
        if (discountOnly == true)
            productsQuery = productsQuery.Where(p => p.ProductCampaignPrice != null);

        // Filter by availability
        if (inStockOnly == true)
            productsQuery = productsQuery.Where(p => p.StockCount > 0);

        var products = await productsQuery.ToListAsync();

        // Populate dropdown data for the filter bar
        ViewData["Categories"] = await _context.Categories.ToListAsync();
        ViewData["Tags"] = await _context.Tags.ToListAsync();
        ViewData["Sizes"] = await _context.PhysicalAttributes.ToListAsync();
        ViewData["SelectedCategoryIds"] = categoryIds ?? new List<int>();
        //ViewData["SelectedTagId"] = tagId;
        ViewData["SelectedSizeId"] = sizeId;
        ViewData["SelectedMinPrice"] = minPrice;
        ViewData["SelectedMaxPrice"] = maxPrice;
        ViewData["SelectedDiscountOnly"] = discountOnly ?? false;
        ViewData["SelectedInStockOnly"] = inStockOnly ?? false;
        ViewData["SearchQuery"] = query;

        return View(products);
    }
}
