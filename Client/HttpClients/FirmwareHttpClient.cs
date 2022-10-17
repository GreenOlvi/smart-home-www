using System.Net.Http.Json;
using SmartHomeWWW.Core.Domain;
using SmartHomeWWW.Core.Firmwares;

namespace SmartHomeWWW.Client.HttpClients;

public class FirmwareHttpClient
{
    public FirmwareHttpClient(HttpClient http)
    {
        _httpClient = http;
    }

    private readonly HttpClient _httpClient;

    public async Task<IEnumerable<IFirmware>> GetAllFirmwares() =>
        await _httpClient.GetFromJsonAsync<IEnumerable<IFirmware>>("api/update")
            ?? Enumerable.Empty<IFirmware>();

    public async Task<FirmwareVersion> GetCurrentVersion() =>
        await _httpClient.GetFromJsonAsync<FirmwareVersion>("api/update/version/current")
            ?? new FirmwareVersion();
}
