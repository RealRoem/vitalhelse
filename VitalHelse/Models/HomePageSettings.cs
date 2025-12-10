using System.ComponentModel.DataAnnotations;

namespace VitalHelse.Models;

/// <summary>
/// Singleton settings for the home page configuration
/// </summary>
public class HomePageSettings
{
    public int HomePageSettingsId { get; set; }

    /// <summary>
    /// Auto-rotate interval in seconds (default 6)
    /// </summary>
    public int CarouselInterval { get; set; } = 6;

    /// <summary>
    /// Number of best-selling products to display
    /// Currently not implemented
    /// </summary>
    public int BestSellersCount { get; set; } = 4;

    /// <summary>
    /// Number of featured products to display
    ///  Currently not implemented
    /// </summary>
    public int FeaturedProductsCount { get; set; } = 4;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
