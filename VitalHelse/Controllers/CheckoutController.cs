using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Data;
using VitalHelse.Models;
using VitalHelse.Models.Enums;
using VitalHelse.Services;

namespace VitalHelse.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<AspNetUsers> _userManager;
    private readonly DiscountService _discountService;

    public CheckoutController(
        ApplicationDbContext db,
        UserManager<AspNetUsers> userManager,
        DiscountService discountService)
    {
        _db = db;
        _userManager = userManager;
        _discountService = discountService;
    }

    
    [HttpGet]
    public async Task<IActionResult> Summary()
    {
        // Fetch the user id
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var items = await _db.CartProducts
            .Include(cp => cp.Product)
            .Where(cp => cp.AspNetUsersId == userId)
            .ToListAsync();

        if (!items.Any())
        {
            return Json(new { sumProducts = 0, sumBefore = 0, discount = 0, shipping = 0, total = 0, codeDiscountPercent = 0 });
        }

        decimal sumBefore = items.Sum(i => (decimal)i.Product.ProductPriceInVAT * i.Quantity);
        decimal sumProducts = items.Sum(i =>
            (decimal)(i.Product.ProductCampaignPrice ?? i.Product.ProductPriceInVAT) * i.Quantity);

        decimal productDiscount = sumBefore - sumProducts;
        decimal shipping = 0;

        // 🔥 NYTT: hent rabattkode prosent
        int codePercent = HttpContext.Session.GetInt32("DiscountPercent") ?? 0;

        // 🔥 NYTT: regn ut rabattkode-beløp
        decimal codeDiscount = sumProducts * (codePercent / 100m);

        // 🔥 NYTT: totalsum med rabattkode
        decimal total = sumProducts - codeDiscount + shipping;

        // Return all values as JSON
        return Json(new
        {
            sumProducts,
            sumBefore,
            discount = productDiscount,   // kun produktkampanjer
            codeDiscountAmount = codeDiscount, // NYTT
            codeDiscountPercent = codePercent, // NYTT
            shipping,
            total
        });
    }

    
    public async Task<IActionResult> Index()
    {
        ViewBag.Step = CheckoutStep.Cart;
        
        // Finds the user id
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Query to get all items in the shopping cart
        var items = await _db.CartProducts
            .Include(cp => cp.Product)
            .Include(cp => cp.Product.ProductPictures)
            .Include(cp => cp.AspNetUsers)
            .Where(cp => cp.AspNetUsersId == userId)
            .ToListAsync();

        // Returns the items to the view
        return View(items);
    }
    
    [HttpGet]
    public async Task<IActionResult> Address()
    {
        // Fetch the user logged in
        var user = await _userManager.GetUserAsync(User);
        
        // Create a viewmodel and add user addresses
        var vm = new UserAddressViewModel
        {
            Existing = await _db.UserAddresses
                .Where(a => a.AspNetUserId == user.Id)
                .OrderByDescending(a => a.Id)
                .ToListAsync(),

            SelectedAddressId = user.DefaultUserAddressId 
        };
        
        return View(vm);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Address(UserAddressViewModel vm)
    {
        // Fetch the user
        var user = await _userManager.GetUserAsync(User);

        // If user selected an address
        if (vm.SelectedAddressId.HasValue)
        {
            user.DefaultUserAddressId = vm.SelectedAddressId.Value;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Shipping");
        }

        // If nothing none were selected
        vm.Existing = await _db.UserAddresses
            .Where(a => a.AspNetUserId == user.Id)
            .ToListAsync();
        
        return View("Address", vm);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddress(UserAddressViewModel vm)
    {
        // Fetch the user
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // If model is invalid (missing input)
        if (!ModelState.IsValid)
        {
            vm.Existing = await _db.UserAddresses.Where(a => a.AspNetUserId == userId).ToListAsync();
            ViewBag.Step = CheckoutStep.Address;
            return View("Address", vm);
        }
    
        // Save the new values and add to database
        vm.NewAddress.AspNetUserId = userId!;
        vm.NewAddress.AspNetUsers =  await _userManager.GetUserAsync(User);
        _db.UserAddresses.Add(vm.NewAddress);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Address));
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAddress(int id)
    {
        // Fetch the user id
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var address = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == id && a.AspNetUserId == userId);

        if (address == null) return NotFound();

        return Json(new
        {
            id = address.Id,
            firstName = address.FirstName,
            lastName = address.LastName,
            phoneNumber = address.PhoneNumber,
            street = address.Street,
            postalCode = address.PostalCode,
            city = address.City
        });
    }
    
    [HttpPost]
    public async Task<IActionResult> EditAddress(int id, UserAddressViewModel model)
    {
        // Fetch the user id
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (!ModelState.IsValid) return View("Address", model);

        var address = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == id && a.AspNetUserId == userId);

        if (address == null) return NotFound();

        address.FirstName = model.NewAddress.FirstName;
        address.LastName = model.NewAddress.LastName;
        address.PhoneNumber = model.NewAddress.PhoneNumber;
        address.Street = model.NewAddress.Street;
        address.PostalCode = model.NewAddress.PostalCode;
        address.City = model.NewAddress.City;

        await _db.SaveChangesAsync();

        return RedirectToAction("Address");
    }
    
    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        // Fetch the user id
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var address = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == id && a.AspNetUserId == userId);

        if (address == null) return NotFound();

        _db.UserAddresses.Remove(address);
        await _db.SaveChangesAsync();

        return Ok(); 
    }
    
    
    
    public async Task<IActionResult> Shipping(){
        ViewBag.Step = CheckoutStep.Shipping;
        
        return View();
    }
    
    public async Task<IActionResult> ReviewOrder(){
        
        var user = await _userManager.GetUserAsync(User);
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (userId == null)
        {
            return NoContent();
        }
        
        var order = await _db.Orders
            .Include(o => o.UserAddress)
            .Include(o => o.AspNetUsers )
            .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product.ProductPictures)
            .Where(o => o.AspNetUsersId == userId && o.Status == "Draft")
            .OrderByDescending(o => o.OrderDate)
            .FirstOrDefaultAsync();
        
        // Legge til where i querien

        if (order == null)
        {
            // HUSK: Legge til error
            return RedirectToAction("Index", "Checkout");
        }

        // Add data to the view model
        var vm = new ReviewOrderViewModel
        {
            UserAddress = order.UserAddress ?? new UserAddress(),
            CartProducts = order.OrderProducts.Select(op => new CartProduct
            {
                ProductId = op.ProductId,
                Quantity = op.Quantity,
                Product = op.Product
            }).ToList()
        };

        return View(vm);
    }
    
    public async Task<IActionResult> Complete(){
        _discountService.RegisterUsage();
        ViewBag.Step = CheckoutStep.Complete;
        return View();
    }
    
    // View for the orders page
    public async Task<IActionResult> Orderss()
    {
        var orders = await _db.Orders
            .Include(o => o.AspNetUsers)
            .Include(o => o.UserAddress)
            .Include(o => o.OrderProducts).ThenInclude(op => op.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
        return View(orders);
    }
    
    [HttpGet]
    public async Task<IActionResult> CartCount()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var count = await _db.CartProducts
            .Where(cp => cp.AspNetUsersId == userId)
            .SumAsync(cp => cp.Quantity);

        return Json(count);
    }

}

