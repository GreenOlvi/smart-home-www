using SmartHomeWWW.Core.Infrastructure.Tasmota;
using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Relays.Tasmota;

public class TasmotaClientFactory(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory, IMessageBus bus)
{
    public const string HttpClientName = "Tasmota";

    private readonly ILoggerFactory _loggerFactory = loggerFactory;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IMessageBus _bus = bus;

    public ITasmotaClient CreateFor(ITasmotaClientConfig config) => config switch
    {
        TasmotaHttpClientConfig http => CreateHttp(http),
        TasmotaMqttClientConfig mqtt => CreateMqtt(mqtt),
        _ => throw new ArgumentOutOfRangeException(nameof(config)),
    };

    private TasmotaHttpClient CreateHttp(TasmotaHttpClientConfig config)
    {
        var withSchema = config.Host.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) ? config.Host : "http://" + config.Host;
        return new TasmotaHttpClient(_loggerFactory.CreateLogger<TasmotaHttpClient>(),
            _httpClientFactory.CreateClient(HttpClientName), new Uri(withSchema));
    }

    private TasmotaMqttClient CreateMqtt(TasmotaMqttClientConfig mqtt) =>
        new(_loggerFactory.CreateLogger<TasmotaMqttClient>(), _bus, mqtt.DeviceId);
}
