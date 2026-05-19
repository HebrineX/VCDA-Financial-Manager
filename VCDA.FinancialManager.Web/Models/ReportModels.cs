using VCDA.FinancialManager.Domain.Entities;
using VCDA.FinancialManager.Web.Data;

namespace VCDA.FinancialManager.Web.Models;

public class ReportFilter
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public Guid? CategoriaId { get; set; }
    public Guid? CuentaId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ReportResult
{
    public List<Transaccion> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal BalanceNeto => TotalIngresos - TotalEgresos;
}

public class MonthlyBalancePoint
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Ingresos { get; set; }
    public decimal Egresos { get; set; }
    public decimal Balance { get; set; }
}

public class UserAdminInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    /// <summary>Cuenta bloqueada (LockoutEnd en el futuro).</summary>
    public bool IsLockedOut { get; set; }
    public IList<string> Roles { get; set; } = [];

    public bool IsAdmin => Roles.Contains(AppRoles.Admin);
}
