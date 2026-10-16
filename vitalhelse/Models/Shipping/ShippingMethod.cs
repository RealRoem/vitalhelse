using System.ComponentModel.DataAnnotations;

namespace VitalHelse.Models.Shipping;

public class ShippingMethod
{
    public int Id { get; set; }

    [Required]
    public string MethodName { get; set; }

    // Example:
    // 0 = free 
    // 1.0 = normal price
    // 1.5 = 50% more expenive
    [Range(0, 2)]
    public decimal RateMultiplier { get; set; }
}