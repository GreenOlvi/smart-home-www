using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Domain.Relays;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using System.Text.Json;

namespace SmartHomeWWW.Server.Relays
{
    public class RelayFactory : IRelayFactory
    {
        public RelayFactory(ITasmotaClientFactory tasmotaFactory)
        {
            _tasmotaClientFactory = tasmotaFactory;
        }

        private readonly ITasmotaClientFactory _tasmotaClientFactory;

        public IRelay Create(RelayEntry entry) => entry.Type switch
        {
            "Tasmota" => CreateTasmota(entry),
            _ => throw new InvalidOperationException($"Unknown relay type '{entry.Type}'"),
        };

        private IRelay CreateTasmota(RelayEntry entry)
        {
            var el = (JsonElement)entry.Config;
            var host = el.GetProperty("Host").GetString();
            var id = el.TryGetProperty("RelayId", out var idProp) ? idProp.GetInt32() : 1;

            return new TasmotaRelay(_tasmotaClientFactory.CreateFor(host), id);
        }
    }
}
