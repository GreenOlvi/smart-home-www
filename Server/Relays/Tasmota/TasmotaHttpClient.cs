using System.Text.Json;
using Flurl;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using SmartHomeWWW.Core.Utils.Functional;
using static SmartHomeWWW.Core.Utils.Functional.Option<System.Text.Json.JsonDocument>;

namespace SmartHomeWWW.Server.Relays.Tasmota;

public sealed class TasmotaHttpClient : ITasmotaClient
{
    public TasmotaHttpClient(ILogger<TasmotaHttpClient> logger, HttpClient httpClient, Uri baseUrl)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
    }

    private readonly ILogger<TasmotaHttpClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly Uri _baseUrl;

    public Task<Option<JsonDocument>> ExecuteCommandAsync(string command, string value) =>
        GetUrl(_baseUrl.AppendPathSegment("cm").SetQueryParam("cmnd", $"{command} {value}"));

    public Task<Option<JsonDocument>> GetValueAsync(string command) =>
        GetUrl(_baseUrl.AppendPathSegment("cm").SetQueryParam("cmnd", command));

    private async Task<Option<JsonDocument>> GetUrl(Url uri)
    {
        try
        {
            var response = await _httpClient.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error from relay: {StatusCode}", response.StatusCode);
                return new None();
            }

            return new Some(await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync()));
        }
        catch (Exception e)
        {
            _logger.LogError("Error while sending request to relay: {Message}", e.Message);
            _logger.LogDebug(e, "Exception caught");
        }
        return new None();
    }

    public void Dispose() => _httpClient.Dispose();
}
