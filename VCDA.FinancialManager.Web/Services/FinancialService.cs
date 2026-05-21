using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VCDA.FinancialManager.Domain.Entities;
using VCDA.FinancialManager.Domain.Enums;
using VCDA.FinancialManager.Web.Data;
using VCDA.FinancialManager.Web.Models;

namespace VCDA.FinancialManager.Web.Services;

public class FinancialService(ApplicationDbContext context)
{
    public async Task<List<Cuenta>> GetCuentasAsync(string userId)
    {
        return await context.Cuentas
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Cuenta> CreateCuentaAsync(string nombre, decimal saldoInicial, string moneda, string userId)
    {
        var cuenta = new Cuenta
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Saldo = saldoInicial,
            Moneda = moneda,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        context.Cuentas.Add(cuenta);
        await context.SaveChangesAsync();
        return cuenta;
    }

    public async Task<Cuenta> UpdateCuentaAsync(Guid cuentaId, string nombre, string moneda, string userId)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre de la cuenta es requerido.");
        }

        var cuenta = await context.Cuentas.FirstOrDefaultAsync(c => c.Id == cuentaId && c.UserId == userId)
            ?? throw new KeyNotFoundException("La cuenta especificada no existe o no pertenece al usuario.");

        cuenta.Nombre = nombre.Trim();
        cuenta.Moneda = moneda;

        await context.SaveChangesAsync();
        return cuenta;
    }

    public async Task DeleteCuentaAsync(Guid cuentaId, string userId)
    {
        var cuenta = await context.Cuentas.FirstOrDefaultAsync(c => c.Id == cuentaId && c.UserId == userId)
            ?? throw new KeyNotFoundException("La cuenta especificada no existe o no pertenece al usuario.");

        var hasTransactions = await context.Transacciones.AnyAsync(t => t.CuentaId == cuentaId && t.UserId == userId);
        if (hasTransactions)
        {
            throw new InvalidOperationException("No podés eliminar una cuenta que ya tiene movimientos asociados.");
        }

        context.Cuentas.Remove(cuenta);
        await context.SaveChangesAsync();
    }

    public async Task<List<Categoria>> GetCategoriasAsync(string userId)
    {
        // Devuelve tanto categorías globales (UserId vacío) como personalizadas del usuario
        return await context.Categorias
            .AsNoTracking()
            .Where(c => c.UserId == userId || c.UserId == string.Empty)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<Categoria> CreateCategoriaAsync(string nombre, string descripcion, TipoTransaccion tipo, string userId)
    {
        var categoria = new Categoria
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Descripcion = descripcion,
            Tipo = tipo,
            UserId = userId
        };

        context.Categorias.Add(categoria);
        await context.SaveChangesAsync();
        return categoria;
    }

    public async Task EnsureDefaultCategoriasAsync(string userId)
    {
        var existingCategories = await context.Categorias
            .AsNoTracking()
            .Where(c => c.UserId == userId || c.UserId == string.Empty)
            .AnyAsync();

        if (existingCategories)
        {
            return;
        }

        var categoriasBase = new (string Nombre, string Descripcion, TipoTransaccion Tipo)[]
        {
            ("Sueldo", "Ingresos laborales", TipoTransaccion.Ingreso),
            ("Ventas", "Ventas de productos o servicios", TipoTransaccion.Ingreso),
            ("Alimentos", "Supermercados y comida", TipoTransaccion.Egreso),
            ("Transporte", "Combustible o boletos", TipoTransaccion.Egreso),
            ("Servicios", "Agua, luz, gas e internet", TipoTransaccion.Egreso),
            ("Ocio", "Salidas y entretenimiento", TipoTransaccion.Egreso)
        };

        foreach (var categoriaBase in categoriasBase)
        {
            context.Categorias.Add(new Categoria
            {
                Id = Guid.NewGuid(),
                Nombre = categoriaBase.Nombre,
                Descripcion = categoriaBase.Descripcion,
                Tipo = categoriaBase.Tipo,
                UserId = userId
            });
        }

        await context.SaveChangesAsync();
    }

    public async Task<List<Transaccion>> GetTransaccionesAsync(string userId)
    {
        return await BuildTransactionsQuery(userId)
            .OrderByDescending(t => t.Fecha)
            .ToListAsync();
    }

    public async Task<Transaccion> CreateTransaccionAsync(Guid cuentaId, Guid categoriaId, decimal monto, string descripcion, DateTime fecha, TipoTransaccion tipo, string userId)
    {
        using var dbTransaction = await context.Database.BeginTransactionAsync();
        try
        {
            var transaccion = await CreateTransaccionCoreAsync(cuentaId, categoriaId, monto, descripcion, fecha, tipo, userId);
            await dbTransaction.CommitAsync();
            return transaccion;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    private async Task<Transaccion> CreateTransaccionCoreAsync(Guid cuentaId, Guid categoriaId, decimal monto, string descripcion, DateTime fecha, TipoTransaccion tipo, string userId)
    {
        if (monto <= 0)
        {
            throw new ArgumentException("El monto de la transacción debe ser mayor a cero.");
        }

        var cuenta = await context.Cuentas.FirstOrDefaultAsync(c => c.Id == cuentaId && c.UserId == userId)
            ?? throw new KeyNotFoundException("La cuenta especificada no existe o no pertenece al usuario.");

        var categoria = await context.Categorias.FirstOrDefaultAsync(c => c.Id == categoriaId && (c.UserId == userId || c.UserId == string.Empty))
            ?? throw new KeyNotFoundException("La categoría especificada no existe.");

        var transaccion = new Transaccion
        {
            Id = Guid.NewGuid(),
            CuentaId = cuentaId,
            CategoriaId = categoriaId,
            Monto = monto,
            Descripcion = descripcion,
            Fecha = fecha,
            Tipo = tipo,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        if (tipo == TipoTransaccion.Ingreso)
        {
            cuenta.Saldo += monto;
        }
        else if (tipo == TipoTransaccion.Egreso)
        {
            cuenta.Saldo -= monto;
        }

        context.Transacciones.Add(transaccion);
        await context.SaveChangesAsync();
        return transaccion;
    }

    public async Task<List<Presupuesto>> GetPresupuestosAsync(string userId, int? mes = null, int? anio = null)
    {
        var query = context.Presupuestos
            .Include(p => p.Categoria)
            .Where(p => p.UserId == userId);

        if (mes.HasValue)
        {
            query = query.Where(p => p.Mes == mes.Value);
        }

        if (anio.HasValue)
        {
            query = query.Where(p => p.Anio == anio.Value);
        }

        return await query.OrderBy(p => p.Categoria!.Nombre).ToListAsync();
    }

    public async Task<Presupuesto> CreatePresupuestoAsync(Guid categoriaId, int mes, int anio, decimal limite, string userId)
    {
        if (limite <= 0)
        {
            throw new ArgumentException("El límite del presupuesto debe ser mayor a cero.");
        }

        var categoria = await context.Categorias.FirstOrDefaultAsync(c =>
            c.Id == categoriaId &&
            c.Tipo == TipoTransaccion.Egreso &&
            (c.UserId == userId || c.UserId == string.Empty))
            ?? throw new KeyNotFoundException("La categoría de egreso no existe.");

        var exists = await context.Presupuestos.AnyAsync(p =>
            p.UserId == userId && p.CategoriaId == categoriaId && p.Mes == mes && p.Anio == anio);

        if (exists)
        {
            throw new InvalidOperationException("Ya existe un presupuesto para esa categoría en el período seleccionado.");
        }

        var presupuesto = new Presupuesto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoriaId = categoriaId,
            Mes = mes,
            Anio = anio,
            Limite = limite
        };

        context.Presupuestos.Add(presupuesto);
        await context.SaveChangesAsync();
        presupuesto.Categoria = categoria;
        return presupuesto;
    }

    public async Task DeletePresupuestoAsync(Guid presupuestoId, string userId)
    {
        var presupuesto = await context.Presupuestos.FirstOrDefaultAsync(p => p.Id == presupuestoId && p.UserId == userId)
            ?? throw new KeyNotFoundException("Presupuesto no encontrado.");

        context.Presupuestos.Remove(presupuesto);
        await context.SaveChangesAsync();
    }

    public async Task<List<BudgetProgress>> GetBudgetProgressAsync(string userId, int mes, int anio)
    {
        var presupuestos = await GetPresupuestosAsync(userId, mes, anio);
        if (presupuestos.Count == 0)
        {
            return [];
        }

        var start = new DateTime(anio, mes, 1);
        var end = start.AddMonths(1);

        var gastosPorCategoria = await context.Transacciones
            .Where(t => t.UserId == userId && t.Tipo == TipoTransaccion.Egreso && t.Fecha >= start && t.Fecha < end)
            .GroupBy(t => t.CategoriaId)
            .Select(g => new { CategoriaId = g.Key, Total = g.Sum(t => t.Monto) })
            .ToDictionaryAsync(x => x.CategoriaId, x => x.Total);

        return presupuestos.Select(p =>
        {
            gastosPorCategoria.TryGetValue(p.CategoriaId, out var gastado);
            var porcentaje = p.Limite > 0 ? (double)(gastado / p.Limite * 100) : 0;
            var alert = porcentaje > 100
                ? BudgetAlertLevel.Danger
                : porcentaje > 80
                    ? BudgetAlertLevel.Warning
                    : BudgetAlertLevel.Normal;

            return new BudgetProgress
            {
                PresupuestoId = p.Id,
                CategoriaNombre = p.Categoria?.Nombre ?? "Sin categoría",
                Limite = p.Limite,
                Gastado = gastado,
                Porcentaje = Math.Min(porcentaje, 999),
                AlertLevel = alert
            };
        }).ToList();
    }

    public async Task<ReportResult> GetReportAsync(string userId, ReportFilter filter)
    {
        var query = BuildTransactionsQuery(userId);

        if (filter.Desde.HasValue)
        {
            query = query.Where(t => t.Fecha >= filter.Desde.Value);
        }

        if (filter.Hasta.HasValue)
        {
            var hastaFin = filter.Hasta.Value.Date.AddDays(1);
            query = query.Where(t => t.Fecha < hastaFin);
        }

        if (filter.CategoriaId.HasValue)
        {
            query = query.Where(t => t.CategoriaId == filter.CategoriaId.Value);
        }

        if (filter.CuentaId.HasValue)
        {
            query = query.Where(t => t.CuentaId == filter.CuentaId.Value);
        }

        var totalCount = await query.CountAsync();
        var totalIngresos = await query
            .Where(t => t.Tipo == TipoTransaccion.Ingreso)
            .SumAsync(t => (decimal?)t.Monto) ?? 0;
        var totalEgresos = await query
            .Where(t => t.Tipo == TipoTransaccion.Egreso)
            .SumAsync(t => (decimal?)t.Monto) ?? 0;

        var items = await query
            .OrderByDescending(t => t.Fecha)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new ReportResult
        {
            Items = items,
            TotalCount = totalCount,
            TotalIngresos = totalIngresos,
            TotalEgresos = totalEgresos
        };
    }

    public async Task<List<Transaccion>> GetTransaccionesForExportAsync(string userId, ReportFilter filter)
    {
        filter.Page = 1;
        filter.PageSize = int.MaxValue;
        var report = await GetReportAsync(userId, filter);
        return report.Items;
    }

    public async Task<List<MonthlyBalancePoint>> GetMonthlyBalanceHistoryAsync(string userId, int months = 6)
    {
        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1).AddMonths(-(months - 1));

        var transactions = await context.Transacciones
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.Fecha >= start)
            .ToListAsync();

        var points = new List<MonthlyBalancePoint>();
        for (var i = 0; i < months; i++)
        {
            var monthStart = start.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            var monthTx = transactions.Where(t => t.Fecha >= monthStart && t.Fecha < monthEnd).ToList();
            var ingresos = monthTx.Where(t => t.Tipo == TipoTransaccion.Ingreso).Sum(t => t.Monto);
            var egresos = monthTx.Where(t => t.Tipo == TipoTransaccion.Egreso).Sum(t => t.Monto);

            points.Add(new MonthlyBalancePoint
            {
                Mes = monthStart.Month,
                Anio = monthStart.Year,
                Label = monthStart.ToString("MMM yy", System.Globalization.CultureInfo.GetCultureInfo("es-AR")),
                Ingresos = ingresos,
                Egresos = egresos,
                Balance = ingresos - egresos
            });
        }

        return points;
    }

    public async Task<int> ImportTransaccionesBatchAsync(IEnumerable<CsvImportRow> validRows, string userId)
    {
        var rows = validRows.Where(r => r.IsValid).ToList();
        if (rows.Count == 0)
        {
            return 0;
        }

        using var dbTransaction = await context.Database.BeginTransactionAsync();
        try
        {
            foreach (var row in rows)
            {
                await CreateTransaccionCoreAsync(
                    row.CuentaId!.Value,
                    row.CategoriaId!.Value,
                    row.Monto!.Value,
                    row.Descripcion,
                    row.Fecha!.Value,
                    row.Tipo!.Value,
                    userId);
            }

            await dbTransaction.CommitAsync();
            return rows.Count;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    private IQueryable<Transaccion> BuildTransactionsQuery(string userId)
    {
        return context.Transacciones
            .AsNoTracking()
            .Include(t => t.Cuenta)
            .Include(t => t.Categoria)
            .Where(t => t.UserId == userId);
    }
}
