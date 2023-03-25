using NUnit.Framework;
using SmartHomeWWW.Server.Relays;
using Moq;
using SmartHomeWWW.Server.Messages;
using Microsoft.EntityFrameworkCore;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Server.Hubs;
using SmartHomeWWW.Core.Domain.Relays;
using SmartHomeWWW.Core.Domain.Entities;
using static SmartHomeWWW.Server.Tests.SmartHomeDbTestContextFactory;

namespace SmartHomeWWW.Server.Tests.Relays;

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

        await adapter.Handle(new Messages.Events.TasmotaPropertyUpdateEvent
        {
            DeviceId = "tas-1234AB",
            PropertyName = "POWER",
            Value = "OFF",
        });

        hub.Verify(h => h.SendUpdateRelayState(relayId, RelayState.Off, CancellationToken.None), Times.Once());
    }
}
