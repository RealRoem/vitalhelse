using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Models;
using VitalHelse.Models.Shipping;

namespace VitalHelse.Data;

public class ApplicationDbContext : IdentityDbContext<AspNetUsers>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    #region Product entities
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductTag> ProductTags => Set<ProductTag>();
    public DbSet<ProductPicture> ProductPictures => Set<ProductPicture>();
    public DbSet<Tag> Tags => Set<Tag>();
    #endregion

    #region Commerce
    public DbSet<CartProduct> CartProducts => Set<CartProduct>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderProduct> OrderProducts => Set<OrderProduct>();
    #endregion
    
    #region Discount
    public DbSet<Models.Discount.DiscountCode> DiscountCodes { get; set; }
    public DbSet<Models.Discount.CategoryDiscount> CategoryDiscounts { get; set; }
    public DbSet<Models.Discount.Campaign> Campaigns { get; set; }
    #endregion
    
    #region Shipping
    public DbSet<ShippingPriceThreshold> ShippingPriceThresholds { get; set; }
    public DbSet<ShippingMethod> ShippingMethods { get; set; }
    #endregion

    #region User and favorites
    public DbSet<FavoriteProduct> FavoriteProducts => Set<FavoriteProduct>();
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    #endregion

    #region Misc
    public DbSet<PhysicalAttribute> PhysicalAttributes => Set<PhysicalAttribute>();
    public DbSet<PhysicalProductAttribute> PhysicalProductAttributes => Set<PhysicalProductAttribute>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    #endregion
    
    
    #region HomePage

    public DbSet<BannerImage> BannerImages => Set<BannerImage>();
    public DbSet<HomePageSettings> HomePageSettings => Set<HomePageSettings>();

    #endregion
}