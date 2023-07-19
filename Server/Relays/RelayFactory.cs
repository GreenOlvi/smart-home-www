using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Domain.Relays;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Server.Relays.Tasmota;

namespace SmartHomeWWW.Server.Relays;

public class RelayFactory : IRelayFactory
{
    public RelayFactory(TasmotaClientFactory tasmotaFactory)
    {
        _tasmotaClientFactory = tasmotaFactory;
    }

    private readonly TasmotaClientFactory _tasmotaClientFactory;

    public IRelay Create(RelayEntry entry) => entry.Type switch
    {
        "Tasmota" => CreateTasmota(entry),
        _ => throw new InvalidOperationException($"Unknown relay type '{entry.Type}'"),
    };

    private IRelay CreateTasmota(RelayEntry entry)
    {
        var config = RelayEntry.ParseTasmotaConfig(entry.Config);
        return new TasmotaRelay(_tasmotaClientFactory.CreateFor(config), config.RelayId);
    }

}
