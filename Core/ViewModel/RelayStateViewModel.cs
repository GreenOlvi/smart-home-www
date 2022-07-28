using System;

namespace SmartHomeWWW.Core.ViewModel;

public record struct RelayStateViewModel
{
    public Guid RelayId { get; init; }
    public bool? State { get; init; }
}
