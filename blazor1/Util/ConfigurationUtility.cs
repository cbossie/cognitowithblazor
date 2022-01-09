using blazor1.Model;

namespace blazor1.Util;

public static class ConfigurationUtility
{
    /// <summary>
    /// Gets the config information from the API asynchronously.
    /// </summary>
    public static async Task<AuthenticationOptions> GetAuthenticationOptions(string configEndpoint, string? host)
    {
        using var client = new HttpClient();
        var result = await client.GetAsync(configEndpoint).ConfigureAwait(false);
        var resultData = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        var authOptions = JsonSerializer.Deserialize<AuthenticationOptions>(resultData) ?? throw new ArgumentNullException(nameof(AuthenticationOptions));
        authOptions.Host = host;
        return authOptions;
    }

}
