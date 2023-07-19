using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Server.Hubs;
using SmartHomeWWW.Core.Domain.Relays;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Server.Relays.Tasmota;
using SmartHomeWWW.Core.MessageBus;

namespace SmartHomeWWW.Server.Tests.Relays.Tasmota;

[TestFixture]
public class TasmotaRelayHubAdapterJobTests
{
    [Test]
    public async Task ReceivingTasmotaPowerUpdateMessageShouldSendHubMessageTest()
    {
        var relayId = Guid.NewGuid();

        var db = await CreateInMemoryAsync();
        db.Relays.Add(new RelayEntry
        {
            Id = relayId,
            Config = new { Kind = "Mqtt", DeviceId = "tas-1234AB", RelayId = 1 },
            Name = "test relay",
            Type = "Tasmota",
        });
        db.Relays.Add(new RelayEntry
        {
            Id = Guid.NewGuid(),
            Config = new { Kind = "Http", Host = "192.168.1.10", RelayId = 1 },
            Name = "test http relay",
            Type = "Tasmota",
        });
        await db.SaveChangesAsync();

        var bus = new Mock<IMessageBus>();
        var dbContextFactory = new Mock<IDbContextFactory<SmartHomeDbContext>>();
        dbContextFactory.Setup(f => f.CreateDbContextAsync(CancellationToken.None).Result).Returns(db);
        var hub = new Mock<IHubConnection>();

        var adapter = new TasmotaRelayHubAdapterJob(dbContextFactory.Object, bus.Object, hub.Object);
        await adapter.Start();

        await adapter.Handle(new Messages.Events.TasmotaPropertyUpdateEvent
        {
            DeviceId = "tas-1234AB",
            PropertyName = "POWER",
            Value = "ON",
        });

        hub.Verify(h => h.SendUpdateRelayState(relayId, RelayState.On, CancellationToken.None), Times.Once());

        hub.Invocations.Clear();
        await adapter.Handle(new Messages.Events.TasmotaPropertyUpdateEvent
        {
            DeviceId = "tas-1234AB",
            PropertyName = "POWER",
            Value = "OFF",
        });

        hub.Verify(h => h.SendUpdateRelayState(relayId, RelayState.Off, CancellationToken.None), Times.Once());
    }

    [Test]
    public async Task ReceivingTasmotaPowerUpdateForMoreRelaysShouldSendHubMessageTest()
    {
        var relay1Id = Guid.NewGuid();
        var relay2Id = Guid.NewGuid();

        var db = await CreateInMemoryAsync();
        db.Relays.Add(new RelayEntry
        {
            Id = relay1Id,
            Config = new { Kind = "Mqtt", DeviceId = "tas-1234AB", RelayId = 1 },
            Name = "test relay-1",
            Type = "Tasmota",
        });
        db.Relays.Add(new RelayEntry
        {
            Id = relay2Id,
            Config = new { Kind = "Mqtt", DeviceId = "tas-1234AB", RelayId = 2 },
            Name = "test relay-2",
            Type = "Tasmota",
        });
        await db.SaveChangesAsync();

        var bus = new Mock<IMessageBus>();
        var dbContextFactory = new Mock<IDbContextFactory<SmartHomeDbContext>>();
        dbContextFactory.Setup(f => f.CreateDbContextAsync(CancellationToken.None).Result).Returns(db);
        var hub = new Mock<IHubConnection>();

        var adapter = new TasmotaRelayHubAdapterJob(dbContextFactory.Object, bus.Object, hub.Object);
        await adapter.Start();

        await adapter.Handle(new Messages.Events.TasmotaPropertyUpdateEvent
        {
            DeviceId = "tas-1234AB",
            PropertyName = "POWER2",
            Value = "ON",
        });

        hub.Verify(h => h.SendUpdateRelayState(relay2Id, RelayState.On, CancellationToken.None), Times.Once());

        hub.Invocations.Clear();
        await adapter.Handle(new Messages.Events.TasmotaPropertyUpdateEvent
        {
            DeviceId = "tas-1234AB",
            PropertyName = "POWER1",
            Value = "OFF",
        });

        hub.Verify(h => h.SendUpdateRelayState(relay1Id, RelayState.Off, CancellationToken.None), Times.Once());

        hub.Invocations.Clear();
        await adapter.Handle(new Messages.Events.TasmotaPropertyUpdateEvent
        {
            DeviceId = "tas-1234AB",
            PropertyName = "POWER",
            Value = "ON",
        });

        hub.Verify(h => h.SendUpdateRelayState(relay1Id, RelayState.On, CancellationToken.None), Times.Once());
    }
}
