using SmartHomeWWW.Core.Firmwares;
using SmartHomeWWW.Core.ViewModel;
using System.Net.Http.Json;

namespace SmartHomeWWW.Client.HttpClients;

public class FirmwareHttpClient(HttpClient http)
{
    private readonly HttpClient _httpClient = http;

    public async Task<IEnumerable<FirmwareViewModel>> GetAllFirmwares(CancellationToken cancellationToken = default) =>
        await _httpClient.GetFromJsonAsync<IEnumerable<FirmwareViewModel>>("api/update", cancellationToken)
            ?? Enumerable.Empty<FirmwareViewModel>();

    public async Task<IDictionary<UpdateChannel, FirmwareVersion>> GetCurrentVersion(CancellationToken cancellationToken = default) =>
        await _httpClient.GetFromJsonAsync<IDictionary<UpdateChannel, FirmwareVersion>>("api/update/version/current", cancellationToken)
            ?? new Dictionary<UpdateChannel, FirmwareVersion>();
}
