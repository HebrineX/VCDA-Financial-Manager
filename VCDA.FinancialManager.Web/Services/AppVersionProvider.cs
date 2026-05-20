using System.Reflection;

namespace VCDA.FinancialManager.Web.Services;

public sealed class AppVersionProvider(IConfiguration configuration)
{
    public string Version { get; } = ResolveVersion(configuration);

    private static string ResolveVersion(IConfiguration configuration)
    {
        var configuredVersion = configuration["App:Version"];
        if (!string.IsNullOrWhiteSpace(configuredVersion))
        {
            return configuredVersion;
        }

        var assembly = typeof(AppVersionProvider).Assembly;
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            return informationalVersion.Split('+', 2)[0];
        }

        return assembly.GetName().Version?.ToString() ?? "dev";
    }
}
