using System.ComponentModel.DataAnnotations;

namespace VitalHelse.Models;

public class PhysicalAttribute
{
    public PhysicalAttribute() {}
    
    public int PhysicalAttributeId { get; set; }
    [Required][StringLength(200)] public string Attribute { get; set; } = null!;

    public ICollection<PhysicalProductAttribute> PhysicalProductAttributes { get; } =
        new List<PhysicalProductAttribute>();

}