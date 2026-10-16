using System.ComponentModel.DataAnnotations;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;

namespace VitalHelse.Models;

[PrimaryKey(nameof(AspNetUsersId), nameof(ProductId))]
public class CartProduct
{
    public CartProduct(){}

    public Product Product { get; set; }
    public AspNetUsers AspNetUsers { get; set; }

    [StringLength(450)] public string AspNetUsersId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}