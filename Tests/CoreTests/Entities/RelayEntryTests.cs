using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Infrastructure;
using SmartHomeWWW.Core.Infrastructure.Tasmota;

namespace SmartHomeWWW.Core.Tests.Entities;

[TestFixture]
public class RelayEntryTests
{
    [SetUp]
    public async Task Setup() => _db = await SmartHomeDbTestContextFactory.CreateInMemoryAsync();

    [TearDown]
    public async Task Teardown()
    {
        if (_db != null)
        {
            await _db.Database.CloseConnectionAsync();
            await _db.DisposeAsync();
        }
    }

    private SmartHomeDbContext? _db;

    [Test]
    public async Task SaveRelayTest()
    {
        var relay = new RelayEntry
        {
            Id = Guid.NewGuid(),
            Type = "tasmota",
            Name = "Light",
            Config = new { Host = "http://192.168.1.12", RelayId = 1 },
        };

        _db!.Relays.Add(relay);
        await _db.SaveChangesAsync();

        var fetched = await _db.Relays.FindAsync(relay.Id);
        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(relay.Id);
        fetched.Type.Should().Be(relay.Type);
        fetched.Name.Should().Be(relay.Name);
        fetched.Config.Should().BeEquivalentTo(relay.Config);
        fetched.Kind.Should().Be(TasmotaClientKind.Http);
    }

    [Test]
    public async Task SaveMqttRelayTest()
    {
        var relay = new RelayEntry
        {
            Id = Guid.NewGuid(),
            Type = "tasmota",
            Name = "Plug",
            Config = new TasmotaMqttClientConfig { DeviceId = "plug-relay" },
        };

        _db!.Relays.Add(relay);
        await _db.SaveChangesAsync();

        var fetched = await _db.Relays.FindAsync(relay.Id);
        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(relay.Id);
        fetched.Type.Should().Be(relay.Type);
        fetched.Name.Should().Be(relay.Name);
        fetched.Config.Should().BeEquivalentTo(relay.Config);
        fetched.Kind.Should().Be(TasmotaClientKind.Mqtt);
    }
}
