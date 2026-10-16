using Microsoft.EntityFrameworkCore;
using VitalHelse.Models;
using VitalHelse.Data;

namespace VitalHelse.Services;

/// <summary>
/// Handles synchronization of products between Tripletex and the local database.
/// </summary>
public class TripletexSyncService
{
    private readonly TripletexService _tripletexService;
    private readonly ApplicationDbContext _db;

    public TripletexSyncService(TripletexService tripletexService, ApplicationDbContext db)
    {
        _tripletexService = tripletexService;
        _db = db;
    }

    /// <summary>
    /// Synchronizes products from Tripletex into the local database.
    /// Tripletex is the source of truth for: Name, Price, Stock.
    /// Local data is preserved for: Description, Images, Categories, Visibility, etc.
    /// </summary>
    /// <returns>Number of added, updated and hidden products.</returns>
    public async Task<(int added, int updated, int hidden)> SyncProductsAsync()
    {
        int added = 0, updated = 0, hidden = 0;

        try
        {
            var sessionToken = await _tripletexService.GetSessionTokenAsync();
            var tripletexProducts = await _tripletexService.GetProductsAsync(sessionToken);

            var existingProducts = await _db.Products.ToListAsync();

            // --- Add or Update Products ---
            foreach (var tProd in tripletexProducts)
            {
                var dbProd = existingProducts.FirstOrDefault(p => p.TripletexProductId == tProd.Id);

                if (dbProd == null)
                {
                    // Create new product
                    _db.Products.Add(new Product
                    {
                        TripletexProductId = tProd.Id,
                        ProductName = tProd.Name,
                        ProductPriceInVAT = tProd.PriceInVat,
                        ProductPriceExVAT = tProd.PriceExVat,
                        StockCount = (int?)tProd.StockCount,
                        ProductVisibility = false // Must be manually approved
                    });

                    added++;
                }
                else
                {
                    // Update existing product
                    dbProd.ProductName = tProd.Name;
                    dbProd.ProductPriceInVAT = tProd.PriceInVat;
                    dbProd.ProductPriceExVAT = tProd.PriceExVat;
                    dbProd.StockCount = (int?)tProd.StockCount;

                    updated++;
                }
            }

            // --- Hide products removed from Tripletex ---
            foreach (var dbProd in existingProducts.Where(p => p.TripletexProductId != null))
            {
                if (!tripletexProducts.Any(tp => tp.Id == dbProd.TripletexProductId))
                {
                    dbProd.ProductVisibility = false;
                    hidden++;
                }
            }

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Tripletex Sync Error: {ex.Message}");
        }

        return (added, updated, hidden);
    }
}
