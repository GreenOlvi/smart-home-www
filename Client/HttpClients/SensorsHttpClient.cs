using SmartHomeWWW.Core.Domain.Entities;
using System.Net.Http.Json;

namespace SmartHomeWWW.Client.HttpClients;

public class SensorsHttpClient(HttpClient http)
{
    private readonly HttpClient _httpClient = http;

    public async Task<IEnumerable<Sensor>> GetSensors(CancellationToken cancellationToken = default) =>
        await _httpClient.GetFromJsonAsync<IEnumerable<Sensor>>("api/sensors", cancellationToken) ?? Enumerable.Empty<Sensor>();

    public Task DeleteSensor(Guid id, CancellationToken cancellationToken = default) =>
        _httpClient.DeleteAsync($"api/sensors/{id}", cancellationToken);
}
