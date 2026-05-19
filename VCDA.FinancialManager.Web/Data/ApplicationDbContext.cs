using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VCDA.FinancialManager.Domain.Entities;

namespace VCDA.FinancialManager.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Cuenta> Cuentas => Set<Cuenta>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Transaccion> Transacciones => Set<Transaccion>();
    public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();

    public override int SaveChanges()
    {
        PreventTransactionDeletions();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        PreventTransactionDeletions();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void PreventTransactionDeletions()
    {
        var deletedTransactions = ChangeTracker.Entries<Transaccion>()
            .Where(e => e.State == EntityState.Deleted);

        if (deletedTransactions.Any())
        {
            throw new InvalidOperationException("Las transacciones financieras son inmutables y no se pueden eliminar. Deben ser anuladas o compensadas.");
        }
    }
}
