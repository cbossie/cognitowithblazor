using blazor1.Model;

namespace blazor1.Service;
public interface IWeatherApiService
{
    Task<IEnumerable<WeatherForecast>?> GetWeatherForecast();
}
