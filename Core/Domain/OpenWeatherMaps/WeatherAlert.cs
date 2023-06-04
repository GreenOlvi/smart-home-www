using System.Text.Json.Serialization;
using SmartHomeWWW.Core.Utils;

namespace SmartHomeWWW.Core.Domain.OpenWeatherMaps;

public record WeatherAlert
{
    [JsonPropertyName("sender_name")]
    public string SenderName { get; init; } = string.Empty;

    public string Event { get; init; } = string.Empty;

    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    public DateTime Start { get; init; } = DateTime.UnixEpoch;

    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    public DateTime End { get; init; } = DateTime.UnixEpoch;

    public string Description { get; init; } = string.Empty;
}
