using SmartHomeWWW.Core.Infrastructure.Tasmota;

namespace SmartHomeWWW.Server.Relays
{
    public class TasmotaClientFactory
    {
        public TasmotaClientFactory(ILogger<TasmotaClientFactory> logger, ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _httpClientFactory = httpClientFactory;
        }

        private readonly ILogger<TasmotaClientFactory> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHttpClientFactory _httpClientFactory;

        public ITasmotaClient CreateFor(ITasmotaClientConfig config)
        {
            return config switch
            {
                TasmotaHttpClientConfig http => CreateHttp(http),
                _ => throw new ArgumentOutOfRangeException(nameof(config)),
            };
        }

        private ITasmotaClient CreateHttp(TasmotaHttpClientConfig config)
        {
            var withSchema = config.Host.StartsWith("http") ? config.Host : "http://" + config.Host;
            return new TasmotaHttpClient(_loggerFactory.CreateLogger<TasmotaHttpClient>(),
                _httpClientFactory.CreateClient("Tasmota"), new Uri(withSchema));
        }
    }
}
