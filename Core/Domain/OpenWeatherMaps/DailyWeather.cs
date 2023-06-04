using System.Text.Json.Serialization;
using SmartHomeWWW.Core.Utils;

namespace SmartHomeWWW.Core.Domain.OpenWeatherMaps;

public record DailyWeather
{
    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    [JsonPropertyName("dt")]
    public DateTime Timestamp { get; init; } = DateTime.UnixEpoch;

    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    public DateTime Sunrise { get; init; } = DateTime.UnixEpoch;

    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    public DateTime Sunset { get; init; } = DateTime.UnixEpoch;

    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    public DateTime Moonrise { get; init; } = DateTime.UnixEpoch;

    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    public DateTime Moonset { get; init; } = DateTime.UnixEpoch;

    [JsonPropertyName("temp")]
    public DailyTemperature Temperature { get; init; } = new DailyTemperature();

    [JsonPropertyName("feels_like")]
    public DailyFeelsLike FeelsLike { get; init; } = new DailyFeelsLike();

    public int Pressure { get; init; }

    public int Humidity { get; init; }

    [JsonPropertyName("dew_point")]
    public float DewPoint { get; init; }

    [JsonPropertyName("wind_speed")]
    public float WindSpeed { get; init; }

    [JsonPropertyName("wind_deg")]
    public int WindDegree { get; init; }

    [JsonPropertyName("wind_gust")]
    public float WindGust { get; init; }

    public WeatherDescription[] Weather { get; init; } = Array.Empty<WeatherDescription>();

    [JsonPropertyName("pop")]
    public float ProbabilityOfPrecipitation { get; init; }

    public float Uvi { get; init; }

    public int Clouds { get; init; }
}
