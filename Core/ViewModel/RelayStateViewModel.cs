using SmartHomeWWW.Core.Domain.Relays;

namespace SmartHomeWWW.Core.ViewModel;

public readonly record struct RelayStateViewModel
{
    public Guid RelayId { get; init; }
    public RelayState State { get; init; }
}
