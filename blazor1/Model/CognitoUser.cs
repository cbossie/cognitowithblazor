namespace blazor1.Model;


public class CognitoUser
{

    [JsonPropertyName("id_token")]
    public string IdToken { get; set; }
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
    [JsonPropertyName("scope")]
    public string Scope { get; set; }
    [JsonPropertyName("expires_at")]
    public int ExpiresAt { get; set; }
}
