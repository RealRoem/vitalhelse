using VitalHelse.Models.Shipping;

namespace VitalHelse.Models;

public class ShippingViewModel
{
    public List<ShippingPriceThreshold> Thresholds { get; set; }
    public List<ShippingMethod> Methods { get; set; }
    
    public int? SelectedMethodId { get; set; }
    public decimal ShippingPrice { get; set; }
    public decimal CartTotal { get; set; }
}