using System.Text.Json;
using SmartHomeWWW.Core.Utils.Functional;

namespace SmartHomeWWW.Core.Infrastructure.Tasmota;

public interface ITasmotaClient : IDisposable
{
    Task<Option<JsonDocument>> ExecuteCommandAsync(string command, string value);
    Task<Option<JsonDocument>> GetValueAsync(string command);
}
