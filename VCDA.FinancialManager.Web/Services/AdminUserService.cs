using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VCDA.FinancialManager.Web.Data;
using VCDA.FinancialManager.Web.Models;

namespace VCDA.FinancialManager.Web.Services;

public class AdminUserService(UserManager<ApplicationUser> userManager)
{
    public async Task<List<UserAdminInfo>> GetUsersAsync()
    {
        var users = await userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        var result = new List<UserAdminInfo>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new UserAdminInfo
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? "",
                EmailConfirmed = user.EmailConfirmed,
                IsLockedOut = await userManager.IsLockedOutAsync(user),
                Roles = roles
            });
        }

        return result;
    }

    public async Task<bool> ToggleLockoutAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            var result = await userManager.SetLockoutEndDateAsync(user, null);
            return result.Succeeded;
        }

        await userManager.SetLockoutEnabledAsync(user, true);
        var lockResult = await userManager.SetLockoutEndDateAsync(
            user, DateTimeOffset.UtcNow.AddYears(100));
        return lockResult.Succeeded;
    }

    public async Task<(bool Success, string? Error)> SetUserAdminRoleAsync(
        string userId,
        bool makeAdmin,
        string? currentUserId = null)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return (false, "Usuario no encontrado.");
        }

        var isAdmin = await userManager.IsInRoleAsync(user, AppRoles.Admin);

        if (makeAdmin)
        {
            if (isAdmin)
            {
                return (true, null);
            }

            var addResult = await userManager.AddToRoleAsync(user, AppRoles.Admin);
            return addResult.Succeeded
                ? (true, null)
                : (false, string.Join(" ", addResult.Errors.Select(e => e.Description)));
        }

        if (!isAdmin)
        {
            return (true, null);
        }

        if (!string.IsNullOrEmpty(currentUserId) && user.Id == currentUserId)
        {
            return (false, "No puedes quitarte el rol de administrador a ti mismo.");
        }

        var admins = await userManager.GetUsersInRoleAsync(AppRoles.Admin);
        if (admins.Count <= 1)
        {
            return (false, "Debe existir al menos un administrador en el sistema.");
        }

        var removeResult = await userManager.RemoveFromRoleAsync(user, AppRoles.Admin);
        return removeResult.Succeeded
            ? (true, null)
            : (false, string.Join(" ", removeResult.Errors.Select(e => e.Description)));
    }
}
