namespace webapi.Model;

public class CognitoSettings
{
    public string? Authority { get; set; }
    public string? ClientId { get; set; }

    public string? JwtKeysetUrl { get; set; }
}
