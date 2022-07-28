namespace SmartHomeWWW.Core.Infrastructure.Tasmota;

public interface ITasmotaClientConfig
{
    TasmotaClientKind Kind { get; }
    int RelayId { get; }
}
