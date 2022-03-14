using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.ViewModel;
using System.Net.Http.Json;
using static SmartHomeWWW.Client.Shared.RelayBox;

namespace SmartHomeWWW.Client.HttpClients
{
    public class RelaysHttpClient
    {
        public RelaysHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private readonly HttpClient _httpClient;

        public async Task<IEnumerable<RelayEntry>> GetRelays() =>
            await _httpClient.GetFromJsonAsync<IEnumerable<RelayEntry>>("api/relay")
                ?? Enumerable.Empty<RelayEntry>();

        public async Task<RelayState> ToggleRelay(Guid id)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("value", "toggle"),
            });

            var result = await _httpClient.PostAsync($"api/relay/{id}/state", content);
            var state = await result.Content.ReadFromJsonAsync<RelayStateViewModel>();

            if (!state.State.HasValue)
            {
                return RelayState.Unknown;
            }
            return state.State.Value ? RelayState.On : RelayState.Off;
        }

        public async Task<RelayState> GetState(Guid id)
        {
            var result = await _httpClient.GetFromJsonAsync<RelayStateViewModel>($"api/relay/{id}/state");
            if (!result.State.HasValue)
            {
                return RelayState.Unknown;
            }
            return result.State.Value ? RelayState.On : RelayState.Off;
        }
    }
}
