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

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Local", options.ProviderOptions);
});

ApiConfig api = new();
builder.Configuration.Bind("Api", api);
builder.Services.AddSingleton<IApiConfig>(api);

builder.Services.AddHttpClient("api", httpClient =>
{
    httpClient.BaseAddress = new Uri(api.Url);
});

builder.Services.AddTransient<IWeatherApiService>();


await builder.Build().RunAsync();
