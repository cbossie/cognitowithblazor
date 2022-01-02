using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using blazor1;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using blazor1.Model;
using Microsoft.Extensions.Http;
using blazor1.Service;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// OIDC Configuration 
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Local", options.ProviderOptions);
});
builder.Services.AddSingleton<CredentialService>();

// API Settings
ApiConfig api = new();
builder.Configuration.Bind("Api", api);
builder.Services.AddSingleton<IApiConfig>(api);

//Cognito Settings
AuthenticationOptions authOptions = new();
builder.Configuration.Bind("Local", authOptions);
builder.Services.AddSingleton(authOptions);



builder.Services.AddHttpClient("Api", httpClient =>
{
    httpClient.BaseAddress = new Uri(api.Url);
});



builder.Services.AddTransient<IWeatherApiService, WeatherApiService>();




await builder.Build().RunAsync();
