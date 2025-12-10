using System.ComponentModel.DataAnnotations;

namespace VitalHelse.Models;

public class Order
{
    public Order(){}

    public int OrderId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    
    public DateTime? OrderDate { get; set; }
    [StringLength(50)] public string? Status { get; set; }
    public decimal TotalCost { get; set; }
    [StringLength(100)] public string? TrackingNumber { get; set; }
    [StringLength(50)] public string? ShippingProvider { get; set; }

    [StringLength(500)] public string? DiscountCodeId { get; set; }

    [Required] public AspNetUsers AspNetUsers { get; set; }
    [StringLength(450)] public string? AspNetUsersId { get; set; }
    
    public string ShippingFirstName { get; set; }
    public string ShippingLastName { get; set; }
    public string ShippingStreet { get; set; }
    public string ShippingPostalCode { get; set; }
    public string ShippingCity { get; set; }
    public string ShippingPhoneNumber { get; set; }

    public string ShippingMethodName { get; set; }
    public decimal ShippingMethodRateMultiplier { get; set; }
    public decimal ShippingPrice { get; set; }

    public ICollection<OrderProduct> OrderProducts { get; } = new List<OrderProduct>();
}