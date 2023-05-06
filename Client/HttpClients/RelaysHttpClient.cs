using System.Net.Http.Json;
using SmartHomeWWW.Core.Domain.Relays;
using SmartHomeWWW.Core.ViewModel;

namespace SmartHomeWWW.Client.HttpClients;

public class RelaysHttpClient
{
    public RelaysHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private readonly HttpClient _httpClient;

    public async Task<IEnumerable<RelayEntryViewModel>> GetRelays(CancellationToken cancellationToken = default) =>
        await _httpClient.GetFromJsonAsync<IEnumerable<RelayEntryViewModel>>("api/relay", cancellationToken)
            ?? Enumerable.Empty<RelayEntryViewModel>();

    public async Task<RelayState> ToggleRelay(Guid id, CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("value", "toggle"),
        });

        var result = await _httpClient.PostAsync($"api/relay/{id}/state", content, cancellationToken);
        var state = await result.Content.ReadFromJsonAsync<RelayStateViewModel>(cancellationToken: cancellationToken);

        return state.State;
    }

    public async Task<RelayState> GetState(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<RelayStateViewModel>($"api/relay/{id}/state", cancellationToken);
        return result.State;
    }

    public async Task SetRelay(Guid id, bool on, CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("value", on ? "on" : "off"),
        });

        await _httpClient.PostAsync($"api/relay/{id}/state", content, cancellationToken);
    }
}
