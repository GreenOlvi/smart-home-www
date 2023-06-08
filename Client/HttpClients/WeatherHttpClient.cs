using System.Text.Json;
using SmartHomeWWW.Core.Domain.OpenWeatherMaps;

namespace SmartHomeWWW.Client.HttpClients;

public class WeatherHttpClient
{
    private readonly ILogger<WeatherHttpClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true };

    public WeatherHttpClient(ILogger<WeatherHttpClient> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

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
