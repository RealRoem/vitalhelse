using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VitalHelse.Models;

/// <summary>
/// Represents a product in the VitalHelse webshop, optionally synchronized with Tripletex.
/// </summary>
public class Product
{
    public Product() { }

    public int ProductId { get; set; }

    /// <summary>
    /// Tripletex product ID. Null if the product is local (not synced).
    /// </summary>
    public int? TripletexProductId { get; set; }

    // Only staff and admin have access to create, edit or delete from this table,
    // therefore leaving it relatively flexible.
    
    [Required]
    [StringLength(500)]
    public string ProductName { get; set; }

    /// <summary>
    /// Current stock count (Tripletex controlled if synced).
    /// </summary>
    public int? StockCount { get; set; }

    [StringLength(1000)]
    public string? ProductDescription { get; set; }

    [StringLength(1000)]
    public string? ProductIngredients { get; set; }
    
    [StringLength(1000)] 
    public string? ProductInstructions { get; set; }


    /// <summary>
    /// Price including VAT (Tripletex controlled).
    /// </summary>
    
    [Required]
    public decimal ProductPriceInVAT { get; set; }

    /// <summary>
    /// Price excluding VAT (Tripletex controlled).
    /// </summary>
    public decimal? ProductPriceExVAT { get; set; }

    /// <summary>
    /// Determines if the product is visible in the webshop. 
    /// Newly synced Tripletex products default to false.
    /// </summary>
    public bool ProductVisibility { get; set; } = false;

    /// <summary>
    /// Optional campaign price (manual override on the webshop).
    /// </summary>
    public decimal? ProductCampaignPrice { get; set; }

    [StringLength(30)]
    public string? LabelDescription { get; set; }

    public ICollection<ProductTag> ProductTags { get; } = new List<ProductTag>();
    public ICollection<ProductPicture> ProductPictures { get; } = new List<ProductPicture>();
    public ICollection<ProductCategory> ProductCategories { get; } = new List<ProductCategory>();
    public ICollection<PhysicalProductAttribute> PhysicalProductAttributes { get; } = new List<PhysicalProductAttribute>();

    public ICollection<FavoriteProduct> FavoriteProducts { get; } = new List<FavoriteProduct>();
    public ICollection<OrderProduct> OrderProducts { get; } = new List<OrderProduct>();
    public ICollection<CartProduct> CartProducts { get; } = new List<CartProduct>();

    [StringLength(100)]
    public string? StripeProductId { get; set; }  // f.eks. "prod_Qwe123ABC"

    [StringLength(100)]
    public string? StripePriceId { get; set; }
    
    [NotMapped]
    public bool IsFavorite { get; set; } = false;
}
