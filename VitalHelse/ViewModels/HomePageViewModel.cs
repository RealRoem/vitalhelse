using VitalHelse.Models;

namespace VitalHelse.ViewModels;

/// <summary>
/// ViewModel for the home page containing banners and products
/// </summary>
public class HomePageViewModel
{
    public List<BannerImage> Banners { get; set; } = new();
    public List<Product> Products { get; set; } = new();
    public HomePageSettings Settings { get; set; } = new();
}
