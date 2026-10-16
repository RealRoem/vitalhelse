using System.ComponentModel.DataAnnotations;

namespace VitalHelse.Models.Discount;

public class Campaign
{
    public int CampaignId { get; set; }

    // Name of the campaign (e.g., "Summer Sale")
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    // The categories included in this campaign.
    // Stored as a simple JSON list using EF Core's built-in support.
    public List<int> CategoryIds { get; set; } = new List<int>();

    // Campaign start and end date
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    // Whether the campaign is currently active
    public bool IsActive { get; set; } = false;
}