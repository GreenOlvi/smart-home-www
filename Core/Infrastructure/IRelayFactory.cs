using SmartHomeWWW.Core.Domain.Entities;
using SmartHomeWWW.Core.Domain.Relays;

namespace SmartHomeWWW.Core.Infrastructure
{
    public interface IRelayFactory
    {
        IRelay Create(RelayEntry entry);
    }
}
