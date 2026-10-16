namespace VitalHelse.Controllers.Admin.Discount;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitalHelse.Models.Discount;
using Data;
using Services;


[Authorize(Roles = "Admin,Staff")]
[AutoValidateAntiforgeryToken]
public class CampaignController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly CampaignService _campaignService;

    public CampaignController(ApplicationDbContext context, CampaignService campaignService)
    {
        _context = context;
        _campaignService = campaignService;
    }

    /* ============================================
       INDEX
    ============================================ */
    [HttpGet("Admin/Kampanjer")]
    public IActionResult Index()
    {
        var campaigns = _context.Campaigns
            .OrderByDescending(c => c.Start)
            .ToList();

        var categories = _context.Categories
            .Where(c => c.ParentCategoryId == null)
            .ToList();

        ViewBag.AllCategories = categories;

        ViewBag.CategoryLookup = campaigns.ToDictionary(
            camp => camp.CampaignId,
            camp => _context.Categories
                .Where(cat => camp.CategoryIds.Contains(cat.CategoryId))
                .Select(cat => cat.CategoryName)
                .ToList()
        );

        return View("Campaign", campaigns);
    }


    /* ============================================
       CREATE CAMPAIGN
    ============================================ */
    [HttpPost("Admin/Campaign/Create")]
    public IActionResult Create(string Name, DateTime Start, DateTime End, List<int> CategoryIds)
    {
        if (string.IsNullOrWhiteSpace(Name) || End < Start || !(CategoryIds?.Any() ?? false))
        {
            TempData["Error"] = "Kampanjen trenger navn, minst én kategori og en sluttdato etter startdato.";
            return RedirectToAction("Index");
        }

        var camp = new Campaign
        {
            Name = Name.Trim(),
            Start = Start,
            End = End,
            CategoryIds = CategoryIds ?? new List<int>(),
            IsActive = true
        };

        _context.Campaigns.Add(camp);
        _context.SaveChanges();

        // Prices follow the campaign's dates, so a campaign that starts later does not discount anything yet
        _campaignService.RecalculateAll();

        return RedirectToAction("Index");
    }


    /* ============================================
       TOGGLE CAMPAIGN (ACTIVATE/DEACTIVATE)
    ============================================ */
    [HttpPost("Admin/Campaign/Toggle")]
    public IActionResult Toggle(int id)
    {
        var c = _context.Campaigns.Find(id);
        if (c == null) return NotFound();

        c.IsActive = !c.IsActive;
        _context.SaveChanges();

        _campaignService.RecalculateAll();

        return RedirectToAction("Index");
    }


    /* ============================================
       DELETE CAMPAIGN
    ============================================ */
    [HttpPost("Admin/Campaign/Delete")]
    public IActionResult Delete(int id)
    {
        var camp = _context.Campaigns.Find(id);
        if (camp == null) return NotFound();

        _context.Campaigns.Remove(camp);
        _context.SaveChanges();

        _campaignService.RecalculateAll();

        return RedirectToAction("Index");
    }
}
