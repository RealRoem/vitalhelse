using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VitalHelse.Models;

[PrimaryKey(nameof(ProductId), nameof(OrderId))]
public class OrderProduct
{
    public OrderProduct(){}
    
    [Required] public Product Product { get; set; } = null!;
    [Required] public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public int OrderId { get; set; }
    [Required] public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
}