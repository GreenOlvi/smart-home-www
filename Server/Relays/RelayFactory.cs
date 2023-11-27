using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Domain.Relays;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using SmartHomeWWW.Server.Relays.Tasmota;
using System.Text.Json;

namespace SmartHomeWWW.Server.Relays;

public class RelayFactory(TasmotaClientFactory tasmotaFactory) : IRelayFactory
{
    private readonly TasmotaClientFactory _tasmotaClientFactory = tasmotaFactory;

    public IRelay Create(RelayEntry entry) => entry.Type switch
    {
        "Tasmota" => CreateTasmota(entry),
        _ => throw new InvalidOperationException($"Unknown relay type '{entry.Type}'"),
    };

    private TasmotaRelay CreateTasmota(RelayEntry entry)
    {
        var config = ParseTasmotaConfig((JsonElement)entry.Config);
        return new TasmotaRelay(_tasmotaClientFactory.CreateFor(config), config.RelayId);
    }

    private static ITasmotaClientConfig ParseTasmotaConfig(JsonElement config)
    {
        var kind = TasmotaClientKind.Http;
        if (config.TryGetProperty("Kind", out var kindProperty))
        {
            if (!Enum.TryParse(kindProperty.GetString(), out kind))
            {
                throw new InvalidOperationException("Could not parse tasmota config");
            }
        }

        return kind switch
        {
            TasmotaClientKind.Http => new TasmotaHttpClientConfig
            {
                Host = config.GetProperty("Host").GetString() ?? string.Empty,
                RelayId = config.TryGetProperty("RelayId", out var idProp)
                    ? idProp.GetInt32()
                    : 1,
            },
            TasmotaClientKind.Mqtt => new TasmotaMqttClientConfig
            {
                DeviceId = config.GetProperty("DeviceId").GetString() ?? string.Empty,
                RelayId = config.TryGetProperty("RelayId", out var idProp)
                    ? idProp.GetInt32()
                    : 1,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(config)),
        };
    }
}
