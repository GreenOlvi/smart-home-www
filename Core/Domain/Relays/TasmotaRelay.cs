using CSharpFunctionalExtensions;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using System;
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
        var response = await _tasmota.GetValueAsync("Power");
        if (!response.HasValue)
        {
            return Maybe.None;
        }

        return StringComparer.InvariantCultureIgnoreCase.Compare(response.Value.ToString(), "on") == 0;
    }

    public async Task<bool> SetStateAsync(bool state) =>
        (await _tasmota.ExecuteCommandAsync("Power", state ? "on" : "off")).HasValue;

    public async Task<bool> ToggleAsync() =>
        (await _tasmota.ExecuteCommandAsync("Power", "toggle")).HasValue;
}
