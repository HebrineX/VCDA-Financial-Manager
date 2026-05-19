using VCDA.FinancialManager.Domain.Enums;

namespace VCDA.FinancialManager.Web.Models;

public class CsvImportRow
{
    public int LineNumber { get; set; }
    public string RawLine { get; set; } = string.Empty;
    public DateTime? Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal? Monto { get; set; }
    public TipoTransaccion? Tipo { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public string CuentaNombre { get; set; } = string.Empty;
    public Guid? CategoriaId { get; set; }
    public Guid? CuentaId { get; set; }
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
