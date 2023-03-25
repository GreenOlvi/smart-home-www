namespace SmartHomeWWW.Core.Domain.Relays;

public interface IRelay
{
    public Task<RelayState> GetStateAsync();
    public Task<RelayState> SetStateAsync(bool state);
    public Task<RelayState> ToggleAsync();
}
