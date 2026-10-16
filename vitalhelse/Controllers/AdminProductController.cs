using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Data;
using VitalHelse.Helpers;
using VitalHelse.Models;

namespace VitalHelse.Controllers;

public class AdminProductController :Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ProductController> _logger;
    private readonly UserManager<AspNetUsers> _um;

    public AdminProductController(ApplicationDbContext db, ILogger<ProductController> logger, UserManager<AspNetUsers> um)
    {
        _db = db;
        _logger = logger;
        _um = um;
    }
    
    public IActionResult Details(int? id)
    {
        return RedirectToAction("Index", "Product", new { id = id });
    }
    
    // For Adding Products
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public IActionResult AddProduct()
    {
        var viewmodel = new ProductFormViewModel
        {
            AllCategories = _db.Categories.OrderBy(c => c.CategoryName).ToList(),
            AllPhysicalAttributes = _db.PhysicalAttributes.OrderBy(p => p.Attribute).ToList(),
            AllTags = _db.Tags.Select(t => t.Tags).ToList()
        };
        
        return View("~/Views/Admin/ProductForm.cshtml",viewmodel);
    }

    // Upload picture to server
    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UploadPicture(IFormFile file, string? tempID) 
    {
        if (file == null || file.Length == 0) 
            return Json(new { success = false, error = "No Image Uploaded" });

        // Validate file type
        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "image/avif" };
        if (!allowedMimeTypes.Contains(file.ContentType.ToLower()))
        { 
            return Json(new { success = false, error = "Unsupported file format" });
        }

        // Validate file size (10MB max)
        if (file.Length > 10 * 1024 * 1024)
        {
            return Json(new { success = false, error = "File is too large (max 10MB)" });
        }

        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
        
        // Ensure directory exists
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        // Get proper extension based on content type
        var extension = file.ContentType.ToLower() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            "image/avif" => ".avif",
            _ => Path.GetExtension(file.FileName)
        };

        // Use GUID for filename to avoid any special character issues
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadPath, fileName);

        try
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var tempPictures = HttpContext.Session.GetObjectFromJson<List<string>>(tempID ?? "temp_pictures") ?? new List<string>();
            tempPictures.Add($"/images/products/{fileName}");
            HttpContext.Session.SetObjectAsJson(tempID ?? "temp_pictures", tempPictures);

            return Json(new { success = true, path = $"/images/products/{fileName}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image");
            
            // Clean up file if it was partially written
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            
            return Json(new { success = false, error = "Failed to save image" });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteTempPicture(string imagePath, string? tempID)
    {
        if (string.IsNullOrEmpty(imagePath))
            return BadRequest("Invalid image path");

        var sessionKey = tempID ?? "temp_pictures";
        var tempPictures = HttpContext.Session.GetObjectFromJson<List<string>>(sessionKey) ?? new List<string>();

        // Remove the image path from the session list
        tempPictures.Remove(imagePath);
        HttpContext.Session.SetObjectAsJson(sessionKey, tempPictures);

        // Delete file from disk
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath.TrimStart('/'));
        if (System.IO.File.Exists(fullPath))
        {
            try
            {
                System.IO.File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file: {Path}", fullPath);
                return Json(new { success = false, error = "Failed to delete file from disk" });
            }
        }

        return Json(new { success = true });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProduct(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Repopulate dropdown lists if validation fails
            model.AllCategories = _db.Categories.OrderBy(c => c.CategoryName).ToList();
            model.AllPhysicalAttributes = _db.PhysicalAttributes.OrderBy(p => p.Attribute).ToList();
            model.AllTags = _db.Tags.Select(t => t.Tags).ToList();
            
            // Repopulate existing images from session
            model.ExistingImagePaths = HttpContext.Session.GetObjectFromJson<List<string>>(model.TempID) ?? new List<string>();
            
            return View("~/Views/Admin/ProductForm.cshtml", model);
        }

        Product product;

        // If ProductId exists, update instead of creating
        if (model.Product.ProductId > 0)
        {
            product = await _db.Products
                .Include(p => p.ProductCategories)
                .Include(p => p.ProductTags)
                .Include(p => p.PhysicalProductAttributes)
                .Include(p => p.ProductPictures)
                .FirstOrDefaultAsync(p => p.ProductId == model.Product.ProductId);

            if (product == null)
                return NotFound();

            // Update properties
            product.ProductName = model.Product.ProductName;
            product.ProductDescription = model.Product.ProductDescription;
            product.ProductInstructions = model.Product.ProductInstructions;
            product.ProductIngredients = model.Product.ProductIngredients;
            product.ProductPriceInVAT = model.Product.ProductPriceInVAT;
            product.ProductCampaignPrice = model.Product.ProductCampaignPrice;
            product.StockCount = model.Product.StockCount;

            // Clear existing relationships before re-adding
            _db.ProductCategories.RemoveRange(product.ProductCategories);
            _db.ProductTags.RemoveRange(product.ProductTags);
            _db.PhysicalProductAttributes.RemoveRange(product.PhysicalProductAttributes);
            _db.ProductPictures.RemoveRange(product.ProductPictures);
        }
        else
        {
            // Create new product
            product = model.Product;
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
        }

        // Save uploaded pictures from session
        var sessionImages = HttpContext.Session.GetObjectFromJson<List<string>>(model.TempID) ?? new List<string>();
        foreach (var path in sessionImages)
        {
            _db.ProductPictures.Add(new ProductPicture
            {
                ProductId = product.ProductId,
                PicturePath = path
            });
        }

        // Save selected categories
        if (model.SelectedCategoryIds != null && model.SelectedCategoryIds.Any())
        {
            foreach (var categoryId in model.SelectedCategoryIds)
            {
                _db.ProductCategories.Add(new ProductCategory
                {
                    ProductId = product.ProductId,
                    CategoryId = categoryId
                });
            }
        }

        // Save selected physical attributes (sizes)
        if (model.SelectedPhysicalAttributeIds != null && model.SelectedPhysicalAttributeIds.Any())
        {
            foreach (var attributeId in model.SelectedPhysicalAttributeIds)
            {
                _db.PhysicalProductAttributes.Add(new PhysicalProductAttribute
                {
                    ProductId = product.ProductId,
                    PhysicalAttributeId = attributeId
                });
            }
        }

        // Save tags
        if (model.Tags != null && model.Tags.Any())
        {
            foreach (var tagName in model.Tags)
            {
                var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Tags == tagName);
                if (tag == null)
                {
                    tag = new Tag { Tags = tagName };
                    _db.Tags.Add(tag);
                    await _db.SaveChangesAsync();
                }

                _db.ProductTags.Add(new ProductTag
                {
                    ProductId = product.ProductId,
                    TagId = tag.TagId
                });
            }
        }

        await _db.SaveChangesAsync();

        // Clear temp images from session
        HttpContext.Session.Remove(model.TempID);

        return RedirectToAction("Details", new { id = product.ProductId });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> EditProduct(int id)
    {
        var product = await _db.Products
            .Include(p => p.ProductCategories)
            .Include(p => p.ProductPictures)
            .Include(p => p.ProductTags)
            .ThenInclude(pt => pt.Tag)
            .Include(p => p.PhysicalProductAttributes)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
            return NotFound();

        var picturePaths = product.ProductPictures.Select(pp => pp.PicturePath).ToList();

        var viewModel = new ProductFormViewModel
        {
            Product = product,
            AllCategories = _db.Categories.OrderBy(c => c.CategoryName).ToList(),
            AllPhysicalAttributes = _db.PhysicalAttributes.OrderBy(p => p.Attribute).ToList(),
            AllTags = _db.Tags.Select(t => t.Tags).ToList(),
            SelectedCategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToList(),
            SelectedPhysicalAttributeIds = product.PhysicalProductAttributes.Select(ppa => ppa.PhysicalAttributeId).ToList(),
            Tags = product.ProductTags.Select(pt => pt.Tag.Tags).ToList(), // Selected tags for this product
            ExistingImagePaths = picturePaths,
            TempID = Guid.NewGuid().ToString()
        };

        // Store images in session for management
        HttpContext.Session.SetObjectAsJson(viewModel.TempID, picturePaths);
        
        ViewData["IsEditMode"] = true;
        return View("~/Views/Admin/ProductForm.cshtml", viewModel);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products
            .Include(p => p.ProductPictures)
            .Include(p => p.ProductCategories)
            .Include(p => p.ProductTags)
            .Include(p => p.PhysicalProductAttributes)
            .Include(p => p.FavoriteProducts)
            .Include(p => p.CartProducts)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
            return NotFound();

        try
        {
            // Delete product images from disk
            foreach (var picture in product.ProductPictures)
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", picture.PicturePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    try
                    {
                        System.IO.File.Delete(fullPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete image file: {Path}", fullPath);
                    }
                }
            }

            // Remove all related data
            _db.ProductPictures.RemoveRange(product.ProductPictures);
            _db.ProductCategories.RemoveRange(product.ProductCategories);
            _db.ProductTags.RemoveRange(product.ProductTags);
            _db.PhysicalProductAttributes.RemoveRange(product.PhysicalProductAttributes);
            _db.FavoriteProducts.RemoveRange(product.FavoriteProducts);
            _db.CartProducts.RemoveRange(product.CartProducts);
            
            // Remove the product itself
            _db.Products.Remove(product);
            
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Produkt slettet" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete product {ProductId}", id);
            return Json(new { success = false, error = "Kunne ikke slette produkt" });
        }
    }
    
}