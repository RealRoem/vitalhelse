using System.ComponentModel.DataAnnotations;

namespace VitalHelse.Models.Discount;

public class DiscountCode
{
    public int DiscountCodeId { get; set; }

    // The code the customer enters at checkout (e.g., "VITAL10")
    [Required]
    [StringLength(100)]
    public string Code { get; set; }

    // Percentage discount applied (1–100)
    [Range(1, 100)]
    public int DiscountPercent { get; set; }

    // When the code was created
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Null means the code never expires
    public DateTime? ExpiresAt { get; set; }

    // Determines whether the code is currently active
    public bool IsActive { get; set; } = true;

    // Tracks total number of times the code has been used
    public int TimesUsed { get; set; } = 0;
}