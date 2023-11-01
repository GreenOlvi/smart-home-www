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

    public IReadOnlyCollection<MinutelyWeather> Minutely { get; init; } = [];

    public IReadOnlyCollection<HourlyWeather> Hourly { get; init; } = [];

    public IReadOnlyCollection<DailyWeather> Daily { get; init; } = [];

    public IReadOnlyCollection<WeatherAlert> Alerts { get; init; } = [];
}
