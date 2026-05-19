using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VCDA.FinancialManager.Domain.Entities;
using VCDA.FinancialManager.Domain.Enums;
using VCDA.FinancialManager.Web.Data;
using VCDA.FinancialManager.Web.Services;
using Xunit;

namespace VCDA.FinancialManager.Domain.Tests;

public class FinancialDomainTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _contextOptions;

    public FinancialDomainTests()
    {
        // Crear una conexión SQLite en memoria y abrirla para mantener viva la base de datos de pruebas
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Asegurar que el esquema se cree correctamente
        using var context = new ApplicationDbContext(_contextOptions);
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    [Fact]
    public async Task CreateTransaccionAsync_Ingreso_ShouldIncreaseAccountBalance()
    {
        // Arrange
        using var context = new ApplicationDbContext(_contextOptions);
        var service = new FinancialService(context);
        var userId = "test-user-id";

        var cuenta = await service.CreateCuentaAsync("Efectivo", 1000m, "ARS", userId);
        var categoria = await service.CreateCategoriaAsync("Sueldo", "Pago de nómina", TipoTransaccion.Ingreso, userId);

        // Act
        var transaccion = await service.CreateTransaccionAsync(
            cuenta.Id,
            categoria.Id,
            500m,
            "Cobro sueldo",
            DateTime.UtcNow,
            TipoTransaccion.Ingreso,
            userId
        );

        // Assert
        var cuentaActualizada = await context.Cuentas.FirstAsync(c => c.Id == cuenta.Id);
        Assert.Equal(1500m, cuentaActualizada.Saldo);
        Assert.Equal(transaccion.Monto, 500m);
    }

    [Fact]
    public async Task CreateTransaccionAsync_Egreso_ShouldDecreaseAccountBalance()
    {
        // Arrange
        using var context = new ApplicationDbContext(_contextOptions);
        var service = new FinancialService(context);
        var userId = "test-user-id";

        var cuenta = await service.CreateCuentaAsync("Efectivo", 1000m, "ARS", userId);
        var categoria = await service.CreateCategoriaAsync("Comida", "Almuerzo", TipoTransaccion.Egreso, userId);

        // Act
        await service.CreateTransaccionAsync(
            cuenta.Id,
            categoria.Id,
            200m,
            "Almuerzo",
            DateTime.UtcNow,
            TipoTransaccion.Egreso,
            userId
        );

        // Assert
        var cuentaActualizada = await context.Cuentas.FirstAsync(c => c.Id == cuenta.Id);
        Assert.Equal(800m, cuentaActualizada.Saldo);
    }

    [Fact]
    public async Task DeleteTransaccion_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var context = new ApplicationDbContext(_contextOptions);
        var service = new FinancialService(context);
        var userId = "test-user-id";

        var cuenta = await service.CreateCuentaAsync("Efectivo", 1000m, "ARS", userId);
        var categoria = await service.CreateCategoriaAsync("Comida", "Almuerzo", TipoTransaccion.Egreso, userId);
        var transaccion = await service.CreateTransaccionAsync(
            cuenta.Id,
            categoria.Id,
            100m,
            "Gasto",
            DateTime.UtcNow,
            TipoTransaccion.Egreso,
            userId
        );

        // Act & Assert
        context.Transacciones.Remove(transaccion);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("inmutables", exception.Message);
    }

    [Fact]
    public async Task GetCuentasAsync_ShouldNotReturnOtherUsersData()
    {
        using var context = new ApplicationDbContext(_contextOptions);
        var service = new FinancialService(context);

        await service.CreateCuentaAsync("Cuenta A", 100m, "ARS", "user-a");
        await service.CreateCuentaAsync("Cuenta B", 200m, "ARS", "user-b");

        var cuentasA = await service.GetCuentasAsync("user-a");
        var cuentasB = await service.GetCuentasAsync("user-b");

        Assert.Single(cuentasA);
        Assert.Equal("Cuenta A", cuentasA[0].Nombre);
        Assert.Single(cuentasB);
        Assert.Equal("Cuenta B", cuentasB[0].Nombre);
    }

    [Fact]
    public async Task GetBudgetProgressAsync_ShouldFlagWarningAndDanger()
    {
        using var context = new ApplicationDbContext(_contextOptions);
        var service = new FinancialService(context);
        var userId = "budget-user";
        var now = DateTime.UtcNow;

        var cuenta = await service.CreateCuentaAsync("Principal", 10000m, "ARS", userId);
        var catComida = await service.CreateCategoriaAsync("Comida", "", TipoTransaccion.Egreso, userId);
        var catTransporte = await service.CreateCategoriaAsync("Transporte", "", TipoTransaccion.Egreso, userId);

        await service.CreatePresupuestoAsync(catComida.Id, now.Month, now.Year, 100m, userId);
        await service.CreatePresupuestoAsync(catTransporte.Id, now.Month, now.Year, 100m, userId);

        await service.CreateTransaccionAsync(cuenta.Id, catComida.Id, 85m, "Almuerzo", now, TipoTransaccion.Egreso, userId);
        await service.CreateTransaccionAsync(cuenta.Id, catTransporte.Id, 110m, "Taxi", now, TipoTransaccion.Egreso, userId);

        var progress = await service.GetBudgetProgressAsync(userId, now.Month, now.Year);

        Assert.Equal(2, progress.Count);
        Assert.Contains(progress, p => p.CategoriaNombre == "Comida" && p.AlertLevel == Web.Models.BudgetAlertLevel.Warning);
        Assert.Contains(progress, p => p.CategoriaNombre == "Transporte" && p.AlertLevel == Web.Models.BudgetAlertLevel.Danger);
    }
}
