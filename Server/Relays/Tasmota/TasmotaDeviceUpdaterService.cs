using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using SmartHomeWWW.Server.Config;
using System.Text.Json;

namespace SmartHomeWWW.Server.Relays.Tasmota;

public class TasmotaDeviceUpdaterService(ILogger<TasmotaDeviceUpdaterService> logger, IDbContextFactory<SmartHomeDbContext> dbContextFactory,
    IOptions<TasmotaDiscoveryConfig> config)
{
    private readonly ILogger<TasmotaDeviceUpdaterService> _logger = logger;
    private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory = dbContextFactory;
    private readonly TasmotaDiscoveryConfig _config = config.Value;

    public async Task UpdateDevice(TasmotaDiscoveryMessage data)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var relays = db.Relays.Where(r => r.Type == "Tasmota").ToArray();

        var pairs = relays.Select(r => TryGetKind(r, out var kind) ? (true, (r, kind)) : (false, (r, kind)))
            .Where(t => t.Item1)
            .Select(t => t.Item2)
            .ToArray();

        var updatedHttp = false;
        if (_config.UpdateHttpRelays)
        {
            updatedHttp = TryUpdateRelays<TasmotaHttpClientConfig>(pairs.Where(p => p.kind == TasmotaClientKind.Http).Select(p => p.r),
                data, db, GetHttpEntriesFromData, HttpRelayMatches, UpdateHttpRelayWithDevice);
        }

        var updatedMqtt = false;
        if (_config.UpdateMqttRelays)
        {
            updatedMqtt = TryUpdateRelays<TasmotaMqttClientConfig>(pairs.Where(p => p.kind == TasmotaClientKind.Mqtt).Select(p => p.r),
                data, db, GetMqttEntriesFromData, MqttRelayMatches, UpdateMqttRelayWithDevice);
        }

        if (updatedMqtt || updatedHttp)
        {
            await db.SaveChangesAsync();
        }
    }

    private bool TryUpdateRelays<TConfig>(IEnumerable<RelayEntry> relays, TasmotaDiscoveryMessage data, SmartHomeDbContext db,
        Func<TasmotaDiscoveryMessage, IEnumerable<RelayEntry>> getEntriesFromData,
        Func<(RelayEntry, TConfig), (RelayEntry, TConfig), bool> relayMatches,
        Func<(RelayEntry, TConfig), (RelayEntry, TConfig), bool> updateRelayWithDevice)
    {
        var changed = false;
        (RelayEntry Relay, TConfig Config)[] devices = getEntriesFromData(data)
            .Select(d => (d, Resolve<TConfig>(d.Config)))
            .ToArray();

        var relaysWithConfigs = relays.Select(r => (r, Resolve<TConfig>(r.Config))).ToArray();

        foreach (var device in devices)
        {
            var matched = relaysWithConfigs.Where(r => relayMatches(r, device)).ToArray();
            if (matched.Length > 0)
            {
                foreach (var matchedRelay in matched)
                {
                    if (updateRelayWithDevice(matchedRelay, device))
                    {
                        changed = true;
                        _logger.LogInformation("Relay updated ({Name})", device.Relay.Name);
                    }
                }
            }
            else
            {
                db.Relays.Add(device.Relay);
                changed = true;
                _logger.LogInformation("New relay found ({Name})", device.Relay.Name);
            }
        }
        return changed;
    }

    private static bool MqttRelayMatches((RelayEntry, TasmotaMqttClientConfig) relay, (RelayEntry, TasmotaMqttClientConfig) device) =>
        relay.Item2.DeviceId == device.Item2.DeviceId && relay.Item2.RelayId == device.Item2.RelayId;

    private static bool UpdateMqttRelayWithDevice((RelayEntry, TasmotaMqttClientConfig) matchedRelay, (RelayEntry, TasmotaMqttClientConfig) device)
    {
        if (matchedRelay.Item1.Name != device.Item1.Name)
        {
            matchedRelay.Item1.Name = device.Item1.Name;
            return true;
        }
        return false;
    }

    private static IEnumerable<RelayEntry> GetMqttEntriesFromData(TasmotaDiscoveryMessage data)
    {
        var relayCount = data.Relays.Count(i => i != 0);
        for (var r = 1; r <= relayCount; r++)
        {
            var nameSuffix = relayCount > 1 ? $"-{r}" : string.Empty;
            yield return new RelayEntry
            {
                Id = Guid.NewGuid(),
                Type = "Tasmota",
                Name = $"{data.FriendlyName}{nameSuffix} MQTT",
                Config = new TasmotaMqttClientConfig
                {
                    DeviceId = data.Topic,
                    RelayId = r,
                },
            };
        }
    }

    private static bool HttpRelayMatches((RelayEntry, TasmotaHttpClientConfig) relay, (RelayEntry, TasmotaHttpClientConfig) device) =>
        relay.Item1.Name == device.Item1.Name && relay.Item2.RelayId == device.Item2.RelayId;

    private static bool UpdateHttpRelayWithDevice((RelayEntry, TasmotaHttpClientConfig) matchedRelay, (RelayEntry, TasmotaHttpClientConfig) device)
    {
        if (matchedRelay.Item2.Host != device.Item2.Host)
        {
            matchedRelay.Item1.Config = matchedRelay.Item2 with { Host = device.Item2.Host };
            return true;
        }
        return false;
    }

    private static IEnumerable<RelayEntry> GetHttpEntriesFromData(TasmotaDiscoveryMessage data)
    {
        var relayCount = data.Relays.Count(i => i != 0);
        for (var r = 1; r <= relayCount; r++)
        {
            var nameSuffix = relayCount > 1 ? $"-{r}" : string.Empty;
            yield return new RelayEntry
            {
                Id = Guid.NewGuid(),
                Type = "Tasmota",
                Name = data.FriendlyName + nameSuffix,
                Config = new TasmotaHttpClientConfig
                {
                    Host = data.Ip,
                    RelayId = r,
                },
            };
        }
    }

    private bool TryGetKind(RelayEntry relay, out TasmotaClientKind kind)
    {
        if (relay.Config is not JsonElement je)
        {
            kind = TasmotaClientKind.Http;
            return false;
        }

        kind = TasmotaClientKind.Http;
        if (je.TryGetProperty("Kind", out var kindProperty) && !Enum.TryParse(kindProperty.GetString(), out kind))
        {
            _logger.LogError("Could not parse tasmota config ({Kind})", kindProperty);
            return false;
        }

        return true;
    }

    private static T Resolve<T>(object config) => config switch
    {
        T c => c,
        JsonElement json => json.Deserialize<T>()!,
        _ => throw new InvalidOperationException(),
    };
}
