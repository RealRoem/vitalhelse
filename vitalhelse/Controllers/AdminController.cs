using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Data;

namespace VitalHelse.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminController(ApplicationDbContext db)
    {
        _db = db;
    }

    // ============================================
    // DASHBOARD (main admin overview)
    // ============================================
    public IActionResult Dashboard()
    {
        ViewData["Title"] = "Dashboard";

        var since = DateTime.Now.AddDays(-30);
        var recentOrders = _db.Orders.Where(o => o.OrderDate >= since).ToList();
        ViewBag.OrdersLast30 = recentOrders.Count;
        ViewBag.RevenueLast30 = recentOrders.Sum(o => o.TotalCost);
        ViewBag.ProductCount = _db.Products.Count();
        ViewBag.LowStockCount = _db.Products.Count(p => p.StockCount != null && p.StockCount <= 5);
        ViewBag.ActiveCampaigns = _db.Campaigns.Count(c => c.IsActive);
        return View();
    }

    // ============================================
    // PRODUCTS (list, edit, delete)
    // ============================================
    public IActionResult Products()
    {
        var products = _db.Products
            .Include(p => p.ProductPictures)
            .AsNoTracking()
            .ToList();

        return View(products);
    }


    // ============================================
    // CATEGORIES (manage product categories)
    // ============================================
    public IActionResult Categories()
    {
        ViewData["Title"] = "Categories";
        return View();
    }

    // ============================================
    // BANNERS (homepage promotional banners)
    // ============================================
    public IActionResult Banners()
    {
        ViewData["Title"] = "Banners";
        return View();
    }
    
    // ============================================
    // DISCOUNTS
    // ============================================
// ============================================
// DISCOUNTS (redirect to AdminDiscountController)
// ============================================
    public IActionResult Discounts()
    {
        ViewData["Title"] = "Kampanjer";
        return RedirectToAction("Index", "DiscountCode");
    }

    
    // ============================================
    // EMPLOYEES (manage employees)
    // ============================================
    public IActionResult Employees()
    {
        ViewData["Title"] = "Employees";
        return View();
    }
}