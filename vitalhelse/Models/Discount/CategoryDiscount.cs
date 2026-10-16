using System.ComponentModel.DataAnnotations;
using VitalHelse.Models;

namespace VitalHelse.Models.Discount;

public class CategoryDiscount
{
    public int CategoryDiscountId { get; set; }

    // The category this discount applies to (only top-level categories)
    public int CategoryId { get; set; }
    public Category Category { get; set; }

    // Percentage discount for the category (0–100)
    [Range(0, 100)]
    public int DiscountPercent { get; set; }
}