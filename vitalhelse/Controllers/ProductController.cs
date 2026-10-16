using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Data;
using VitalHelse.Helpers;
using VitalHelse.Models;

namespace VitalHelse.Controllers;

    /// <summary>
    /// Handles product- and category-related requests, including SEO-friendly URLs,
    /// category navigation, and product detail rendering.
    /// </summary>
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ProductController> _logger;
        private readonly UserManager<AspNetUsers> _um;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductController"/> class.
        /// </summary>
        /// <param name="db">The database context used for querying products and categories.</param>
        /// <param name="logger">Logger instance for tracking product and category requests.</param>
        /// <param name="um">UserManager for retrieving the current user and favorite data.</param>
        public ProductController(ApplicationDbContext db, ILogger<ProductController> logger,
            UserManager<AspNetUsers> um)
        {
            _db = db;
            _logger = logger;
            _um = um;
        }

        /// <summary>
        /// Builds a view model for a specific product, including its parent category and category path.
        /// </summary>
        /// <param name="product">The product to build the view model for.</param>
        /// <returns>A populated <see cref="ProductDetailViewModel"/>.</returns>
        private ProductDetailViewModel BuildProductViewModel(Product product)
        {
            var category = product.ProductCategories.Select(pc => pc.Category).FirstOrDefault();
            var fullCategoryPath = category != null ? GetCategoryFullPathFromDb(category.CategoryId) : string.Empty;

            return new ProductDetailViewModel
            {
                CurrentProduct = product,
                ParentCategory = category?.ParentCategory,
                CategoryPath = fullCategoryPath,
                IsFavorite = product.IsFavorite,
            };
        }

        /// <summary>
        /// Displays a product by its ID (used for direct links or fallback scenarios).
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <returns>The product details view, or a redirect if no ID is provided.</returns>
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

            // If the user is logged in, check the database to see if the product is in their favorites and store the result so the page shows the correct icon.
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userId = _um.GetUserId(User);
                var isFavorite = _db.FavoriteProducts
                    .Any(f => f.ProductId == product.ProductId && f.AspNetUsersId == userId);

                product.IsFavorite = isFavorite;
            }

            _logger.LogInformation("Product {ProductId} viewed via direct ID link.", id);
            return View("ProductDetails", BuildProductViewModel(product));
        }

        /// <summary>
        /// Handles SEO-friendly routes for both product and category pages.
        /// </summary>
        /// <param name="fullPath">The URL path representing either a product slug or category path.</param>
        /// <returns>
        /// The appropriate view: either product details or category listing, depending on the matched route.
        /// </returns>
        [Route("produkt/{*fullPath}")]
        public async Task<IActionResult> ProductOrCategory(string? fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                return NotFound();

            var parts = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var lastPart = parts.Last().ToLowerInvariant();

            // 🔹 Check if path refers to a product
            var product = _db.Products
                .Include(p => p.ProductPictures)
                .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .ThenInclude(c => c.ParentCategory)
                .AsEnumerable() // Slug comparison performed in-memory
                .FirstOrDefault(p => SlugHelper.ToUrlFriendly(p.ProductName) == lastPart);

            if (product != null)
            {
                // If the user is logged in, check the database to see if the product is in their favorites and store the result so the page shows the correct icon.
                if (User.Identity.IsAuthenticated)
                {
                    var userId = _um.GetUserId(User);
                    var isFavorite = _db.FavoriteProducts
                        .Any(f => f.ProductId == product.ProductId && f.AspNetUsersId == userId);
                    product.IsFavorite = isFavorite;
                }

                _logger.LogInformation("Product '{ProductName}' accessed via SEO route '{Path}'.", product.ProductName,
                    fullPath);
                return View("ProductDetails", BuildProductViewModel(product));
            }

            // 🔹 Otherwise treat it as a category
            var currentCategory = _db.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories)
                .AsEnumerable()
                .FirstOrDefault(c =>
                    SlugHelper.ToUrlFriendly(GetCategoryFullPath(c)) == SlugHelper.ToUrlFriendly(fullPath)
                );

            if (currentCategory == null)
                return NotFound();

            var allCategoryIds = GetAllCategoryIds(currentCategory);

            var products = _db.Products
                .Include(p => p.ProductPictures)
                .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
                .Where(p => p.ProductCategories.Any(pc => allCategoryIds.Contains(pc.CategoryId)))
                .ToList();

            // Mark favorites for authenticated users
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

            var viewModel = new CategoryViewModel
            {
                CurrentCategory = currentCategory,
                ParentCategoryName = currentCategory.ParentCategory?.CategoryName,
                SubCategories = currentCategory.ChildCategories.ToList(),
                Products = products
            };

            _logger.LogInformation("Category '{CategoryName}' accessed with {Count} products.",
                currentCategory.CategoryName, products.Count);
            return View("CategoryTemplate", viewModel);
        }

        /// <summary>
        /// Builds the full hierarchical path of a category such as "skincare/cream/face-cream".
        /// </summary>
        /// <param name="category">The category whose path should be built.</param>
        /// <returns>A slash-separated string representing the category path.</returns>
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

        /// <summary>
        /// Retrieves the full category path from the database using the category ID.
        /// </summary>
        /// <param name="categoryId">The category ID to resolve.</param>
        /// <returns>The full category path string.</returns>
        private string GetCategoryFullPathFromDb(int categoryId)
        {
            var category = _db.Categories.Find(categoryId);
            if (category == null) return string.Empty;

            var names = new List<string> { category.CategoryName };
            while (category.ParentCategoryId != null)
            {
                category = _db.Categories.Find(category.ParentCategoryId.Value);
                if (category == null) break;
                names.Insert(0, category.CategoryName);
            }

            return string.Join('/', names);
        }

        /// <summary>
        /// Recursively retrieves all category IDs within a hierarchy, including child categories.
        /// </summary>
        /// <param name="category">The root category.</param>
        /// <returns>A list of category IDs including the root and all descendants.</returns>
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

        /// <summary>
        /// Displays the shared product grid partial view for use in category, favorites, and search pages.
        /// </summary>
        /// <param name="products">A collection of products to render in the grid.</param>
        /// <returns>A partial view containing the product grid markup.</returns>
        [HttpGet]
        public IActionResult ProductGrid(IEnumerable<Product> products)
        {
            _logger.LogInformation("Rendering partial product grid with {Count} products.", products.Count());
            return PartialView("_ProductGrid", products);
        }

        /// <summary>
        /// Displays an error page when an exception occurs.
        /// </summary>
        /// <returns>The error view with diagnostic information.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("Unhandled error occurred. Request ID: {RequestId}", requestId);
            return View(new ErrorViewModel { RequestId = requestId });
        }
    }

