using SmartHomeWWW.Client.HttpClients;
using SmartHomeWWW.Core.Domain.Relays;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using SmartHomeWWW.Core.ViewModel;

namespace SmartHomeWWW.Client.Models;

public class RelayProxy
{
    private static readonly TimeSpan RelayClientTimeout = TimeSpan.FromSeconds(5);

    private readonly RelayEntryViewModel _relay;
    private readonly RelaysHttpClient _client;

    public RelayProxy(RelayEntryViewModel relay, RelaysHttpClient client)
    {
        _relay = relay;
        _client = client;
    }

    public Guid Id => _relay.Id;
    public string Name => _relay.Name;
    public string Type => _relay.Type;
    public TasmotaClientKind? Kind => _relay.Kind;
    public RelayViewState State { get; private set; } = RelayViewState.Fetching;

    public async ValueTask Toggle()
    {
        using var cts = new CancellationTokenSource(RelayClientTimeout);
        var s = await _client.ToggleRelay(Id, cts.Token);

        if (Kind.HasValue && Kind.Value == TasmotaClientKind.Http)
        {
            UpdateState(s);
        }
    }

    public async ValueTask SetState(bool on)
    {
        using var cts = new CancellationTokenSource(RelayClientTimeout);
        await _client.SetRelay(Id, on, cts.Token);
    }

    public async ValueTask FetchState()
    {
        State = RelayViewState.Fetching;
        using var cts = new CancellationTokenSource(RelayClientTimeout);
        var state = await _client.GetState(_relay.Id, cts.Token);
        State = ToViewState(state);
    }

    public void UpdateState(RelayState state) => State = ToViewState(state);

    public Task<bool> Delete() => _client.DeleteRelay(Id);

    private static RelayViewState ToViewState(RelayState state) =>
        state switch
        {
            RelayState.Unknown => RelayViewState.Fetching,
            RelayState.On => RelayViewState.On,
            RelayState.Off => RelayViewState.Off,
            RelayState.Error => RelayViewState.Error,
            _ => RelayViewState.Unknown,
        };

    public enum RelayViewState
    {
        Unknown,
        Fetching,
        On,
        Off,
        Error,
    }
}
