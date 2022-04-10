namespace SmartHomeWWW.Core.Infrastructure.Tasmota
{
    public record TasmotaHttpClientConfig : ITasmotaClientConfig
    {
        public TasmotaClientKind Kind => TasmotaClientKind.Http;
        public string Host { get; set; } = string.Empty;
        public int RelayId { get; set; } = 1;
    }
}
