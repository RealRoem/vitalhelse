using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VitalHelse.Models;

[PrimaryKey(nameof(ProductId), nameof(CategoryId))]
public class ProductCategory
{
    public ProductCategory(){}

    public int CategoryId { get; set; }
    public int ProductId { get; set; }

    public Category Category { get; set; }
    public Product Product { get; set; }
}