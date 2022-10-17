using System.Net.Http.Json;
using SmartHomeWWW.Core.Domain;
using SmartHomeWWW.Core.Firmwares;
using SmartHomeWWW.Core.ViewModel;

namespace SmartHomeWWW.Client.HttpClients;

public class FirmwareHttpClient
{
    public FirmwareHttpClient(HttpClient http)
    {
        _httpClient = http;
    }

    private readonly HttpClient _httpClient;

    public async Task<IEnumerable<FirmwareViewModel>> GetAllFirmwares() =>
        await _httpClient.GetFromJsonAsync<IEnumerable<FirmwareViewModel>>("api/update")
            ?? Enumerable.Empty<FirmwareViewModel>();

    public async Task<IDictionary<UpdateChannel, FirmwareVersion>> GetCurrentVersion() =>
        await _httpClient.GetFromJsonAsync<IDictionary<UpdateChannel, FirmwareVersion>>("api/update/version/current")
            ?? new Dictionary<UpdateChannel, FirmwareVersion>();
}
