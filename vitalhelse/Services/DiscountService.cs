using VitalHelse.Data;
namespace VitalHelse.Services;

public class DiscountService
{
    private readonly IHttpContextAccessor _http;
    private readonly ApplicationDbContext _db;

    private const string SESSION_CODE = "DiscountCode";
    private const string SESSION_PERCENT = "DiscountPercent";

    public DiscountService(IHttpContextAccessor http, ApplicationDbContext db)
    {
        _http = http;
        _db = db;
    }

    private ISession Session => _http.HttpContext!.Session;

    /* -----------------------------------------------------------
       1) Legg inn kode og prosent i session
       ----------------------------------------------------------- */
    public void SetDiscountCode(string code, int percent)
    {
        Session.SetString(SESSION_CODE, code);
        Session.SetInt32(SESSION_PERCENT, percent);
    }

    /* -----------------------------------------------------------
       2) Apply rabatt på totalsum
       ----------------------------------------------------------- */
    public decimal? ApplyDiscount(decimal orderTotal)
    {
        var percent = Session.GetInt32(SESSION_PERCENT);
        if (percent == null || percent <= 0) return null;

        var discountedTotal = orderTotal * (1 - (percent.Value / 100m));
        return discountedTotal;
    }

    /* -----------------------------------------------------------
       3) Oppdater TimesUsed når checkout er complete
       ----------------------------------------------------------- */
    public void RegisterUsage()
    {
        var code = Session.GetString(SESSION_CODE);
        if (string.IsNullOrWhiteSpace(code)) return;

        var entry = _db.DiscountCodes.FirstOrDefault(d => d.Code == code);
        if (entry == null) return;

        entry.TimesUsed += 1;
        _db.SaveChanges();

        // Fjern rabatt-info etter bruk
        Session.Remove(SESSION_CODE);
        Session.Remove(SESSION_PERCENT);
    }

    /* -----------------------------------------------------------
       4) Fjern rabatt (hvis kunden sletter den senere)
       ----------------------------------------------------------- */
    public void Clear()
    {
        Session.Remove(SESSION_CODE);
        Session.Remove(SESSION_PERCENT);
    }
}
