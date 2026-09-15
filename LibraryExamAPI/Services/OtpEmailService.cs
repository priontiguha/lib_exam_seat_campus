using System.Net;
using System.Net.Mail;

namespace LibraryExamAPI.Services;

public interface IOtpEmailService
{
    Task SendOtpAsync(string email, string otpCode);
}

public class OtpEmailService : IOtpEmailService
{
    private readonly IConfiguration _configuration;

    public OtpEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendOtpAsync(string email, string otpCode)
    {
        var smtpHost = _configuration["Smtp:Host"];
        var smtpPort = _configuration["Smtp:Port"];
        var smtpFrom = _configuration["Smtp:From"] ?? "noreply@library.edu";
        var enableSsl = _configuration["Smtp:EnableSsl"];

        if (string.IsNullOrWhiteSpace(smtpHost) || !int.TryParse(smtpPort, out var port))
        {
            Console.WriteLine($"[OTP Email Fallback] Email: {email} | OTP: {otpCode}");
            return;
        }

        using var client = new SmtpClient(smtpHost, port)
        {
            EnableSsl = bool.TryParse(enableSsl, out var sslEnabled) && sslEnabled,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = string.IsNullOrWhiteSpace(_configuration["Smtp:Username"]) ? CredentialCache.DefaultNetworkCredentials : new NetworkCredential(_configuration["Smtp:Username"], _configuration["Smtp:Password"])
        };

        using var message = new MailMessage
        {
            From = new MailAddress(smtpFrom),
            Subject = "Library & Exam Portal OTP Verification",
            Body = $"Your OTP code is: {otpCode}\n\nUse it to verify your account in the portal.",
            IsBodyHtml = false
        };

        message.To.Add(email);

        try
        {
            await client.SendMailAsync(message);
            Console.WriteLine($"[OTP Email Sent] {email} -> {otpCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OTP Email Failed] {email}: {ex.Message}");
            Console.WriteLine($"[OTP Email Fallback] Email: {email} | OTP: {otpCode}");
        }
    }
}
