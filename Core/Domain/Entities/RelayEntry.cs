using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartHomeWWW.Core.Infrastructure.Tasmota;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace SmartHomeWWW.Core.Domain.Entities;

[Table("Relays")]
public record RelayEntry
{
    [Key]
    public Guid Id { get; init; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public object Config { get; set; } = new { };

    [NotMapped]
    public TasmotaClientKind? Kind => GetKind(this);

    private static TasmotaClientKind? GetKind(RelayEntry relay) =>
        ((ITasmotaClientConfig?)ParseTasmotaConfig(relay.Config))?.Kind;

    public static ITasmotaClientConfig ParseTasmotaConfig(object config)
    {
        if (config is ITasmotaClientConfig c)
        {
            return c;
        }

        var configType = config.GetType();
        var kindValue = configType.GetProperty("Kind")?.GetValue(config) as string;

        var kind = TasmotaClientKind.Http;
        if (kindValue is not null && !Enum.TryParse(kindValue, out kind))
        {
            throw new InvalidOperationException("Could not parse tasmota config");
        }

        return kind switch
        {
            TasmotaClientKind.Http => new TasmotaHttpClientConfig
            {
                Host = configType.GetProperty("Host")?.GetValue(config) as string ?? string.Empty,
                RelayId = (int)(configType.GetProperty("RelayId")?.GetValue(config) ?? 1),
            },
            TasmotaClientKind.Mqtt => new TasmotaMqttClientConfig
            {
                DeviceId = configType.GetProperty("DeviceId")?.GetValue(config) as string ?? string.Empty,
                RelayId = (int)(configType.GetProperty("RelayId")?.GetValue(config) ?? 1),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(config)),
        };
    }
}
