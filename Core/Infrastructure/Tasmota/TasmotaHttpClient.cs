using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Flurl;

namespace SmartHomeWWW.Core.Infrastructure.Tasmota
{
    public class TasmotaHttpClient : ITasmotaClient
    {
        public TasmotaHttpClient(HttpClient httpClient, Uri baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
        }

        private readonly HttpClient _httpClient;
        private readonly Uri _baseUrl;

        public Task<Maybe<JsonDocument>> ExecuteCommandAsync(string command, string value) =>
            GetUrl(_baseUrl.AppendPathSegment("cm").SetQueryParam("cmnd", $"{command} {value}"));

        public Task<Maybe<JsonDocument>> GetValueAsync(string command) =>
            GetUrl(_baseUrl.AppendPathSegment("cm").SetQueryParam("cmnd", command));

        private async Task<Maybe<JsonDocument>> GetUrl(Url uri)
        {
            var response = await _httpClient.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                return Maybe.None;
            }

            return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        }
    }
}
