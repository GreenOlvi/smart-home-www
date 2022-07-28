using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace SmartHomeWWW.Core.Domain.Relays;

public interface IRelay
{
    public Task<Maybe<bool>> GetStateAsync();
    public Task<bool> SetStateAsync(bool state);
    public Task<bool> ToggleAsync();
}
