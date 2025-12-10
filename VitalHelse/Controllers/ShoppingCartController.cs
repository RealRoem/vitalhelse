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

    [HttpPatch]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuantity(int id)
    {
        // Fetch the user id
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Finds the row in CartProducts where Product and userId match 
        var shoppingCart = await _db.CartProducts
            .Include(c => c.Product)
            .FirstOrDefaultAsync(m => m.ProductId == id && m.AspNetUsersId == userId);

        if (shoppingCart != null)
        {
            /*
            if (shoppingCart.Quantity >= shoppingCart.Product.StockCount)
            {
                // Returns a 400 bad request if the quantity equals the amount of stock
                return BadRequest("Vi har desverre ikke dette antaller tilgjengelig på lager");
            }*/

            // Adds quantity by 1 and save it
            shoppingCart.Quantity += 1;
            await _db.SaveChangesAsync();
        }
        return NoContent();
    }

    [HttpPatch]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DecreaseQuantity(int id)
    {
        // Fetch the user id
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Finds the row in CartProducts where Product and userId match
        var shoppingCart = await _db.CartProducts.FirstOrDefaultAsync(m => m.ProductId == id && m.AspNetUsersId == userId);

        // If quantity is 1 or less
        if (shoppingCart.Quantity <= 1) {
            return NoContent();  
        }

        // Decrease the quantity by one
        shoppingCart.Quantity -= 1;
        await _db.SaveChangesAsync();
        
        return NoContent();
    }
    
    [HttpPatch]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetQuantity(int id, int quantity)
    {
        // Fetch the user id
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Finds the row in CartProducts where Product and userId match
        var shoppingCart = await _db.CartProducts
            .Include(c => c.Product)
            .FirstOrDefaultAsync(m => m.ProductId == id && m.AspNetUsersId == userId);

        // If cart item doesnt exist
        if (shoppingCart == null)
            return NotFound();
        
        // If quantity is less than zero or zero
        if (quantity <= 0)
            return BadRequest("Antall må være minst 1.");
        
        // If stock is NULL (not set) then use 0
        var stock = shoppingCart.Product.StockCount ?? 0;
        
        // If quantity is more than stock then set quantity to stock 
        /*if (quantity > stock)
            quantity = stock;*/

        // If quantity is more than stock then set quantity to stock 
        /*
        if (quantity > shoppingCart.Product.StockCount.GetValueOrDefault())
            quantity = shoppingCart.Product.StockCount.GetValueOrDefault();
        */
        
        // Update the quantity 
        shoppingCart.Quantity = quantity;
        await _db.SaveChangesAsync();
        
        // Return a JSON object with the adjusted quantity
        return Json(new { correctedQuantity = quantity }); 
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    //[Authorize]
    public async Task<IActionResult> AddToCart(int id, int quantity)
    {
        // Fetch the user id
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Search for a matching row in the shoppingcart
        var shoppingCart = await _db.CartProducts
            .FirstOrDefaultAsync(m => m.ProductId == id && m.AspNetUsersId == userId);

        // Gets the productId, so that we can match the cart quantity with stock count
        var product = await _db.Products
            .FirstOrDefaultAsync(m => m.ProductId == id);

        if (product == null) return NoContent();

        // The stock count has to be higher than 0 to add the item to cart.
        if (product.StockCount <= 0) return NoContent();
        
        // If the item already exists in the shoppingcart increment the quantity.
        if (shoppingCart != null)
        { 
            var newQuantity = shoppingCart.Quantity + quantity;
            if (newQuantity >= product.StockCount)
            {
                return BadRequest("Vi har desverre ikke dette antallet tilgjengelig på lager");
            } 
            
            /*
            // If the quantity tries to go higher than the stock count
            if (shoppingCart.Quantity >= product.StockCount)
            {
                // Returns a 400 bad request if the quantity is too low to use this function
                return BadRequest("Vi har desverre ikke dette antallet tilgjengelig på lager");
            } */
            
            shoppingCart.Quantity = newQuantity;

           // shoppingCart.Quantity += 1; 
        }

        // If not. Create a new row and add the item
        else
        {
            if (quantity > product.StockCount)
                return BadRequest("Vi har desverre ikke dette antallet tilgjengelig på lager");
            
            // Creates a new row
            CartProduct cartProduct = new CartProduct
            {
                Quantity = quantity,
                //Quantity = 1,
                AspNetUsersId = userId,
                ProductId = id
            };

            _db.Add(cartProduct);
        }
        await _db.SaveChangesAsync();
        
        // Dont change the view
        return NoContent();
    }
}