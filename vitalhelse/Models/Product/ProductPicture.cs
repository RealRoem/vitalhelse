using System.ComponentModel.DataAnnotations;

namespace VitalHelse.Models;

public class ProductPicture
{
    public ProductPicture(){}
    
    public int ProductPictureId { get; set; }

    [StringLength(500)] public string PicturePath { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
}