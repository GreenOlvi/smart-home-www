﻿using SmartHomeWWW.Core.Utils;
using System.Text.Json.Serialization;

namespace SmartHomeWWW.Core.Domain.OpenWeatherMaps;

public readonly record struct WeatherAlert
{
    public WeatherAlert()
    {
    }

    [JsonPropertyName("sender_name")]
    public string SenderName { get; init; } = string.Empty;

    public string Event { get; init; } = string.Empty;

    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    public DateTime Start { get; init; } = DateTime.UnixEpoch;

    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    public DateTime End { get; init; } = DateTime.UnixEpoch;

    public string Description { get; init; } = string.Empty;
}
