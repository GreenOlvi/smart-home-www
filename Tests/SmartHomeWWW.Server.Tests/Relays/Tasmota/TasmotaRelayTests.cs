using SmartHomeWWW.Core.Domain.Relays;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using SmartHomeWWW.Core.Utils.Functional;
using SmartHomeWWW.Server.Relays.Tasmota;
using static SmartHomeWWW.Core.Utils.Functional.Option<System.Text.Json.JsonDocument>;

namespace SmartHomeWWW.Server.Tests.Relays.Tasmota;

[TestFixture]
public class TasmotaRelayTests
{
    [Test]
    public async Task GetStateAsyncTest()
    {
        var client = Substitute.For<ITasmotaClient>();
        client.GetValueAsync("POWER")
            .Returns(Task.FromResult<Option<JsonDocument>>(new Some(JsonDocument.Parse("""{"POWER":"ON"}"""))));

        using var relay = new TasmotaRelay(client, 1);
        var response = await relay.GetStateAsync();
        response.Should().Be(RelayState.On);
    }

    [Test]
    public async Task GetStateAsyncOnNoResponseTest()
    {
        var client = Substitute.For<ITasmotaClient>();
        client.GetValueAsync("POWER")
            .Returns(Task.FromResult<Option<JsonDocument>>(new None()));

        using var relay = new TasmotaRelay(client, 1);
        var response = await relay.GetStateAsync();
        response.Should().Be(RelayState.Unknown);
    }

    [Test]
    public async Task GetStateAsyncWithRelayIdsTest()
    {
        var client = Substitute.For<ITasmotaClient>();
        client.GetValueAsync("POWER")
            .Returns(Task.FromResult<Option<JsonDocument>>(new Some(JsonDocument.Parse("""{"POWER1":"OFF"}"""))));

        using var relay = new TasmotaRelay(client, 1);
        var response = await relay.GetStateAsync();
        response.Should().Be(RelayState.Off);
    }
}
