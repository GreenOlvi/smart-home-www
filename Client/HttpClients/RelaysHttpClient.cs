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

    public async Task<IEnumerable<RelayEntryViewModel>> GetRelays() =>
        await _httpClient.GetFromJsonAsync<IEnumerable<RelayEntryViewModel>>("api/relay")
            ?? Enumerable.Empty<RelayEntryViewModel>();

    public async Task<RelayState> ToggleRelay(Guid id)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("value", "toggle"),
        });

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(4));
        var result = await _httpClient.PostAsync($"api/relay/{id}/state", content, cts.Token);
        var state = await result.Content.ReadFromJsonAsync<RelayStateViewModel>(cancellationToken: cts.Token);

        return state.State;
    }

    public async Task<RelayState> GetState(Guid id, CancellationToken? cancellationToken = null)
    {
        var result = await _httpClient.GetFromJsonAsync<RelayStateViewModel>($"api/relay/{id}/state", cancellationToken ?? CancellationToken.None);
        return result.State;
    }
}
