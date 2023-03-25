using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Domain.Relays;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using SmartHomeWWW.Server.Hubs;
using SmartHomeWWW.Server.Messages;
using SmartHomeWWW.Server.Messages.Events;
using System.Text.Json;

namespace SmartHomeWWW.Server.Relays;

public sealed class TasmotaRelayHubAdapterJob : IOrchestratorJob, IMessageHandler<TasmotaPropertyUpdateEvent>
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
            result[c.DeviceId] = relay.Id;
        }

        return result;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public async Task Handle(TasmotaPropertyUpdateEvent message)
    {
        if  (message.PropertyName != "POWER")
        {
            return;
        }

        var id = await GetRelayId(message.DeviceId);
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

    private Task<Guid?> GetRelayId(string deviceId) =>
        Task.FromResult((Guid?)(_deviceIdCache.Value.TryGetValue(deviceId, out var id) ? id : null));

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
}
