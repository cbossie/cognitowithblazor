namespace webapi.Model;

public class CognitoSettings
{
    public string Host { get; set; }





    public string? Authority { get; set; }
    public string? ClientId { get; set; }

    public string RedirectUriTemplate { get; set; } = string.Empty;

    public string PostLogoutRedirectUriTemplate { get; set; } = string.Empty;

    public string? ResponseType { get; set; }

    public string? MetaDataUrl { get; set; }

    public string? UserPoolId { get; set; }

    public string? PoolDomain { get; set; }

    public string? ValidIssuer { get; set; }

    public string? Jwks { get; set; }
}
