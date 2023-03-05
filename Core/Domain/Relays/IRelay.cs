namespace SmartHomeWWW.Core.Domain.Relays;

public interface IRelay
{
    public Task<RelayState> GetStateAsync();
    public Task<bool> SetStateAsync(bool state);
    public Task<bool> ToggleAsync();
}
