namespace SmartHomeWWW.Server.Config;

public record TasmotaConfig
{
    public TasmotaDiscoveryConfig Discovery { get; set; } = new TasmotaDiscoveryConfig();
}
