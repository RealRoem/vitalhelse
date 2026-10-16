using System.Collections.Generic;

namespace VitalHelse.Models
{
    public class CategoryViewModel
    {
        public Category CurrentCategory { get; set; }
        public string? ParentCategoryName { get; set; }
        public List<Category> SubCategories { get; set; } = new();
        public List<Product> Products { get; set; } = new();
    }
}