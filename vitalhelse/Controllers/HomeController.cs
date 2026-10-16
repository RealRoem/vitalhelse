using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Data;
using VitalHelse.Models;
using VitalHelse.ViewModels;

namespace VitalHelse.Controllers
{
    /// <summary>
    /// Handles requests related to the main pages of the application such as
    /// the home page, privacy policy, and global error handling.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;        
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for recording diagnostic and error information.</param>
        public HomeController(ILogger<HomeController> logger,ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        /// <summary>
        /// Displays the main landing page of the website with carousel banners.
        /// </summary>
        /// <returns>The home page view.</returns>
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Home page accessed.");
            
            var banners = await _db.BannerImages
                .Where(b => b.IsActive)
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();

            var products = await _db.Products
                .Include(p => p.ProductPictures)
                .ToListAsync();

          
            var settings = await _db.HomePageSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new HomePageSettings();
            }

            var viewModel = new HomePageViewModel
            {
                Banners = banners,
                Products = products,
                Settings = settings
            };

            return View(viewModel);
        }

        /// <summary>
        /// Displays the privacy policy page.
        /// </summary>
        /// <returns>The privacy policy view.</returns>
        public IActionResult Privacy()
        {
            _logger.LogInformation("Privacy page accessed.");
            return View();
        }

        /// <summary>
        /// Displays a detailed error page when an unhandled exception occurs.
        /// </summary>
        /// <remarks>
        /// The response is not cached and includes diagnostic information such as the request ID.
        /// </remarks>
        /// <returns>An error view with diagnostic details.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("Unhandled error occurred. Request ID: {RequestId}", requestId);

            return View(new ErrorViewModel { RequestId = requestId });
        }
    }
}
