using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using VCDA.FinancialManager.Web.Data;
using VCDA.FinancialManager.Web.Models;

namespace VCDA.FinancialManager.Web.Services;

internal sealed class SmtpEmailSender(
    IOptions<SmtpOptions> smtpOptions,
    IMemoryCache memoryCache,
    ILogger<SmtpEmailSender> logger) : IEmailSender<ApplicationUser>
{
    private readonly SmtpOptions options = smtpOptions.Value;

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        SendEmailAsync(
            user,
            email,
            "confirmation-link",
            "Confirma tu cuenta en VCDA-Financial-Manager",
            $"""
             <p>Hola{GetGreetingSuffix(user)}.</p>
             <p>Confirma tu cuenta haciendo clic en el siguiente enlace:</p>
             <p><a href="{confirmationLink}">Confirmar cuenta</a></p>
             """);

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        SendEmailAsync(
            user,
            email,
            "password-reset-link",
            "Recupera tu contraseña de VCDA-Financial-Manager",
            $"""
             <p>Hola{GetGreetingSuffix(user)}.</p>
             <p>Puedes restablecer tu contraseña desde el siguiente enlace:</p>
             <p><a href="{resetLink}">Restablecer contraseña</a></p>
             """);

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        SendEmailAsync(
            user,
            email,
            "password-reset-code",
            "Código de recuperación de VCDA-Financial-Manager",
            $"""
             <p>Hola{GetGreetingSuffix(user)}.</p>
             <p>Usa el siguiente código para recuperar tu contraseña:</p>
             <p><strong>{resetCode}</strong></p>
             """);

    private async Task SendEmailAsync(ApplicationUser user, string toEmail, string purpose, string subject, string htmlBody)
    {
        options.Validate();

        if (RecentlySent(user, toEmail, purpose))
        {
            logger.LogInformation(
                "SMTP email skipped for {MaskedEmail} with purpose {Purpose}: recently sent.",
                MaskEmail(toEmail),
                purpose);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(options.FromEmail, options.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(options.Host, options.Port)
        {
            EnableSsl = options.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(options.Username))
        {
            client.Credentials = new NetworkCredential(options.Username, options.Password);
        }

        await client.SendMailAsync(message);
        logger.LogInformation(
            "SMTP email sent to {MaskedEmail} with purpose {Purpose} and subject {Subject}",
            MaskEmail(toEmail),
            purpose,
            subject);
    }

    private bool RecentlySent(ApplicationUser user, string toEmail, string purpose)
    {
        var userKey = string.IsNullOrWhiteSpace(user.Id) ? "anonymous" : user.Id;
        var emailKey = NormalizeEmailForCache(toEmail);
        var cacheKey = $"identity-email-sent:{purpose}:{userKey}:{emailKey}";

        if (memoryCache.TryGetValue(cacheKey, out _))
        {
            return true;
        }

        memoryCache.Set(cacheKey, true, TimeSpan.FromMinutes(5));
        return false;
    }

    private static string GetGreetingSuffix(ApplicationUser user)
    {
        return string.IsNullOrWhiteSpace(user.UserName) ? string.Empty : $" {user.UserName}";
    }

    private static string NormalizeEmailForCache(string email)
    {
        return email.Trim().ToUpperInvariant();
    }

    private static string MaskEmail(string email)
    {
        var trimmedEmail = email.Trim();
        var atIndex = trimmedEmail.IndexOf('@');
        if (atIndex <= 0 || atIndex == trimmedEmail.Length - 1)
        {
            return "***";
        }

        var localPart = trimmedEmail[..atIndex];
        var domain = trimmedEmail[(atIndex + 1)..];
        var visibleLocal = localPart.Length <= 2 ? localPart[0..1] : localPart[..2];
        return $"{visibleLocal}***@{domain}";
    }
}
