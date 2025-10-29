namespace VitalHelse.Models;

public class AddProductViewModel
{
    public Product Product { get; set; } = new Product();
    public List<Category> AllCategories { get; set; } = new();
    public List<int> SelectedCategoryIds { get; set; } = new();
    public string TempID { get; set; } = Guid.NewGuid().ToString(); 
    
    public List<string> Tags { get; set; } = new List<string>();
}
