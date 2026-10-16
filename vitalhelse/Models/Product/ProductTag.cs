using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VitalHelse.Models;

[PrimaryKey(nameof(ProductId), nameof(TagId))]
public class ProductTag
{
    public ProductTag(){}
    public int ProductId { get; set; }
    public int TagId { get; set; }

    public Product Product { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}