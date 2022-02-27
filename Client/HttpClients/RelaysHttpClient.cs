using SmartHomeWWW.Core.Domain.Entities;
using System.Net.Http.Json;

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

        public async Task<bool> ToggleRelay(Guid id)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("value", "toggle"),
            });

            var result = await _httpClient.PostAsync($"api/relay/{id}/state", content);
            return true;
        }
    }
}
