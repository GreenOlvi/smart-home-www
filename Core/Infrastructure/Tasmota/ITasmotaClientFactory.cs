using System;

namespace SmartHomeWWW.Core.Infrastructure.Tasmota
{
    public interface ITasmotaClientFactory
    {
        ITasmotaClient CreateFor(string baseUrl);
    }
}