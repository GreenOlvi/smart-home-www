namespace SmartHomeWWW.Server.Config;

public record TasmotaDiscoveryConfig
{
    public bool UpdateHttpRelays { get; set; } = true;
    public bool UpdateMqttRelays { get; set; } = true;
}
