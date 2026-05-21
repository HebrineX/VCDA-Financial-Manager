using System;
using VCDA.FinancialManager.Domain.Enums;

namespace VCDA.FinancialManager.Domain.Entities;

public class Transaccion
{
    public Guid Id { get; set; }
    public Guid CuentaId { get; set; }
    public Cuenta? Cuenta { get; set; }
    public Guid CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    public decimal Monto { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public TipoTransaccion Tipo { get; set; }
    public bool IsImported { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
}
