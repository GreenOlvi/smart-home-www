using SmartHomeWWW.Core.Utils;
using System.Text.Json.Serialization;

namespace SmartHomeWWW.Core.Domain.OpenWeatherMaps;

public readonly record struct MinutelyWeather
{
    public MinutelyWeather()
    {
    }

    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    [JsonPropertyName("dt")]
    public DateTime Timestamp { get; init; } = DateTime.UnixEpoch;

    [JsonPropertyName("precipitation")]
    public float Precipitation { get; init; }
}
