using SmartHomeCore.Utils;
using System;
using System.Text.Json.Serialization;

namespace SmartHomeCore.Domain.OpenWeatherMaps
{
    public record WeatherAlert
    {
        [JsonPropertyName("sender_name")]
        public string SenderName { get; init; }

        public string Event { get; init; }

        [JsonConverter(typeof(UnixEpochDateTimeConverter))]
        public DateTime Start { get; init; } = DateTime.UnixEpoch;

        [JsonConverter(typeof(UnixEpochDateTimeConverter))]
        public DateTime End { get; init; } = DateTime.UnixEpoch;

        public string Description { get; init; }
    }
}