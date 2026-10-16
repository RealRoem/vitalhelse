namespace VitalHelse.Models;

public class ProductDetailViewModel
{
    public Product CurrentProduct { get; set; }
   // public Category CurrentCategory { get; set; }
    public Category? ParentCategory { get; set; }
    public string? CategoryPath { get; set; }
    public bool IsFavorite { get; set; }

}