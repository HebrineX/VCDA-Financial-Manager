using System;

namespace VCDA.FinancialManager.Domain.Entities;

public class Presupuesto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    public int Mes { get; set; }
    public int Anio { get; set; }
    public decimal Limite { get; set; }
}
