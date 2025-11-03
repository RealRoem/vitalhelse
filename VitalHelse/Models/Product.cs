using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VitalHelse.Models;


public class   Product
{
    public Product(){}
    
    public int ProductId { get; set; }

    // Only staff and admin have access to create, edit or delete from this table,
    // therefore leaving it relatively flexible
    
    [Required]
    [StringLength(500)] public string ProductName { get; set; }
    public int? StockCount { get; set; }
    [StringLength(1000)] public string? ProductDescription { get; set; }
    [StringLength(1000)] public string? ProductIngredients { get; set; }
    
    [StringLength(1000)] public string? ProductInstructions { get; set; }
    
    [Required]
    public double ProductPrice { get; set; }
    public double? ProductCampaignPrice { get; set; }
    [StringLength(30)] public string? LabelDescription { get; set; }

    public ICollection<ProductTags> ProductTags { get; } = new List<ProductTags>();
    public ICollection<ProductPicture> ProductPictures { get; } = new List<ProductPicture>();
    public ICollection<ProductCategory> ProductCategories { get; } = new List<ProductCategory>();
    public ICollection<PhysicalProductAttribute> PhysicalProductAttributes { get; } =
        new List<PhysicalProductAttribute>();

    public ICollection<FavoriteProduct> FavoriteProducts { get; } = new List<FavoriteProduct>();
    public ICollection<OrderProduct> OrderProducts { get; } = new List<OrderProduct>();
    public ICollection<CartProduct> CartProducts { get; } = new List<CartProduct>();
    
    [NotMapped]
    public bool IsFavorite { get; set; } = false;
}