using SmartHomeWWW.Core.Domain.Entities;
using System.Net.Http.Json;

namespace SmartHomeWWW.Client.HttpClients;

public class SensorsHttpClient
{
    public SensorsHttpClient(HttpClient http)
    {
        _httpClient = http;
    }

    private readonly HttpClient _httpClient;

    public async Task<IEnumerable<Sensor>> GetSensors() =>
        await _httpClient.GetFromJsonAsync<IEnumerable<Sensor>>("api/sensors") ?? Enumerable.Empty<Sensor>();

    public Task DeleteSensor(Guid id) => _httpClient.DeleteAsync($"api/sensors/{id}");
}
