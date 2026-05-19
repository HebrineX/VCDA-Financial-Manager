using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using VCDA.FinancialManager.Web.Data;
using VCDA.FinancialManager.Web.Models;

namespace VCDA.FinancialManager.Web.Services;

internal sealed class SmtpEmailSender(
    IOptions<SmtpOptions> smtpOptions,
    ILogger<SmtpEmailSender> logger) : IEmailSender<ApplicationUser>
{
    private readonly SmtpOptions options = smtpOptions.Value;

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        SendEmailAsync(
            email,
            "Confirma tu cuenta en VCDA-Financial-Manager",
            $"""
             <p>Hola{GetGreetingSuffix(user)}.</p>
             <p>Confirma tu cuenta haciendo clic en el siguiente enlace:</p>
             <p><a href="{confirmationLink}">Confirmar cuenta</a></p>
             """);

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        SendEmailAsync(
            email,
            "Recupera tu contraseña de VCDA-Financial-Manager",
            $"""
             <p>Hola{GetGreetingSuffix(user)}.</p>
             <p>Puedes restablecer tu contraseña desde el siguiente enlace:</p>
             <p><a href="{resetLink}">Restablecer contraseña</a></p>
             """);

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        SendEmailAsync(
            email,
            "Código de recuperación de VCDA-Financial-Manager",
            $"""
             <p>Hola{GetGreetingSuffix(user)}.</p>
             <p>Usa el siguiente código para recuperar tu contraseña:</p>
             <p><strong>{resetCode}</strong></p>
             """);

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        ValidateConfiguration();

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
        logger.LogInformation("SMTP email sent to {Email} with subject {Subject}", toEmail, subject);
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(options.Host))
        {
            throw new InvalidOperationException("SMTP no configurado: falta Smtp:Host.");
        }

        if (string.IsNullOrWhiteSpace(options.FromEmail))
        {
            throw new InvalidOperationException("SMTP no configurado: falta Smtp:FromEmail.");
        }
    }

    private static string GetGreetingSuffix(ApplicationUser user)
    {
        return string.IsNullOrWhiteSpace(user.UserName) ? string.Empty : $" {user.UserName}";
    }
}
