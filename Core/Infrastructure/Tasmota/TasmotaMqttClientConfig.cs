namespace SmartHomeWWW.Core.Infrastructure.Tasmota
{
    public class TasmotaMqttClientConfig : ITasmotaClientConfig
    {
        public TasmotaClientKind Kind => TasmotaClientKind.Mqtt;
        public string DeviceId { get; set; } = string.Empty;
        public int RelayId { get; set; } = 1;
    }
}
