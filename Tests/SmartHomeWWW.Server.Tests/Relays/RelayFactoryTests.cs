using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.MessageBus;
using SmartHomeWWW.Server.Relays;
using SmartHomeWWW.Server.Relays.Tasmota;

namespace SmartHomeWWW.Server.Tests.Relays;

[TestFixture]
public class RelayFactoryTests
{
    [Test]
    public void BuildRelayFromEntryHttpTest()
    {
        var entry = new RelayEntry
        {
            Id = Guid.Parse("3be25090-efe0-4b4e-a0d1-4218733ceecc"),
            Name = "Test relay",
            Type = "Tasmota",
            Config = new { Host = "relay1.local", RelayId = 1 },
        };

        var httpFactory = new Mock<IHttpClientFactory>();
        httpFactory.Setup(f => f.CreateClient(TasmotaClientFactory.HttpClientName))
            .Returns(() => new HttpClient());

        var tcf = new TasmotaClientFactory(
            NullLoggerFactory.Instance,
            httpFactory.Object,
            new Mock<IMessageBus>().Object);

        var factory = new RelayFactory(tcf);

        using var relay = factory.Create(entry);

        relay.Should().BeOfType<TasmotaRelay>();
    }

    [Test]
    public void BuildRelayFromEntryMqttTest()
    {
        var entry = new RelayEntry
        {
            Id = Guid.Parse("3be25090-efe0-4b4e-a0d1-4218733ceecd"),
            Name = "Test relay",
            Type = "Tasmota",
            Config = new { Kind = "Mqtt", DeviceId = "tasmota_0A1B2C", RelayId = 1 },
        };

        var httpFactory = new Mock<IHttpClientFactory>();
        httpFactory.Setup(f => f.CreateClient(TasmotaClientFactory.HttpClientName))
            .Returns(() => new HttpClient());

        var tcf = new TasmotaClientFactory(
            NullLoggerFactory.Instance,
            httpFactory.Object,
            new Mock<IMessageBus>().Object);

        var factory = new RelayFactory(tcf);

        using var relay = factory.Create(entry);

        relay.Should().BeOfType<TasmotaRelay>();
    }
}
