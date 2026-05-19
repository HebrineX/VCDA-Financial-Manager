using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
                "IdentitySeed encontró {Count} usuarios con el email admin {Email}. Se utilizará {UserId}.",
                adminCandidates.Count,
                adminSeed.Email,
                adminCandidates[0].Id);
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
}
