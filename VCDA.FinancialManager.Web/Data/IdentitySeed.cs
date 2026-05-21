using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VCDA.FinancialManager.Web.Components.Account;
using VCDA.FinancialManager.Web.Models;

namespace VCDA.FinancialManager.Web.Data;

public static class IdentitySeed
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeed");
        var configuration = services.GetRequiredService<IConfiguration>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var context = services.GetRequiredService<ApplicationDbContext>();
        var adminSeed = configuration.GetSection(AdminSeedOptions.SectionName).Get<AdminSeedOptions>() ?? new AdminSeedOptions();

        await context.Database.MigrateAsync();
        await EnsureNicknameColumnsAsync(context, logger);

        foreach (var role in new[] { AppRoles.Admin, AppRoles.User })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (!adminSeed.Enabled)
        {
            logger.LogInformation("IdentitySeed: bootstrap admin deshabilitado por configuración.");
            return;
        }

        if (!adminSeed.IsConfigured)
        {
            logger.LogWarning("IdentitySeed: AdminSeed incompleto. No se creará usuario administrador inicial.");
            return;
        }

        var normalizedAdminEmail = userManager.NormalizeEmail(adminSeed.Email);
        var adminCandidates = await context.Users
            .Where(user => user.NormalizedEmail == normalizedAdminEmail)
            .OrderByDescending(user => user.UserName == adminSeed.UserName)
            .ThenBy(user => user.UserName)
            .ThenBy(user => user.Id)
            .ToListAsync();

        if (adminCandidates.Count > 1)
        {
            logger.LogWarning(
                "IdentitySeed encontró {Count} usuarios con el email admin {MaskedEmail}. Se utilizará {MaskedUserId}.",
                adminCandidates.Count,
                SecurityLogSanitizer.MaskEmail(adminSeed.Email),
                SecurityLogSanitizer.MaskUserId(adminCandidates[0].Id));
        }

        var admin = adminCandidates.FirstOrDefault();
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminSeed.UserName,
                Email = adminSeed.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminSeed.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
                logger.LogInformation("IdentitySeed: usuario administrador inicial creado desde configuración.");
            }
            else
            {
                var errors = string.Join("; ", result.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"No se pudo crear el administrador inicial: {errors}");
            }
        }
        else if (!await userManager.IsInRoleAsync(admin, AppRoles.Admin))
        {
            await userManager.AddToRoleAsync(admin, AppRoles.Admin);
        }
    }

    private static async Task EnsureNicknameColumnsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (!await HasTableAsync(context, "AspNetUsers") || await HasColumnAsync(context, "AspNetUsers", "Nickname"))
        {
            return;
        }

        logger.LogWarning("IdentitySeed: reparando columnas de nickname faltantes en AspNetUsers.");

        await context.Database.ExecuteSqlRawAsync("""
            ALTER TABLE AspNetUsers ADD COLUMN Nickname TEXT NOT NULL DEFAULT '';
            """);

        await context.Database.ExecuteSqlRawAsync("""
            ALTER TABLE AspNetUsers ADD COLUMN NormalizedNickname TEXT NOT NULL DEFAULT '';
            """);

        await context.Database.ExecuteSqlRawAsync("""
            UPDATE AspNetUsers
            SET Nickname = substr(trim(coalesce(UserName, Email, Id)), 1, 23) || '-' || substr(Id, 1, 8),
                NormalizedNickname = upper(substr(trim(coalesce(UserName, Email, Id)), 1, 23) || '-' || substr(Id, 1, 8))
            WHERE Nickname = '' OR NormalizedNickname = '';
            """);

        await context.Database.ExecuteSqlRawAsync("""
            CREATE UNIQUE INDEX IF NOT EXISTS NicknameIndex ON AspNetUsers (NormalizedNickname);
            """);
    }

    private static async Task<bool> HasTableAsync(ApplicationDbContext context, string tableName)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $tableName";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "$tableName";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    private static async Task<bool> HasColumnAsync(ApplicationDbContext context, string tableName, string columnName)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info('{tableName.Replace("'", "''")}')";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
