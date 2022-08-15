using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.Infrastructure.Tasmota;

namespace SmartHomeWWW.Server.Mqtt;

public class TasmotaDeviceUpdaterService
{
    private readonly ILogger<TasmotaDeviceUpdaterService> _logger;
    private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;

    public TasmotaDeviceUpdaterService(ILogger<TasmotaDeviceUpdaterService> logger, IDbContextFactory<SmartHomeDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task UpdateDevice(TasmotaDiscoveryMessage data)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var relays = db.Relays.Where(r => r.Type == "Tasmota").ToArray();

        var pairs = relays.Select(r => TryGetKind(r, out var kind) ? (true, (r, kind)) : (false, (r, kind)))
            .Where(t => t.Item1)
            .Select(t => t.Item2)
            .ToArray();

        var updatedHttp = TryUpdateHttpRelays(pairs.Where(p => p.kind == TasmotaClientKind.Http).Select(p => p.r), data, db);
        var updatedMqtt = TryUpdateMqttRelays(pairs.Where(p => p.kind == TasmotaClientKind.Mqtt).Select(p => p.r), data, db);

        if (updatedMqtt || updatedHttp)
        {
            await db.SaveChangesAsync();
        }
    }

    private bool TryUpdateMqttRelays(IEnumerable<RelayEntry> mqttRelays, TasmotaDiscoveryMessage data, SmartHomeDbContext db)
    {
        var found = false;
        var changed = false;
        foreach (var relay in mqttRelays)
        {
            var config = ((JsonElement)relay.Config).Deserialize<TasmotaMqttClientConfig>();
            if (config is not null && config.DeviceId == data.Topic)
            {
                found = true;

                var name = $"{data.FriendlyName} MQTT";
                if (relay.Name != name)
                {
                    relay.Name = name;
                    changed = true;

                    _logger.LogDebug("Updater {Name} relay", name);
                }

                break;
            }
        }

        if (found)
        {
            return changed;
        }

        db.Relays.Add(new RelayEntry
        {
            Id = Guid.NewGuid(),
            Type = "Tasmota",
            Name = $"{data.FriendlyName} MQTT",
            Config = new TasmotaMqttClientConfig { DeviceId = data.Topic },
        });

        _logger.LogInformation("New relay found ({Name})", $"{data.FriendlyName} MQTT");

        return true;
    }

    private bool TryUpdateHttpRelays(IEnumerable<RelayEntry> httpRelays, TasmotaDiscoveryMessage data, SmartHomeDbContext db)
    {
        var found = false;
        var changed = false;
        foreach (var relay in httpRelays)
        {
            if (relay.Name == data.FriendlyName || relay.Name == data.Topic)
            {
                found = true;
                var config = ((JsonElement)relay.Config).Deserialize<TasmotaHttpClientConfig>();
                if (config is not null && config.Host != data.Ip)
                {
                    relay.Config = new TasmotaHttpClientConfig
                    {
                        Host = data.Ip,
                        RelayId = 1,
                    };
                    changed = true;
                    _logger.LogDebug("Updater {Name} relay ip to {Ip}", relay.Name, data.Ip);
                }

                break;
            }
        }

        if (found)
        {
            return changed;
        }

        var name = data.FriendlyName ?? data.Topic;
        db.Relays.Add(new RelayEntry
        {
            Id = Guid.NewGuid(),
            Type = "Tasmota",
            Name = name,
            Config = new TasmotaHttpClientConfig { Host = data.Ip },
        });

        _logger.LogInformation("New relay found ({Name})", name);
        return true;
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
}
