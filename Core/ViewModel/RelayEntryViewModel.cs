using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Infrastructure.Tasmota;

namespace SmartHomeWWW.Core.ViewModel;

public readonly record struct RelayEntryViewModel
{
    public Guid Id { get; init; }
    public string Type { get; init; }
    public TasmotaClientKind? Kind { get; init; }
    public string Name { get; init; }

    public static RelayEntryViewModel From(RelayEntry entry) => new()
    {
        Id = entry.Id,
        Type = entry.Type,
        Kind = entry.Kind,
        Name = entry.Name,
    };
}
