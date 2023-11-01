using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using System.Text.Json;

namespace SmartHomeWWW.Client.HttpClients;

public class WeatherHttpClient(ILogger<WeatherHttpClient> logger, HttpClient httpClient)
{
    private readonly ILogger<WeatherHttpClient> _logger = logger;
    private readonly HttpClient _httpClient = httpClient;
    private readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<WeatherReport?> GetCurrent(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/weather/current", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("No current weather");
            return null;
        }

        var weather = await JsonSerializer.DeserializeAsync<WeatherReport?>(await response.Content.ReadAsStreamAsync(cancellationToken), _serializerOptions, cancellationToken);
        if (weather is null)
        {
            _logger.LogWarning("Could not parse weather");
            _logger.LogDebug("Response content: {Content}", (string?)await response.Content.ReadAsStringAsync(cancellationToken));
            return null;
        }

        _logger.LogDebug("Current weather: {Weather}", weather);

        return weather;
    }
}
