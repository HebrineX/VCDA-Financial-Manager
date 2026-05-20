using VCDA.FinancialManager.Web.Data;

namespace VCDA.FinancialManager.Web.Models;

public class AdminUserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    /// <summary>Cuenta desactivada por lockout administrativo.</summary>
    public bool IsDeactivated { get; set; }
    public IList<string> Roles { get; set; } = [];

    public bool IsAdmin => Roles.Contains(AppRoles.Admin);
}
