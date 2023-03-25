using System.Net.Http.Json;
using SmartHomeWWW.Core.Domain.Entities;

namespace SmartHomeWWW.Client.HttpClients;

public class SensorsHttpClient
{
    public SensorsHttpClient(HttpClient http)
    {
        _httpClient = http;
    }

    private readonly HttpClient _httpClient;

    public async Task<IEnumerable<Sensor>> GetSensors(CancellationToken cancellationToken = default) =>
        await _httpClient.GetFromJsonAsync<IEnumerable<Sensor>>("api/sensors", cancellationToken) ?? Enumerable.Empty<Sensor>();

    public Task DeleteSensor(Guid id, CancellationToken cancellationToken = default) =>
        _httpClient.DeleteAsync($"api/sensors/{id}", cancellationToken);
}
