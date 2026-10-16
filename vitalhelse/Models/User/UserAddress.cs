using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace VitalHelse.Models;

public class UserAddress
{
    public UserAddress() {}
    
    public int Id { get; set; }
    public AspNetUsers? AspNetUsers { get; set; }
    
    // Gjorde den om slik at den kan være NULL foreløpig siden det funket ikke ellers
    [StringLength(450)] public string? AspNetUserId { get; set; } = null!;
    
    [Required]
    [StringLength(100)]
    [Display(Name = "Fornavn")]
    public string? FirstName { get; set; }
     
    [Required]
    [StringLength(100)] 
    [Display(Name = "Etternavn")]
    public string? LastName { get; set; }
    
    [Required]
    [StringLength(9)]
    [Display(Name = "Telefonnummer")]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required] 
    [StringLength(100)]
    [Display(Name = "Addresse")]
    public string Street { get; set; } = string.Empty;
    
    [Required] 
    [StringLength(100)]
    [Display(Name = "Sted")]
    public string City { get; set; } = string.Empty;
    
    [Required] 
    [StringLength(4, MinimumLength = 4)]
    [Display(Name = "Postnummer")]
    public string PostalCode { get; set; } = string.Empty;
}