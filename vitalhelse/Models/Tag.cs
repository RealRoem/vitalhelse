using System.ComponentModel.DataAnnotations;

namespace VitalHelse.Models;

public class Tag
{
    public Tag(){}
    
    public int TagId { get; set; }
    [Required][StringLength(50)] public string Tags { get; set; }

    public ICollection<ProductTag> ProductTags { get; } = new List<ProductTag>();
}