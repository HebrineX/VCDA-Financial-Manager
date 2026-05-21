namespace VCDA.FinancialManager.Web.Components.Account;

internal static class SecurityLogSanitizer
{
    public static string MaskUserId(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return "anonymous";
        }

        var trimmed = userId.Trim();
        return trimmed.Length <= 8
            ? $"{trimmed[..1]}***"
            : $"{trimmed[..4]}...{trimmed[^4..]}";
    }

    public static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "***";
        }

        var trimmed = email.Trim();
        var atIndex = trimmed.IndexOf('@');
        if (atIndex <= 0 || atIndex == trimmed.Length - 1)
        {
            return "***";
        }

        var localPart = trimmed[..atIndex];
        var domain = trimmed[(atIndex + 1)..];
        var visibleLocal = localPart.Length <= 2 ? localPart[..1] : localPart[..2];
        return $"{visibleLocal}***@{MaskHost(domain)}";
    }

    public static string MaskLoginIdentifier(string? identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return "***";
        }

        var trimmed = identifier.Trim();
        return trimmed.Contains('@', StringComparison.Ordinal)
            ? MaskEmail(trimmed)
            : MaskUserId(trimmed);
    }

    public static string MaskHost(string? host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return "unknown";
        }

        var trimmed = host.Trim();
        var dotIndex = trimmed.IndexOf('.');
        if (dotIndex <= 0 || dotIndex == trimmed.Length - 1)
        {
            return trimmed.Length <= 4 ? "***" : $"{trimmed[..2]}***";
        }

        return $"***{trimmed[dotIndex..]}";
    }
}
