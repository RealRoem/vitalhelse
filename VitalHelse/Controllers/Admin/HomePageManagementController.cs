using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Data;
using VitalHelse.Models;

namespace VitalHelse.Controllers;

/// <summary>
/// Controller for managing home page banners and settings
/// Restricted to Admin and Staff roles
/// </summary>
[Authorize(Roles = "Admin,Staff")]
public class HomePageManagementController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<HomePageManagementController> _logger;

    public HomePageManagementController(
        ApplicationDbContext db,
        IWebHostEnvironment environment,
        ILogger<HomePageManagementController> logger)
    {
        _db = db;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Main admin panel for home page management
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var banners = await _db.BannerImages
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync();

        var settings = await _db.HomePageSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new HomePageSettings();
            _db.HomePageSettings.Add(settings);
            await _db.SaveChangesAsync();
        }

        ViewBag.Settings = settings;
        ViewBag.Categories = await _db.Categories
            .Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        return View(banners);
    }

    /// <summary>
    /// Upload a new banner image
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadBanner(IFormFile bannerFile, string altText, string linkType, string linkTarget)
    {
        if (bannerFile == null || bannerFile.Length == 0)
        {
            return Json(new { success = false, message = "No file uploaded" });
        }

        // Validate file size (5MB max)
        if (bannerFile.Length > 5 * 1024 * 1024)
        {
            return Json(new { success = false, message = "File size exceeds 5MB limit" });
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(bannerFile.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return Json(new { success = false, message = "Invalid file type. Allowed: JPG, PNG, GIF, WEBP" });
        }

        try
        {
            // Create unique filename
            var fileName = $"banner_{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "banners");
            Directory.CreateDirectory(uploadsFolder);
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await bannerFile.CopyToAsync(stream);
            }

            // Get next display order
            var maxOrder = await _db.BannerImages.AnyAsync()
                ? await _db.BannerImages.MaxAsync(b => b.DisplayOrder)
                : -1;

            // Create banner record
            var banner = new BannerImage
            {
                ImagePath = $"/images/banners/{fileName}",
                AltText = altText,
                DisplayOrder = maxOrder + 1,
                IsActive = true,
                LinkType = linkType ?? "None",
                LinkTarget = linkTarget,
                CreatedAt = DateTime.UtcNow
            };

            _db.BannerImages.Add(banner);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Banner uploaded: {BannerId} by {User}", banner.BannerImageId, User.Identity?.Name);

            return Json(new { success = true, banner = new {
                banner.BannerImageId,
                banner.ImagePath,
                banner.AltText,
                banner.DisplayOrder,
                banner.IsActive,
                banner.LinkType,
                banner.LinkTarget
            }});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading banner");
            return Json(new { success = false, message = "Error uploading file" });
        }
    }

    /// <summary>
    /// Update banner order (for drag-and-drop)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBannerOrder([FromBody] List<int> bannerIds)
    {
        try
        {
            for (int i = 0; i < bannerIds.Count; i++)
            {
                var banner = await _db.BannerImages.FindAsync(bannerIds[i]);
                if (banner != null)
                {
                    banner.DisplayOrder = i;
                    banner.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating banner order");
            return Json(new { success = false, message = "Error updating order" });
        }
    }

    /// <summary>
    /// Update banner link settings
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBannerLink(int bannerId, string linkType, string linkTarget)
    {
        try
        {
            var banner = await _db.BannerImages.FindAsync(bannerId);
            if (banner == null)
            {
                return Json(new { success = false, message = "Banner not found" });
            }

            banner.LinkType = linkType;
            banner.LinkTarget = linkTarget;
            banner.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Banner link updated: {BannerId} by {User}", bannerId, User.Identity?.Name);

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating banner link");
            return Json(new { success = false, message = "Error updating link" });
        }
    }

    /// <summary>
    /// Toggle banner active status
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBannerActive(int bannerId)
    {
        try
        {
            var banner = await _db.BannerImages.FindAsync(bannerId);
            if (banner == null)
            {
                return Json(new { success = false, message = "Banner not found" });
            }

            banner.IsActive = !banner.IsActive;
            banner.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Json(new { success = true, isActive = banner.IsActive });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling banner active status");
            return Json(new { success = false, message = "Error toggling status" });
        }
    }

    /// <summary>
    /// Delete a banner
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBanner(int bannerId)
    {
        try
        {
            var banner = await _db.BannerImages.FindAsync(bannerId);
            if (banner == null)
            {
                return Json(new { success = false, message = "Banner not found" });
            }

            // Delete physical file
            var filePath = Path.Combine(_environment.WebRootPath, banner.ImagePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _db.BannerImages.Remove(banner);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Banner deleted: {BannerId} by {User}", bannerId, User.Identity?.Name);

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting banner");
            return Json(new { success = false, message = "Error deleting banner" });
        }
    }

    /// <summary>
    /// Update home page settings
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSettings(int carouselInterval, int bestSellersCount, int featuredProductsCount)
    {
        try
        {
            var settings = await _db.HomePageSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new HomePageSettings();
                _db.HomePageSettings.Add(settings);
            }

            settings.CarouselInterval = carouselInterval;
            settings.BestSellersCount = bestSellersCount;
            settings.FeaturedProductsCount = featuredProductsCount;
            settings.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings");
            return Json(new { success = false, message = "Error updating settings" });
        }
    }
}
