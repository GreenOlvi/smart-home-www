using System;
using System.Net.Http;

namespace SmartHomeWWW.Core.Infrastructure.Tasmota
{
    public class TasmotaHttpClientFactory : ITasmotaClientFactory
    {
        public TasmotaHttpClientFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private readonly IHttpClientFactory _httpClientFactory;

        public ITasmotaClient CreateFor(string baseUrl) =>
            new TasmotaHttpClient(_httpClientFactory.CreateClient(), new Uri(baseUrl));
    }
}
