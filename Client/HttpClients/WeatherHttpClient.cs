using SmartHomeWWW.Core.Domain.OpenWeatherMaps;
using System.Net.Http.Json;

namespace SmartHomeWWW.Client.HttpClients
{
    public class WeatherHttpClient
    {
        public WeatherHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private readonly HttpClient _httpClient;

        public async Task<WeatherReport?> GetCurrent() =>
            await _httpClient.GetFromJsonAsync<WeatherReport>("api/weather/current");

    }
}
