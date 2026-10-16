using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using VitalHelse.Models;
using VitalHelse.Models.Discount;
using VitalHelse.Services;

namespace VitalHelse.Data;

/// <summary>
/// Fills the shop with realistic operating data: discounts, campaigns, discount codes, banners,
/// tags, sizes, customers with addresses, orders and favorites. Every section only runs when its table is empty,
/// so it is safe to call on every startup.
/// </summary>
public static class DemoData
{
    private const string DemoPassword = "Password1.";

    public static async Task InitializeAsync(ApplicationDbContext db, UserManager<AspNetUsers> um, CampaignService campaigns, string webRoot)
    {
        if (!db.Products.Any()) return; // nothing to build on (seeding disabled)

        SeedCategoryDiscounts(db);
        SeedDiscountCodes(db);
        SeedCampaigns(db, campaigns);
        SeedBanners(db, webRoot);
        SeedTagsAndSizes(db);
        await SeedCustomersAndOrders(db, um);

        // Switch off campaigns that have ended and make sure sale prices match the running ones
        campaigns.SyncSchedule();
    }

    // ------------------------------------------------------------------
    //  Rabattsatser per hovedkategori
    // ------------------------------------------------------------------
    private static void SeedCategoryDiscounts(ApplicationDbContext db)
    {
        if (db.CategoryDiscounts.Any()) return;

        var rates = new Dictionary<string, int>
        {
            ["hudpleie"] = 15,
            ["aloe-vera"] = 20,
            ["hygiene"] = 10,
            ["massasje"] = 10,
            ["tilbud"] = 25
        };

        foreach (var (name, percent) in rates)
        {
            var category = db.Categories.FirstOrDefault(c => c.CategoryName == name && c.ParentCategoryId == null);
            if (category != null)
                db.CategoryDiscounts.Add(new CategoryDiscount { CategoryId = category.CategoryId, DiscountPercent = percent });
        }

        db.SaveChanges();
    }

    // ------------------------------------------------------------------
    //  Rabattkoder
    // ------------------------------------------------------------------
    private static void SeedDiscountCodes(ApplicationDbContext db)
    {
        if (db.DiscountCodes.Any()) return;

        var now = DateTime.UtcNow;
        db.DiscountCodes.AddRange(
            new DiscountCode { Code = "VITAL10", DiscountPercent = 10, CreatedAt = now.AddMonths(-6), ExpiresAt = null, IsActive = true, TimesUsed = 12 },
            new DiscountCode { Code = "VELKOMMEN15", DiscountPercent = 15, CreatedAt = now.AddMonths(-3), ExpiresAt = now.AddMonths(6), IsActive = true, TimesUsed = 4 },
            new DiscountCode { Code = "SOMMER20", DiscountPercent = 20, CreatedAt = now.AddMonths(-5), ExpiresAt = now.AddMonths(-2), IsActive = true, TimesUsed = 31 },
            new DiscountCode { Code = "ANSATT30", DiscountPercent = 30, CreatedAt = now.AddMonths(-8), ExpiresAt = null, IsActive = false, TimesUsed = 2 });
        db.SaveChanges();
    }

    // ------------------------------------------------------------------
    //  Kampanjer (priser settes via CampaignService, som i admin)
    // ------------------------------------------------------------------
    private static void SeedCampaigns(ApplicationDbContext db, CampaignService campaigns)
    {
        if (db.Campaigns.Any()) return;

        int TopCategory(string name) =>
            db.Categories.First(c => c.CategoryName == name && c.ParentCategoryId == null).CategoryId;

        var today = DateTime.Now.Date;
        var list = new List<Campaign>
        {
            new() { Name = "Frisk hud – 15 % på hudpleie", Start = today.AddDays(-7), End = today.AddDays(30), IsActive = true, CategoryIds = { TopCategory("hudpleie") } },
            new() { Name = "Aloe Vera-uken", Start = today.AddDays(-2), End = today.AddDays(12), IsActive = true, CategoryIds = { TopCategory("aloe-vera") } },
            new() { Name = "Hygiene – vinterkampanje", Start = today.AddDays(45), End = today.AddDays(75), IsActive = false, CategoryIds = { TopCategory("hygiene") } },
            new() { Name = "Sommersalg massasje", Start = today.AddMonths(-5), End = today.AddMonths(-4), IsActive = false, CategoryIds = { TopCategory("massasje") } }
        };
        db.Campaigns.AddRange(list);
        db.SaveChanges();

        campaigns.RecalculateAll();
    }

    // ------------------------------------------------------------------
    //  Forsidebannere (bildene ligger i wwwroot/images/banners)
    // ------------------------------------------------------------------
    private static void SeedBanners(ApplicationDbContext db, string webRoot)
    {
        if (!db.HomePageSettings.Any())
            db.HomePageSettings.Add(new HomePageSettings { CarouselInterval = 6 });

        if (!db.BannerImages.Any())
        {
            db.BannerImages.AddRange(
                new BannerImage { ImagePath = "/images/banners/banner-1.jpg", AltText = "Oppdag din glød med naturlig hudpleie", DisplayOrder = 0, LinkType = "None" },
                new BannerImage { ImagePath = "/images/banners/banner-2.jpg", AltText = "Frisk hud med 15 % rabatt på hudpleie", DisplayOrder = 1, LinkType = "Category", LinkTarget = "hudpleie" },
                new BannerImage { ImagePath = "/images/banners/banner-3.jpg", AltText = "Opplev den beroligende kraften i Aloe Vera", DisplayOrder = 2, LinkType = "Category", LinkTarget = "aloe-vera" });
        }

        // Drop banners whose image file no longer exists (e.g. from an older database)
        foreach (var banner in db.BannerImages.ToList())
        {
            var file = Path.Combine(webRoot, banner.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(file))
                db.BannerImages.Remove(banner);
        }

        db.SaveChanges();
    }

    // ------------------------------------------------------------------
    //  Tags og størrelser (brukes av sidefilteret)
    // ------------------------------------------------------------------
    private static void SeedTagsAndSizes(ApplicationDbContext db)
    {
        var products = db.Products.OrderBy(p => p.ProductId).ToList();

        if (!db.Tags.Any())
        {
            var tags = new[] { "Vegansk", "Parfymefri", "Nyhet", "Bestselger", "Dermatologisk testet" }
                .Select(t => new Tag { Tags = t }).ToList();
            db.Tags.AddRange(tags);
            db.SaveChanges();

            var hudpleieIds = db.ProductCategories
                .Where(pc => pc.Category.CategoryName == "hudpleie" || pc.Category.ParentCategory!.CategoryName == "hudpleie")
                .Select(pc => pc.ProductId).ToHashSet();

            for (var i = 0; i < products.Count; i++)
            {
                var chosen = new HashSet<int>();
                if (i % 3 == 0) chosen.Add(0);
                if (i % 4 == 1) chosen.Add(1);
                if (i % 9 == 2) chosen.Add(2);
                if (i % 7 == 3) chosen.Add(3);
                if (hudpleieIds.Contains(products[i].ProductId)) chosen.Add(4);

                foreach (var index in chosen)
                    db.ProductTags.Add(new ProductTag { ProductId = products[i].ProductId, TagId = tags[index].TagId });
            }
            db.SaveChanges();
        }

        if (!db.PhysicalAttributes.Any())
        {
            var sizeRegex = new Regex(@"(\d+(?:[.,]\d+)?)\s?(ml|l|g|gr|kg)\b", RegexOptions.IgnoreCase);
            var attributes = new Dictionary<string, PhysicalAttribute>();

            foreach (var product in products)
            {
                var match = sizeRegex.Match(product.ProductName);
                if (!match.Success) continue;

                var unit = match.Groups[2].Value.ToLowerInvariant();
                unit = unit switch { "gr" => "g", "l" => "L", _ => unit };
                var label = $"{match.Groups[1].Value} {unit}";

                if (!attributes.TryGetValue(label, out var attribute))
                {
                    attribute = new PhysicalAttribute { Attribute = label };
                    attributes[label] = attribute;
                    db.PhysicalAttributes.Add(attribute);
                }

                attribute.PhysicalProductAttributes.Add(new PhysicalProductAttribute { Product = product, PhysicalAttribute = attribute });
            }
            db.SaveChanges();
        }
    }

    // ------------------------------------------------------------------
    //  Kunder, adresser, favoritter og bestillinger
    // ------------------------------------------------------------------
    private record Person(string Email, string First, string Last, string Phone, string Street, string PostalCode, string City);

    private record OrderSpec(string Email, int DaysAgo, string Status, int ShippingMethodIndex, int DiscountPercent, params (int Product, int Qty)[] Lines);

    private static async Task SeedCustomersAndOrders(ApplicationDbContext db, UserManager<AspNetUsers> um)
    {
        if (db.Orders.Any()) return;

        var people = new[]
        {
            new Person("customer@vitalhelse.no", "Ingrid", "Solberg", "91234567", "Storgata 15", "0155", "Oslo"),
            new Person("business@vitalhelse.no", "Per", "Hansen", "32123456", "Kirkegata 7", "3015", "Drammen"),
            new Person("kari.berg@example.com", "Kari", "Berg", "99887766", "Strandveien 4", "4005", "Stavanger"),
            new Person("ola.johansen@example.com", "Ola", "Johansen", "41223344", "Torggata 9", "5014", "Bergen"),
            new Person("mari.lund@example.com", "Mari", "Lund", "47556677", "Kongens gate 21", "7013", "Trondheim"),
            new Person("anders.dahl@example.com", "Anders", "Dahl", "90112233", "Dronningens gate 3", "9008", "Tromsø")
        };

        var users = new Dictionary<string, AspNetUsers>();
        var addresses = new Dictionary<string, UserAddress>();

        foreach (var person in people)
        {
            var user = await um.FindByEmailAsync(person.Email);
            if (user == null)
            {
                user = new AspNetUsers { UserName = person.Email, Email = person.Email, EmailConfirmed = true };
                var created = await um.CreateAsync(user, DemoPassword);
                if (!created.Succeeded) continue;
            }

            user.FirstName = person.First;
            user.LastName = person.Last;
            user.Address = person.Street;
            user.PostalCode = person.PostalCode;
            user.PostalPlace = person.City;
            user.PhoneNumber = person.Phone;
            if (person.Email == "business@vitalhelse.no")
                user.OrgName = "Hansen Fysioterapi AS";

            var address = new UserAddress
            {
                AspNetUserId = user.Id,
                FirstName = person.First,
                LastName = person.Last,
                PhoneNumber = person.Phone,
                Street = person.Street,
                PostalCode = person.PostalCode,
                City = person.City
            };
            db.UserAddresses.Add(address);
            db.SaveChanges();

            user.DefaultUserAddressId = address.Id;
            await um.UpdateAsync(user);

            users[person.Email] = user;
            addresses[person.Email] = address;
        }

        var products = db.Products.OrderBy(p => p.ProductId).ToList();
        var methods = db.ShippingMethods.OrderBy(m => m.Id).ToList();
        var thresholds = db.ShippingPriceThresholds.AsEnumerable().OrderBy(t => t.MinOrderAmount).ToList();
        if (products.Count < 60 || methods.Count == 0) return;

        var specs = new[]
        {
            new OrderSpec("customer@vitalhelse.no", 61, "Levert", 2, 0, (3, 1), (14, 2)),
            new OrderSpec("customer@vitalhelse.no", 33, "Levert", 2, 10, (22, 1), (41, 1), (7, 1)),
            new OrderSpec("customer@vitalhelse.no", 6, "Sendt", 2, 0, (55, 2)),
            new OrderSpec("business@vitalhelse.no", 52, "Levert", 2, 0, (27, 6), (28, 6), (60, 4)),
            new OrderSpec("business@vitalhelse.no", 19, "Levert", 2, 10, (31, 8), (45, 4)),
            new OrderSpec("business@vitalhelse.no", 2, "Betalt", 2, 0, (29, 10), (30, 5)),
            new OrderSpec("kari.berg@example.com", 47, "Levert", 0, 0, (11, 1), (12, 1)),
            new OrderSpec("kari.berg@example.com", 9, "Sendt", 2, 15, (36, 2), (48, 1)),
            new OrderSpec("ola.johansen@example.com", 28, "Levert", 2, 0, (5, 3)),
            new OrderSpec("ola.johansen@example.com", 1, "Betalt", 1, 0, (18, 1), (52, 2)),
            new OrderSpec("mari.lund@example.com", 38, "Levert", 2, 10, (9, 1), (33, 2), (44, 1)),
            new OrderSpec("anders.dahl@example.com", 14, "Sendt", 2, 0, (24, 2), (57, 1))
        };

        var rng = new Random(42);
        foreach (var spec in specs)
        {
            if (!users.TryGetValue(spec.Email, out var user)) continue;
            var address = addresses[spec.Email];
            var method = methods[Math.Min(spec.ShippingMethodIndex, methods.Count - 1)];

            var lines = spec.Lines.Select(l => (Product: products[l.Product % products.Count], l.Qty)).ToList();
            decimal productTotal = lines.Sum(l => (l.Product.ProductCampaignPrice ?? l.Product.ProductPriceInVAT) * l.Qty);

            decimal basePrice = thresholds
                .Where(t => productTotal >= t.MinOrderAmount)
                .Select(t => t.ShippingPrice)
                .DefaultIfEmpty(0)
                .Last();
            decimal shipping = basePrice * method.RateMultiplier;
            decimal total = productTotal - productTotal * (spec.DiscountPercent / 100m) + shipping;

            var order = new Order
            {
                OrderDate = DateTime.Now.AddDays(-spec.DaysAgo).AddHours(-rng.Next(0, 10)),
                Status = spec.Status,
                TotalCost = total,
                AspNetUsersId = user.Id,
                DiscountCodeId = spec.DiscountPercent > 0 ? $"{spec.DiscountPercent}%" : null,
                ShippingProvider = method.MethodName,
                ShippingFirstName = address.FirstName!,
                ShippingLastName = address.LastName!,
                ShippingStreet = address.Street,
                ShippingPostalCode = address.PostalCode,
                ShippingCity = address.City,
                ShippingPhoneNumber = address.PhoneNumber,
                ShippingMethodName = method.MethodName,
                ShippingMethodRateMultiplier = method.RateMultiplier,
                ShippingPrice = shipping,
                TrackingNumber = spec.Status is "Sendt" or "Levert"
                    ? $"NO{rng.NextInt64(100_000_000_000, 999_999_999_999)}"
                    : "Sporingsnummer sendes ved utsending."
            };
            db.Orders.Add(order);
            db.SaveChanges();

            foreach (var (product, qty) in lines.DistinctBy(l => l.Product.ProductId))
            {
                db.OrderProducts.Add(new OrderProduct
                {
                    OrderId = order.OrderId,
                    ProductId = product.ProductId,
                    Quantity = qty,
                    UnitPrice = product.ProductCampaignPrice ?? product.ProductPriceInVAT
                });
            }
            db.SaveChanges();
        }

        if (users.TryGetValue("customer@vitalhelse.no", out var customer) && !db.FavoriteProducts.Any())
        {
            foreach (var index in new[] { 2, 9, 31, 47 })
                db.FavoriteProducts.Add(new FavoriteProduct { ProductId = products[index].ProductId, AspNetUsersId = customer.Id });
            db.SaveChanges();
        }
    }
}
