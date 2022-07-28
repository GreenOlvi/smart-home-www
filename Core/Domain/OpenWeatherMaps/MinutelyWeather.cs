using System;
using System.Text.Json.Serialization;
using SmartHomeWWW.Core.Utils;

namespace SmartHomeWWW.Core.Domain.OpenWeatherMaps;

public record MinutelyWeather
{
    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    [JsonPropertyName("dt")]
    public DateTime Timestamp { get; init; } = DateTime.UnixEpoch;

    public float Precipitation { get; init; }
}
