namespace VitalHelse.Models;

public class ReccomendationsViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int? StockCount { get; set; }
    public decimal ProductPriceInVAT { get; set; }
    public decimal? ProductCampaignPrice { get; set; }
    public string? FirstImagePath { get; set; }
    public List<string> CategoryNames { get; set; } = new List<string>();
    public string ProductUrl { get; set; }
}