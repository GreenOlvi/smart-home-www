using System.Text.Json.Serialization;

namespace SmartHomeWWW.Core.Domain.OpenWeatherMaps;

public readonly record struct WeatherReport
{
    public WeatherReport()
    {
    }

    [JsonPropertyName("lat")]
    public float Latitude { get; init; }

    [JsonPropertyName("lon")]
    public float Longitude { get; init; }

    public string Timezone { get; init; } = string.Empty;

    [JsonPropertyName("timezone_offset")]
    public long TimezoneOffset { get; init; }

    public CurrentWeather Current { get; init; }

    public IReadOnlyCollection<MinutelyWeather> Minutely { get; init; } = Array.Empty<MinutelyWeather>();

    public IReadOnlyCollection<HourlyWeather> Hourly { get; init; } = Array.Empty<HourlyWeather>();

    public IReadOnlyCollection<DailyWeather> Daily { get; init; } = Array.Empty<DailyWeather>();

    public IReadOnlyCollection<WeatherAlert> Alerts { get; init; } = Array.Empty<WeatherAlert>();
}
