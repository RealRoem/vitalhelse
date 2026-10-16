using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using VitalHelse.Data;
using VitalHelse.Models;
using VitalHelse.Services;

namespace VitalHelse.Controllers;

// All of these actions demands a user, therefore, authorize the whole class
[Authorize]
public class ShoppingCartController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<AspNetUsers> _userManager;
    private readonly DiscountService _discountService;

    public ShoppingCartController(ApplicationDbContext db, UserManager<AspNetUsers> userManager, DiscountService discountService)
    {
        _db = db;
        _userManager = userManager;
        _discountService = discountService;
    }
    
    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        // Fetch the userId
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Finds the row in CartProducts where Product and userId match
        var shoppingCart = await _db.CartProducts
            .FirstOrDefaultAsync(m => m.ProductId == id && m.AspNetUsersId == userId);

        // Removes the product
        if (shoppingCart != null)
            _db.Remove(shoppingCart);

        // Save the changes
        await _db.SaveChangesAsync();
        
        return NoContent();
    }

    private const int MaxQuantity = 99;

    private string? CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    // Checks a wanted quantity against the allowed range and the stock. Returns an error message, or null when fine.
    private static string? QuantityProblem(Product product, int quantity)
    {
        if (quantity < 1)
            return "Antall må være minst 1.";
        if (quantity > MaxQuantity)
            return $"Du kan maks bestille {MaxQuantity} stk av samme vare.";
        if (product.StockCount.HasValue && quantity > Math.Max(0, product.StockCount.Value))
        {
            var left = Math.Max(0, product.StockCount.Value);
            return left == 0
                ? $"{product.ProductName} er dessverre utsolgt."
                : $"Vi har bare {left} stk av {product.ProductName} på lager.";
        }
        return null;
    }

    [HttpPatch]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuantity(int id)
    {
        var item = await _db.CartProducts.Include(c => c.Product)
            .FirstOrDefaultAsync(m => m.ProductId == id && m.AspNetUsersId == CurrentUserId);
        if (item == null) return NotFound("Varen finnes ikke i handlekurven.");

        var problem = QuantityProblem(item.Product, item.Quantity + 1);
        if (problem != null) return BadRequest(problem);

        item.Quantity += 1;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DecreaseQuantity(int id)
    {
        var item = await _db.CartProducts.FirstOrDefaultAsync(m => m.ProductId == id && m.AspNetUsersId == CurrentUserId);
        if (item == null) return NotFound("Varen finnes ikke i handlekurven.");

        if (item.Quantity > 1)
        {
            item.Quantity -= 1;
            await _db.SaveChangesAsync();
        }
        return NoContent();
    }

    [HttpPatch]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetQuantity(int id, int quantity)
    {
        var item = await _db.CartProducts.Include(c => c.Product)
            .FirstOrDefaultAsync(m => m.ProductId == id && m.AspNetUsersId == CurrentUserId);
        if (item == null) return NotFound("Varen finnes ikke i handlekurven.");

        var problem = QuantityProblem(item.Product, quantity);
        if (problem != null)
        {
            // Too many for the stock: fall back to what is available instead of failing
            var stock = item.Product.StockCount;
            if (quantity > 1 && stock.HasValue && stock.Value >= 1 && quantity > stock.Value && stock.Value <= MaxQuantity)
                quantity = stock.Value;
            else
                return BadRequest(problem);
        }

        item.Quantity = quantity;
        await _db.SaveChangesAsync();
        return Json(new { correctedQuantity = quantity });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int id, int quantity)
    {
        var userId = CurrentUserId;
        if (userId == null) return Unauthorized();

        var product = await _db.Products.FirstOrDefaultAsync(m => m.ProductId == id && (m.ProductVisibility || User.IsInRole("Admin") || User.IsInRole("Staff")));
        if (product == null) return NotFound("Produktet finnes ikke.");

        if (quantity < 1 || quantity > MaxQuantity)
            return BadRequest(QuantityProblem(product, quantity) ?? "Ugyldig antall.");

        var item = await _db.CartProducts.FirstOrDefaultAsync(m => m.ProductId == id && m.AspNetUsersId == userId);

        var problem = QuantityProblem(product, (item?.Quantity ?? 0) + quantity);
        if (problem != null) return BadRequest(problem);

        if (item != null)
            item.Quantity += quantity;
        else
            _db.CartProducts.Add(new CartProduct { Quantity = quantity, AspNetUsersId = userId, ProductId = id });

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
