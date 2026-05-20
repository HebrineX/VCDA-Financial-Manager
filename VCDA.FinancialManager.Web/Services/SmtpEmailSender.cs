using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using VCDA.FinancialManager.Web.Data;
using VCDA.FinancialManager.Web.Models;

namespace VCDA.FinancialManager.Web.Services;

internal sealed class SmtpEmailSender(
    IOptions<SmtpOptions> smtpOptions,
    IOptions<DataProtectionTokenProviderOptions> tokenOptions,
    IMemoryCache memoryCache,
    ILogger<SmtpEmailSender> logger) : IEmailSender<ApplicationUser>
{
    private readonly SmtpOptions options = smtpOptions.Value;
    private readonly TimeSpan tokenLifespan = tokenOptions.Value.TokenLifespan;

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        SendEmailAsync(
            user,
            email,
            "confirmation-link",
            Localized(
                "Confirma tu cuenta en VCDA-Financial-Manager",
                "Confirm your VCDA-Financial-Manager account"),
            BuildActionEmail(
                user,
                Localized("Confirmacion de cuenta", "Account confirmation"),
                Localized("Activa tu acceso a VCDA", "Activate your VCDA access"),
                Localized(
                    "Confirmá tu correo para empezar a cargar cuentas, movimientos, presupuestos y reportes con seguridad.",
                    "Confirm your email to start managing accounts, transactions, budgets, and reports securely."),
                Localized("Confirmar cuenta", "Confirm account"),
                confirmationLink,
                Localized("Este enlace vence en {0}.", "This link expires in {0}."),
                Localized("Si no creaste esta cuenta, podés ignorar este mensaje.", "If you did not create this account, you can ignore this message.")));

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        SendEmailAsync(
            user,
            email,
            "password-reset-link",
            Localized(
                "Recupera tu contraseña de VCDA-Financial-Manager",
                "Reset your VCDA-Financial-Manager password"),
            BuildActionEmail(
                user,
                Localized("Recuperacion de contraseña", "Password recovery"),
                Localized("Restablece tu contraseña", "Reset your password"),
                Localized(
                    "Recibimos una solicitud para recuperar el acceso a tu cuenta. Usá el botón de abajo para definir una contraseña nueva.",
                    "We received a request to recover access to your account. Use the button below to set a new password."),
                Localized("Restablecer contraseña", "Reset password"),
                resetLink,
                Localized("Este enlace vence en {0}.", "This link expires in {0}."),
                Localized("Si no solicitaste este cambio, no compartas este correo y revisá la seguridad de tu cuenta.", "If you did not request this change, do not share this email and review your account security.")));

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        SendEmailAsync(
            user,
            email,
            "password-reset-code",
            Localized(
                "Código de recuperación de VCDA-Financial-Manager",
                "VCDA-Financial-Manager recovery code"),
            BuildCodeEmail(
                user,
                Localized("Código de recuperación", "Recovery code"),
                Localized("Usá este código para continuar", "Use this code to continue"),
                Localized(
                    "Ingresalo en VCDA-Financial-Manager para recuperar el acceso a tu cuenta.",
                    "Enter it in VCDA-Financial-Manager to recover access to your account."),
                resetCode,
                Localized("Este código vence en {0}.", "This code expires in {0}."),
                Localized("Si no solicitaste este código, podés ignorar el mensaje.", "If you did not request this code, you can ignore this message.")));

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

    private string BuildActionEmail(
        ApplicationUser user,
        string preheader,
        string title,
        string intro,
        string buttonText,
        string link,
        string expirationTemplate,
        string securityNote)
    {
        var safeLink = HtmlEncoder.Default.Encode(WebUtility.HtmlDecode(link));
        var expiration = string.Format(expirationTemplate, FormatTokenLifespan(tokenLifespan));

        return BuildEmailShell(
            preheader,
            title,
            intro,
            $"""
            <table role="presentation" cellspacing="0" cellpadding="0" style="margin: 24px 0;">
                <tr>
                    <td>
                        <a href="{safeLink}" style="display: inline-block; padding: 13px 22px; background: #2563eb; color: #ffffff; border-radius: 12px; font-weight: 700; text-decoration: none;">{HtmlEncoder.Default.Encode(buttonText)}</a>
                    </td>
                </tr>
            </table>
            <p style="margin: 0 0 12px; color: #475569; font-size: 14px;">{HtmlEncoder.Default.Encode(expiration)}</p>
            <p style="margin: 0; color: #64748b; font-size: 13px; line-height: 1.6;">{Localized("Si el boton no funciona, copia y pega este enlace:", "If the button does not work, copy and paste this link:")}</p>
            <p style="margin: 6px 0 0; word-break: break-all; color: #2563eb; font-size: 13px;"><a href="{safeLink}" style="color: #2563eb;">{safeLink}</a></p>
            """,
            user,
            securityNote);
    }

    private string BuildCodeEmail(
        ApplicationUser user,
        string preheader,
        string title,
        string intro,
        string code,
        string expirationTemplate,
        string securityNote)
    {
        var expiration = string.Format(expirationTemplate, FormatTokenLifespan(tokenLifespan));

        return BuildEmailShell(
            preheader,
            title,
            intro,
            $"""
            <div style="margin: 24px 0; padding: 18px 20px; border-radius: 16px; background: #eff6ff; border: 1px solid #bfdbfe; color: #1e3a8a; font-size: 28px; font-weight: 800; letter-spacing: 0.16em; text-align: center;">{HtmlEncoder.Default.Encode(code)}</div>
            <p style="margin: 0; color: #475569; font-size: 14px;">{HtmlEncoder.Default.Encode(expiration)}</p>
            """,
            user,
            securityNote);
    }

    private static string BuildEmailShell(string preheader, string title, string intro, string mainContent, ApplicationUser user, string securityNote)
    {
        return $"""
        <!doctype html>
        <html lang="{GetCurrentLanguage()}">
        <body style="margin: 0; padding: 0; background: #f1f5f9; font-family: 'Segoe UI', Arial, sans-serif; color: #0f172a;">
            <div style="display: none; max-height: 0; overflow: hidden; opacity: 0;">{HtmlEncoder.Default.Encode(preheader)}</div>
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background: #f1f5f9; padding: 28px 12px;">
                <tr>
                    <td align="center">
                        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width: 620px; overflow: hidden; border-radius: 22px; background: #ffffff; border: 1px solid #e2e8f0; box-shadow: 0 18px 45px rgba(15, 23, 42, 0.12);">
                            <tr>
                                <td style="padding: 24px 28px; background: linear-gradient(135deg, #0f172a 0%, #1e3a8a 58%, #2563eb 100%); color: #ffffff;">
                                    <div style="font-size: 13px; font-weight: 800; letter-spacing: 0.14em; text-transform: uppercase; opacity: 0.82;">VCDA</div>
                                    <div style="margin-top: 6px; font-size: 24px; font-weight: 800;">Financial Manager</div>
                                    <div style="margin-top: 6px; color: #bfdbfe; font-size: 14px;">{Localized("Control financiero claro, visual y multiusuario.", "Clear, visual, multi-user financial control.")}</div>
                                </td>
                            </tr>
                            <tr>
                                <td style="padding: 30px 28px;">
                                    <p style="margin: 0 0 10px; color: #64748b; font-size: 14px;">{HtmlEncoder.Default.Encode(Localized("Hola", "Hi"))}{HtmlEncoder.Default.Encode(GetGreetingSuffix(user))},</p>
                                    <h1 style="margin: 0 0 12px; color: #0f172a; font-size: 26px; line-height: 1.2;">{HtmlEncoder.Default.Encode(title)}</h1>
                                    <p style="margin: 0; color: #334155; font-size: 16px; line-height: 1.65;">{HtmlEncoder.Default.Encode(intro)}</p>
                                    {mainContent}
                                    <div style="margin-top: 26px; padding: 16px; border-radius: 14px; background: #f8fafc; border: 1px solid #e2e8f0; color: #475569; font-size: 13px; line-height: 1.6;">
                                        <strong style="color: #0f172a;">{HtmlEncoder.Default.Encode(Localized("Nota de seguridad", "Security note"))}:</strong>
                                        {HtmlEncoder.Default.Encode(securityNote)}
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td style="padding: 18px 28px; background: #f8fafc; color: #64748b; font-size: 12px; line-height: 1.5;">
                                    {HtmlEncoder.Default.Encode(Localized("Mensaje automatico de VCDA-Financial-Manager. No respondas este correo.", "Automated message from VCDA-Financial-Manager. Please do not reply."))}
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;
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
        if (!string.IsNullOrWhiteSpace(user.Nickname))
        {
            return $" {user.Nickname}";
        }

        return string.IsNullOrWhiteSpace(user.UserName) ? string.Empty : $" {user.UserName}";
    }

    private static string Localized(string spanish, string english)
    {
        return GetCurrentLanguage() == "en" ? english : spanish;
    }

    private static string GetCurrentLanguage()
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase)
            ? "en"
            : "es";
    }

    private static string FormatTokenLifespan(TimeSpan lifespan)
    {
        if (GetCurrentLanguage() == "en")
        {
            return lifespan.TotalDays >= 1
                ? $"{Math.Round(lifespan.TotalDays):0} day(s)"
                : $"{Math.Round(lifespan.TotalHours):0} hour(s)";
        }

        return lifespan.TotalDays >= 1
            ? $"{Math.Round(lifespan.TotalDays):0} dia(s)"
            : $"{Math.Round(lifespan.TotalHours):0} hora(s)";
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
