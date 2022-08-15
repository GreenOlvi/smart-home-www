using System.Net;
using System.Text.Json.Serialization;

namespace SmartHomeWWW.Server.Mqtt;

public record TasmotaDiscoveryMessage
{
    [JsonPropertyName("ip")]
    public string Ip { get; init; } = string.Empty;

    [JsonPropertyName("dn")]
    public string DeviceName { get; init; } = string.Empty;

    [JsonPropertyName("fn")]
    public string[] FriendlyNames { get; init; } = Array.Empty<string>();

    [JsonIgnore]
    public string? FriendlyName => FriendlyNames.FirstOrDefault();

    [JsonPropertyName("mac")]
    public string Mac { get; init; } = string.Empty;

    [JsonPropertyName("md")]
    public string Module { get; init; } = string.Empty;

    [JsonPropertyName("hn")]
    public string HostName { get; init; } = string.Empty;

    [JsonPropertyName("sw")]
    public string SoftwareVersion { get; init; } = string.Empty;

    [JsonPropertyName("t")]
    public string Topic { get; init; } = string.Empty;

    [JsonPropertyName("rl")]
    public int[] Relays { get; init; } = Array.Empty<int>();
}
