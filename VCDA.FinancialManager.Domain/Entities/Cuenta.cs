using System;

namespace VCDA.FinancialManager.Domain.Entities;

public class Cuenta
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
    public string Moneda { get; set; } = "ARS";
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
