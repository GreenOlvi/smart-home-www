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
            ConfigSerialized = """{"Kind":"Mqtt","DeviceId":"tas-1234AB","RelayId":1}""",
            Name = "test relay",
            Type = "Tasmota",
        });
        db.Relays.Add(new RelayEntry
        {
            Id = Guid.NewGuid(),
            ConfigSerialized = """{"Kind":"Http","Host":"192.168.1.10","RelayId":1}""",
            Name = "test http relay",
            Type = "Tasmota",
        });
        await db.SaveChangesAsync();

        var bus = Substitute.For<IMessageBus>();
        var dbContextFactory = Substitute.For<IDbContextFactory<SmartHomeDbContext>>();
        dbContextFactory.CreateDbContextAsync(CancellationToken.None).Returns(db);
        var hub = Substitute.For<IHubConnection>();

        var adapter = new TasmotaRelayHubAdapterJob(dbContextFactory, bus, hub);
        await adapter.Start();

        await adapter.Handle(new Messages.Events.TasmotaPropertyUpdateEvent
        {
            DeviceId = "tas-1234AB",
            PropertyName = "POWER",
            Value = "ON",
        });

        _ = hub.Received().SendUpdateRelayState(relayId, RelayState.On, CancellationToken.None);

        hub.ClearReceivedCalls();
        await adapter.Handle(new Messages.Events.TasmotaPropertyUpdateEvent
        {
            DeviceId = "tas-1234AB",
            PropertyName = "POWER",
            Value = "OFF",
        });

        _ = hub.Received().SendUpdateRelayState(relayId, RelayState.Off, CancellationToken.None);
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
            ConfigSerialized = """{"Kind":"Mqtt","DeviceId":"tas-1234AB","RelayId":1}""",
            Name = "test relay-1",
            Type = "Tasmota",
        });
        db.Relays.Add(new RelayEntry
        {
            Id = relay2Id,
            ConfigSerialized = """{"Kind":"Mqtt","DeviceId":"tas-1234AB","RelayId":2}""",
            Name = "test relay-2",
            Type = "Tasmota",
        });
        await db.SaveChangesAsync();

        var bus = Substitute.For<IMessageBus>();
        var dbContextFactory = Substitute.For<IDbContextFactory<SmartHomeDbContext>>();
        dbContextFactory.CreateDbContextAsync(CancellationToken.None).Returns(db);
        var hub = Substitute.For<IHubConnection>();

        var adapter = new TasmotaRelayHubAdapterJob(dbContextFactory, bus, hub);
        await adapter.Start();

        await adapter.Handle(new Messages.Events.TasmotaPropertyUpdateEvent
        {
            DeviceId = "tas-1234AB",
            PropertyName = "POWER2",
            Value = "ON",
        });

        _ = hub.Received().SendUpdateRelayState(relay2Id, RelayState.On, CancellationToken.None);

        hub.ClearReceivedCalls();
        await adapter.Handle(new Messages.Events.TasmotaPropertyUpdateEvent
        {
            DeviceId = "tas-1234AB",
            PropertyName = "POWER1",
            Value = "OFF",
        });

        _ = hub.Received().SendUpdateRelayState(relay1Id, RelayState.Off, CancellationToken.None);

        hub.ClearReceivedCalls();
        await adapter.Handle(new Messages.Events.TasmotaPropertyUpdateEvent
        {
            DeviceId = "tas-1234AB",
            PropertyName = "POWER",
            Value = "ON",
        });

        _ = hub.Received().SendUpdateRelayState(relay1Id, RelayState.On, CancellationToken.None);
    }
}
