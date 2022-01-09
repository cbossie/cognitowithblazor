using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace blazor1.Model;

public record AuthenticationOptions
{
    [JsonIgnore]
    public string? Host { get; set; }

    [JsonPropertyName("authority")]
    public string? Authority { get; init; }

    [JsonPropertyName("metaDataUrl")]
    public string? MetadataUrl { get; init; }

    [JsonPropertyName("clientId")] 
    public string? ClientId { get; init; }

     [JsonPropertyName("redirectUriTemplate")] 
    public string? RedirectUriTemplate { get; init; }

    [JsonPropertyName("postLogoutRedirectUriTemplate")] 
    public string? PostLogoutRedirectUriTemplate { get; init; }

    [JsonPropertyName("responseType")] 
    public string? ResponseType { get; set; }

    public string RedirectUri => string.Format(RedirectUriTemplate ?? "{0}", Host);
    public string PostLogoutRedirectUri => string.Format(PostLogoutRedirectUriTemplate ?? "{0}", Host);
}
