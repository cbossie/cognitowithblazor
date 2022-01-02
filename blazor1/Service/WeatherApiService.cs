using blazor1.Model;
using System.Text.Json;

namespace blazor1.Service;

public class WeatherApiService : IWeatherApiService
{
    HttpClient Client { get; init; }

    public WeatherApiService(IHttpClientFactory httpfact)
    {
        Client = httpfact.CreateClient("Api");
         
    }

    public async Task<IEnumerable<WeatherForecast>?> GetWeatherForecast()
    {
        var response = (await Client.GetAsync($"/api/WeatherForecast/").ConfigureAwait(false)).EnsureSuccessStatusCode();
        var data = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        var returnVal = await JsonSerializer.DeserializeAsync<List<WeatherForecast>>(data);
        return returnVal;
    }
}
