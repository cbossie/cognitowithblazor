namespace webapi.Model;

public class CognitoSettings
{
    public string? Authority { get; set; }
    public string? ClientId { get; set; }

    public string? UserPoolId { get; set; }

    public string? PoolDomain { get; set; }
}
