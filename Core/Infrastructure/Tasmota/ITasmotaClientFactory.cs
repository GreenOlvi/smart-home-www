namespace SmartHomeWWW.Core.Infrastructure.Tasmota
{
    public interface ITasmotaClientFactory
    {
        ITasmotaClient CreateFor(ITasmotaClientConfig config);
    }
}