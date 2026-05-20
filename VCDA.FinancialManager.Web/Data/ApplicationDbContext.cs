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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.Nickname)
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(user => user.NormalizedNickname)
                .HasMaxLength(32)
                .IsRequired();

            entity.HasIndex(user => user.NormalizedNickname)
                .IsUnique()
                .HasDatabaseName("NicknameIndex");
        });
    }

    public override int SaveChanges()
    {
        NormalizeNicknames();
        PreventTransactionDeletions();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        NormalizeNicknames();
        PreventTransactionDeletions();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void NormalizeNicknames()
    {
        foreach (var entry in ChangeTracker.Entries<ApplicationUser>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            var nickname = string.IsNullOrWhiteSpace(entry.Entity.Nickname)
                ? entry.Entity.UserName ?? entry.Entity.Email ?? entry.Entity.Id
                : entry.Entity.Nickname;

            entry.Entity.Nickname = nickname.Trim();
            entry.Entity.NormalizedNickname = entry.Entity.Nickname.ToUpperInvariant();
        }
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
