using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;

namespace VitalHelse.Models;

public class ProductReview
{
    public ProductReview() {}
    
    public int ProductReviewId { get; set; }

    // Every user can leave a review, therefore a little more strict on their flexibility
    [MaxLength(300)]
    public string? Content { get; set; } = string.Empty;
    
    [Required]
    [Range(0, 5)]
    public int StarCount { get; set; }

    public AspNetUsers User { get; set; } = null!;
    public Product Product { get; set; } = null!;

    public int ProductId { get; set; }
    [StringLength(450)] public string UserId { get; set; } = null!;
}