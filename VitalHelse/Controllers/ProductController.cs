using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Data;
using VitalHelse.Helpers;
using VitalHelse.Models;

namespace VitalHelse.Controllers;

public class ProductController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ProductController> _logger;
    private readonly UserManager<AspNetUsers> _um;

    public ProductController(ApplicationDbContext db, ILogger<ProductController> logger, UserManager<AspNetUsers> um)
    {
        _db = db;
        _logger = logger;
        _um = um;
    }


    // Viser et spesifikt produkt via ID (fallback / direkte lenke)
    public IActionResult Index(int? id)
    {
        if (id == null)
            return RedirectToAction("Index", "Home");

        var product = _db.Products
            .Include(p => p.ProductPictures)
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                    .ThenInclude(c => c.ParentCategory)
            .FirstOrDefault(p => p.ProductId == id);

        if (product == null)
            return NotFound();

        return View("ProductDetails", product);
    }

    // Hovedrute som håndterer både kategori- og produkt-URL-er
    [Route("produkt/{*fullPath}")]
    public async Task<IActionResult> ProductOrCategory(string? fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
            return NotFound();

        var parts = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var lastPart = parts.Last().ToLowerInvariant();

        // 🔹 Sjekk om siste del matcher et produkt (via slug)
        var product = _db.Products
            .Include(p => p.ProductPictures)
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                    .ThenInclude(c => c.ParentCategory)
            .AsEnumerable() // kreves for SlugHelper i minne
            .FirstOrDefault(p => SlugHelper.Slugify(p.ProductName) == lastPart);

        if (product != null)
            return View("ProductDetails", product);

        // 🔹 Ellers: behandle som kategori
        var currentCategory = _db.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.ChildCategories)
            .AsEnumerable()
            .FirstOrDefault(c =>
                SlugHelper.Slugify(GetCategoryFullPath(c)) == SlugHelper.Slugify(fullPath)
            );

        if (currentCategory == null)
            return NotFound();

        var allCategoryIds = GetAllCategoryIds(currentCategory);

        var products = _db.Products
            .Include(p => p.ProductPictures)
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
            .Where(p => p.ProductCategories.Any(pc => allCategoryIds.Contains(pc.CategoryId)))
            .ToList();

        // 🔹 Sett IsFavorite-feltet her
        if (User.Identity?.IsAuthenticated ?? false)
        {
            var userId = _um.GetUserId(User);

            var favoriteIds = await _db.FavoriteProducts
                .Where(f => f.AspNetUsersId == userId)
                .Select(f => f.ProductId)
                .ToListAsync();

            foreach (var p in products)
                p.IsFavorite = favoriteIds.Contains(p.ProductId);
        }


        var subcategories = currentCategory.ChildCategories.ToList();

        var viewModel = new CategoryViewModel
        {
            CurrentCategory = currentCategory,
            ParentCategoryName = currentCategory.ParentCategory?.CategoryName,
            SubCategories = subcategories,
            Products = products
        };

        return View("CategoryTemplate", viewModel);
    }

    // 📂 Lager full kategori-sti som "hudpleie/krem/ansiktskrem"
    private string GetCategoryFullPath(Category category)
    {
        var names = new List<string> { category.CategoryName };
        var parent = category.ParentCategory;
        while (parent != null)
        {
            names.Insert(0, parent.CategoryName);
            parent = parent.ParentCategory;
        }
        return string.Join('/', names);
    }

    // 🔁 Henter alle underkategorier rekursivt
    private List<int> GetAllCategoryIds(Category category)
    {
        var ids = new List<int> { category.CategoryId };

        var children = _db.Categories
            .Where(c => c.ParentCategoryId == category.CategoryId)
            .ToList();

        foreach (var child in children)
            ids.AddRange(GetAllCategoryIds(child));

        return ids;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    [HttpGet]
    public IActionResult ProductGrid(IEnumerable<Product> products)
    {
        _logger.LogInformation("Rendering partial product grid with {Count} products", products.Count());
        return PartialView("_ProductGrid", products);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public IActionResult AddProduct()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UploadPicture(IFormFile file, string? tempID)
    {
        if (file == null || file.Length == 0)
        {return BadRequest("No Image Uploaded");}
        
        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");

        var fileName = $"{Guid.NewGuid()}.{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadPath, fileName);
        
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        var tempPictures = HttpContext.Session.GetObjectFromJson<List<string>>(tempID ?? "temp_pictures") ?? new List<string>();
        tempPictures.Add($"/images/products/{fileName}");
        HttpContext.Session.SetObjectAsJson(tempID, tempPictures);

        return Json(new { success = true, path = $"/images/products/{fileName}" });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProduct(Product model, string tempID)
    {
        if (!ModelState.IsValid)
            return View(model);

        _db.Products.Add(model);
        await _db.SaveChangesAsync();

        //saves uploaded pictures
        var tempPictures = HttpContext.Session.GetObjectFromJson<List<string>>(tempID) ?? new List<string>();
        foreach (var path in tempPictures)
        {
            var picture = new ProductPicture
            {
                ProductId = model.ProductId,
                PicturePath = path
            };
            _db.ProductPictures.Add(picture);
        }

        await _db.SaveChangesAsync();

        //clears session
        HttpContext.Session.Remove(tempID);

        return RedirectToAction("Index", new { id = model.ProductId });
    }


}

