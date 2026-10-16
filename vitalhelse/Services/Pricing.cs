using System.Globalization;
using VitalHelse.Models;

namespace VitalHelse.Services;

/// <summary>
/// Single place for price rules: what a customer pays, the VAT split and number formatting.
/// All stored prices include VAT (25 %); ex-VAT prices are always derived from them.
/// </summary>
public static class Pricing
{
    public const decimal VatRate = 0.25m;

    private static readonly CultureInfo Norwegian = BuildNorwegian();

    private static CultureInfo BuildNorwegian()
    {
        try { return new CultureInfo("nb-NO"); }
        catch (CultureNotFoundException) { return CultureInfo.InvariantCulture; }
    }

    public static CultureInfo Culture => Norwegian;

    /// <summary>Business customers and staff can see prices excluding VAT as well (the MVA toggle in the header).</summary>
    public static bool SeesExVat(System.Security.Claims.ClaimsPrincipal user) =>
        user.IsInRole("BusinessCustomer") || user.IsInRole("Admin") || user.IsInRole("Staff");

    public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    /// <summary>The price per unit the customer pays (campaign price when there is one), including VAT.</summary>
    public static decimal Effective(Product product) => product.ProductCampaignPrice ?? product.ProductPriceInVAT;

    public static bool HasCampaign(Product product) =>
        product.ProductCampaignPrice.HasValue && product.ProductCampaignPrice.Value < product.ProductPriceInVAT;

    public static decimal ExVat(decimal incVat) => Round(incVat / (1 + VatRate));

    public static decimal VatPart(decimal incVat) => Round(incVat - incVat / (1 + VatRate));

    public static int DiscountPercent(Product product) =>
        HasCampaign(product) && product.ProductPriceInVAT > 0
            ? (int)Math.Round((1 - product.ProductCampaignPrice!.Value / product.ProductPriceInVAT) * 100)
            : 0;

    /// <summary>1 234,50 kr (Norwegian format regardless of the server's culture).</summary>
    public static string Money(decimal value) => Round(value).ToString("N2", Norwegian) + " kr";

    /// <summary>Plain number with a dot as decimal separator, safe to put in JavaScript or data attributes.</summary>
    public static string Plain(decimal value) => Round(value).ToString("0.00", CultureInfo.InvariantCulture);

    public static PriceInfo Info(Product product) => new(product.ProductPriceInVAT, product.ProductCampaignPrice);
}

/// <summary>What a price display needs: the ordinary price and, when on sale, the campaign price (both including VAT).</summary>
public record PriceInfo(decimal Original, decimal? Campaign)
{
    public bool OnSale => Campaign.HasValue && Campaign.Value < Original;
    public decimal Current => OnSale ? Campaign!.Value : Original;
    public int Percent => OnSale && Original > 0 ? (int)Math.Round((1 - Campaign!.Value / Original) * 100) : 0;
}
