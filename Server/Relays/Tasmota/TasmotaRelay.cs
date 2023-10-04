using SmartHomeWWW.Core.Domain.Relays;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using SmartHomeWWW.Core.Utils.Functional;
using System.Text.Json;

namespace SmartHomeWWW.Server.Relays.Tasmota;

public sealed class TasmotaRelay : IRelay
{
    public TasmotaRelay(ITasmotaClient tasmota, int relayId = 1)
    {
        _tasmota = tasmota;
        _relayId = relayId;
        _powerTopic = relayId == 1 ? "POWER" : $"POWER{_relayId}";
    }

    private readonly ITasmotaClient _tasmota;
    private readonly int _relayId;
    private string _powerTopic;
    private bool _unknownTopic;

    public async Task<RelayState> GetStateAsync() =>
        GetValueFromResponse(await _tasmota.GetValueAsync(_powerTopic));

    public async Task<RelayState> SetStateAsync(bool state) =>
        GetValueFromResponse(await _tasmota.ExecuteCommandAsync(_powerTopic, state ? "on" : "off"));

    public async Task<RelayState> ToggleAsync() =>
        GetValueFromResponse(await _tasmota.ExecuteCommandAsync(_powerTopic, "toggle"));

    private RelayState GetValueFromResponse(Option<JsonDocument> response) =>
        response.MatchSome(
            val => GetValueFromResponse(val.Value.RootElement),
            () => RelayState.Unknown);

    private RelayState GetValueFromResponse(JsonElement obj)
    {
        if (obj.TryGetProperty("Command", out var cmd) &&
            cmd.GetString()?.ToLowerInvariant() == "error")
        {
            return RelayState.Error;
        }

        if (!TryGetPowerValue(obj, out var value))
        {
            // log error
            return RelayState.Unknown;
        }

        return value == "on" ? RelayState.On : RelayState.Off;
    }

    private bool TryGetPowerValue(JsonElement obj, out string value)
    {
        value = string.Empty;
        if (_unknownTopic)
        {
            return false;
        }

        if (obj.TryGetProperty(_powerTopic, out var value1))
        {
            var val1Str = value1.GetString();
            if (val1Str != null)
            {
                value = val1Str.ToLowerInvariant();
                return true;
            }
        }

        if (_relayId == 1 && obj.TryGetProperty("POWER1", out var valueAlt))
        {
            var val2Str = valueAlt.GetString();
            if (val2Str != null)
            {
                _powerTopic = "POWER1";
                value = val2Str.ToLowerInvariant();
                return true;
            }
        }
        _unknownTopic = true;
        return false;
    }

    public void Dispose() => _tasmota.Dispose();
}
