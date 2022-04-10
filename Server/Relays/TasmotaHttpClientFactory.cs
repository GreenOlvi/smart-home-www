using SmartHomeWWW.Core.Infrastructure.Tasmota;

namespace SmartHomeWWW.Server.Relays
{
    public class TasmotaHttpClientFactory : ITasmotaClientFactory
    {
        public TasmotaHttpClientFactory(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
        {
            _loggerFactory = loggerFactory;
            _httpClientFactory = httpClientFactory;
        }

        private readonly ILoggerFactory _loggerFactory;
        private readonly IHttpClientFactory _httpClientFactory;

        public ITasmotaClient CreateFor(string baseUrl)
        {
            var withSchema = baseUrl.StartsWith("http") ? baseUrl : "http://" + baseUrl;
            return new TasmotaHttpClient(_loggerFactory.CreateLogger<TasmotaHttpClient>(),
                _httpClientFactory.CreateClient("Tasmota"), new Uri(withSchema));
        }
    }
}
