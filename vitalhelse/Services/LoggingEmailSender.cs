using Microsoft.AspNetCore.Identity.UI.Services;

namespace VitalHelse.Services;

/// <summary>
/// Demo replacement for the SendGrid sender: writes the email to the log instead of sending it.
/// </summary>
public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger) => _logger = logger;

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation("Demo mode, email not sent. To: {Email}, Subject: {Subject}\n{Body}", email, subject, htmlMessage);
        return Task.CompletedTask;
    }
}
