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
        // TODO: Integrate data from Stripe / Tripletex later
        ViewData["Title"] = "Dashboard";
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