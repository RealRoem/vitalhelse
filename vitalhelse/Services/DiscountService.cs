using VitalHelse.Data;

namespace VitalHelse.Services;

public record ActiveDiscount(string Code, int Percent);

/// <summary>
/// Remembers the discount code a customer has entered (in the session) and always re-checks it against the
/// database, so a code that has been deactivated, has expired or belongs to another user is never applied.
/// </summary>
public class DiscountService
{
    private readonly IHttpContextAccessor _http;
    private readonly ApplicationDbContext _db;

    private const string SessionCode = "DiscountCode";
    private const string SessionPercent = "DiscountPercent";
    private const string SessionUser = "DiscountUser";

    public DiscountService(IHttpContextAccessor http, ApplicationDbContext db)
    {
        _http = http;
        _db = db;
    }

    private ISession Session => _http.HttpContext!.Session;

    private string? CurrentUserId =>
        _http.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    /// <summary>Looks a code up the way a customer would type it. Null when unknown, inactive or expired.</summary>
    public Models.Discount.DiscountCode? FindValid(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var trimmed = code.Trim().ToLower();
        var now = DateTime.UtcNow;

        return _db.DiscountCodes.FirstOrDefault(d =>
            d.Code.ToLower() == trimmed && d.IsActive && (d.ExpiresAt == null || d.ExpiresAt > now));
    }

    public void SetDiscountCode(string code, int percent)
    {
        Session.SetString(SessionCode, code);
        Session.SetInt32(SessionPercent, percent);
        Session.SetString(SessionUser, CurrentUserId ?? "");
    }

    /// <summary>The code that currently applies to this customer, or null (and the session entry is cleared).</summary>
    public ActiveDiscount? GetActive()
    {
        var code = Session.GetString(SessionCode);
        if (string.IsNullOrWhiteSpace(code)) return null;

        var owner = Session.GetString(SessionUser);
        var entry = FindValid(code);
        if (entry == null || (!string.IsNullOrEmpty(owner) && owner != CurrentUserId))
        {
            Clear();
            return null;
        }

        return new ActiveDiscount(entry.Code, entry.DiscountPercent);
    }

    /// <summary>Counts a use of the code. Call when an order has been placed with it.</summary>
    public void RegisterUsage(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return;

        var entry = _db.DiscountCodes.FirstOrDefault(d => d.Code == code);
        if (entry == null) return;

        entry.TimesUsed += 1;
        _db.SaveChanges();
    }

    public void Clear()
    {
        Session.Remove(SessionCode);
        Session.Remove(SessionPercent);
        Session.Remove(SessionUser);
    }
}
