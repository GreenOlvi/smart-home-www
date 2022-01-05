using SmartHomeWWW.Core.Domain;
using System.Net.Http.Json;

namespace SmartHomeWWW.Client.HttpClients;

public class FirmwareHttpClient
{
    public FirmwareHttpClient(HttpClient http)
    {
        _httpClient = http;
    }

    private readonly HttpClient _httpClient;

    public async Task<IEnumerable<Firmware>> GetAllFirmwares() =>
        await _httpClient.GetFromJsonAsync<IEnumerable<Firmware>>("api/update") ?? Enumerable.Empty<Firmware>();

    public async Task<Version?> GetCurrentVersion() =>
        await _httpClient.GetFromJsonAsync<Version>("api/update/version/current");
}
