using System.Text.Json.Serialization;

namespace SmartHomeWWW.Core.Domain.OpenWeatherMaps;

public readonly record struct DailyFeelsLike
{
    [JsonPropertyName("morn")]
    public float Morning { get; init; }

    [JsonPropertyName("day")]
    public float Day { get; init; }

    [JsonPropertyName("eve")]
    public float Evening { get; init; }

    [JsonPropertyName("night")]
    public float Night { get; init; }

}
