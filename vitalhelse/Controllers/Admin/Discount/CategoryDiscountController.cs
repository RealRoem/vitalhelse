namespace VitalHelse.Controllers.Admin.Discount;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Data;
using Services;


[Authorize(Roles = "Admin,Staff")]
[AutoValidateAntiforgeryToken]
public class CategoryDiscountController : Controller
{
    private readonly ApplicationDbContext _context;

    private readonly CampaignService _campaignService;

    public CategoryDiscountController(ApplicationDbContext context, CampaignService campaignService)
    {
        _context = context;
        _campaignService = campaignService;
    }


    public class CategoryDiscountViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int DiscountPercent { get; set; }
    }

    [HttpGet("Admin/Rabattsatser")]
    public IActionResult Index()
    {
        var topCategories = _context.Categories
            .Where(c => c.ParentCategoryId == null)
            .ToList();

        var dbDiscounts = _context.CategoryDiscounts.ToList();

        var model = topCategories.Select(c => new CategoryDiscountViewModel
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            DiscountPercent = dbDiscounts.FirstOrDefault(d => d.CategoryId == c.CategoryId)?.DiscountPercent ?? 0
        }).ToList();

        return View("CategoryDiscount", model);
    }
    
    [HttpPost("Admin/CategoryDiscounts/SaveAll")]
    public IActionResult SaveAll(List<CategoryDiscountViewModel> model)
    {
        foreach (var item in model ?? new List<CategoryDiscountViewModel>())
        {
            if (item.DiscountPercent < 0 || item.DiscountPercent > 100) continue;
            var existing = _context.CategoryDiscounts
                .FirstOrDefault(cd => cd.CategoryId == item.CategoryId);

            if (existing == null)
            {
                _context.CategoryDiscounts.Add(new Models.Discount.CategoryDiscount
                {
                    CategoryId = item.CategoryId,
                    DiscountPercent = item.DiscountPercent
                });
            }
            else
            {
                existing.DiscountPercent = item.DiscountPercent;
            }
        }

        _context.SaveChanges();

        // New rates change the sale prices of every running campaign
        _campaignService.RecalculateAll();

        return RedirectToAction("Index");
    }


}