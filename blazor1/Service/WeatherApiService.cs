using blazor1.Model;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace blazor1.Service;

public class WeatherApiService : IWeatherApiService
{
    HttpClient Client { get; init; }
    CredentialService CredService { get; init; }
    CognitoUser CogUser { get; set; }
    

    public WeatherApiService(IHttpClientFactory httpfact, CredentialService credSvc)
    {
        Client = httpfact.CreateClient("Api");              
        CredService = credSvc;      
    }

    private async Task UpdateHttpClient()
    {
        var cognitoData = await CredService.GetCognitoCredentials();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cognitoData.AccessToken);
    }


    public async Task<IEnumerable<WeatherForecast>?> GetWeatherForecast()
    {
        await UpdateHttpClient();
        var resp = (await Client.GetAsync($"/WeatherForecast").ConfigureAwait(false)).EnsureSuccessStatusCode();
        var data = await resp.Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>();
        return data;
    }
}
