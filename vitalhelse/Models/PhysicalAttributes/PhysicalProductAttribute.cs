using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VitalHelse.Models;

[PrimaryKey(nameof(ProductId), nameof(PhysicalAttributeId))]
public class PhysicalProductAttribute
{
    public PhysicalProductAttribute() {}
    
    [Required] public Product Product { get; set; } = null!;
    [Required] public PhysicalAttribute PhysicalAttribute { get; set; } = null!;

    public int PhysicalAttributeId { get; set; }
    public int ProductId { get; set; }
}