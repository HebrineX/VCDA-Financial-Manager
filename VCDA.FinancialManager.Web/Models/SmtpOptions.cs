using System.ComponentModel.DataAnnotations;

namespace VCDA.FinancialManager.Web.Models;

public class SmtpOptions
{
    public const string SectionName = "Smtp";

    [Required]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    [Required]
    [EmailAddress]
    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = "VCDA-Financial-Manager";

    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host)
        && Port is >= 1 and <= 65535
        && !string.IsNullOrWhiteSpace(FromEmail)
        && (string.IsNullOrWhiteSpace(Username) || !string.IsNullOrWhiteSpace(Password));

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Host))
        {
            throw new InvalidOperationException("SMTP no configurado: falta Smtp:Host.");
        }

        if (Port is < 1 or > 65535)
        {
            throw new InvalidOperationException("SMTP no configurado: Smtp:Port debe estar entre 1 y 65535.");
        }

        if (string.IsNullOrWhiteSpace(FromEmail))
        {
            throw new InvalidOperationException("SMTP no configurado: falta Smtp:FromEmail.");
        }

        if (!string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Password))
        {
            throw new InvalidOperationException("SMTP no configurado: falta Smtp:Password para el usuario configurado.");
        }
    }
}
