using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace VitalHelse.Models;

public class AspNetUsers : IdentityUser
{
    [StringLength(200)]
    [Display(Name = "Address")]
    public string? Address { get; set; }
    
    [MinLength(4), MaxLength(4)]
    [Display(Name = "Postal Code")]
    public string? PostalCode { get; set; }
    
    [Display(Name = "Poststed")]
    public string? PostalPlace { get; set; }
    
    [StringLength(100)]
    [Display(Name = "First name")]
    public string? FirstName { get; set; }
     
    [StringLength(100)] 
    [Display(Name = "Last name")]
    public string? LastName { get; set; }

    [MinLength(9), MaxLength(9)] public string? OrgNr { get; set; }

    [StringLength(100)] public string? OrgName { get; set; }
    
    public ICollection<ProductReview> ProductReviews { get; } = new List<ProductReview>();
    public ICollection<FavoriteProduct> FavoriteProducts { get; } = new List<FavoriteProduct>();
    public ICollection<Order> Orders { get; } = new List<Order>();
    public ICollection<CartProduct> ShoppingCarts { get; } = new List<CartProduct>();
    
    public ICollection<UserAddress> UserAddresses { get; } = new List<UserAddress>();
    public int? DefaultUserAddressId { get; set; }
    public int? DefaultShippingMethodId { get; set; }
}