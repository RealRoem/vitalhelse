using System.ComponentModel.DataAnnotations;

namespace VitalHelse.Models;

/// <summary>
/// Represents a banner image displayed in the home page carousel.
/// Admins can configure click actions to navigate to categories, products, or external URLs.
/// </summary>
public class BannerImage
{
    public int BannerImageId { get; set; }

    [Required]
    [StringLength(500)]
    public string ImagePath { get; set; } = string.Empty;

    [StringLength(200)]
    public string? AltText { get; set; }

    /// <summary>
    /// Display order for the carousel (lower numbers appear first)
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether this banner is currently active/visible
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Type of link action: None, Category, Product, ExternalUrl
    /// </summary>
    [Required]
    [StringLength(50)]
    public string LinkType { get; set; } = "None";

    /// <summary>
    /// The target of the link (CategoryId, ProductId, or URL depending on LinkType)
    /// </summary>
    [StringLength(500)]
    public string? LinkTarget { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}