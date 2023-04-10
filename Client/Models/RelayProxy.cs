using SmartHomeWWW.Client.HttpClients;
using SmartHomeWWW.Core.Domain.Relays;
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
    public string Kind => _relay.Kind?.ToString() ?? string.Empty;
    public RelayViewState State { get; private set; }

    public async ValueTask Toggle()
    {
        State = RelayViewState.Fetching;
        var s = await _client.ToggleRelay(_relay.Id);
        UpdateState(s);
    }

    public async ValueTask FetchState()
    {
        State = RelayViewState.Fetching;
        using var cts = new CancellationTokenSource(RelayClientTimeout);
        var state = await _client.GetState(_relay.Id, cts.Token);
        State = ToViewState(state);
    }

    public void UpdateState(RelayState state) => State = ToViewState(state);

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
