using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VitalHelse.Models;

[PrimaryKey(nameof(ProductId), nameof(AspNetUsersId))]

public class FavoriteProduct
{
    public FavoriteProduct() {}
    
    [Required] public Product Product { get; set; }
    [Required] public AspNetUsers AspNetUsers { get; set; }

    public int ProductId { get; set; }
    [StringLength(450)] public string AspNetUsersId { get; set; } = null!;
}