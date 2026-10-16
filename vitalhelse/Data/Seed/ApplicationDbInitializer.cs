using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VitalHelse.Models;

namespace VitalHelse.Data;

public static class ApplicationDbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext db, UserManager<AspNetUsers> um, RoleManager<IdentityRole> rm, bool reset)
    {
        if (reset)
            await db.Database.EnsureDeletedAsync();

        await db.Database.EnsureCreatedAsync();

        // A database left over from an older version of the model has a different schema: rebuild it.
        try
        {
            await db.Products.AnyAsync();
            await db.Users.AnyAsync();
        }
        catch (Exception ex) when (ex is Microsoft.Data.Sqlite.SqliteException or InvalidOperationException)
        {
            Console.WriteLine("Existing database does not match the current model, recreating it.");
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }

        foreach (var role in new[] { "Admin", "Staff", "BusinessCustomer" })
        {
            if (!await rm.RoleExistsAsync(role))
                await rm.CreateAsync(new IdentityRole(role));
        }

        await EnsureUser(um, "admin@vitalhelse.no", "Admin");
        await EnsureUser(um, "staff@vitalhelse.no", "Staff");
        await EnsureUser(um, "business@vitalhelse.no", "BusinessCustomer", orgNr: "123456789");
        await EnsureUser(um, "customer@vitalhelse.no", null);
    }

    private static async Task EnsureUser(UserManager<AspNetUsers> um, string email, string? role, string? orgNr = null)
    {
        if (await um.FindByEmailAsync(email) != null) return;

        var user = new AspNetUsers { UserName = email, Email = email, EmailConfirmed = true, OrgNr = orgNr };
        var result = await um.CreateAsync(user, "Password1.");
        if (!result.Succeeded)
            throw new InvalidOperationException($"Could not seed {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        if (role != null)
            await um.AddToRoleAsync(user, role);
    }
}
