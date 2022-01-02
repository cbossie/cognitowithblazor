using blazor1.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;


namespace blazor1.Service;

public class CredentialService
{
    private IJSRuntime Runtime { get; init; }
    private AuthenticationOptions AuthOptions { get; init; }
    private NavigationManager NavManager { get; init; }
    public bool IsAuthenticated { get; private set; }

    private string UserDataKey =>  $"oidc.user:{AuthOptions.Authority}:{AuthOptions.ClientId}";

    public CredentialService(IJSRuntime jsRuntime, AuthenticationOptions authOptions, NavigationManager navManager)
    {
        Runtime = jsRuntime;
        AuthOptions = authOptions;
        NavManager = navManager;
    }

    public async Task<CognitoUser?> GetCognitoCredentials()
    {
        var data = await Runtime.InvokeAsync<string>("sessionStorage.getItem", UserDataKey);
        if(data == null)
        {
            return null;
        }
        return JsonSerializer.Deserialize<CognitoUser>(data);
    }
}
