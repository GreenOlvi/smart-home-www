using CSharpFunctionalExtensions;
using System.Threading.Tasks;

namespace SmartHomeWWW.Core.Domain.Relays
{
    public interface IRelay
    {
        public Task<Maybe<bool>> GetStateAsync();
        public Task<bool> SetStateAsync();
        public Task<bool> ToggleAsync();
    }
}
