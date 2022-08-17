using CSharpFunctionalExtensions;

namespace SmartHomeWWW.Server.Infrastructure;

public interface IKeyValueStore
{
    public Task AddValueAsync<T>(string key, T value, TimeSpan? lifetime = default);
    public Task<T?> GetValueAsync<T>(string key);
    public Task<Maybe<T>> TryGetValueAsync<T>(string key);
}
