using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Data;
using VitalHelse.Helpers;
using VitalHelse.Models;

namespace VitalHelse.Controllers;

public class ReccomendationsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ReccomendationsController> _logger;

    public ReccomendationsController(ApplicationDbContext db, ILogger<ReccomendationsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecommendations(int productId)
    {
        var categoryIds = await _db.Products
            .Where(p => p.ProductId == productId)
            .SelectMany(p => p.ProductCategories.Select(pc => pc.CategoryId))
            .ToListAsync();
    
        var recommendations = await _db.Products
            .Where(p => p.ProductId != productId && 
                        p.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryId)))
            .Include(p => p.ProductPictures)
            .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
            .ToListAsync();
    
        var recommendationViewModels = recommendations.Select(p => new ReccomendationsViewModel
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            StockCount = p.StockCount,
            ProductPriceInVAT = p.ProductPriceInVAT,
            ProductCampaignPrice = p.ProductCampaignPrice,
            FirstImagePath = p.ProductPictures.FirstOrDefault()?.PicturePath,
            CategoryNames = p.ProductCategories
                .Select(pc => pc.Category.CategoryName)
                .ToList(),
            ProductUrl = SlugHelper.BuildProductPath(p)
        }).ToList();
    
        return PartialView("_ReccomendationsLayout", recommendationViewModels);
    }
}