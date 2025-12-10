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

    public CheckoutController(
        ApplicationDbContext db,
        UserManager<AspNetUsers> userManager,
        DiscountService discountService)
    {
        _db = db;
        _userManager = userManager;
        _discountService = discountService;
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
        // Fetch the user
        var user = await _userManager.GetUserAsync(User);
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        // Get all the items in the cart
        var items = await _db.CartProducts
            .Include(cp => cp.Product)
            .Where(cp => cp.AspNetUsersId == userId)
            .ToListAsync();

        // If cart is empty
        if (!items.Any())
        {
            return Json(new { sumProducts = 0, sumBefore = 0, discount = 0, shipping = 0, total = 0, codeDiscountPercent = 0 });
        }

        // Calculate totals
        decimal sumBefore = items.Sum(i => i.Product.ProductPriceInVAT * i.Quantity);
        decimal sumProducts = items.Sum(i => (i.Product.ProductCampaignPrice ?? i.Product.ProductPriceInVAT) * i.Quantity);
        decimal productDiscount = sumBefore - sumProducts;
        decimal shipping = 0;
        decimal cartTotal = sumProducts;

        // Get the thresholds
        var thresholds = _db.ShippingPriceThresholds
            .AsEnumerable()
            .OrderBy(t => t.MinOrderAmount)
            .ToList();
        
        // Get correct shipping price based on cart total
        decimal basePrice = thresholds
            .Where(t => cartTotal >= t.MinOrderAmount)
            .Select(t => t.ShippingPrice)
            .DefaultIfEmpty(0)
            .Last();

        // Calculate the shipping price based on thresholds and method
        if (user.DefaultShippingMethodId != null) {
            var method = await _db.ShippingMethods
                .FirstOrDefaultAsync(m => m.Id == user.DefaultShippingMethodId);

            if (method != null) {
                shipping = basePrice * method.RateMultiplier;
            }
        }
        
        int codePercent = HttpContext.Session.GetInt32("DiscountPercent") ?? 0;
        decimal codeDiscount = sumProducts * (codePercent / 100m);
        decimal total = sumProducts - codeDiscount + shipping;
        
        // Return JSON that will be used by javascript to display all the prices
        return Json(new
        {
            sumProducts,
            sumBefore,
            discount = productDiscount,
            codeDiscountAmount = codeDiscount, 
            codeDiscountPercent = codePercent,
            shipping,
            total
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

        // Store the chosen address as default address
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

        // If no address is chosen return error
        if (selectedAddressId == null)
        {
            TempData["Error"] = "Du må velge en adresse for å gå videre.";
            return RedirectToAction("Address");
        }

        // Store the chosen address as default address
        user.DefaultUserAddressId = selectedAddressId.Value;
        await _userManager.UpdateAsync(user);

        // Go to next step
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
        // Fetch the user id
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var address = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == id && a.AspNetUserId == userId);

        if (address == null) return NotFound();

        _db.UserAddresses.Remove(address);
        await _db.SaveChangesAsync();

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
            Methods = await _db.ShippingMethods.ToListAsync(),
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

        user.DefaultShippingMethodId = methodId;
        await _userManager.UpdateAsync(user);

        return Ok();
    }
    
    // Function that sets the selected shipping method and goes to next step
    [HttpPost]
    public async Task<IActionResult> SelectShippingMethod(ShippingViewModel vm)
    {
        var user = await _userManager.GetUserAsync(User);

        if (!vm.SelectedMethodId.HasValue)
            return RedirectToAction("Shipping");

        user.DefaultShippingMethodId = vm.SelectedMethodId.Value;
        await _userManager.UpdateAsync(user);

        return RedirectToAction("ReviewOrder");
    }

    // Function for displaying the whlole order
    [HttpGet]
    public async Task<IActionResult> ReviewOrder()
    {
        ViewBag.Step = CheckoutStep.ReviewOrder;
        
        var user = await _userManager.GetUserAsync(User);

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
        
        var userId = user?.Id;
        if (userId == null) return RedirectToAction("Index");

        // Get the cart
        var cartItems = await _db.CartProducts
            .Include(cp => cp.Product)
            .Include(cp => cp.Product.ProductPictures)
            .Where(cp => cp.AspNetUsersId == userId)
            .ToListAsync();

        // Get user address
        var address = await _db.UserAddresses
            .Include(a => a.AspNetUsers)
            .FirstOrDefaultAsync(a => a.Id == user.DefaultUserAddressId);
        
        ShippingMethod? shippingMethod = null;
        decimal shippingPrice = 0;

        if (user.DefaultShippingMethodId != null)
        {
            // Get user shipping method
            shippingMethod = await _db.ShippingMethods
                .FirstOrDefaultAsync(m => m.Id == user.DefaultShippingMethodId);

            if (shippingMethod != null)
            {
                // Calculate cart total
                decimal productTotal = cartItems.Sum(i => (i.Product.ProductCampaignPrice ?? i.Product.ProductPriceInVAT) * i.Quantity);
                
                // Get the thresholds
                var thresholds = _db.ShippingPriceThresholds
                    .AsEnumerable()
                    .OrderBy(t => t.MinOrderAmount)
                    .ToList();

                // Get correct shipping price based on cart total
                decimal basePrice = thresholds
                    .Where(t => productTotal >= t.MinOrderAmount)
                    .Select(t => t.ShippingPrice)
                    .DefaultIfEmpty(0)
                    .Last();

                // Calculate the shipping price based on thresholds and method
                shippingPrice = basePrice * shippingMethod.RateMultiplier;
            }
        }

        // Create a viewmodel
        var vm = new ReviewOrderViewModel
        {
            CartProducts = cartItems,
            UserAddress = address ?? new UserAddress(),
            ShippingMethod = shippingMethod,
            ShippingPrice = shippingPrice
        };

        return View(vm);
    }


    // LIM ALT SOM STÅR I DENNE FUNKSJONEN INN I DIN BETALINGSFUNKSJON TORMOD
    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        var user = await _userManager.GetUserAsync(User);
        var userId = user?.Id;

        if (user == null || userId == null) return Unauthorized();
        
        // Get cart
        var cartItems = await _db.CartProducts
            .Include(cp => cp.Product)
            .Where(cp => cp.AspNetUsersId == userId)
            .ToListAsync();

        if (!cartItems.Any()) return RedirectToAction("Index");

        decimal beforeDiscount = cartItems.Sum(i => i.Product.ProductPriceInVAT * i.Quantity);
        decimal productTotal = cartItems.Sum(i => (i.Product.ProductCampaignPrice ?? i.Product.ProductPriceInVAT) * i.Quantity);
        decimal productDiscount = beforeDiscount - productTotal;

       // Get user address
        var address = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == user.DefaultUserAddressId);
        if (address == null) return RedirectToAction("Address");
        
        // Get user shipping method
        var method = await _db.ShippingMethods.FirstOrDefaultAsync(s => s.Id == user.DefaultShippingMethodId);
        if (method == null) return RedirectToAction("Shipping");

        // Get the thresholds
        var thresholds = _db.ShippingPriceThresholds
            .AsEnumerable()
            .OrderBy(t => t.MinOrderAmount)
            .ToList();

        // Get correct shipping price based on cart total
        decimal basePrice = thresholds
            .Where(t => productTotal >= t.MinOrderAmount)
            .Select(t => t.ShippingPrice)
            .DefaultIfEmpty(0)
            .Last();

        // Calculate the shipping price based on thresholds and method
        decimal shippingPrice = basePrice * method.RateMultiplier;
        
        int percent = HttpContext.Session.GetInt32("DiscountPercent") ?? 0;
        decimal discountAmount = productTotal * (percent / 100m);
        decimal total = productTotal - discountAmount + shippingPrice;

        // Create order
        var order = new Order
        {
            OrderDate = DateTime.UtcNow,
            Status = "Paid",
            TotalCost = total,
            AspNetUsersId = userId,

            DiscountCodeId = percent > 0 ? $"{percent}%" : null,
            ShippingProvider = method.MethodName,
            
            ShippingFirstName = address.FirstName,
            ShippingLastName = address.LastName,
            ShippingStreet = address.Street,
            ShippingPostalCode = address.PostalCode,
            ShippingCity = address.City,
            ShippingPhoneNumber = address.PhoneNumber,
            
            ShippingMethodName = method.MethodName,
            ShippingMethodRateMultiplier = method.RateMultiplier,
            ShippingPrice = shippingPrice
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(); 
        
        // Add products
        foreach (var item in cartItems)
        {
            _db.OrderProducts.Add(new OrderProduct
            {
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity
            });
        }

        await _db.SaveChangesAsync();

        // Clear cart
        _db.CartProducts.RemoveRange(cartItems);
        await _db.SaveChangesAsync();

        // Remove discount code after use
        HttpContext.Session.Remove("DiscountPercent");

        return RedirectToAction("Complete");
    }

    public async Task<IActionResult> Complete(){
        _discountService.RegisterUsage();
        ViewBag.Step = CheckoutStep.Complete;
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
