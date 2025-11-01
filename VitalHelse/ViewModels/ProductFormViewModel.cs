namespace VitalHelse.Models;

public class ProductFormViewModel
{
    public Product Product { get; set; } = new Product();
    public List<Category> AllCategories { get; set; } = new();
    public List<int> SelectedCategoryIds { get; set; } = new();
    public string TempID { get; set; } = Guid.NewGuid().ToString(); 
    public List<string> Tags { get; set; } = new List<string>();
    
    public List<string> AllTags { get; set; } = new List<string>();
    public List<PhysicalAttribute> AllPhysicalAttributes { get; set; } = new();
    public List<int> SelectedPhysicalAttributeIds { get; set; } = new();
    
    public List<string> ExistingImagePaths { get; set; } = new List<string>();
    
}
