using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace VCDA.FinancialManager.Web.Services;

internal sealed class PublicLinkService(IConfiguration configuration, NavigationManager navigationManager, IWebHostEnvironment environment)
{
    public string Build(string relativePath, IDictionary<string, object?> queryParameters)
    {
        var baseUrl = configuration["App:PublicBaseUrl"];
        var absoluteBaseUrl = string.IsNullOrWhiteSpace(baseUrl)
            ? navigationManager.BaseUri
            : EnsureTrailingSlash(baseUrl);

        if (environment.IsProduction()
            && (!Uri.TryCreate(absoluteBaseUrl, UriKind.Absolute, out var configuredUri)
                || configuredUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("App:PublicBaseUrl debe ser una URL absoluta HTTPS en Production.");
        }

        var path = relativePath.TrimStart('/');
        var absoluteUrl = new Uri(new Uri(absoluteBaseUrl), path).AbsoluteUri;
        return QueryHelpers.AddQueryString(
            absoluteUrl,
            queryParameters
                .Where(parameter => parameter.Value is not null)
                .ToDictionary(
                    parameter => parameter.Key,
                    parameter => parameter.Value?.ToString()));
    }

    private static string EnsureTrailingSlash(string value) =>
        value.EndsWith('/') ? value : $"{value}/";
}
