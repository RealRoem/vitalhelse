using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Data;
using VitalHelse.Models;
using VitalHelse.Models.Enums;
using VitalHelse.Models.Shipping;
using VitalHelse.Services;

namespace VitalHelse.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<AspNetUsers> _userManager;
    private readonly DiscountService _discountService;
    private readonly CartPricingService _pricing;
    private readonly OrderService _orders;

    public CheckoutController(
        ApplicationDbContext db,
        UserManager<AspNetUsers> userManager,
        DiscountService discountService,
        CartPricingService pricing,
        OrderService orders)
    {
        _db = db;
        _userManager = userManager;
        _discountService = discountService;
        _pricing = pricing;
        _orders = orders;
    }
    
    // Helper function to check if cart is empty
    private async Task<bool> CartIsEmpty()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return true;

        return !await _db.CartProducts.AnyAsync(c => c.AspNetUsersId == userId);
    }
    
    // Helper function to check if user has a defaultaddress
    private bool NoAddressSelected(AspNetUsers user)
    {
        return user.DefaultUserAddressId == null;
    }

    // Helper function to check user has a default shipping method
    private bool NoShippingSelected(AspNetUsers user)
    {
        return user.DefaultShippingMethodId == null;
    }

    // Function for updating summary data that is used by a javascript function to update the html
    [HttpGet]
    public async Task<IActionResult> Summary()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var items = await _pricing.GetCartAsync(user.Id);
        var t = _pricing.Calculate(items, user.DefaultShippingMethodId, items.Count == 0 ? null : _discountService.GetActive());

        return Json(new
        {
            sumBefore = t.SumBefore,
            campaignDiscount = t.CampaignDiscount,
            sumProducts = t.SumProducts,
            codeName = t.CodeName,
            codePercent = t.CodePercent,
            codeDiscount = t.CodeDiscount,
            shipping = t.Shipping,
            shippingChosen = t.ShippingMethod != null,
            total = t.Total,
            vat = t.VatAmount,
            totalExVat = t.TotalExVat,
            itemCount = items.Sum(i => i.Quantity)
        });
    }
    
    // Function to show all the items in the shopping cart
    [HttpGet]
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
    
    // Function to show all user addresses 
    [HttpGet]
    public async Task<IActionResult> Address()
    {
        ViewBag.Step = CheckoutStep.Address;
        
        if (await CartIsEmpty())
        {
            TempData["Error"] = "Handlekurven er tom.";
            return RedirectToAction("Index");
        }
        
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
    
    // Function to set the selected address as the default address used by javascript
    [HttpPost]
    public async Task<IActionResult> SetAddress(int addressId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Only the user's own addresses can be chosen
        if (!await _db.UserAddresses.AnyAsync(a => a.Id == addressId && a.AspNetUserId == user.Id))
            return NotFound();

        user.DefaultUserAddressId = addressId;
        await _userManager.UpdateAsync(user);

        return Ok();
    }

    // Function that sets the selected address and goes to next step
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectAddress(int? selectedAddressId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (selectedAddressId == null
            || !await _db.UserAddresses.AnyAsync(a => a.Id == selectedAddressId && a.AspNetUserId == user.Id))
        {
            TempData["Error"] = "Du må velge en adresse for å gå videre.";
            return RedirectToAction("Address");
        }

        user.DefaultUserAddressId = selectedAddressId.Value;
        await _userManager.UpdateAsync(user);

        return RedirectToAction("Shipping");
    }
    // Function for adding an address
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
            return View("Address", vm);
        }
    
        // Save the new values and add to database
        vm.NewAddress.AspNetUserId = userId!;
        vm.NewAddress.AspNetUsers =  await _userManager.GetUserAsync(User);
        _db.UserAddresses.Add(vm.NewAddress);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Address));
    }
    
    // Function for getting the edited address
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
    
    // Function for editing an address
    [HttpPost]
    public async Task<IActionResult> EditAddress(int id, UserAddressViewModel model)
    {
        // Fetch the user id
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (!ModelState.IsValid) return View("Address", model);

        var address = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == id && a.AspNetUserId == userId);

        if (address == null) return NotFound();

        // Update the address
        address.FirstName = model.NewAddress.FirstName;
        address.LastName = model.NewAddress.LastName;
        address.PhoneNumber = model.NewAddress.PhoneNumber;
        address.Street = model.NewAddress.Street;
        address.PostalCode = model.NewAddress.PostalCode;
        address.City = model.NewAddress.City;

        await _db.SaveChangesAsync();

        return RedirectToAction("Address");
    }
    
    // Function for deleting an address
    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var address = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == id && a.AspNetUserId == user.Id);
        if (address == null) return NotFound();

        _db.UserAddresses.Remove(address);
        await _db.SaveChangesAsync();

        // Do not leave the default address pointing at a row that no longer exists
        if (user.DefaultUserAddressId == id)
        {
            user.DefaultUserAddressId = null;
            await _userManager.UpdateAsync(user);
        }

        return Ok();
    }
    // Function for displaying shipping methods
    [HttpGet]
    public async Task<IActionResult> Shipping()
    {
        ViewBag.Step = CheckoutStep.Shipping;
        
        var user = await _userManager.GetUserAsync(User);
        var userId = user.Id;

        if (await CartIsEmpty())
        {
            TempData["Error"] = "Handlekurven er tom.";
            return RedirectToAction("Index");
        }

        if (NoAddressSelected(user))
        {
            TempData["Error"] = "Du må velge en adresse før du kan gå videre.";
            return RedirectToAction("Address");
        }

        // Get the cart
        var items = await _db.CartProducts
            .Include(i => i.Product)
            .Where(i => i.AspNetUsersId == userId)
            .ToListAsync();

        // Calculate cart total
        decimal cartTotal = items.Sum(i => (i.Product.ProductCampaignPrice ?? i.Product.ProductPriceInVAT) * i.Quantity);

        // Get thresholds
        var thresholds = _db.ShippingPriceThresholds
            .AsEnumerable()
            .OrderBy(t => t.MinOrderAmount)
            .ToList();

        // Calculate price
        decimal basePrice = thresholds
            .Where(t => cartTotal >= t.MinOrderAmount)
            .Select(t => t.ShippingPrice)
            .DefaultIfEmpty(0)
            .Last();

        // Create a viewmodel
        var vm = new ShippingViewModel
        {
            CartTotal = cartTotal,
            Thresholds = thresholds,
            Methods = _db.ShippingMethods.AsEnumerable().OrderByDescending(m => m.RateMultiplier).ToList(),
            SelectedMethodId = user.DefaultShippingMethodId,
            ShippingPrice = basePrice // før metode-rate
        };

        return View(vm);
    }

    // Function for setting a method as default shippingmethod used by javascript
    [HttpPost]
    public async Task<IActionResult> SetShippingMethod(int methodId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (!await _db.ShippingMethods.AnyAsync(m => m.Id == methodId))
            return NotFound();

        user.DefaultShippingMethodId = methodId;
        await _userManager.UpdateAsync(user);

        return Ok();
    }

    // Function that sets the selected shipping method and goes to next step
    [HttpPost]
    public async Task<IActionResult> SelectShippingMethod(ShippingViewModel vm)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (!vm.SelectedMethodId.HasValue || !await _db.ShippingMethods.AnyAsync(m => m.Id == vm.SelectedMethodId.Value))
        {
            TempData["Error"] = "Du må velge en leveringsmetode for å gå videre.";
            return RedirectToAction("Shipping");
        }

        user.DefaultShippingMethodId = vm.SelectedMethodId.Value;
        await _userManager.UpdateAsync(user);

        return RedirectToAction("ReviewOrder");
    }
    // Function for displaying the whole order
    [HttpGet]
    public async Task<IActionResult> ReviewOrder()
    {
        ViewBag.Step = CheckoutStep.ReviewOrder;

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (await CartIsEmpty())
        {
            TempData["Error"] = "Handlekurven er tom.";
            return RedirectToAction("Index");
        }

        if (NoAddressSelected(user))
        {
            TempData["Error"] = "Du må velge en adresse først.";
            return RedirectToAction("Address");
        }

        if (NoShippingSelected(user))
        {
            TempData["Error"] = "Du må velge en leveringsmetode.";
            return RedirectToAction("Shipping");
        }

        var cartItems = await _pricing.GetCartAsync(user.Id);

        var address = await _db.UserAddresses
            .FirstOrDefaultAsync(a => a.Id == user.DefaultUserAddressId && a.AspNetUserId == user.Id);
        if (address == null)
        {
            TempData["Error"] = "Du må velge en adresse først.";
            return RedirectToAction("Address");
        }

        var totals = _pricing.Calculate(cartItems, user.DefaultShippingMethodId, _discountService.GetActive());

        var vm = new ReviewOrderViewModel
        {
            CartProducts = cartItems,
            UserAddress = address,
            ShippingMethod = totals.ShippingMethod,
            ShippingPrice = totals.Shipping
        };

        return View(vm);
    }
    // Places the order. In demo mode (no Stripe keys) this is the final step of the checkout.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrder()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // The discount code is checked again here: it may have been deactivated or expired since it was entered
        var code = _discountService.GetActive();

        var result = await _orders.PlaceOrderAsync(user, code, "Betalt", null, enforceStock: true);
        if (result.Order == null)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(await CartIsEmpty() ? "Index" : "ReviewOrder");
        }

        _discountService.Clear();
        TempData["OrderId"] = result.Order.OrderId;
        return RedirectToAction("Complete");
    }

    public IActionResult Complete()
    {
        // Only reachable straight after placing an order
        if (TempData["OrderId"] == null)
            return RedirectToAction("Index", "Home");

        ViewBag.Step = CheckoutStep.Complete;
        TempData.Keep("OrderId");
        return View();
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
