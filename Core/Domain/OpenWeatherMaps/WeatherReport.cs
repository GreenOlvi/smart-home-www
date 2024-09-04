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

    [JsonPropertyName("timezone")]
    public string Timezone { get; init; } = string.Empty;

    [JsonPropertyName("timezone_offset")]
    public long TimezoneOffset { get; init; }

    [JsonPropertyName("current")]
    public CurrentWeather Current { get; init; }

    [JsonPropertyName("minutely")]
    public IReadOnlyCollection<MinutelyWeather> Minutely { get; init; } = [];

    [JsonPropertyName("hourly")]
    public IReadOnlyCollection<HourlyWeather> Hourly { get; init; } = [];

    [JsonPropertyName("daily")]
    public IReadOnlyCollection<DailyWeather> Daily { get; init; } = [];

    [JsonPropertyName("alerts")]
    public IReadOnlyCollection<WeatherAlert> Alerts { get; init; } = [];
}
