using CSharpFunctionalExtensions;
using System.Collections.Concurrent;
using System.Text.Json;

namespace SmartHomeWWW.Server.Infrastructure;

public class MemoryKeyValueStore : IKeyValueStore
{
    private readonly ConcurrentDictionary<string, (string Value, DateTime? ExpireAt)> _store = new();
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = false };

    public Task AddValueAsync(string key, string value, TimeSpan? lifetime = null)
    {
        DateTime? expireAt = lifetime.HasValue ? DateTime.UtcNow.Add(lifetime.Value) : null;
        _store.AddOrUpdate(key, (value, expireAt), (k, v) => (value, expireAt));
        return Task.CompletedTask;
    }

    public Task AddValueAsync<T>(string key, T value, TimeSpan? lifetime = null) => AddValueAsync(key, Serialize(value), lifetime);

    public Task<T?> GetValueAsync<T>(string key) => throw new NotImplementedException();

    public Task<Maybe<T>> TryGetValueAsync<T>(string key)
    {
        if (!_store.TryGetValue(key, out var val))
        {
            return Task.FromResult(Maybe<T>.None);
        }

        var (value, expireAt) = val;
        if (expireAt <= DateTime.UtcNow)
        {
            return Task.FromResult(Maybe<T>.None);
        }

        var deserialized = Deserialize<T>(value);
        if (deserialized is null)
        {
            return Task.FromResult(Maybe<T>.None);
        }

        return Task.FromResult(Maybe<T>.From(deserialized));
    }

    public Task<bool> ContainsKeyAsync(string key)
    {
        if (!_store.TryGetValue(key, out var val))
        {
            return Task.FromResult(false);
        }

        var (value, expireAt) = val;
        if (expireAt <= DateTime.UtcNow)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    private string Serialize<T>(T value) => JsonSerializer.Serialize(value, _serializerOptions);
    private T? Deserialize<T>(string value) => JsonSerializer.Deserialize<T>(value, _serializerOptions);
}
