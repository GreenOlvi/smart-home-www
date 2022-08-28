using CSharpFunctionalExtensions;

namespace SmartHomeWWW.Server.Infrastructure;

public interface IKeyValueStore
{
    public Task AddValueAsync(string key, string value, TimeSpan? lifetime = default);
    public Task AddValueAsync<T>(string key, T value, TimeSpan? lifetime = default);
    public Task<T?> GetValueAsync<T>(string key);
    public Task<Maybe<T>> TryGetValueAsync<T>(string key);
    public Task<bool> ContainsKeyAsync(string key);
}
