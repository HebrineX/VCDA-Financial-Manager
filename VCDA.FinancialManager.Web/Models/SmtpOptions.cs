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
}
