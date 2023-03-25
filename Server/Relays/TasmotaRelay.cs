using CSharpFunctionalExtensions;
using SmartHomeWWW.Core.Domain.Relays;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using System.Text.Json;

namespace SmartHomeWWW.Server.Relays;

public class TasmotaRelay : IRelay
{
    public TasmotaRelay(ITasmotaClient tasmota, int relayId = 1)
    {
        _tasmota = tasmota;
        _relayId = relayId;
        _powerTopic = relayId == 1 ? "POWER" : $"POWER{_relayId}";
    }

    private readonly ITasmotaClient _tasmota;
    private readonly int _relayId;
    private readonly string _powerTopic;

    public async Task<RelayState> GetStateAsync() =>
        GetValueFromResponse(await _tasmota.GetValueAsync(_powerTopic));

    public async Task<RelayState> SetStateAsync(bool state) =>
        GetValueFromResponse(await _tasmota.ExecuteCommandAsync(_powerTopic, state ? "on" : "off"));

    public async Task<RelayState> ToggleAsync() =>
        GetValueFromResponse(await _tasmota.ExecuteCommandAsync(_powerTopic, "toggle"));

    private static RelayState GetValueFromResponse(Maybe<JsonDocument> response)
    {
        if (!response.HasValue)
        {
            return RelayState.Unknown;
        }

        var obj = response.Value.RootElement;
        if (obj.TryGetProperty("Command", out var cmd))
        {
            if (cmd.GetString()?.ToLowerInvariant() == "error")
            {
                return RelayState.Error;
            }
        }

        var value = obj.GetProperty("POWER").GetString()?.ToLowerInvariant();
        return value == "on" ? RelayState.On : RelayState.Off;
    }

}
