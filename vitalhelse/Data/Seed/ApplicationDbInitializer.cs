using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Models;

namespace VitalHelse.Data;

public static class ApplicationDbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext db, UserManager<AspNetUsers> um, RoleManager<IdentityRole> rm)
    {
        // Apply any pending migrations (safe for production)
        //await db.Database.MigrateAsync();
        
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        // Ensure roles exist
        foreach (var role in new[] { "Admin", "Staff", "BusinessCustomer" })
        {
            if (!await rm.RoleExistsAsync(role))
                await rm.CreateAsync(new IdentityRole(role));
        }

        // Admin user
        if (await um.FindByEmailAsync("admin@vitalhelse.no") == null)
        {
            var adminUser = new AspNetUsers { UserName = "admin@vitalhelse.no", Email = "admin@vitalhelse.no", EmailConfirmed = true };
            await um.CreateAsync(adminUser, "Password1.");
            await um.AddToRoleAsync(adminUser, "Admin");
        }

        // Staff user
        if (await um.FindByEmailAsync("staff@vitalhelse.no") == null)
        {
            var staffUser = new AspNetUsers { UserName = "staff@vitalhelse.no", Email = "staff@vitalhelse.no", EmailConfirmed = true };
            await um.CreateAsync(staffUser, "Password1.");
            await um.AddToRoleAsync(staffUser, "Staff");
        }

        // Business user
        if (await um.FindByEmailAsync("business@vitalhelse.no") == null)
        {
            var businessUser = new AspNetUsers { UserName = "business@vitalhelse.no", Email = "business@vitalhelse.no", EmailConfirmed = true, OrgNr = "123456789"};
            await um.CreateAsync(businessUser, "Password1.");
            await um.AddToRoleAsync(businessUser, "BusinessCustomer");
        }

        // Private customer user
        if (await um.FindByEmailAsync("customer@vitalhelse.no") == null)
        {
            var customerUser = new AspNetUsers { UserName = "customer@vitalhelse.no", Email = "customer@vitalhelse.no", EmailConfirmed = true };
            await um.CreateAsync(customerUser, "Password1.");
        }

        await db.SaveChangesAsync();
    }
}
