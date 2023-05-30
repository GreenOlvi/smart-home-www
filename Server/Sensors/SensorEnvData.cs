using SmartHomeWWW.Core.Utils;
using System.Text.Json.Serialization;

namespace SmartHomeWWW.Server.Sensors;

public readonly record struct SensorEnvData
{
    [JsonPropertyName("TempIn")]
    public double TemperatureIn { get; init; }

    [JsonPropertyName("HumIn")]
    public double HumidityIn { get; init; }

    public string DeviceName { get; init; }
    public string Version { get; init; }
    public string Ip { get; init; }
    public string Mac { get; init; }
    public uint ConnectTime { get; init; }

    [JsonPropertyName("RSSI")]
    public int Rssi { get; init; }

    [JsonConverter(typeof(UnixEpochDateTimeConverter))]
    public DateTime Timestamp { get; init; }
}
