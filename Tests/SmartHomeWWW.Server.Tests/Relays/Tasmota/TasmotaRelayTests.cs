using CSharpFunctionalExtensions;
using SmartHomeWWW.Core.Domain.Relays;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using SmartHomeWWW.Server.Relays.Tasmota;

namespace SmartHomeWWW.Server.Tests.Relays.Tasmota;

[TestFixture]
public class TasmotaRelayTests
{
    [Test]
    public async Task GetStateAsyncTest()
    {
        var client = Substitute.For<ITasmotaClient>();
        client.GetValueAsync("POWER")
            .Returns(Task.FromResult(Maybe.From(JsonDocument.Parse("""{"POWER":"ON"}"""))));

        using var relay = new TasmotaRelay(client, 1);
        var response = await relay.GetStateAsync();
        response.Should().Be(RelayState.On);
    }

    [Test]
    public async Task GetStateAsyncOnNoResponseTest()
    {
        var client = Substitute.For<ITasmotaClient>();
        client.GetValueAsync("POWER")
            .Returns(Task.FromResult(Maybe<JsonDocument>.None));

        using var relay = new TasmotaRelay(client, 1);
        var response = await relay.GetStateAsync();
        response.Should().Be(RelayState.Unknown);
    }

    [Test]
    public async Task GetStateAsyncWithRelayIdsTest()
    {
        var client = Substitute.For<ITasmotaClient>();
        client.GetValueAsync("POWER")
            .Returns(Task.FromResult(Maybe.From(JsonDocument.Parse("""{"POWER1":"OFF"}"""))));

        using var relay = new TasmotaRelay(client, 1);
        var response = await relay.GetStateAsync();
        response.Should().Be(RelayState.Off);
    }
}
