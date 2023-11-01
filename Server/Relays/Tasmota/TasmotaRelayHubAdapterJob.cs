using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Relays;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.Hubs;
using SmartHomeWWW.Server.Messages.Events;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SmartHomeWWW.Server.Relays.Tasmota;

public sealed partial class TasmotaRelayHubAdapterJob : IOrchestratorJob, IMessageHandler<TasmotaPropertyUpdateEvent>
{
    private readonly IDbContextFactory<SmartHomeDbContext> _dbContextFactory;
    private readonly IMessageBus _bus;
    private readonly IHubConnection _hubConnection;
    private readonly Lazy<Dictionary<string, Guid>> _deviceIdCache;

    public TasmotaRelayHubAdapterJob(IDbContextFactory<SmartHomeDbContext> dbContextFactory, IMessageBus bus, IHubConnection hubConnection)
    {
        _dbContextFactory = dbContextFactory;
        _bus = bus;
        _hubConnection = hubConnection;
        _deviceIdCache = new Lazy<Dictionary<string, Guid>>(() => LoadDeviceIdCache().GetAwaiter().GetResult());
    }

    private async Task<Dictionary<string, Guid>> LoadDeviceIdCache()
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();
        var result = new Dictionary<string, Guid>();

        foreach (var relay in db.Relays)
        {
            var c = ((JsonElement)relay.Config).Deserialize<TasmotaMqttClientConfig>();
            if (c is null || c.DeviceId == string.Empty)
            {
                continue;
            }
            result[$"{c.DeviceId}-{c.RelayId}"] = relay.Id;
        }

        return result;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static readonly Regex PowerPattern = BuildPowerPattern();

    public async Task Handle(TasmotaPropertyUpdateEvent message)
    {
        if (!PowerPattern.IsMatch(message.PropertyName))
        {
            return;
        }

        var relayId = 1;
        var m = PowerPattern.Match(message.PropertyName);
        if (m.Groups["relayId"].Success && int.TryParse(m.Groups["relayId"].Value, out var parsedId))
        {
            relayId = parsedId;
        }

        var id = await GetRelayId(message.DeviceId, relayId);
        if (id is null)
        {
            return;
        }

        var state = message.Value.ToUpperInvariant() switch
        {
            "ON" => RelayState.On,
            "OFF" => RelayState.Off,
            _ => RelayState.Unknown,
        };

        await _hubConnection.SendUpdateRelayState(id.Value, state);
    }

    private Task<Guid?> GetRelayId(string deviceId, int relayId) =>
        Task.FromResult((Guid?)(_deviceIdCache.Value.TryGetValue($"{deviceId}-{relayId}", out var id) ? id : null));

    public Task Start(CancellationToken cancellationToken = default)
    {
        _bus.Subscribe(this);
        return Task.CompletedTask;
    }

    public Task Stop(CancellationToken cancellationToken = default)
    {
        _bus.Unsubscribe(this);
        return Task.CompletedTask;
    }

    [GeneratedRegex(@"^POWER(?<relayId>\d+)?$", RegexOptions.Compiled)]
    private static partial Regex BuildPowerPattern();
}
