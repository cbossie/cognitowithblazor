using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using blazor1;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using blazor1.Model;
using Microsoft.Extensions.Http;
using blazor1.Service;
using blazor1.Util;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


// API Settings
ApiConfig api = new();
builder.Configuration.Bind("Api", api);
builder.Services.AddSingleton<IApiConfig>(api);

// Config Api
ConfigApi cfgApiConfig = new();
builder.Configuration.Bind("ConfigApi", cfgApiConfig);

// Get host
LocalSettings localSettings = new();
builder.Configuration.Bind("LocalSettings", localSettings);
builder.Services.AddSingleton(localSettings);

// OIDC Configuration 
//Cognito Settings from the API
AuthenticationOptions authOptions = await ConfigurationUtility.GetAuthenticationOptions(cfgApiConfig.Endpoint, localSettings.Host);
builder.Services.AddSingleton(authOptions);

builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.Authority = authOptions.Authority;
    options.ProviderOptions.ResponseType = authOptions.ResponseType;
    options.ProviderOptions.ClientId = authOptions.ClientId;
    options.ProviderOptions.MetadataUrl = authOptions.MetadataUrl;
    options.ProviderOptions.PostLogoutRedirectUri = authOptions.PostLogoutRedirectUri;  
    options.ProviderOptions.RedirectUri = authOptions.RedirectUri;
    options.UserOptions.NameClaim = "email";
});
builder.Services.AddSingleton<CredentialService>();

builder.Services.AddHttpClient("Api", httpClient =>
{
    httpClient.BaseAddress = new Uri(api.Url);
});

builder.Services.AddTransient<IWeatherApiService, WeatherApiService>();

var appHost = builder.Build();

await appHost.RunAsync();
