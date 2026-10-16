namespace VitalHelse.Services;

public class AuthMessageSenderOptions
{
    public string? SenderGridKey { get; set; } = Environment.GetEnvironmentVariable("AUTHMESSAGESENDEROPTIONS__SENDERGRIDKEY");
}