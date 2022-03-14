using CSharpFunctionalExtensions;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using System.Threading.Tasks;

namespace SmartHomeWWW.Core.Domain.Relays;

public class TasmotaRelay : IRelay
{
    public TasmotaRelay(ITasmotaClient tasmota, int relayId = 1)
    {
        _tasmota = tasmota;
        _relayId = relayId;
    }

    private readonly ITasmotaClient _tasmota;
    private readonly int _relayId;

    public async Task<Maybe<bool>> GetStateAsync()
    {
        var response = await _tasmota.GetValueAsync($"Power{_relayId}");
        if (!response.HasValue)
        {
            return Maybe.None;
        }

        var obj = response.Value.RootElement;

        if (obj.TryGetProperty("Command", out var cmd))
        {
            if (cmd.GetString().ToLowerInvariant() == "error")
            {
                return Maybe.None;
            }
        }

        var value = obj.GetProperty("POWER").GetString().ToLowerInvariant();
        return value == "on";
    }

    public async Task<bool> SetStateAsync(bool state) =>
        (await _tasmota.ExecuteCommandAsync($"Power{_relayId}", state ? "on" : "off")).HasValue;

    public async Task<bool> ToggleAsync() =>
        (await _tasmota.ExecuteCommandAsync($"Power{_relayId}", "toggle")).HasValue;
}
