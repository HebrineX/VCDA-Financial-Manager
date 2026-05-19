using System;
using VCDA.FinancialManager.Domain.Enums;

namespace VCDA.FinancialManager.Domain.Entities;

public class Categoria
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public TipoTransaccion Tipo { get; set; }
    public string UserId { get; set; } = string.Empty;
}
