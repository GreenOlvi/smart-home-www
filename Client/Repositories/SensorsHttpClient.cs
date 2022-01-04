using SmartHomeWWW.Core.Domain;
using System.Net.Http.Json;

namespace SmartHomeWWW.Client.Repositories
{
    public class SensorsHttpClient
    {
        public SensorsHttpClient(HttpClient http)
        {
            _httpClient = http;
        }

        private readonly HttpClient _httpClient;

        public async Task<IEnumerable<Sensor>> GetSensors() =>
            (await _httpClient.GetFromJsonAsync<IEnumerable<Sensor>>("api/sensors")) ?? Enumerable.Empty<Sensor>();

        public async Task DeleteSensor(Guid id)
        {
            await _httpClient.DeleteAsync($"api/sensors/{id}");
        }
    }
}
