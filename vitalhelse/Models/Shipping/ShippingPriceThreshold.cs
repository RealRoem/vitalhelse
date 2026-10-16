using System.ComponentModel.DataAnnotations;

namespace VitalHelse.Models.Shipping;

public class ShippingPriceThreshold
{
    public int Id { get; set; }
    
    // The amount the customer has to purchase for to get this price
    [Required]
    public decimal MinOrderAmount { get; set; }

    // The shipping price that applies if MinOrderAmount is met
    [Required]
    public decimal ShippingPrice { get; set; }
}