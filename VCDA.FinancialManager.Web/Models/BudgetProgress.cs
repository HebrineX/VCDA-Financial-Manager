namespace VCDA.FinancialManager.Web.Models;

public enum BudgetAlertLevel
{
    Normal,
    Warning,
    Danger
}

public class BudgetProgress
{
    public Guid PresupuestoId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public decimal Limite { get; set; }
    public decimal Gastado { get; set; }
    public double Porcentaje { get; set; }
    public BudgetAlertLevel AlertLevel { get; set; }
}
