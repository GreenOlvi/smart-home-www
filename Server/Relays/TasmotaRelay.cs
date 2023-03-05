using SmartHomeWWW.Core.Domain.Relays;
using SmartHomeWWW.Core.Infrastructure.Tasmota;

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

    public async Task<RelayState> GetStateAsync()
    {
        var response = await _tasmota.GetValueAsync(_powerTopic);
        if (!response.HasValue)
        {
            return RelayState.Error;
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

    public async Task<bool> SetStateAsync(bool state) =>
        (await _tasmota.ExecuteCommandAsync(_powerTopic, state ? "on" : "off")).HasValue;

    public async Task<bool> ToggleAsync() =>
        (await _tasmota.ExecuteCommandAsync(_powerTopic, "toggle")).HasValue;
}
