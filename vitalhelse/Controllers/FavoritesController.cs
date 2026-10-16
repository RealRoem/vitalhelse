using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Data;
using VitalHelse.Models;

namespace VitalHelse.Controllers
{
    /// <summary>
    /// Handles all favorite-related actions such as adding, removing, and listing favorite products for a logged-in user.
    /// </summary>
    /// <remarks>
    /// This controller requires the user to be authenticated.
    /// It interacts with the <see cref="FavoriteProduct"/> entity and returns data to the "FavoritesPage" view.
    /// </remarks>
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AspNetUsers> _um;
        private readonly ILogger<FavoritesController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FavoritesController"/> class.
        /// </summary>
        /// <param name="db">The database context used for accessing favorite and product data.</param>
        /// <param name="um">The user manager used to retrieve the currently logged-in user.</param>
        /// <param name="logger">Logger instance for recording events and errors.</param>
        public FavoritesController(ApplicationDbContext db, UserManager<AspNetUsers> um,
            ILogger<FavoritesController> logger)
        {
            _db = db;
            _um = um;
            _logger = logger;
        }

        /// <summary>
        /// Displays all favorite products for the currently authenticated user.
        /// </summary>
        /// <returns>
        /// A view containing the list of favorite products, or a challenge if the user is not authenticated.
        /// </returns>
        [HttpGet("/favoritter")]
        public async Task<IActionResult> Favorites()
        {
            var user = await _um.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Unauthorized access attempt to Favorites page.");
                return Challenge();
            }

            var favoriteProducts = await _db.FavoriteProducts
                .AsNoTracking()
                .Include(fp => fp.Product)
                .ThenInclude(p => p.ProductPictures)
                .Where(fp => fp.AspNetUsersId == user.Id)
                .Select(fp => fp.Product)
                .ToListAsync();

            foreach (var p in favoriteProducts)
                p.IsFavorite = true;

            var viewModel = new FavoritesViewModel
            {
                Products = favoriteProducts
            };

            _logger.LogInformation("User {UserId} viewed their favorites list with {Count} items.", user.Id, favoriteProducts.Count);
            return View("FavoritesPage", viewModel);
        }

        /// <summary>
        /// Toggles the favorite status of a product for the current user.
        /// </summary>
        /// <param name="productId">The ID of the product to add or remove from favorites.</param>
        /// <returns>
        /// A JSON response indicating whether the operation succeeded and the product's new favorite status.
        /// Returns a 404 if the product does not exist.
        /// </returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(int productId)
        {
            var user = await _um.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var productExists = await _db.Products.AnyAsync(p => p.ProductId == productId);
            if (!productExists)
                return NotFound();

            var existing = await _db.FavoriteProducts
                .FirstOrDefaultAsync(f => f.AspNetUsersId == user.Id && f.ProductId == productId);

            bool isFavorite;

            if (existing == null)
            {
                _db.FavoriteProducts.Add(new FavoriteProduct
                {
                    AspNetUsersId = user.Id,
                    ProductId = productId
                });
                isFavorite = true;
                _logger.LogInformation("User {UserId} added product {ProductId} to favorites.", user.Id, productId);
            }
            else
            {
                _db.FavoriteProducts.Remove(existing);
                isFavorite = false;
                _logger.LogInformation("User {UserId} removed product {ProductId} from favorites.", user.Id, productId);
            }

            await _db.SaveChangesAsync();

            return Ok(new { success = true, isFavorite });
        }

        [HttpPost]
        public async Task<IActionResult> QuickAddFavorites()
        {
            var user = await _um.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Unauthorized access attempt to Favorites page.");
                return Challenge();
            }
            
            var products = await _db.FavoriteProducts
                .Include(fp => fp.Product)
                .Where(fp => fp.AspNetUsersId == user.Id)
                .Select(fp => fp.Product)
                .ToListAsync();

            foreach (var p in products)
            {
                // For every product, check if it exists in the shopping cart
                var shoppingCart = await _db.CartProducts
                    .FirstOrDefaultAsync(c => c.AspNetUsersId == user.Id && c.ProductId == p.ProductId);
                
                // If it exists in the shopping cart, increment quantity by one
                if (shoppingCart != null)
                {
                    shoppingCart.Quantity += 1;
                    continue;
                }
                
                CartProduct cartProduct = new CartProduct
                {
                    Quantity = 1,
                    AspNetUsersId = user.Id,
                    ProductId = p.ProductId
                };
                _db.Add(cartProduct);
            }
            await _db.SaveChangesAsync();

            return NoContent();
        }

    }
}
