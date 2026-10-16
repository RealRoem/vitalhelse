using Microsoft.Extensions.Options;
using MimeKit;
using VitalHelse.Configuration;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;


namespace VitalHelse.Services;

public class EmailService 
{
    private readonly EmailConfig _emailConfig;

    public EmailService(IOptions<EmailConfig> emailConfig)
    {
        _emailConfig = emailConfig.Value;
    }
    
    /// <summary>
    /// Sends email from the website domain
    /// </summary>
    /// <param name="to">Email of receiver</param>
    /// <param name="subject">Subject of email</param>
    /// <param name="body">Main info of email</param>
    /// <param name="htmlBody">The body nested in HTML format</param>
    public async Task SendEmailAsync(string to, string firstAndLastName, string subject, string body, string htmlBody)
    {
        
        var message = new MimeMessage(); // Creates a new message
        message.From.Add(new MailboxAddress("VitalHelse", _emailConfig.UserName)); // Info about the sender
        message.To.Add(new MailboxAddress(firstAndLastName, to)); // Info about the receiver
        message.Subject = subject;

        // Bodybuilder gives an opportunity to write HTML format
        var builder = new BodyBuilder
        {
            TextBody = body,
            HtmlBody = string.Format(htmlBody)
        };

        message.Body = builder.ToMessageBody();

        // Gives information about the email sender MOVE THIS TO ENV POSSIBLY
        using var client = new SmtpClient();
        try
        {
            // MIGHT CHANGE BASED ON VITALHELSES GMAIL
            await client.ConnectAsync(_emailConfig.Host, 587, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);

            await client.SendAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
            throw; // rethrow to the terminal
        }
        finally
        {
            Console.WriteLine("EmailService finished");
            await client.DisconnectAsync(true);
        }

    }
}