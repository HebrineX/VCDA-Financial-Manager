using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace VCDA.FinancialManager.Web.Services;

internal sealed class PublicLinkService(IConfiguration configuration, NavigationManager navigationManager)
{
    public string Build(string relativePath, IDictionary<string, object?> queryParameters)
    {
        var baseUrl = configuration["App:PublicBaseUrl"];
        var absoluteBaseUrl = string.IsNullOrWhiteSpace(baseUrl)
            ? navigationManager.BaseUri
            : EnsureTrailingSlash(baseUrl);

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
