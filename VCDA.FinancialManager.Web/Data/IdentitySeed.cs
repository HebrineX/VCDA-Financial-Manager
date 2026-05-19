using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace VCDA.FinancialManager.Web.Data;

public static class IdentitySeed
{
    public const string DefaultAdminEmail = "Biancolucasgerman@gmail.com";
    public const string DefaultAdminUserName = "HebrineX";
    private const string DefaultAdminPassword = "Admin123!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeed");
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        await context.Database.MigrateAsync();

        foreach (var role in new[] { AppRoles.Admin, AppRoles.User })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var normalizedAdminEmail = userManager.NormalizeEmail(DefaultAdminEmail);
        var adminCandidates = await context.Users
            .Where(user => user.NormalizedEmail == normalizedAdminEmail)
            .OrderByDescending(user => user.UserName == DefaultAdminUserName)
            .ThenBy(user => user.UserName)
            .ThenBy(user => user.Id)
            .ToListAsync();

        if (adminCandidates.Count > 1)
        {
            logger.LogWarning(
                "IdentitySeed encontró {Count} usuarios con el email admin {Email}. Se utilizará {UserId}.",
                adminCandidates.Count,
                DefaultAdminEmail,
                adminCandidates[0].Id);
        }

        var admin = adminCandidates.FirstOrDefault();
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = DefaultAdminUserName,
                Email = DefaultAdminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, DefaultAdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
            }
        }
        else if (!await userManager.IsInRoleAsync(admin, AppRoles.Admin))
        {
            await userManager.AddToRoleAsync(admin, AppRoles.Admin);
        }
    }
}
