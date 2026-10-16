using System.ComponentModel.DataAnnotations;

namespace VitalHelse.Models;

public class Category
{
    public Category() {}

    public int CategoryId { get; set; }

    [StringLength(400)]
    [Required] 
    public string CategoryName { get; set; }

    // Nytt: Hierarki
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> ChildCategories { get; } = new List<Category>();

    public ICollection<ProductCategory> ProductCategories { get; } = new List<ProductCategory>();
}
