namespace VitalHelse.Services;

/// <summary>
/// Describes which external integrations are configured. Anything not configured
/// falls back to a local demo behaviour so the site runs without secrets.
/// </summary>
public class AppFeatures
{
    public bool StripeEnabled { get; init; }
    public bool EmailEnabled { get; init; }
    public bool TripletexEnabled { get; init; }

    public static AppFeatures From(IConfiguration config) => new()
    {
        StripeEnabled = !string.IsNullOrWhiteSpace(config["STRIPE_SECRET_KEY"]),
        EmailEnabled = !string.IsNullOrWhiteSpace(config["AUTHMESSAGESENDEROPTIONS__SENDERGRIDKEY"]),
        TripletexEnabled = config.GetValue<bool>("Tripletex:Enabled")
                           && !string.IsNullOrWhiteSpace(config["Tripletex:ConsumerToken"])
                           && !string.IsNullOrWhiteSpace(config["Tripletex:EmployeeToken"])
    };
}
