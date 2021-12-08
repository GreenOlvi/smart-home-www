using System.Text.Json.Serialization;

namespace SmartHomeCore.Domain.OpenWeatherMaps
{
    public record DailyFeelsLike
    {
        public float Day { get; init; }
        public float Night { get; init; }

        [JsonPropertyName("eve")]
        public float Evening { get; init; }

        [JsonPropertyName("morn")]
        public float Morning { get; init; }
    }
}