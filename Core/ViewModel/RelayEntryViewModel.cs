using System;
using SmartHomeWWW.Core.Domain.Entities;

namespace SmartHomeWWW.Core.ViewModel;

public record struct RelayEntryViewModel
{
    public Guid Id { get; init; }
    public string Type { get; init; }
    public string Name { get; init; }

    public static RelayEntryViewModel From(RelayEntry entry) =>
        new () { Id = entry.Id, Type = entry.Type, Name = entry.Name };
}
