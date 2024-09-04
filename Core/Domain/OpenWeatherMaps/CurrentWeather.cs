using SmartHomeWWW.Core.Utils;
using System.Text.Json.Serialization;

namespace SmartHomeWWW.Core.Domain.OpenWeatherMaps;

public readonly record struct CurrentWeather
{
    public CurrentWeather()
    {
    }

    [JsonPropertyName("dt")]
    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    public DateTime Timestamp { get; init; } = DateTime.UnixEpoch;

    [JsonPropertyName("sunrise")]
    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    public DateTime Sunrise { get; init; } = DateTime.UnixEpoch;

    [JsonPropertyName("sunset")]
    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    public DateTime Sunset { get; init; } = DateTime.UnixEpoch;

    [JsonPropertyName("temp")]
    public float Temperature { get; init; }

    [JsonPropertyName("feels_like")]
    public float FeelsLike { get; init; }

    [JsonPropertyName("pressure")]
    public int Pressure { get; init; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; init; }

    [JsonPropertyName("dew_point")]
    public float DewPoint { get; init; }

    [JsonPropertyName("uvi")]
    public float Uvi { get; init; }

    [JsonPropertyName("clouds")]
    public int Clouds { get; init; }

    [JsonPropertyName("visibility")]
    public int Visibility { get; init; }

    [JsonPropertyName("wind_speed")]
    public float WindSpeed { get; init; }

    [JsonPropertyName("wind_deg")]
    public int WindDegree { get; init; }

    [JsonPropertyName("weather")]
    public IReadOnlyCollection<WeatherDescription> Weather { get; init; } = [];
}
