using VitalHelse.Models.Shipping;

namespace VitalHelse.Models;

public class ReviewOrderViewModel
{
    public List<CartProduct> CartProducts { get; set; } = new();
    public UserAddress UserAddress { get; set; } = new();
    public ShippingMethod? ShippingMethod { get; set; }
    public decimal ShippingPrice { get; set; }

}