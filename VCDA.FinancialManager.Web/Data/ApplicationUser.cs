using Microsoft.AspNetCore.Identity;

namespace VCDA.FinancialManager.Web.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string Nickname { get; set; } = string.Empty;

    public string NormalizedNickname { get; set; } = string.Empty;
}

